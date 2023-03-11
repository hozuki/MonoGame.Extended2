using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Sdcb.FFmpeg.Raw;

namespace MonoGame.Extended.VideoPlayback.AudioDecoding;

/// <inheritdoc />
/// <summary>
/// Audio decoding context.
/// </summary>
internal sealed unsafe class AudioDecodingContext : DisposableBase
{

    /// <summary>
    /// Creates a new <see cref="AudioDecodingContext"/> instance.
    /// </summary>
    /// <param name="audioCodec">Audio codec.</param>
    /// <param name="audioStream">The audio stream.</param>
    internal AudioDecodingContext([NotNull] AVCodec* audioCodec, [NotNull] AVStream* audioStream)
    {
        // https://riptutorial.com/ffmpeg/example/30961/open-a-codec-context
        var codecContext = ffmpeg.avcodec_alloc_context3(audioCodec);
        Debug.Assert(codecContext != null, "codecContext != null");

        FFmpegHelper.Verify(ffmpeg.avcodec_parameters_to_context(codecContext, audioStream->codecpar));

        _codecContext = codecContext;
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
    internal int Channels
    {
        get
        {
            EnsureNotDisposed();

            return _codecContext->ch_layout.nb_channels;
        }
    }

    /// <summary>
    /// Gets the sample rate of the audio stream, in Hz.
    /// </summary>
    internal int SampleRate
    {
        get
        {
            EnsureNotDisposed();

            return _codecContext->sample_rate;
        }
    }

    /// <summary>
    /// Gets the sample size (in bytes) of the audio stream.
    /// </summary>
    internal int SampleSize
    {
        get
        {
            EnsureNotDisposed();

            switch (_codecContext->sample_fmt)
            {
                case AVSampleFormat.U8:
                case AVSampleFormat.U8p:
                    return 8;
                case AVSampleFormat.S16:
                case AVSampleFormat.S16p:
                    return 16;
                case AVSampleFormat.S32:
                case AVSampleFormat.S32p:
                case AVSampleFormat.Flt:
                case AVSampleFormat.Fltp:
                    return 32;
                case AVSampleFormat.S64:
                case AVSampleFormat.S64p:
                case AVSampleFormat.Dbl:
                case AVSampleFormat.Dblp:
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
    /// <param name="channelCount">Number of channels of the output.</param>
    /// <param name="sampleRate">Sample rate of the output (Hz).</param>
    /// <returns>Cached or created resample context.</returns>
    internal SwrContext* GetSuitableResampleContext(AVSampleFormat sampleFormat, int channelCount, int sampleRate)
    {
        EnsureNotDisposed();

        Trace.Assert(channelCount > 0 && sampleRate > 0);
        Trace.Assert(channelCount == FFmpegHelper.RequiredChannels);

        var resampleContext = _resampleContext;

        if (resampleContext == null || sampleFormat != _lastSampleFormat || channelCount != _lastChannels || sampleRate != _lastSampleRate)
        {
            if (resampleContext != null)
            {
                ffmpeg.swr_close(resampleContext);
                ffmpeg.swr_free(&resampleContext);
                _resampleContext = null;
            }

            var codecContext = CodecContext;

            ref readonly AVChannelLayout srcChannelLayout = ref codecContext->ch_layout;

            ref readonly AVChannelLayout dstChannelLayout = ref SelectChannelLayout(channelCount);

            // There is another function swr_alloc_setopts(), but re-creating doesn't matter,
            // since changing parameters is not frequent.
            resampleContext = ffmpeg.swr_alloc();

            if (resampleContext == null)
            {
                Dispose();

                throw new FFmpegException("Failed to init audio resample context.");
            }

            // Set options.
            {
                fixed (AVChannelLayout* srcLayout = &srcChannelLayout)
                {
                    FFmpegHelper.Verify(ffmpeg.av_opt_set_chlayout(resampleContext, "in_chlayout", srcLayout, 0), Dispose);
                }

                FFmpegHelper.Verify(ffmpeg.av_opt_set_int(resampleContext, "in_sample_rate", codecContext->sample_rate, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_sample_fmt(resampleContext, "in_sample_fmt", codecContext->sample_fmt, 0), Dispose);

                fixed (AVChannelLayout* dstLayout = &dstChannelLayout)
                {
                    FFmpegHelper.Verify(ffmpeg.av_opt_set_chlayout(resampleContext, "out_chlayout", dstLayout, 0), Dispose);
                }

                FFmpegHelper.Verify(ffmpeg.av_opt_set_int(resampleContext, "out_sample_rate", sampleRate, 0), Dispose);
                FFmpegHelper.Verify(ffmpeg.av_opt_set_sample_fmt(resampleContext, "out_sample_fmt", sampleFormat, 0), Dispose);
            }

            // Don't forget to initialize the context. This is the difference between SWR and SWS (sws_getContext).
            FFmpegHelper.Verify(ffmpeg.swr_init(resampleContext));

            _resampleContext = resampleContext;

            _lastSampleFormat = sampleFormat;
            _lastChannels = channelCount;
            _lastSampleRate = sampleRate;
        }

        return _resampleContext;
    }

    protected override void Dispose(bool disposing)
    {
        if (_resampleContext != null)
        {
            ffmpeg.swr_close(_resampleContext);
            _resampleContext = null;
        }

        if (_codecContext != null)
        {
            ffmpeg.avcodec_close(_codecContext);
            var codecContext = _codecContext;
            ffmpeg.avcodec_free_context(&codecContext);
            _codecContext = null;
        }

        // Will be freed by AVFormatContext
        _audioStream = null;
    }

    private static ref readonly AVChannelLayout SelectChannelLayout(int channelCount)
    {
        switch (channelCount)
        {
            case 1:
                return ref ffmpeg.AV_CHANNEL_LAYOUT_MONO;
            case 2:
                return ref ffmpeg.AV_CHANNEL_LAYOUT_STEREO;
            case > 2:
                // There are more kinds of layouts; but we don't actually need them.
                // TODO: Support more channel layouts.
                return ref ffmpeg.AV_CHANNEL_LAYOUT_SURROUND;
            default:
                throw new ArgumentOutOfRangeException(nameof(channelCount), channelCount, "Invalid channel count.");
        }
    }

    private AVCodecContext* _codecContext;
    private AVStream* _audioStream;
    private SwrContext* _resampleContext;

    private AVSampleFormat _lastSampleFormat = AVSampleFormat.None;
    private int _lastChannels = -1;
    private int _lastSampleRate = -1;

}
