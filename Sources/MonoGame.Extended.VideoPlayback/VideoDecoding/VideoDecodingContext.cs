using System.Diagnostics;
using FFmpeg.AutoGen;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback.VideoDecoding {
    /// <inheritdoc />
    /// <summary>
    /// Video decoding context.
    /// </summary>
    internal sealed unsafe class VideoDecodingContext : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="VideoDecodingContext"/> instance.
        /// </summary>
        /// <param name="videoStream">The video stream.</param>
        /// <param name="formatContext">Format context.</param>
        /// <param name="videoCodec">Video codec.</param>
        /// <param name="decodingOptions">Decoding options.</param>
        internal VideoDecodingContext([NotNull] AVFormatContext* formatContext, AVCodec* videoCodec, [NotNull] AVStream* videoStream, [NotNull] DecodingOptions decodingOptions) {
            // https://riptutorial.com/ffmpeg/example/30961/open-a-codec-context
            var codecContext = ffmpeg.avcodec_alloc_context3(videoCodec);
            Debug.Assert(codecContext != null, "codecContext != null");

            FFmpegHelper.Verify(ffmpeg.avcodec_parameters_to_context(codecContext, videoStream->codecpar));

            _codecContext = codecContext;
            _videoStream = videoStream;
            _decodingOptions = decodingOptions;
        }

        /// <summary>
        /// The underlying <see cref="AVCodecContext"/>.
        /// </summary>
        internal AVCodecContext* CodecContext {
            get {
                EnsureNotDisposed();

                return _codecContext;
            }
        }

        /// <summary>
        /// The underlying <see cref="AVStream"/>.
        /// </summary>
        internal AVStream* VideoStream {
            get {
                EnsureNotDisposed();

                return _videoStream;
            }
        }

        /// <summary>
        /// Returns a suitable audio rescale context according to output width and height.
        /// The context returned will be cached until this function is called again with different arguments.
        /// </summary>
        /// <param name="width">Output width, in pixels.</param>
        /// <param name="height">Output height, in pixels.</param>
        /// <returns>Cached or created rescale context.</returns>
        [NotNull]
        internal SwsContext* GetSuitableScaleContext(int width, int height) {
            EnsureNotDisposed();

            Trace.Assert(width > 0 && height > 0);

            var scaleContext = _scaleContext;

            if (scaleContext == null || width != _lastScaledWidth || height != _lastScaledHeight) {
                if (scaleContext != null) {
                    ffmpeg.sws_freeContext(scaleContext);
                    _scaleContext = null;
                }

                var codec = CodecContext;
                const AVPixelFormat destPixelFormat = FFmpegHelper.RequiredPixelFormat;
                var frameScaling = (int)_decodingOptions.FrameScalingMethod;

                // Unlike SWR context, SWS context can be allocated and options set in one function.
                scaleContext = ffmpeg.sws_getContext(codec->width, codec->height, codec->pix_fmt, width, height, destPixelFormat, frameScaling, null, null, null);

                if (scaleContext == null) {
                    Dispose();

                    throw new FFmpegException("Failed to get video frame conversion context.");
                }

                _lastScaledWidth = width;
                _lastScaledHeight = height;

                _scaleContext = scaleContext;
            }

            Trace.Assert(_scaleContext != null, nameof(_scaleContext) + " != null");

            return _scaleContext;
        }

        /// <summary>
        /// Gets the number of average frames per second (FPS) of the video stream.
        /// </summary>
        /// <returns>Average FPS of the video stream.</returns>
        internal float GetFramesPerSecond() {
            EnsureNotDisposed();

            if (_videoStream == null) {
                return 0;
            } else {
                var frameRate = _videoStream->avg_frame_rate;

                return (float)frameRate.num / frameRate.den;
            }
        }

        /// <summary>
        /// Gets the width of the video stream, in pixels.
        /// </summary>
        /// <returns>The width of the video stream.</returns>
        internal int GetWidth() {
            EnsureNotDisposed();

            return _codecContext == null ? 0 : _codecContext->width;
        }

        /// <summary>
        /// Gets the height of the video stream, in pixels.
        /// </summary>
        /// <returns>The height of the video stream.</returns>
        internal int GetHeight() {
            EnsureNotDisposed();

            return _codecContext == null ? 0 : _codecContext->height;
        }

        protected override void Dispose(bool disposing) {
            if (_codecContext != null) {
                ffmpeg.avcodec_close(_codecContext);
                var codecContext = _codecContext;
                ffmpeg.avcodec_free_context(&codecContext);
                _codecContext = null;
            }

            if (_scaleContext != null) {
                ffmpeg.sws_freeContext(_scaleContext);
                _scaleContext = null;
            }

            // Will be freed by AVFormatContext
            _videoStream = null;
        }

        [CanBeNull]
        private AVCodecContext* _codecContext;

        [CanBeNull]
        private AVStream* _videoStream;

        private int _lastScaledWidth = -1;
        private int _lastScaledHeight = -1;

        [CanBeNull]
        private SwsContext* _scaleContext;

        [NotNull]
        private readonly DecodingOptions _decodingOptions;

    }
}
