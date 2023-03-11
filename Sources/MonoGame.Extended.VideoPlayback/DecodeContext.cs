using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback.AudioDecoding;
using MonoGame.Extended.VideoPlayback.Extensions;
using MonoGame.Extended.VideoPlayback.VideoDecoding;
using Sdcb.FFmpeg.Raw;

namespace MonoGame.Extended.VideoPlayback;

/// <inheritdoc />
/// <summary>
/// The coordinator and core of decoding management.
/// </summary>
internal sealed unsafe class DecodeContext : DisposableBase
{

    /// <summary>
    /// Creates a new <see cref="DecodeContext"/> instance.
    /// </summary>
    /// <param name="url">The URL of the video source. If a file system path is provided, make sure it is an absolute path.</param>
    /// <param name="decodingOptions">Decoding options.</param>
    internal DecodeContext(string url, DecodingOptions decodingOptions)
    {
        _decodingOptions = decodingOptions;

        // 1. Open a format context.
        var formatContext = ffmpeg.avformat_alloc_context();
        _formatContext = formatContext;
        FFmpegHelper.Verify(ffmpeg.avformat_open_input(&formatContext, url, null, null), Dispose);

        // 2. Try to read stream information (codec, duration, etc.).
        FFmpegHelper.Verify(ffmpeg.avformat_find_stream_info(formatContext, null), Dispose);

        // The rest are initialized lazily. See LazyInitialize().
    }

    /// <summary>
    /// Raises when video playback is ended.
    /// </summary>
    /// <remarks>
    /// This event MUST be raised asynchronously.
    /// The reason is that subscribers (i.e. <see cref="Framework.Media.Video.decodeContext_Ended"/>) may raise their events synchronously.
    /// The call flow can go to <see cref="Framework.Media.VideoPlayer.video_Ended"/>, where <see cref="Framework.Media.VideoPlayer.Stop"/> is called.
    /// Inside <see cref="Framework.Media.VideoPlayer.Stop"/>, it calls <see cref="Thread.Join()"/> on <see cref="DecodingThread.SystemThread"/>,
    /// which waits for the decoding thread that raises the initial <see cref="Ended"/> event infinitely.
    /// So you see, there will be a dead lock if <see cref="Ended"/> is raised synchronously.
    /// </remarks>
    internal event EventHandler<EventArgs>? Ended;

    /// <summary>
    /// The underlying <see cref="AVFormatContext"/>.
    /// </summary>
    [MaybeNull]
    internal AVFormatContext* FormatContext
    {
        [DebuggerStepThrough]
        get
        {
            EnsureNotDisposed();

            return _formatContext;
        }
    }

    /// <summary>
    /// The <see cref="VideoDecodingContext"/> associated with this <see cref="DecodeContext"/>.
    /// May be <see langword="null"/> if there is no video stream.
    /// </summary>
    internal VideoDecodingContext? VideoContext
    {
        [DebuggerStepThrough]
        get
        {
            EnsureNotDisposed();
            EnsureInitialized();

            return _videoContext;
        }
    }

    /// <summary>
    /// The <see cref="AudioDecodingContext"/> associated with this <see cref="DecodeContext"/>.
    /// May be <see langword="null"/> if there is no audio stream.
    /// </summary>
    internal AudioDecodingContext? AudioContext
    {
        [DebuggerStepThrough]
        get
        {
            EnsureNotDisposed();
            EnsureInitialized();

            return _audioContext;
        }
    }

    /// <summary>
    /// The index of the video stream finally selected in the <see cref="AVFormatContext"/>.
    /// May be <code>-1</code> if there is no video stream.
    /// </summary>
    internal int VideoStreamIndex
    {
        [DebuggerStepThrough]
        get
        {
            EnsureInitialized();

            return _videoStreamIndex;
        }
    }

    /// <summary>
    /// The index of the audio stream finally selected in the <see cref="AVFormatContext"/>.
    /// May be <code>-1</code> if there is no audio stream.
    /// </summary>
    internal int AudioStreamIndex
    {
        [DebuggerStepThrough]
        get
        {
            EnsureInitialized();

            return _audioStreamIndex;
        }
    }

    /// <summary>
    /// Fetches the latest decoded video frame.
    /// Remember to lock <see cref="VideoFrameTransmissionLock"/> when you use the fetched frame.
    /// </summary>
    /// <returns>The latest decoded video frame.</returns>
    [return: MaybeNull]
    internal AVFrame* FetchAvailableVideoFrame() => _currentVideoFrame;

    /// <summary>
    /// Name of the video codec.
    /// May be <see cref="string.Empty"/> if there is no video stream.
    /// </summary>
    internal string VideoCodecName
    {
        [DebuggerStepThrough]
        get
        {
            EnsureInitialized();

            return _videoCodecName ?? string.Empty;
        }
    }

