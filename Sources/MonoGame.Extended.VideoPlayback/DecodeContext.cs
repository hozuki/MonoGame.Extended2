using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using FFmpeg.AutoGen;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback.AudioDecoding;
using MonoGame.Extended.VideoPlayback.Extensions;
using MonoGame.Extended.VideoPlayback.VideoDecoding;

namespace MonoGame.Extended.VideoPlayback {
    /// <inheritdoc />
    /// <summary>
    /// The coordinator and core of decoding management.
    /// </summary>
    internal sealed unsafe class DecodeContext : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="DecodeContext"/> instance.
        /// </summary>
        /// <param name="url">The URL of the video source. If a file system path is provided, make sure it is an absolute path.</param>
        /// <param name="decodingOptions">Decoding options.</param>
        internal DecodeContext([NotNull] string url, [NotNull] DecodingOptions decodingOptions) {
            _decodingOptions = decodingOptions;

            // 1. Open a format context.
            var formatContext = ffmpeg.avformat_alloc_context();
            FFmpegHelper.Verify(ffmpeg.avformat_open_input(&formatContext, url, null, null), Dispose);
            _formatContext = formatContext;

            // 2. Try to read stream information (codec, duration, etc.).
            FFmpegHelper.Verify(ffmpeg.avformat_find_stream_info(formatContext, null), Dispose);

            // 3. If we are opening an ASF container, we need a different stream start time calculation method.
            // About start_time and ASF container format (.asf, .wmv), see:
            // https://www.ffmpeg.org/doxygen/3.0/structAVStream.html#a7c67ae70632c91df8b0f721658ec5377
            var containerName = FFmpegHelper.PtrToStringNullEnded(formatContext->iformat->name, Encoding.UTF8);
            var isAsfContainer = containerName == "asf";

            VideoDecodingContext videoContext = null;
            AudioDecodingContext audioContext = null;

            // 4. Search for video and audio streams.
            for (var i = 0; i < formatContext->nb_streams; ++i) {
                var avStream = formatContext->streams[i];

                if (videoContext == null && avStream->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO) {
                    // 5. If this is a video stream, create a VideoDecodingContext from it...
                    videoContext = new VideoDecodingContext(avStream, decodingOptions);
                    _videoStreamIndex = i;

                    // ... and record its start time.
                    if (!isAsfContainer) {
                        if (avStream->start_time != ffmpeg.AV_NOPTS_VALUE) {
                            _videoStartPts = avStream->start_time;
                        }
                    } else {
                        // I don't know why but this hack works... at least on FFmpeg 3.4.
                        _videoStartPts = avStream->cur_dts / 2;
                    }
                } else if (audioContext == null && avStream->codec->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO) {
                    // 6. If this is an audio stream, create an AudioDecodingContext from it...
                    audioContext = new AudioDecodingContext(avStream);
                    _audioStreamIndex = i;

                    // ... and record its start time.
                    if (!isAsfContainer) {
                        if (avStream->start_time != ffmpeg.AV_NOPTS_VALUE) {
                            _audioStartPts = avStream->start_time;
                        }
                    } else {
                        _audioStartPts = 0;
                    }
                }
            }

            // 7. If we got a video stream...
            if (videoContext != null) {
                // ... we should get its codec...
                var codec = ffmpeg.avcodec_find_decoder(videoContext.CodecContext->codec_id);

                if (codec == null) {
                    throw new FFmpegException("Failed to find video codec.");
                }

                FFmpegHelper.Verify(ffmpeg.avcodec_open2(videoContext.CodecContext, codec, null), Dispose);

                // ... and initialize the helper data structures.
                _videoFramePool = new ObjectPool<IntPtr>(decodingOptions.VideoPacketQueueSizeThreshold, AllocFrame, DeallocFrame);
                _preparedVideoFrames = new SortedList<long, IntPtr>(decodingOptions.VideoPacketQueueSizeThreshold);
                _videoCodecName = FFmpegHelper.PtrToStringNullEnded(codec->name, Encoding.UTF8);

                IntPtr AllocFrame() {
                    return (IntPtr)ffmpeg.av_frame_alloc();
                }

                void DeallocFrame(IntPtr p) {
                    var frame = (AVFrame*)p;

                    ffmpeg.av_frame_free(&frame);
                }
            }

            // 8. If we got an audio stream...
            if (audioContext != null) {
                // ...we should get its codec...
                var codec = ffmpeg.avcodec_find_decoder(audioContext.CodecContext->codec_id);

                if (codec == null) {
                    throw new FFmpegException("Failed to find audio codec.");
                }

                FFmpegHelper.Verify(ffmpeg.avcodec_open2(audioContext.CodecContext, codec, null), Dispose);

                // ... and prepare the buffer frame.
                _audioFrame = ffmpeg.av_frame_alloc();
                _audioCodecName = FFmpegHelper.PtrToStringNullEnded(codec->name, Encoding.UTF8);
            }

            // 9. Finally, set up other objects.
            if (_videoStreamIndex >= 0) {
                _videoStream = formatContext->streams[_videoStreamIndex];
            }

            if (_audioStreamIndex >= 0) {
                _audioStream = formatContext->streams[_audioStreamIndex];
            }

            if (videoContext != null) {
                _videoPackets = new PacketQueue(decodingOptions.VideoPacketQueueCapacity);
            }

            if (audioContext != null) {
                _audioPackets = new PacketQueue(decodingOptions.AudioPacketQueueCapacity);
            }

            _videoContext = videoContext;
            _audioContext = audioContext;
        }

