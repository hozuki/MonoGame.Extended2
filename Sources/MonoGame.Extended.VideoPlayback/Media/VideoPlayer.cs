using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.VideoPlayback;

// ReSharper disable once CheckNamespace
namespace MonoGame.Extended.Framework.Media {
    /// <inheritdoc />
    /// <summary>
    /// Provides video playing service.
    /// </summary>
    public sealed partial class VideoPlayer : DisposableBase {

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="T:MonoGame.Extended.Framework.Media.VideoPlayer" /> instance.
        /// </summary>
        [Obsolete(@"This constructor requires doing reflection hack on private properties of Game class. The value may subject to changes. Avoid using it if possible.")]
        public VideoPlayer()
            : this(GameHelper.GetCurrentGame().GraphicsDevice) {
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="T:MonoGame.Extended.Framework.Media.VideoPlayer" /> instance using specified <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> and default player options..
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        public VideoPlayer([NotNull] GraphicsDevice graphicsDevice)
            : this(graphicsDevice, VideoPlayerOptions.Default) {
        }

        /// <summary>
        /// Creates a new <see cref="VideoPlayer"/> instance using specified <see cref="GraphicsDevice"/> and <see cref="VideoPlayerOptions"/>.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="playerOptions">Player options.</param>
        public VideoPlayer([NotNull] GraphicsDevice graphicsDevice, [NotNull] VideoPlayerOptions playerOptions) {
            _stopwatch = new Stopwatch();
            _playerOptions = playerOptions;

            _graphicsDevice = graphicsDevice;
            _soundEffectInstance = new DynamicSoundEffectInstance(FFmpegHelper.RequiredSampleRate, FFmpegHelper.RequiredChannelsXna);
        }

        // TODO: Implement real looping.
        /// <summary>
        /// Sets whether video playbacks are looped.
        /// </summary>
        /// <remarks>
        /// This implementation provides simulated looping.
        /// Note that <see cref="DynamicSoundEffectInstance"/> supports looping naturally.
        /// Looping via carefully managing packet buffers ("real looping") may be implemented in the future.
        /// </remarks>
        public bool IsLooped {
            get {
                EnsureNotDisposed();

                return _isLooped;
            }
            set {
                EnsureNotDisposed();

                var decodeContext = Video?.DecodeContext;

                if (decodeContext != null) {
                    decodeContext.IsLooped = value;
                }

                _isLooped = value;
            }
        }

        /// <summary>
        /// Gets/sets whether video playbacks are muted.
        /// </summary>
        public bool IsMuted {
            get {
                EnsureNotDisposed();

                return _isMuted;
            }
            set {
                EnsureNotDisposed();

                _isMuted = value;

                using (var seAccess = AccessSoundEffect()) {
                    var se = seAccess.SoundEffect;

                    if (se != null) {
                        if (value) {
                            se.Volume = 0;
                        } else {
                            se.Volume = _originalVolume;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets current playback position.
        /// Note: the setter is a non-standard extension.
        /// </summary>
        public TimeSpan PlayPosition {
            get {
                EnsureNotDisposed();

                return GetPlayPosition(true);
            }
            set {
                EnsureNotDisposed();

                var video = Video;

                if (video == null) {
                    return;
                }

                if (TimeSpan.Zero >= value || value >= video.Duration) {
                    Stop();
                } else {
                    _stopwatch.Restart();
                    _soughtTime = value;
                    video.DecodeContext?.Seek(value.TotalSeconds);
                }
            }
        }

        /// <summary>
        /// Gets the playback state.
        /// </summary>
        public MediaState State {
            get {
                EnsureNotDisposed();

                var state = _state;

                switch (state) {
                    case MediaState.Stopped:
                    case MediaState.Paused:
                        return state;
                    case MediaState.Playing:
                        var video = Video;

                        if (video == null) {
                            return MediaState.Stopped;
                        }

                        var playingTime = PlayPosition;

                        if (playingTime >= video.Duration && !IsLooped) {
                            _stopwatch.Stop();
                            _state = MediaState.Stopped;

                            return MediaState.Stopped;
                        } else {
                            return MediaState.Playing;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            private set => _state = value;
        }

        /// <summary>
        /// Gets the playing video, if there is any.
        /// </summary>
        public Video Video {
            // No disposing protection
            get => _video;
            private set {
                ref var oldVideo = ref _video;

                if (oldVideo == value) {
                    return;
                }

                if (oldVideo != null) {
                    oldVideo.CurrentVideoPlayer = null;
                }

                if (value != null) {
                    if (value.CurrentVideoPlayer != null) {
                        throw new InvalidOperationException(nameof(Video) + " can only be used by one " + nameof(VideoPlayer) + " at a time.");
                    }

                    value.CurrentVideoPlayer = this;
                }

                oldVideo = value;
            }
        }

        /// <summary>
        /// Gets/sets playback volume.
        /// </summary>
        public float Volume {
            get {
                EnsureNotDisposed();

                using (var seAccess = AccessSoundEffect()) {
                    return seAccess.SoundEffect?.Volume ?? 0;
                }
            }
            set {
                EnsureNotDisposed();

                value = MathHelper.Clamp(value, 0, 1);
                _originalVolume = value;

                if (!IsMuted) {
                    using (var seAccess = AccessSoundEffect()) {
                        var se = seAccess.SoundEffect;

                        if (se != null) {
                            se.Volume = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Texture2D"/> containing data of current video frame.
        /// Do NOT dispose the obtained texture.
        /// </summary>
        /// <returns>Current video frame represented by a <see cref="Texture2D"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no video is loaded.</exception>
        [NotNull]
        public Texture2D GetTexture() {
            EnsureNotDisposed();

            var video = Video;

            if (video == null) {
                throw new InvalidOperationException("A video should be loaded to get texture.");
            }

            var texture = _textureBuffer;

            Debug.Assert(texture != null, nameof(texture) + " != null");

            var r = GetTexture(texture);

            if (!r) {
                Debug.WriteLine("Failed to get texture from video. Returning last texture.");
            }

            return texture;
        }

        /// <summary>
        /// Pauses the playback.
        /// </summary>
        public void Pause() {
            EnsureNotDisposed();

            if (Video == null) {
                return;
            }

            var state = State;

            if (state == MediaState.Playing) {
                State = MediaState.Paused;

                _stopwatch.Stop();

                using (var seAccess = AccessSoundEffect()) {
                    seAccess.SoundEffect?.Pause();
                }
            }
        }

        /// <summary>
        /// Plays a <see cref="Framework.Media.Video"/> from the beginning.
        /// </summary>
        /// <param name="video">The video to play.</param>
        public void Play([CanBeNull] Video video) {
            EnsureNotDisposed();

            LoadVideo(video);

            ResetAndPlayVideoFromStart(video);
        }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        public void Resume() {
            EnsureNotDisposed();

            if (Video == null) {
                return;
            }

            var state = State;

            if (state == MediaState.Paused) {
                State = MediaState.Playing;

                _stopwatch.Start();

                using (var seAccess = AccessSoundEffect()) {
                    seAccess.SoundEffect?.Play();
                }
            }
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void Stop() {
            // Set the state first because the decoding thread will detect this value.
            State = MediaState.Stopped;

            using (var seAccess = AccessSoundEffect()) {
                seAccess.SoundEffect?.Stop();
            }
            
            _decodingThread?.Terminate();
            _decodingThread = null;

            _stopwatch.Reset();

            _soughtTime = TimeSpan.Zero;

            Video = null;
        }

        /// <summary>
        /// (Non-standard extension) Equivalent for <see cref="Stop"/> then <see cref="Play"/> the same video file.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no video is loaded.</exception>
        public void Replay() {
            EnsureNotDisposed();

            var video = Video;

            if (video == null) {
                throw new InvalidOperationException("Cannot replay video when no video is loaded.");
            }

            video.InitializeDecodeContext();

            ResetAndPlayVideoFromStart(video);
        }

        /// <summary>
        /// (Non-standard extension) Gets or sets the subtitle renderer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if setting subtitle renderer when playback is not stopped.</exception>
        [CanBeNull]
        public ISubtitleRenderer SubtitleRenderer {
            [DebuggerStepThrough]
            get => _subtitleRenderer;
            set {
                if (State != MediaState.Stopped) {
                    throw new InvalidOperationException("Cannot set subtitle renderer when playback is not stopped.");
                }

                _subtitleRenderer = value;
            }
        }

        /// <summary>
        /// (Non-standard extension) Required texture format (<see cref="SurfaceFormat.Color"/>).
        /// </summary>
        /// <seealso cref="FFmpegHelper.RequiredPixelFormat"/>
        public const SurfaceFormat RequiredSurfaceFormat = SurfaceFormat.Color;

        /// <summary>
        /// Creates a new <see cref="DynamicSoundEffectInstanceAccess"/> on <see langword="this"/>.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DynamicSoundEffectInstanceAccess AccessSoundEffect() {
            return new DynamicSoundEffectInstanceAccess(this);
        }

        protected override void Dispose(bool disposing) {
            Stop();

            // Cannot use DynamicSoundEffectInstanceAccess.
            lock (_soundEffectInstanceLock) {
                ref var se = ref _soundEffectInstance;

                se?.Dispose();
                se = null;
            }

            _textureBuffer?.Dispose();
            _textureBuffer = null;
        }

        /// <summary>
        /// Loads a video.
        /// </summary>
        /// <param name="video">The video to load.</param>
        private void LoadVideo([CanBeNull] Video video) {
            var originalVideo = Video;

            if (originalVideo != null) {
                Stop();
            }

            _textureBuffer?.Dispose();
            _textureBuffer = null;

            Video = video;

            if (video != null) {
                video.InitializeDecodeContext();

                Debug.Assert(video.DecodeContext != null);

                video.DecodeContext.IsLooped = IsLooped;

                _textureBuffer = new RenderTarget2D(_graphicsDevice, video.Width, video.Height, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }
        }

        /// <summary>
        /// Retrieves the data of current video frame and writes it into specified <see cref="Texture2D"/> buffer.
        /// The <see cref="Texture2D"/> must use surface format <see cref="SurfaceFormat.Color"/>.
        /// This method avoids recreating textures every time to achieve a better performance.
        /// </summary>
        /// <param name="texture">The texture that receives the data of current video frame.</param>
        /// <returns><see langword="true"/> if the operation succeeds, otherwise <see langword="false"/>.</returns>
        private bool GetTexture([NotNull] RenderTarget2D texture) {
            EnsureNotDisposed();

            if (_decodingThread == null) {
                return false;
            }

            if (_decodingThread.ExceptionalExit.HasValue && _decodingThread.ExceptionalExit.Value) {
                throw new FFmpegException("Decoding thread exited unexpectedly.");
            }

            var video = Video;

            if (video == null) {
                return false;
            }

            var thread = _decodingThread.SystemThread;

            if (thread.IsAlive) {
                var gotTexture = video.RetrieveCurrentVideoFrame(texture);

                if (gotTexture) {
                    var now = GetPlayPosition(false);
                    var subtitleRenderer = SubtitleRenderer;

                    if (subtitleRenderer != null) {
                        var targets = _graphicsDevice.GetRenderTargets();

                        _graphicsDevice.SetRenderTarget(_textureBuffer);

                        if (subtitleRenderer.Enabled) {
                            subtitleRenderer.Render(now, texture);
                        }

                        _graphicsDevice.SetRenderTargets(targets);
                    }
                }

                return gotTexture;
            } else {
                return false;
            }
        }

        private TimeSpan GetPlayPosition(bool testAndSetState) {
            var video = Video;

            if (video == null) {
                return TimeSpan.Zero;
            }

            var elapsed = _stopwatch.Elapsed + _soughtTime;

            if (IsLooped) {
                return elapsed;
            }

            if (elapsed >= video.Duration) {
                if (testAndSetState) {
                    State = MediaState.Stopped;
                }

                return video.Duration;
            } else {
                return elapsed;
            }
        }

        private void ResetAndPlayVideoFromStart([CanBeNull] Video video) {
            ref var decodingThread = ref _decodingThread;

            decodingThread?.Terminate();

            _soughtTime = TimeSpan.Zero;

            if (video == null) {
                return;
            }

            var subtitleRenderer = SubtitleRenderer;

            if (subtitleRenderer != null) {
                subtitleRenderer.Dimensions = new Point(video.Width, video.Height);
            }

            // Reset states so that this method also fits restarting playback.
            video.DecodeContext?.Reset();

            // Since we have to update _soundEffectInstance here, we cannot use the lock wrapper (DynamicSoundEffectInstanceAccess).
            lock (_soundEffectInstanceLock) {
                ref var se = ref _soundEffectInstance;

                Debug.Assert(se != null, nameof(se) + " != null");

                se.Stop();
                se.Dispose();
                se = new DynamicSoundEffectInstance(FFmpegHelper.RequiredSampleRate, FFmpegHelper.RequiredChannelsXna);

                decodingThread = new DecodingThread(this, _playerOptions);

                State = MediaState.Playing;

                var stopwatch = _stopwatch;

                stopwatch.Stop();
                stopwatch.Reset();
                stopwatch.Start();

                decodingThread.Start();
                se.Play();
            }
        }

        private readonly GraphicsDevice _graphicsDevice;

        [CanBeNull]
        private Video _video;

        private MediaState _state = MediaState.Stopped;

        [NotNull]
        private readonly Stopwatch _stopwatch;

        [CanBeNull]
        private DecodingThread _decodingThread;

        private bool _isLooped;
        private bool _isMuted;
        private float _originalVolume = 1f;

        private TimeSpan _soughtTime = TimeSpan.Zero;

        [CanBeNull]
        private RenderTarget2D _textureBuffer;

        [CanBeNull]
        private DynamicSoundEffectInstance _soundEffectInstance;

        [NotNull]
        private readonly object _soundEffectInstanceLock = new object();

        [NotNull]
        private readonly VideoPlayerOptions _playerOptions;

        [CanBeNull]
        private ISubtitleRenderer _subtitleRenderer;

    }
}