    /// <summary>
    /// Name of the audio codec.
    /// May be <see cref="string.Empty"/> if there is no audio stream.
    /// </summary>
    internal string AudioCodecName
    {
        [DebuggerStepThrough]
        get
        {
            EnsureInitialized();

            return _audioCodecName ?? string.Empty;
        }
    }

    /// <summary>
    /// The index of the video stream user selected.
    /// May be <code>-1</code> for auto selection.
    /// </summary>
    public int UserSelectedVideoStreamIndex
    {
        [DebuggerStepThrough]
        get => _userSelectedVideoStreamIndex;
    }

    /// <summary>
    /// The index of the audio stream user selected.
    /// May be <code>-1</code> for auto selection.
    /// </summary>
    public int UserSelectedAudioStreamIndex
    {
        [DebuggerStepThrough]
        get => _userSelectedAudioStreamIndex;
    }

    /// <summary>
    /// Whether auto looping is enabled.
    /// </summary>
    internal bool IsLooped
    {
        [DebuggerStepThrough]
        get => _isLooped;
        [DebuggerStepThrough]
        set
        {
            _isLooped = value;

            if (!value)
            {
                _currentLoopNumber = 0;
            }
        }
    }

    /// <summary>
    /// Gets the number of all types of streams in the media file.
    /// </summary>
    /// <returns>Number of all type of streams.</returns>
    internal int GetTotalStreamCount()
    {
        return (int)_formatContext->nb_streams;
    }