        /// <summary>
        /// Raises when video playback is ended.
        /// </summary>
        /// <remarks>
        /// This event MUST be raised asynchrously.
        /// The reason is that subscribers (i.e. <see cref="Framework.Media.Video.decodeContext_Ended"/>) may raise their events synchrously.
        /// The call flow can go to <see cref="Framework.Media.VideoPlayer.video_Ended"/>, where <see cref="Framework.Media.VideoPlayer.Stop"/> is called.
        /// Inside <see cref="Framework.Media.VideoPlayer.Stop"/>, it calls <see cref="Thread.Join()"/> on <see cref="Framework.Media.VideoPlayer.DecodingThread.SystemThread"/>,
        /// which waits for the decoding thread that raises the initial <see cref="Ended"/> event infinitely.
        /// So you see, there will be a dead lock if <see cref="Ended"/> is raised synchrously.
        /// </remarks>
        internal event EventHandler<EventArgs> Ended;

        /// <summary>
        /// The underlying <see cref="AVFormatContext"/>.
        /// </summary>
        internal AVFormatContext* FormatContext => _formatContext;

        /// <summary>
        /// The <see cref="VideoDecodingContext"/> associated with this <see cref="DecodeContext"/>.
        /// May be <see langword="null"/> if there is no video stream.
        /// </summary>
        [CanBeNull]
        internal VideoDecodingContext VideoContext => _videoContext;

        /// <summary>
        /// The <see cref="AudioDecodingContext"/> associated with this <see cref="DecodeContext"/>.
        /// May be <see langword="null"/> if there is no audio stream.
        /// </summary>
        [CanBeNull]
        internal AudioDecodingContext AudioContext => _audioContext;

        /// <summary>
        /// The index of the first video stream in the <see cref="AVFormatContext"/>.
        /// May be <code>-1</code> if there is no video stream.
        /// </summary>
        internal int VideoStreamIndex => _videoStreamIndex;

        /// <summary>
        /// The index of the first audio stream in the <see cref="AVFormatContext"/>.
        /// May be <code>-1</code> if there is no audio stream.
        /// </summary>
        internal int AudioStreamIndex => _audioStreamIndex;

        /// <summary>
        /// Fetches the latest decoded video frame.
        /// Remember to lock <see cref="VideoFrameTransmissionLock"/> when you use the fetched frame.
        /// </summary>
        /// <returns>The latest decoded video frame.</returns>
        [CanBeNull]
        internal AVFrame* FetchAvailableVideoFrame() => _currentVideoFrame;

        /// <summary>
        /// Name of the video codec.
        /// May be <see cref="string.Empty"/> if there is no video stream.
        /// </summary>
        [NotNull]
        internal string VideoCodecName => _videoCodecName ?? string.Empty;

