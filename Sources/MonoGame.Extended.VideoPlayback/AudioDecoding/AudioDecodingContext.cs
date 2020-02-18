using System;
using System.Diagnostics;
using FFmpeg.AutoGen;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback.AudioDecoding {
    /// <inheritdoc />
    /// <summary>
    /// Audio decoding context.
    /// </summary>
    internal sealed unsafe class AudioDecodingContext : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="AudioDecodingContext"/> instance.
        /// </summary>
        /// <param name="audioStream">The audio stream.</param>
        internal AudioDecodingContext([NotNull] AVStream* audioStream) {
            _codecContext = audioStream->codec;
            _audioStream = audioStream;
        }

        /// <summary>
        /// The underlying <see cref="AVCodecContext"/>.
        /// </summary>
        internal AVCodecContext* CodecContext => _codecContext;

        /// <summary>
        /// The underlying <see cref="AVStream"/>.
        /// </summary>
        internal AVStream* AudioStream => _audioStream;

        /// <summary>
        /// Gets the number of channels of the audio stream.
        /// </summary>
        internal int Channels {
            get {
                EnsureNotDisposed();

                return _codecContext->channels;
            }
        }

        /// <summary>
        /// Gets the sample rate of the audio stream, in Hz.
        /// </summary>
        internal int SampleRate {
            get {
                EnsureNotDisposed();

                return _codecContext->sample_rate;
            }
        }

        /// <summary>
        /// Gets the sample size (in bytes) of the audio stream.
        /// </summary>
        internal int SampleSize {
            get {
                EnsureNotDisposed();

                switch (_codecContext->sample_fmt) {
                    case AVSampleFormat.AV_SAMPLE_FMT_U8:
                    case AVSampleFormat.AV_SAMPLE_FMT_U8P:
                        return 8;
                    case AVSampleFormat.AV_SAMPLE_FMT_S16:
                    case AVSampleFormat.AV_SAMPLE_FMT_S16P:
                        return 16;
                    case AVSampleFormat.AV_SAMPLE_FMT_S32:
                    case AVSampleFormat.AV_SAMPLE_FMT_S32P:
                    case AVSampleFormat.AV_SAMPLE_FMT_FLT:
                    case AVSampleFormat.AV_SAMPLE_FMT_FLTP:
                        return 32;
                    case AVSampleFormat.AV_SAMPLE_FMT_S64:
                    case AVSampleFormat.AV_SAMPLE_FMT_S64P:
                    case AVSampleFormat.AV_SAMPLE_FMT_DBL:
                    case AVSampleFormat.AV_SAMPLE_FMT_DBLP:
                        return 64;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns a suitable audio resample context according to sample format, number of channels, and sample rate.
        /// The context returned will be cached until this function is called again with different arguments.
        /// </summary>
        /// <param name="sampleFormat">Sample format of the output.</param>
        /// <param name="channels">Number of channels of the output.</param>
        /// <param name="sampleRate">Sample rate of the output (Hz).</param>
        /// <returns>Cached or created resample context.</returns>
        internal SwrContext* GetSuitableResampleContext(AVSampleFormat sampleFormat, int channels, int sampleRate) {
            EnsureNotDisposed();

            Trace.Assert(channels > 0 && sampleRate > 0);
            Trace.Assert(channels == FFmpegHelper.RequiredChannels);

            var resampleContext = _resampleContext;

            if (resampleContext == null || sampleFormat != _lastSampleFormat || channels != _lastChannels || sampleRate != _lastSampleRate) {
                if (resampleContext != null) {
                    ffmpeg.swr_close(resampleContext);
                    ffmpeg.swr_free(&resampleContext);
                    _resampleContext = null;
                }

                var codecContext = CodecContext;

                var srcChannelLayout = codecContext->channels == ffmpeg.av_get_channel_layout_nb_channels(codecContext->channel_layout) ? codecContext->channel_layout : (ulong)ffmpeg.av_get_default_channel_layout(codecContext->channels);

                int dstChannelLayout;

                if (channels == 1) {
                    dstChannelLayout = ffmpeg.AV_CH_LAYOUT_MONO;
                } else if (channels == 2) {
                    dstChannelLayout = ffmpeg.AV_CH_LAYOUT_STEREO;
                } else {
                    // There are more kinds of layouts; but we don't actually need them.
                    dstChannelLayout = ffmpeg.AV_CH_LAYOUT_SURROUND;
                }

                // There is another function swr_alloc_setopts(), but re-creating doesn't matter,
                // since changing parameters is not frequent.
                resampleContext = ffmpeg.swr_alloc();

                if (resampleContext == null) {
                    Dispose();

                    throw new FFmpegException("Failed to init audio resample context.");
                }

                // Set options.
                FFmpegHelper.Verify(ffmpeg.av_opt_set_int(resampleContext, "in_channel_layout", (int)srcChannelLayout, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_int(resampleContext, "in_sample_rate", codecContext->sample_rate, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_sample_fmt(resampleContext, "in_sample_fmt", codecContext->sample_fmt, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_int(resampleContext, "out_channel_layout", dstChannelLayout, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_int(resampleContext, "out_sample_rate", sampleRate, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_sample_fmt(resampleContext, "out_sample_fmt", sampleFormat, 0), Dispose);

                // Don't forget to initialize the context. This is the difference between SWR and SWS (sws_getContext).
                FFmpegHelper.Verify(ffmpeg.swr_init(resampleContext));

                _resampleContext = resampleContext;

                _lastSampleFormat = sampleFormat;
                _lastChannels = channels;
                _lastSampleRate = sampleRate;
            }

            return _resampleContext;
        }

        protected override void Dispose(bool disposing) {
            if (_resampleContext != null) {
                ffmpeg.swr_close(_resampleContext);
                _resampleContext = null;
            }

            if (_codecContext != null) {
                ffmpeg.avcodec_close(_codecContext);
                _codecContext = null;
            }

            // Will be freed by AVFormatContext
            _audioStream = null;
        }

        private AVCodecContext* _codecContext;
        private AVStream* _audioStream;
        private SwrContext* _resampleContext;

        private AVSampleFormat _lastSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_NONE;
        private int _lastChannels = -1;
        private int _lastSampleRate = -1;

    }
}