    /// <summary>
    /// Gets the number of video streams in the media file.
    /// </summary>
    /// <returns>Number of video streams.</returns>
    internal int GetVideoStreamCount()
    {
        var count = 0;

        for (var i = 0; i < _formatContext->nb_streams; ++i)
        {
            var avStream = _formatContext->streams[i];

            if (avStream->codecpar->codec_type == AVMediaType.Video)
            {
                ++count;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the number of audio streams in the media file.
    /// </summary>
    /// <returns>Number of audio streams.</returns>
    internal int GetAudioStreamCount()
    {
        var count = 0;

        for (var i = 0; i < _formatContext->nb_streams; ++i)
        {
            var avStream = _formatContext->streams[i];

            if (avStream->codecpar->codec_type == AVMediaType.Audio)
            {
                ++count;
            }
        }

        return count;
    }

    /// <summary>
    /// Selects a video stream by index.
    /// This method is only valid before initialization.
    /// </summary>
    /// <param name="streamIndex">The index of video streams.</param>
    internal void SelectVideoStream(int streamIndex)
    {
        if (_isInitialized)
        {
            return;
        }

        var totalStreams = GetTotalStreamCount();

        if (streamIndex >= totalStreams)
        {
            streamIndex = -1;
        }

        _userSelectedVideoStreamIndex = streamIndex;
    }

    /// <summary>
    /// Selects an audio stream by index.
    /// This method is only valid before initialization.
    /// </summary>
    /// <param name="streamIndex">The index of audio streams.</param>
    internal void SelectAudioStream(int streamIndex)
    {
        if (_isInitialized)
        {
            return;
        }

        var totalStreams = GetTotalStreamCount();

        if (streamIndex >= totalStreams)
        {
            streamIndex = -1;
        }

        _userSelectedAudioStreamIndex = streamIndex;
    }

    /// <summary>
    /// Forces initializing <see cref="DecodeContext"/> before it is automatically initialized by calling specific methods.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Initialize()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// Reset the state of this <see cref="DecodeContext"/> for restarting playback.
    /// </summary>
    internal void Reset()
    {
        EnsureInitialized();

        // This method may be called in Dispose() so no EnsureNotDisposed() call here.

        LockFrameQueuesUpdate();

        try
        {
            ResetBeforeSeek();

            // When playing large video files, independently seeking audio and video streams
            // causes error number -11 thrown in TryFetchVideoFrames at first several (sometimes more than 40) frames.
            // But using the index -1 (seeking all streams at the same time) works...
            if (VideoStreamIndex >= 0 || AudioStreamIndex >= 0)
            {
                FFmpegHelper.Verify(ffmpeg.av_seek_frame(FormatContext, -1, 0, (int)AVSEEK_FLAG.Backward), Dispose);
            }

            ResetAfterSeek();
        }
        finally
        {
            UnlockFrameQueueUpdate();
        }
    }

    /// <summary>
    /// Seek to the specified time. If the time is out of video range, it calls <see cref="Reset"/>.
    /// </summary>
    /// <param name="timeInSeconds">Target time in seconds.</param>
    internal void Seek(double timeInSeconds)
    {
        Seek(timeInSeconds, true);
    }

    /// <summary>
    /// Seek to the specified time. If the time is out of video range, it calls <see cref="Reset"/>.
    /// </summary>
    /// <param name="timeInSeconds">Target time in seconds.</param>
    /// <param name="pseudoReset">Whether reset decoding state.</param>
    private void Seek(double timeInSeconds, bool pseudoReset)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        // Normal playback seeking behavior
        if (!IsLooped)
        {
            if (timeInSeconds <= 0 || timeInSeconds >= GetDurationInSeconds())
            {
                Reset();

                return;
            }
        }

        LockFrameQueuesUpdate();

        try
        {
            if (pseudoReset)
            {
                ResetBeforeSeek();
            }

            AVSEEK_FLAG seekFlags = 0;

            if (_nextDecodingVideoTime > timeInSeconds)
            {
                seekFlags |= AVSEEK_FLAG.Backward;
            }

            if (VideoStreamIndex >= 0 || AudioStreamIndex >= 0)
            {
                var timestamp = FFmpegHelper.ConvertSecondsToPts(timeInSeconds);
                FFmpegHelper.Verify(ffmpeg.av_seek_frame(FormatContext, -1, timestamp, (int)seekFlags), Dispose);
            }

            if (pseudoReset)
            {
                ResetAfterSeek();
            }
        }
        finally
        {
            UnlockFrameQueueUpdate();
        }
    }

    /// <summary>
    /// Gets the duration of the video file, in seconds.
    /// </summary>
    /// <returns>The duration of the video file, in seconds.</returns>
    internal double GetDurationInSeconds()
    {
        if (_duration == null)
        {
            // Duration info can be retrieved without initialization, so no need to call EnsureInitialized().
            EnsureNotDisposed();

            _duration = FFmpegHelper.GetDurationInSeconds(_formatContext);
        }

        return _duration.Value;
    }

    /// <summary>
    /// Reads video packets and decode them into video frames, until presentation time of the frame decoded is after specified time.
    /// </summary>
    /// <param name="presentationTime">Current playback time, in seconds.</param>
    internal void ReadVideoUntilPlaybackIsAfter(double presentationTime)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        // If we don't have to decode any new frame (just use the current one), skip the later procedures.
        if (_nextDecodingVideoTime > presentationTime)
        {
            return;
        }

        var videoContext = _videoContext;
        var videoStream = _videoStream;
        var framePool = _videoFramePool;
        var frameQueue = _preparedVideoFrames;
        var videoPackets = _videoPackets;

        if (videoContext == null || videoStream == null || framePool == null || frameQueue == null || videoPackets == null)
        {
            return;
        }

        bool r;
        double decodedPresentationTime = 0;

        // The double queue + object pool design is used to solve the I/P/B frame order problem.
        // For details about I/P/B frames, see http://dranger.com/ffmpeg/tutorial05.html.

        do
        {
            // Try to fetch a number of video frames.
            r = TryFetchVideoFrames(_decodingOptions.VideoPacketQueueSizeThreshold);

            if (r)
            {
                // If the queue of decoded frames is not empty, then get its presentation timestamp (PTS) and convert it to seconds.
                var decodedPresentationPts = frameQueue.PeekFirstKey();
                decodedPresentationTime = PtsToSeconds(videoStream, decodedPresentationPts + _videoStartPts);

                // Now get the frame.
                var p = frameQueue.Dequeue();
                var frame = (AVFrame*)p;

                if (decodedPresentationTime < presentationTime)
                {
                    // If the time is before current playback time, release the frame because we don't need it anymore.
                    framePool.Release(p);
                }
                else
                {
                    // Otherwise, set it as the current frame.
                    lock (VideoFrameTransmissionLock)
                    {
                        var origFrame = _currentVideoFrame;

                        _currentVideoFrame = frame;

                        if (origFrame != null)
                        {
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
        }
        while (r && decodedPresentationTime < presentationTime);

        // If we did get a frame to show, then update the time.
        if (r)
        {
            _nextDecodingVideoTime = decodedPresentationTime;
        }

        // If the video stream ends, raise Ended event.
        if (!r && videoPackets.Count == 0)
        {
            if (IsLooped)
            {
                ++_currentLoopNumber;

                // About avcodec_flush_buffers:
                // https://ffmpeg.org/doxygen/3.1/group__lavc__encdec.html

                if (_videoContext != null)
                {
                    ffmpeg.avcodec_flush_buffers(_videoContext.CodecContext);
                }

                if (_audioContext != null)
                {
                    ffmpeg.avcodec_flush_buffers(_audioContext.CodecContext);
                }

                Seek(0, false);
            }
            else
            {
                if (!_isEndedTriggered)
                {
                    // Must use BeginInvoke. See the explanations of Ended.
                    Ended?.BeginInvoke(this, EventArgs.Empty, null, null);
                    // var thread = new Thread(thisObject => {
                    //     var context = (DecodeContext)thisObject;
                    //     context.Ended?.Invoke(context, EventArgs.Empty);
                    // });

                    // thread.Start(this);

                    _isEndedTriggered = true;
                }
            }
        }
    }

    /// <summary>
    /// Reads audio packets, decode them into audio frames and send the data to an <see cref="DynamicSoundEffectInstance"/>, until presentation time of the frame decoded is after specified time.
    /// </summary>
    /// <param name="sound">The <see cref="DynamicSoundEffectInstance"/> to play audio data.</param>
    /// <param name="presentationTime">Current playback time, in seconds.</param>
    internal void ReadAudioUntilPlaybackIsAfter(DynamicSoundEffectInstance sound, double presentationTime)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        var audioContext = _audioContext;
        var audioStream = _audioStream;
        var audioFrame = _audioFrame;
        var extraAudioBufferingTime = (float)_decodingOptions.ExtraAudioBufferingTime / 1000;

        if (audioContext == null || audioStream == null || audioFrame == null)
        {
            return;
        }

        using (var memoryStream = new MemoryStream())
        {
            // We want there is as much data in one audio buffer as possible, so that we don't have to allocate too many audio buffers.
            // dataPushed is used for this task.
            var dataPushed = false;
            var fetchFramesPlease = true;

            {
                var pts = audioFrame->best_effort_timestamp;
                var framePresentationTime = PtsToSeconds(audioStream, pts + _audioStartPts);

                if (framePresentationTime > presentationTime + extraAudioBufferingTime)
                {
                    // If our time has not come, skip the rest of the procedures.
                    fetchFramesPlease = false;
                }
                else
                {
                    // Otherwise prepare to send the first lump of data.
                    if (FFmpegHelper.TransmitAudioFrame(this, audioFrame, out var buffer))
                    {
                        if (buffer != null)
                        {
                            memoryStream.Write(buffer, 0, buffer.Length);

                            dataPushed = true;
                        }
                    }
                }
            }

            if (fetchFramesPlease)
            {
                // Fetch audio frames until its presentation timestamp (PTS) is later than current playback time.
                // Prepare to send all the data received during this procedure.
                while (TryFetchAudioFrame(audioFrame))
                {
                    if (audioFrame->nb_samples == 0)
                    {
                        continue;
                    }

                    var pts = audioFrame->best_effort_timestamp;
                    var framePresentationTime = PtsToSeconds(audioStream, pts + _audioStartPts);

                    if (framePresentationTime > presentationTime + extraAudioBufferingTime)
                    {
                        break;
                    }

                    if (FFmpegHelper.TransmitAudioFrame(this, audioFrame, out var buffer))
                    {
                        if (buffer != null)
                        {
                            memoryStream.Write(buffer, 0, buffer.Length);

                            dataPushed = true;
                        }
                    }
                }
            }

            // If there is data pushed...
            if (dataPushed)
            {
                var audioData = memoryStream.ToArray();

                // ... send the data to OpenAL's audio buffer...
                // TODO: XAudio2 0x88960001?
                sound.SubmitBuffer(audioData);

                // ... and play it.
                if (sound.State == SoundState.Stopped)
                {
                    sound.Play();
                }
            }
        }
    }

    /// <summary>
    /// The lock object used to avoid unwanted frame data overwrites.
    /// </summary>
    internal readonly object VideoFrameTransmissionLock = new object();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void LockFrameQueuesUpdate()
    {
        Monitor.Enter(_frameQueueUpdateLock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnlockFrameQueueUpdate()
    {
        Monitor.Exit(_frameQueueUpdateLock);
    }

    protected override void Dispose(bool disposing)
    {
        // Prevents initializing after disposal
        _isInitialized = true;

        Reset();

        // Video frames are allocated by a frame pool. So we don't need to dispose it here.
        _currentVideoFrame = null;

        _videoFramePool?.Dispose();
        _videoFramePool = null;

        if (_audioFrame != null)
        {
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

        if (_formatContext != null)
        {
            ffmpeg.avformat_free_context(_formatContext);
            _formatContext = null;
        }
    }

    /// <summary>
    /// Tries to get the next video packet.
    /// </summary>
    /// <param name="packet">The packet got, or an empty packet if the operation fails.</param>
    /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
    private bool TryGetNextVideoPacket([MaybeNullWhen(false)] out Packet packet)
    {
        EnsureNotDisposed();

        var queue = _videoPackets;

        Debug.Assert(queue != null, nameof(queue) + " != null");

        if (!TryFillPacketQueue(queue, _decodingOptions.VideoPacketQueueSizeThreshold))
        {
            if (queue.Count == 0)
            {
                packet = default;

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
    private bool TryGetNextAudioPacket([MaybeNullWhen(false)] out Packet packet)
    {
        EnsureNotDisposed();

        var queue = _audioPackets;

        Debug.Assert(queue != null, nameof(queue) + " != null");

        if (!TryFillPacketQueue(queue, _decodingOptions.AudioPacketQueueSizeThreshold))
        {
            if (queue.Count == 0)
            {
                packet = default;

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
    private bool TryFillPacketQueue(PacketQueue queue, int count)
    {
        while (queue.Count < count)
        {
            if (!ReadNextPacket())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Tries to decode and fetch some video frames, and store them in the video frame queue.
    /// </summary>
    /// <param name="count">Expected count of the frame queue, after fetching video frames.</param>
    /// <returns><see langword="true"/> if the video frame queue has at least one frame when the function returns, otherwise <see langword="false"/>.</returns>
    private bool TryFetchVideoFrames(int count)
    {
        EnsureNotDisposed();

        if (_videoContext is null)
        {
            return false;
        }

        var avStream = _videoContext.VideoStream;
        var codecContext = _videoContext.CodecContext;

        var frameQueue = _preparedVideoFrames;
        var framePool = _videoFramePool;

        Debug.Assert(frameQueue != null && framePool != null);

        Packet? packet = null;

        try
        {
            packet = new Packet(_currentLoopNumber);

            while (frameQueue.Count < count)
            {
                var decodingSuccessful = false;

                while (true)
                {
                    packet?.Dispose();

                    if (!TryGetNextVideoPacket(out packet))
                    {
                        break;
                    }

                    if (packet.RawPacket->size == 0)
                    {
                        break;
                    }

                    var videoTime = PtsToSeconds(avStream, packet.GetRobustTimestamp() + _videoStartPts);

                    if (videoTime < 0)
                    {
                        break;
                    }

                    // About avcodec_send_packet and avcodec_receive_frame:
                    // https://ffmpeg.org/doxygen/3.1/group__lavc__encdec.html

                    // First we send a video packet.
                    var error = ffmpeg.avcodec_send_packet(codecContext, packet.RawPacket);

                    if (error != 0)
                    {
                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            // End of video, maybe some other buffered frames.
                            error = ffmpeg.avcodec_send_packet(codecContext, null);

                            if (error != 0)
                            {
                                if (error == ffmpeg.AVERROR_EOF)
                                {
                                    // Go on.
                                }
                                else
                                {
                                    FFmpegHelper.Verify(error, Dispose);
                                }
                            }
                        }
                        else
                        {
                            FFmpegHelper.Verify(error, Dispose);
                        }
                    }

                    var frame = (AVFrame*)framePool.Acquire();

                    // Then we receive the decoded frame.
                    error = ffmpeg.avcodec_receive_frame(codecContext, frame);

                    var bestEffortPts = frame->best_effort_timestamp;

                    if (error == 0)
                    {
                        // If everything goes well, then again, lucky us.
                        packet.Dispose();

                        // Fix duplicate key (why does this happen?)
                        if (frameQueue.ContainsKey(bestEffortPts))
                        {
                            // Drop the old frame and put it back to the pool.
                            frameQueue.TryGetValue(bestEffortPts, out var oldFramePtr);
                            frameQueue.Remove(bestEffortPts);
                            framePool.Release(oldFramePtr);
                        }

                        frameQueue.Enqueue(bestEffortPts, (IntPtr)frame);

                        decodingSuccessful = true;

                        break;
                    }
                    else
                    {
                        // If this packet contains multiple frames, we have to enqueue all those frames.
                        if (error == ffmpeg.EAGAIN)
                        {
                            frameQueue.Enqueue(bestEffortPts, (IntPtr)frame);
                            decodingSuccessful = true;
                        }

                        while (error == ffmpeg.EAGAIN)
                        {
                            frame = (AVFrame*)framePool.Acquire();
                            error = ffmpeg.avcodec_receive_frame(codecContext, frame);
                            decodingSuccessful = true;

                            if (error >= 0)
                            {
                                frameQueue.Enqueue(bestEffortPts, (IntPtr)frame);
                            }
                        }

                        // If an error occurs, release the frame we acquired because its data will not be used by now.
                        if (error < 0)
                        {
                            framePool.Release((IntPtr)frame);
                        }

                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            // Go on.
                        }
                        else if (error == -11)
                        {
                            // Device not ready; don't know why this happens.
                        }
                        else
                        {
                            FFmpegHelper.Verify(error, Dispose);
                        }
                    }
                }

                if (!decodingSuccessful)
                {
                    break;
                }
            }
        }
        finally
        {
            packet?.Dispose();
        }

        return frameQueue.Count > 0;
    }

    /// <summary>
    /// Tries to decode and fetch an audio frame and fill the data in given <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="frame">The frame which stores the data.</param>
    /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
    private bool TryFetchAudioFrame([JetBrains.Annotations.NotNull] AVFrame* frame)
    {
        EnsureNotDisposed();

        if (_audioContext == null)
        {
            return false;
        }

        var avStream = _audioContext.AudioStream;
        var codecContext = _audioContext.CodecContext;

        // If we were decoding audio packets, and the last packets is not fully consumed yet,
        // then try to receive one more frame.
        if (_audioPacketDecodingOngoing)
        {
            var error = ffmpeg.avcodec_receive_frame(codecContext, frame);

            if (error == 0)
            {
                return true;
            }
            else
            {
                _audioPacketDecodingOngoing = false;

                if (error == ffmpeg.AVERROR_EOF)
                {
                    return false;
                }
            }
        }

        Packet? packet = null;

        try
        {
            packet = new Packet(_currentLoopNumber);

            // Other logics are similar to those in TryFetchVideoFrames.

            while (true)
            {
                packet.Dispose();

                if (!TryGetNextAudioPacket(out packet))
                {
                    break;
                }

                if (packet.RawPacket->size == 0)
                {
                    break;
                }

                var audioTime = PtsToSeconds(avStream, packet.GetRobustTimestamp() + _audioStartPts);

                if (audioTime < 0)
                {
                    break;
                }

                var error = ffmpeg.avcodec_send_packet(codecContext, packet.RawPacket);

                if (error != 0)
                {
                    if (error == ffmpeg.AVERROR_EOF)
                    {
                        // End of video, maybe some other buffered frames.
                        error = ffmpeg.avcodec_send_packet(codecContext, null);

                        if (error != 0)
                        {
                            if (error == ffmpeg.AVERROR_EOF)
                            {
                                // Go on.
                            }
                            else if (error == ffmpeg.EAGAIN)
                            {
                                continue;
                            }
                            else
                            {
                                FFmpegHelper.Verify(error, Dispose);
                            }
                        }
                    }
                    else if (error == ffmpeg.EAGAIN)
                    {
                        continue;
                    }
                    else
                    {
                        FFmpegHelper.Verify(error, Dispose);
                    }
                }

                error = ffmpeg.avcodec_receive_frame(codecContext, frame);

                if (error == 0)
                {
                    packet.Dispose();

                    // The packet received is fully consumed.
                    _audioPacketDecodingOngoing = true;

                    return true;
                }
                else if (error == ffmpeg.AVERROR_EOF)
                {
                    break;
                }
                else if (error == ffmpeg.EAGAIN)
                {
                    // Go on.
                }
                else
                {
                    FFmpegHelper.Verify(error, Dispose);
                }
            }
        }
        finally
        {
            packet?.Dispose();
        }

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
    private bool ReadNextPacket()
    {
        var packet = new Packet(_currentLoopNumber);

        // Read the next packet from current format container.
        var error = ffmpeg.av_read_frame(FormatContext, packet.RawPacket);

        if (error < 0)
        {
            if (error == ffmpeg.AVERROR_EOF)
            {
                return false;
            }
            else
            {
                FFmpegHelper.Verify(error, Dispose);
            }
        }

        // Is this an empty packet?
        if (!(packet.RawPacket->data != null && packet.RawPacket->size > 0))
        {
            packet.Dispose();

            return false;
        }

        if (packet.RawPacket->stream_index == VideoStreamIndex)
        {
            // If this packet belongs to the video stream, enqueue it to the video packet queue.
            Debug.Assert(_videoPackets != null);

            _videoPackets.Enqueue(packet);
        }
        else if (packet.RawPacket->stream_index == AudioStreamIndex)
        {
            // If this packet belongs to the audio stream, enqueue it to the audio packet queue.
            Debug.Assert(_audioPackets != null);

            _audioPackets.Enqueue(packet);
        }
        else
        {
            // Otherwise, ignore this packet.
            packet.Dispose();
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            LazyInitialize();
        }
    }

    private void LazyInitialize()
    {
        if (_isInitialized)
        {
            return;
        }

        var formatContext = _formatContext;

        Trace.Assert(formatContext != null, nameof(formatContext) + " != null");

        var decodingOptions = _decodingOptions;

        // 3. If we are opening an ASF container, we need a different stream start time calculation method.
        // About start_time and ASF container format (.asf, .wmv), see:
        // https://www.ffmpeg.org/doxygen/3.0/structAVStream.html#a7c67ae70632c91df8b0f721658ec5377
        // This field is fixed in FFmpeg 4.0.
        var containerName = FFmpegHelper.PtrToStringNullTerminated(formatContext->iformat->name, Encoding.UTF8);
        var isAsfContainer = containerName == "asf";
        var isAsfContainerStartTimeFixed = FFmpegBinariesHelper.IsFFmpegVersion4OrAbove();

        VideoDecodingContext? videoContext = null;
        AudioDecodingContext? audioContext = null;

        AVCodec* videoCodec = null;
        AVCodec* audioCodec = null;

        // 4. Search for video and audio streams.
        _videoStreamIndex = ffmpeg.av_find_best_stream(formatContext, AVMediaType.Video, _userSelectedVideoStreamIndex, -1, &videoCodec, 0);
        if (videoCodec != null && _videoStreamIndex != ffmpeg.AVERROR_STREAM_NOT_FOUND)
        {
            var avStream = formatContext->streams[_videoStreamIndex];

            // 5. If this is a video stream, create a VideoDecodingContext from it...
            videoContext = new VideoDecodingContext(videoCodec, avStream, decodingOptions);

            // ... and record its start time.
            if (isAsfContainerStartTimeFixed)
            {
                if (avStream->start_time != ffmpeg.AV_NOPTS_VALUE)
                {
                    _videoStartPts = avStream->start_time;
                }
                else
                {
                    _videoStartPts = 0;
                }
            }
            else
            {
                if (!isAsfContainer)
                {
                    if (avStream->start_time != ffmpeg.AV_NOPTS_VALUE)
                    {
                        _videoStartPts = avStream->start_time;
                    }
                    else
                    {
                        _videoStartPts = 0;
                    }
                }
                else
                {
                    // I don't know why but this hack works... at least on FFmpeg 3.4.
                    // DTS is not accessible from FFmpeg 4.x? (3.4: cur_dts / 2)
                    if (avStream->start_time != ffmpeg.AV_NOPTS_VALUE)
                    {
                        _videoStartPts = avStream->start_time;
                    }
                    else
                    {
                        _videoStartPts = 0;
                    }
                }
            }
        }
        else
        {
            _videoStreamIndex = -1;
        }

        _audioStreamIndex = ffmpeg.av_find_best_stream(formatContext, AVMediaType.Audio, _userSelectedAudioStreamIndex, -1, &audioCodec, 0);
        if (audioCodec != null && _audioStreamIndex != ffmpeg.AVERROR_STREAM_NOT_FOUND)
        {
            var avStream = formatContext->streams[_audioStreamIndex];

            // 6. If this is an audio stream, create an AudioDecodingContext from it...
            audioContext = new AudioDecodingContext(audioCodec, avStream);

            // ... and record its start time.
            if (!isAsfContainer)
            {
                if (avStream->start_time != ffmpeg.AV_NOPTS_VALUE)
                {
                    _audioStartPts = avStream->start_time;
                }
            }
            else
            {
                _audioStartPts = 0;
            }
        }
        else
        {
            _audioStreamIndex = -1;
        }

        // 7. If we got a video stream...
        if (videoContext != null)
        {
            // ... we should get its codec...
            var codec = ffmpeg.avcodec_find_decoder(videoContext.CodecContext->codec_id);

            if (codec == null)
            {
                throw new FFmpegException("Failed to find video codec.");
            }

            FFmpegHelper.Verify(ffmpeg.avcodec_open2(videoContext.CodecContext, codec, null), Dispose);

            // ... and initialize the helper data structures.
            _videoFramePool = new ObjectPool<IntPtr>(decodingOptions.VideoPacketQueueSizeThreshold, AllocFrame, DeallocFrame);
            _preparedVideoFrames = new SortedList<long, IntPtr>(decodingOptions.VideoPacketQueueSizeThreshold);
            _videoCodecName = FFmpegHelper.PtrToStringNullTerminated(codec->name, Encoding.UTF8);

            IntPtr AllocFrame()
            {
                return (IntPtr)ffmpeg.av_frame_alloc();
            }

            void DeallocFrame(IntPtr p)
            {
                var frame = (AVFrame*)p;

                ffmpeg.av_frame_free(&frame);
            }
        }

        // 8. If we got an audio stream...
        if (audioContext != null)
        {
            // ...we should get its codec...
            var codec = ffmpeg.avcodec_find_decoder(audioContext.CodecContext->codec_id);

            if (codec == null)
            {
                throw new FFmpegException("Failed to find audio codec.");
            }

            FFmpegHelper.Verify(ffmpeg.avcodec_open2(audioContext.CodecContext, codec, null), Dispose);

            // ... and prepare the buffer frame.
            _audioFrame = ffmpeg.av_frame_alloc();
            _audioCodecName = FFmpegHelper.PtrToStringNullTerminated(codec->name, Encoding.UTF8);
        }

        // 9. Finally, set up other objects.
        if (_videoStreamIndex >= 0)
        {
            _videoStream = formatContext->streams[_videoStreamIndex];
        }

        if (_audioStreamIndex >= 0)
        {
            _audioStream = formatContext->streams[_audioStreamIndex];
        }

        if (videoContext != null)
        {
            _videoPackets = new PacketQueue(decodingOptions.VideoPacketQueueCapacity);
        }

        if (audioContext != null)
        {
            _audioPackets = new PacketQueue(decodingOptions.AudioPacketQueueCapacity);
        }

        _videoContext = videoContext;
        _audioContext = audioContext;

        _isInitialized = true;
    }

    private void ResetBeforeSeek()
    {
        var videoPackets = _videoPackets;

        if (videoPackets != null)
        {
            while (videoPackets.Count > 0)
            {
                var packet = videoPackets.Dequeue();

                packet.Dispose();
            }
        }

        var audioPackets = _audioPackets;

        if (audioPackets != null)
        {
            while (audioPackets.Count > 0)
            {
                var packet = audioPackets.Dequeue();

                packet.Dispose();
            }
        }
    }

    private void ResetAfterSeek()
    {
        _videoFramePool?.Reset();
        _preparedVideoFrames?.Clear();

        // Clear the staging data in buffer audio frame.
        // Do NOT free the audio frame here.
        ffmpeg.av_frame_unref(_audioFrame);
        // The point is the same. But since video frames are managed by a frame pool, we just need to set this to null.
        _currentVideoFrame = null;

        _audioPacketDecodingOngoing = false;

        _isEndedTriggered = false;

        _nextDecodingVideoTime = double.MinValue;
    }

    /// <summary>
    /// A helper function to calculate seconds from PTS, considering looping.
    /// </summary>
    /// <param name="stream">The <see cref="AVStream"/> whose time base is going to be the calculation standard.</param>
    /// <param name="pts">Value of the presentation timestamp.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double PtsToSeconds([MaybeNull] AVStream* stream, long pts)
    {
        if (stream == null)
        {
            return 0;
        }

        var rawSeconds = FFmpegHelper.ConvertPtsToSeconds(stream, pts);
        double result;

        if (_currentLoopNumber > 0)
        {
            result = rawSeconds + _currentLoopNumber * GetDurationInSeconds();
        }
        else
        {
            result = rawSeconds;
        }

        return result;
    }

    [MaybeNull]
    private AVFormatContext* _formatContext;

    private PacketQueue? _videoPackets;

    private PacketQueue? _audioPackets;

    private VideoDecodingContext? _videoContext;

    private AudioDecodingContext? _audioContext;

    // Current video frame and buffer audio frame.
    // Note that their usage and life cycle are totally different.
    [MaybeNull]
    private AVFrame* _currentVideoFrame;

    [MaybeNull]
    private AVFrame* _audioFrame;

    [MaybeNull]
    private AVStream* _videoStream;

    [MaybeNull]
    private AVStream* _audioStream;

    private string? _videoCodecName;

    private string? _audioCodecName;

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
    private long _videoStartPts;

    /// <summary>
    /// Start time of the first frame in the audio stream, in audio stream's time base.
    /// </summary>
    private long _audioStartPts;

    /// <summary>
    /// Does the data from the last audio packet still have remains to fill into another audio frame?
    /// </summary>
    private bool _audioPacketDecodingOngoing;

    /// <summary>
    /// The video frame pool.
    /// </summary>
    private ObjectPool<IntPtr>? _videoFramePool;

    /// <summary>
    /// Decoded video frames. For each entry, the key is the presentation timestamp (PTS) and the value is pointer to the decoded frame (<see cref="AVFrame"/>).
    /// </summary>
    /// <remarks>
    /// The frames are managed by the video frame pool. You should not dispose frames retrived from this list.
    /// </remarks>
    private SortedList<long, IntPtr>? _preparedVideoFrames;

    private int _videoStreamIndex = -1;

    private int _audioStreamIndex = -1;

    private int _userSelectedVideoStreamIndex = -1;

    private int _userSelectedAudioStreamIndex = -1;

    private double? _duration;

    private bool _isLooped;

    /// <summary>
    /// Current loop number.
    /// </summary>
    /// <remarks>
    /// For example, <code>0</code> is the first playback (not looped),
    /// <code>1</code> is the first time the video is looped, etc.
    /// This value will be used with packet PTS for playback time expansion,
    /// i.e. expand some PTS in the N-th loop to <code>N * total_pts + pts</code>.
    /// </remarks>
    private int _currentLoopNumber;

    private readonly object _frameQueueUpdateLock = new object();

    private readonly DecodingOptions _decodingOptions;

    private bool _isInitialized;

}