        /// <summary>
        /// Name of the audio codec.
        /// May be <see cref="string.Empty"/> if there is no audio stream.
        /// </summary>
        [NotNull]
        internal string AudioCodecName => _audioCodecName ?? string.Empty;

        /// <summary>
        /// Reset the state of this <see cref="DecodeContext"/> for restarting playback.
        /// </summary>
        internal void Reset() {
            // This method may be called in Dispose() so no EnsureNotDisposed() call here.

            if (_videoPackets != null) {
                while (_videoPackets.Count > 0) {
                    var packet = _videoPackets.Dequeue();

                    ffmpeg.av_packet_unref(&packet);
                }
            }

            if (_audioPackets != null) {
                while (_audioPackets.Count > 0) {
                    var packet = _audioPackets.Dequeue();

                    ffmpeg.av_packet_unref(&packet);
                }
            }

            // When playing large video files, independently seeking audio and video streams
            // causes error number -11 thrown in TryFetchVideoFrames at first several (sometimes more than 40) frames.
            // But using the index -1 (seeking all streams at the same time) works...
            if (VideoStreamIndex >= 0 || AudioStreamIndex >= 0) {
                FFmpegHelper.Verify(ffmpeg.av_seek_frame(FormatContext, -1, 0, ffmpeg.AVSEEK_FLAG_BACKWARD), Dispose);
            }

            _videoFramePool?.Reset();
            _preparedVideoFrames?.Clear();

            // Clear the staging data in buffer audio frame.
            ffmpeg.av_frame_unref(_audioFrame);
            // The point is the same. But since video frames are managed by a frame pool, we just need to set this to null.
            _currentVideoFrame = null;

            _audioPacketDecodingOngoing = false;

            _isEndedTriggered = false;

            _nextDecodingVideoTime = double.MinValue;
        }

        /// <summary>
        /// Gets the duration of the video file, in seconds.
        /// </summary>
        /// <returns>The duration of the video file, in seconds.</returns>
        internal double GetDurationInSeconds() {
            EnsureNotDisposed();

            return FFmpegHelper.GetDurationInSeconds(_formatContext);
        }

        /// <summary>
        /// Reads video packets and decode them into video frames, until presentation time of the frame decoded is after specified time.
        /// </summary>
        /// <param name="presentationTime">Current playback time, in seconds.</param>
        internal void ReadVideoUntilPlaybackIsAfter(double presentationTime) {
            // If we don't have to decode any new frame (just use the current one), skip the later procedures.
            if (_nextDecodingVideoTime > presentationTime) {
                return;
            }

            var videoContext = _videoContext;
            var videoStream = _videoStream;
            var framePool = _videoFramePool;
            var frameQueue = _preparedVideoFrames;
            var videoPackts = _videoPackets;

            if (videoContext == null || videoStream == null || framePool == null || frameQueue == null || videoPackts == null) {
                return;
            }

            bool r;
            double decodedPresentationTime = 0;

            // The double queue + object pool design is used to solve the I/P/B frame order problem.
            // For details about I/P/B frames, see http://dranger.com/ffmpeg/tutorial05.html.

            do {
                // Try to fetch a number of video frames.
                r = TryFetchVideoFrames(_decodingOptions.VideoPacketQueueSizeThreshold);

                if (r) {
                    // If the queue of decoded frames is not empty, then get its presentation timestamp (PTS) and convert it to seconds.
                    var decodedPresentationPts = frameQueue.PeekFirstKey();
                    decodedPresentationTime = FFmpegHelper.ConvertPtsToSeconds(videoStream, decodedPresentationPts + _videoStartPts);

                    // Now get the frame.
                    var p = frameQueue.Dequeue();
                    var frame = (AVFrame*)p;

                    if (decodedPresentationTime < presentationTime) {
                        // If the time is before current playback time, release the frame because we don't need it anymore.
                        framePool.Release(p);
                    } else {
                        // Otherwise, set it as the current frame.
                        lock (VideoFrameTransmissionLock) {
                            var origFrame = _currentVideoFrame;

                            _currentVideoFrame = frame;

                            if (origFrame != null) {
                                framePool.Release((IntPtr)origFrame);
                            }
                        }
                    }

                    // Caveats:
                    // The "current" frame is actually the first frame whose PTS is after current playback time.
                    // But for most videos, this does not matter.
                    // We can make it a little more complicated (queries if there is a second frame in the sorted list)
                    // to get the actual "current" frame.
                }
            } while (r && decodedPresentationTime < presentationTime);

            // If we did get a frame to show, then update the time.
            if (r) {
                _nextDecodingVideoTime = decodedPresentationTime;
            }

            // If the video stream ends, raise Ended event.
            if (!r && videoPackts.Count == 0 && !_isEndedTriggered) {
                // Must use BeginInvoke. See the explanations of Ended.
                Ended?.BeginInvoke(this, EventArgs.Empty, null, null);

                _isEndedTriggered = true;
            }
        }

