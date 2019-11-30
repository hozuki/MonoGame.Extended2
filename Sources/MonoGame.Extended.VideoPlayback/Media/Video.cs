using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.VideoPlayback;

// ReSharper disable once CheckNamespace
namespace MonoGame.Extended.Framework.Media {
    /// <inheritdoc />
    /// <summary>
    /// Provides access to video file information.
    /// </summary>
    public sealed class Video : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="Video"/> instance.
        /// </summary>
        /// <param name="url">Video source URL. May be a file system path.</param>
        /// <param name="decodingOptions">Decoding options.</param>
        internal Video([NotNull] string url, [NotNull] DecodingOptions decodingOptions) {
            _decodeContext = new DecodeContext(url, decodingOptions);
            _decodingOptions = decodingOptions;

            _decodeContext.Ended += decodeContext_Ended;

            Duration = TimeSpan.FromSeconds(_decodeContext.GetDurationInSeconds());
        }

        /// <summary>
        /// Raises when the playback of this video is ended (normally or exceptionally).
        /// </summary>
        internal event EventHandler<EventArgs> Ended;

        /// <summary>
        /// Duration of this video.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Number of frames per second (FPS) of this video.
        /// </summary>
        public float FramesPerSecond {
            get {
                EnsureNotDisposed();

                return _decodeContext?.VideoContext?.GetFramesPerSecond() ?? 0;
            }
        }

        /// <summary>
        /// Height of this video, in pixels.
        /// </summary>
        public int Height {
            get {
                EnsureNotDisposed();

                return _decodeContext?.VideoContext?.GetHeight() ?? 0;
            }
        }

        /// <summary>
        /// Video soundtrack type.
        /// </summary>
        /// <remarks>In this implementation, the value is always <see cref="Microsoft.Xna.Framework.Media.VideoSoundtrackType.Music"/>.</remarks>
        public VideoSoundtrackType VideoSoundtrackType {
            get {
                EnsureNotDisposed();

                return VideoSoundtrackType.Music;
            }
        }

        /// <summary>
        /// Width of this video, in pixels.
        /// </summary>
        public int Width {
            get {
                EnsureNotDisposed();

                return _decodeContext?.VideoContext?.GetWidth() ?? 0;
            }
        }

        /// <summary>
        /// (Non-standard extension) The decoding options used to create this video.
        /// </summary>
        [NotNull]
        public DecodingOptions DecodingOptions => _decodingOptions;

        /// <summary>
        /// (Non-standard extension) Gets the number of video streams in the media file.
        /// </summary>
        /// <returns>Number of video streams.</returns>
        public int GetVideoStreamCount() {
            return _decodeContext?.GetVideoStreamCount() ?? 0;
        }

        /// <summary>
        /// (Non-standard extension) Gets the number of audio streams in the media file.
        /// </summary>
        /// <returns>Number of audio streams.</returns>
        public int GetAudioStreamCount() {
            return _decodeContext?.GetAudioStreamCount() ?? 0;
        }

        /// <summary>
        /// (Non-standard extension) Selects a video stream by index.
        /// This method is only valid before initialization.
        /// </summary>
        /// <param name="streamIndex">The index of video streams.</param>
        public void SelectVideoStream(int streamIndex) {
            _decodeContext?.SelectVideoStream(streamIndex);
        }

        /// <summary>
        /// (Non-standard extension) Selects an audio stream by index.
        /// This method is only valid before initialization.
        /// </summary>
        /// <param name="streamIndex">The index of video streams.</param>
        public void SelectAudioStream(int streamIndex) {
            _decodeContext?.SelectAudioStream(streamIndex);
        }

        /// <summary>
        /// (Non-standard extension) The index of the video stream user selected.
        /// May be <code>-1</code> for auto selection.
        /// </summary>
        public int UserSelectedVideoStreamIndex {
            [DebuggerStepThrough]
            get => _decodeContext?.UserSelectedVideoStreamIndex ?? -1;
        }

        /// <summary>
        /// (Non-standard extension) The index of the audio stream user selected.
        /// May be <code>-1</code> for auto selection.
        /// </summary>
        public int UserSelectedAudioStreamIndex {
            [DebuggerStepThrough]
            get => _decodeContext?.UserSelectedAudioStreamIndex ?? -1;
        }

        /// <summary>
        /// The decode context of this video.
        /// </summary>
        [CanBeNull]
        internal DecodeContext DecodeContext => _decodeContext;

        /// <summary>
        /// Current <see cref="VideoPlayer"/> which is using this <see cref="Video"/>.
        /// </summary>
        [CanBeNull]
        internal VideoPlayer CurrentVideoPlayer {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        /// <summary>
        /// Forces initializing the <see cref="T:DecodeContext"/>.
        /// </summary>
        internal void InitializeDecodeContext() {
            _decodeContext?.Initialize();
        }

        /// <summary>
        /// Retrieves latest decoded video frame in internal <see cref="MonoGame.Extended.VideoPlayback.DecodeContext"/> and transmits its data to a <see cref="Texture2D"/>.
        /// Returns whether the operation is successful.
        /// </summary>
        /// <param name="textureBuffer">The <see cref="Texture2D"/> serving as the data buffer.</param>
        /// <returns><see langword="true"/> if the operation is successful, otherwise <see langword="false"/>.</returns>
        /// <remarks>If the operation fails, you should not use the data inside that <see cref="Texture2D"/>.</remarks>
        internal bool RetrieveCurrentVideoFrame([NotNull] Texture2D textureBuffer) {
            var width = Width;
            var height = Height;

            // If the frame data buffer doesn't exist, create one.
            // We assume the texture's surface format is RGB0 (SurfaceFormat.Color), so here we use a uint array whose size is width*height.
            if (_frameDataBuffer == null) {
                _frameDataBuffer = new uint[width * height];
            }

            bool r;

            // The decoding thread also tries to update the frame content.
            // So here we need to add a lock to prevent strange things from happening.
            if (_decodeContext != null) {
                lock (_decodeContext.VideoFrameTransmissionLock) {
                    r = FFmpegHelper.TransmitVideoFrame(_decodeContext, textureBuffer, _frameDataBuffer);
                }
            } else {
                r = false;
            }

            return r;
        }

        protected override void Dispose(bool disposing) {
            if (_decodeContext != null) {
                _decodeContext.Ended -= decodeContext_Ended;
                _decodeContext.Dispose();
            }

            _decodeContext = null;
        }

        private void decodeContext_Ended(object sender, EventArgs e) {
            Ended?.Invoke(this, EventArgs.Empty);
        }

        [CanBeNull]
        private uint[] _frameDataBuffer;

        [CanBeNull]
        private DecodeContext _decodeContext;

        [NotNull]
        private readonly DecodingOptions _decodingOptions;

    }
}