        /// <summary>
        /// Reads audio packets, decode them into audio frames and send the data to an <see cref="DynamicSoundEffectInstance"/>, until presentation time of the frame decoded is after specified time.
        /// </summary>
        /// <param name="sound">The <see cref="DynamicSoundEffectInstance"/> to play audio data.</param>
        /// <param name="presentationTime">Current playback time, in seconds.</param>
        internal void ReadAudioUntilPlaybackIsAfter([NotNull] DynamicSoundEffectInstance sound, double presentationTime) {
            var audioContext = _audioContext;
            var audioStream = _audioStream;
            var audioFrame = _audioFrame;
            var extraAudioBufferingTime = (float)_decodingOptions.ExtraAudioBufferingTime / 1000;

            if (audioContext == null || audioStream == null || audioFrame == null) {
                return;
            }

            using (var memoryStream = new MemoryStream()) {
                // We want there is as much data in one audio buffer as possible, so that we don't have to allocate too many audio buffers.
                // dataPushed is used for this task.
                var dataPushed = false;
                var fetchFramesPlease = true;

                {
                    var pts = audioFrame->pts;
                    var framePresentationTime = FFmpegHelper.ConvertPtsToSeconds(audioStream, pts + _audioStartPts);

                    if (framePresentationTime > presentationTime + extraAudioBufferingTime) {
                        // If our time has not come, skip the rest of the procedures.
                        fetchFramesPlease = false;
                    } else {
                        // Otherwise prepare to send the first lump of data.
                        if (FFmpegHelper.TransmitAudioFrame(this, audioFrame, out var buffer)) {
                            if (buffer != null) {
                                memoryStream.Write(buffer, 0, buffer.Length);

                                dataPushed = true;
                            }
                        }
                    }
                }

                if (fetchFramesPlease) {
                    // Fetch audio frames until its presentation timestamp (PTS) is later than current playback time.
                    // Prepare to send all the data received during this procedure.
                    while (TryFetchAudioFrame(audioFrame)) {
                        if (audioFrame->nb_samples == 0) {
                            continue;
                        }

                        var pts = audioFrame->pts;
                        var framePresentationTime = FFmpegHelper.ConvertPtsToSeconds(audioStream, pts);

                        if (framePresentationTime > presentationTime + extraAudioBufferingTime) {
                            break;
                        }

                        if (FFmpegHelper.TransmitAudioFrame(this, audioFrame, out var buffer)) {
                            Trace.Assert(buffer != null);

                            memoryStream.Write(buffer, 0, buffer.Length);

                            dataPushed = true;
                        }
                    }
                }

                // If there is data pushed...
                if (dataPushed) {
                    var audioData = memoryStream.ToArray();

                    // ... send the data to OpenAL's audio buffer...
                    sound.SubmitBuffer(audioData);

                    // ... and play it.
                    if (sound.State == SoundState.Stopped) {
                        sound.Play();
                    }
                }
            }
        }

        /// <summary>
        /// The lock object used to avoid unwanted frame data overwrites.
        /// </summary>
        internal readonly object VideoFrameTransmissionLock = new object();

        protected override void Dispose(bool disposing) {
            Reset();

            // Video frames are allocated by a frame pool. So we don't need to dispose it here.
            _currentVideoFrame = null;

            _videoFramePool?.Dispose();
            _videoFramePool = null;

            if (_audioFrame != null) {
                var audioFrame = _audioFrame;
                ffmpeg.av_frame_free(&audioFrame);
                _audioFrame = null;
            }

            _videoStream = null;
            _audioStream = null;

            _videoContext?.Dispose();
            _audioContext?.Dispose();

            _videoContext = null;
            _audioContext = null;

            if (_formatContext != null) {
                ffmpeg.avformat_free_context(_formatContext);
                _formatContext = null;
            }
        }

        /// <summary>
        /// Tries to get the next video packet.
        /// </summary>
        /// <param name="packet">The packet got, or an empty packet if the operation fails.</param>
        /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
        private bool TryGetNextVideoPacket(out AVPacket packet) {
            EnsureNotDisposed();

            var queue = _videoPackets;

            Debug.Assert(queue != null, nameof(queue) + " != null");

            if (!TryFillPacketQueue(queue, _decodingOptions.VideoPacketQueueSizeThreshold)) {
                if (queue.Count == 0) {
                    packet = default(AVPacket);

                    return false;
                }
            }

            packet = queue.Dequeue();

            return true;
        }

        /// <summary>
        /// Tries to get the next audio packet.
        /// </summary>
        /// <param name="packet">The packet got, or an empty packet if the operation fails.</param>
        /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
        private bool TryGetNextAudioPacket(out AVPacket packet) {
            EnsureNotDisposed();

            var queue = _audioPackets;

            Debug.Assert(queue != null, nameof(queue) + " != null");

            if (!TryFillPacketQueue(queue, _decodingOptions.AudioPacketQueueSizeThreshold)) {
                if (queue.Count == 0) {
                    packet = default(AVPacket);

                    return false;
                }
            }

            packet = queue.Dequeue();

            return true;
        }

        /// <summary>
        /// Tries to fill a <see cref="PacketQueue"/> until the number of queued packets reaches expected count.
        /// </summary>
        /// <param name="queue">The packet queue to fill.</param>
        /// <param name="count">Expected count.</param>
        /// <returns><see langword="true"/> if the queue has at least expected number of items when the function returns, otherwise <see langword="false"/>.</returns>
        private bool TryFillPacketQueue([NotNull] PacketQueue queue, int count) {
            while (queue.Count < count) {
                if (!ReadNextPacket()) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to decode and fetch some video frames, and store them in the videoframe queue.
        /// </summary>
        /// <param name="count">Expected count.</param>
        /// <returns><see langword="true"/> if the video frame queue has at least one frame when the function returns, otherwise <see langword="false"/>.</returns>
        private bool TryFetchVideoFrames(int count) {
            EnsureNotDisposed();

            Trace.Assert(_videoContext != null);

            var avStream = _videoContext.VideoStream;
            var codecContext = _videoContext.CodecContext;

            var frameQueue = _preparedVideoFrames;
            var framePool = _videoFramePool;

            Debug.Assert(frameQueue != null && framePool != null);

            var packet = new AVPacket();

            while (frameQueue.Count < count) {
                var decodingSuccessful = false;

                while (true) {
                    ffmpeg.av_packet_unref(&packet);

                    if (!TryGetNextVideoPacket(out packet)) {
                        break;
                    }

                    if (packet.size == 0) {
                        break;
                    }

                    var videoTime = FFmpegHelper.ConvertPtsToSeconds(avStream, packet.pts);

                    if (videoTime < 0) {
                        break;
                    }

                    // About avcodec_send_packet and avcodec_receive_frame:
                    // https://ffmpeg.org/doxygen/3.1/group__lavc__encdec.html

                    // First we send a video packet.
                    var error = ffmpeg.avcodec_send_packet(codecContext, &packet);

                    if (error != 0) {
                        if (error == ffmpeg.AVERROR_EOF) {
                            // End of video, maybe some other buffered frames.
                            error = ffmpeg.avcodec_send_packet(codecContext, null);

                            if (error != 0) {
                                if (error == ffmpeg.AVERROR_EOF) {
                                    // Go on.
                                } else {
                                    FFmpegHelper.Verify(error, Dispose);
                                }
                            }
                        } else {
                            FFmpegHelper.Verify(error, Dispose);
                        }
                    }

                    var frame = (AVFrame*)framePool.Acquire();

                    // Then we receive the decoded frame.
                    error = ffmpeg.avcodec_receive_frame(codecContext, frame);

                    if (error == 0) {
                        // If everything goes well, then again, lucky us.
                        ffmpeg.av_packet_unref(&packet);
                        frameQueue.Enqueue(frame->pts, (IntPtr)frame);
                        decodingSuccessful = true;

                        break;
                    } else {
                        // If this packet contains multiple frames, we have to enqueue all those frames.
                        if (error == ffmpeg.EAGAIN) {
                            frameQueue.Enqueue(frame->pts, (IntPtr)frame);
                            decodingSuccessful = true;
                        }

                        while (error == ffmpeg.EAGAIN) {
                            frame = (AVFrame*)framePool.Acquire();
                            error = ffmpeg.avcodec_receive_frame(codecContext, frame);
                            decodingSuccessful = true;

                            if (error >= 0) {
                                frameQueue.Enqueue(frame->pts, (IntPtr)frame);
                            }
                        }

                        // If an error occurs, release the frame we acquired because its data will not be used by now.
                        if (error < 0) {
                            framePool.Release((IntPtr)frame);
                        }

                        if (error == ffmpeg.AVERROR_EOF) {
                            // Go on.
                        } else if (error == -11) {
                            // Device not ready; don't know why this happens.
                        } else {
                            FFmpegHelper.Verify(error, Dispose);
                        }
                    }
                }

                if (!decodingSuccessful) {
                    break;
                }
            }

            // Clean up.
            ffmpeg.av_packet_unref(&packet);

            return frameQueue.Count > 0;
        }

        /// <summary>
        /// Tries to decode and fetch an audio frame and fill the data in given <see cref="AVFrame"/>.
        /// </summary>
        /// <param name="frame">The frame which stores the data.</param>
        /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
        private bool TryFetchAudioFrame([NotNull] AVFrame* frame) {
            EnsureNotDisposed();

            if (_audioContext == null) {
                return false;
            }

            var avStream = _audioContext.AudioStream;
            var codecContext = _audioContext.CodecContext;

            // If we were decoding audio packets, and the last packets is not fully consumed yet,
            // then try to receive one more frame.
            if (_audioPacketDecodingOngoing) {
                var error = ffmpeg.avcodec_receive_frame(codecContext, frame);

                if (error == 0) {
                    return true;
                } else {
                    _audioPacketDecodingOngoing = false;

                    if (error == ffmpeg.AVERROR_EOF) {
                        return false;
                    }
                }
            }

            var packet = new AVPacket();

            // Other logics are similar to those in TryFetchVideoFrames.

            while (true) {
                ffmpeg.av_packet_unref(&packet);

                if (!TryGetNextAudioPacket(out packet)) {
                    break;
                }

                if (packet.size == 0) {
                    break;
                }

                var audioTime = FFmpegHelper.ConvertPtsToSeconds(avStream, packet.pts);

                if (audioTime < 0) {
                    break;
                }

                var error = ffmpeg.avcodec_send_packet(codecContext, &packet);

                if (error != 0) {
                    if (error == ffmpeg.AVERROR_EOF) {
                        // End of video, maybe some other buffered frames.
                        error = ffmpeg.avcodec_send_packet(codecContext, null);

                        if (error != 0) {
                            if (error == ffmpeg.AVERROR_EOF) {
                                // Go on.
                            } else if (error == ffmpeg.EAGAIN) {
                                continue;
                            } else {
                                FFmpegHelper.Verify(error, Dispose);
                            }
                        }
                    } else if (error == ffmpeg.EAGAIN) {
                        continue;
                    } else {
                        FFmpegHelper.Verify(error, Dispose);
                    }
                }

                error = ffmpeg.avcodec_receive_frame(codecContext, frame);

                if (error == 0) {
                    ffmpeg.av_packet_unref(&packet);

                    // The packet received is fully consumed.
                    _audioPacketDecodingOngoing = true;

                    return true;
                } else if (error == ffmpeg.AVERROR_EOF) {
                    break;
                } else if (error == ffmpeg.EAGAIN) {
                    // Go on.
                } else {
                    FFmpegHelper.Verify(error, Dispose);
                }
            }

            ffmpeg.av_packet_unref(&packet);

            return false;
        }

        /// <summary>
        /// Reads the next packet, and enqueue it to corresponding packet queue.
        /// </summary>
        /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// When this function returns <see langword="true"/>, it doesn't mean that a packet is enqueued.
        /// Since the <see cref="DecodeContext"/> only holds at most one video stream and one audio stream,
        /// packets from any other streams are ignored. So you should always use an "ensure" function to make sure
        /// a packet is enqueued.
        /// </remarks>
        private bool ReadNextPacket() {
            var packet = new AVPacket();

            // Read the next packet from current format container.
            var error = ffmpeg.av_read_frame(FormatContext, &packet);

            if (error < 0) {
                if (error == ffmpeg.AVERROR_EOF) {
                    return false;
                } else {
                    FFmpegHelper.Verify(error, Dispose);
                }
            }

            // Is this an empty packet?
            if (!(packet.data != null && packet.size > 0)) {
                ffmpeg.av_packet_unref(&packet);

                return false;
            }

            if (packet.stream_index == VideoStreamIndex) {
                // If this packet belongs to the video stream, enqueue it to the video packet queue.
                Debug.Assert(_videoPackets != null);

                _videoPackets.Enqueue(packet);
            } else if (packet.stream_index == AudioStreamIndex) {
                // If this packet belongs to the audio stream, enqueue it to the audio packet queue.
                Debug.Assert(_audioPackets != null);

                _audioPackets.Enqueue(packet);
            } else {
                // Otherwise, ignore this packet.
                ffmpeg.av_packet_unref(&packet);
            }

            return true;
        }

        private AVFormatContext* _formatContext;

        [CanBeNull]
        private readonly PacketQueue _videoPackets;
        [CanBeNull]
        private readonly PacketQueue _audioPackets;

        [CanBeNull]
        private VideoDecodingContext _videoContext;
        [CanBeNull]
        private AudioDecodingContext _audioContext;

        // Current video frame and buffer audio frame.
        // Note that their usage and life cycle are totally different.
        [CanBeNull]
        private AVFrame* _currentVideoFrame;
        [CanBeNull]
        private AVFrame* _audioFrame;

        [CanBeNull]
        private AVStream* _videoStream;
        [CanBeNull]
        private AVStream* _audioStream;

        [CanBeNull]
        private readonly string _videoCodecName;
        [CanBeNull]
        private readonly string _audioCodecName;

        /// <summary>
        /// The time of latest decoded video frame, in seconds.
        /// See <see cref="ReadVideoUntilPlaybackIsAfter(double)"/> for its usage.
        /// </summary>
        private double _nextDecodingVideoTime;

        /// <summary>
        /// Whether <see cref="Ended"/> is raised.
        /// </summary>
        private bool _isEndedTriggered;

        /// <summary>
        /// Start time of the first frame in the video stream, in video stream's time base.
        /// </summary>
        private readonly long _videoStartPts;
        /// <summary>
        /// Start time of the first frame in the audio stream, in audio stream's time base.
        /// </summary>
        private readonly long _audioStartPts;

        /// <summary>
        /// Does the data from the last audio packet still have remains to fill into another audio frame?
        /// </summary>
        private bool _audioPacketDecodingOngoing;

        /// <summary>
        /// The video frame pool.
        /// </summary>
        [CanBeNull]
        private ObjectPool<IntPtr> _videoFramePool;

        /// <summary>
        /// Decoded video frames. For each entry, the key is the presentation timestamp (PTS) and the value is pointer to the decoded frame (<see cref="AVFrame"/>).
        /// </summary>
        /// <remarks>
        /// The frames are managed by the video frame pool. You should not dispose frames retrived from this list.
        /// </remarks>
        [CanBeNull]
        private readonly SortedList<long, IntPtr> _preparedVideoFrames;

        private readonly int _videoStreamIndex = -1;
        private readonly int _audioStreamIndex = -1;

        private readonly DecodingOptions _decodingOptions;

    }
}
