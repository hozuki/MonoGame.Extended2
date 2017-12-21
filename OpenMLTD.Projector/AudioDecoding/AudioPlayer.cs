using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenMLTD.Projector.AudioDecoding {
    /// <inheritdoc />
    /// <summary>
    /// An audio player using OpenAL backend.
    /// </summary>
    internal sealed partial class AudioPlayer : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="AudioPlayer"/> instance.
        /// </summary>
        internal AudioPlayer([NotNull] VideoPlayerOptions playerOptions) {
            _playerOptions = playerOptions;

            _audioDevice = new AudioDevice(null);
            _audioContext = new AudioContext(_audioDevice);
            _audioSource = new AudioSource(_audioContext);

            _audioContext.SetAsCurrent();

            _originalVolume = _audioSource.Volume;

            _audioBufferPool = new ObjectPool<AudioBuffer>(playerOptions.MinimumAudioBufferCount, AllocAudioBuffer, DeallocAudioBuffer);
            _audioBufferMap = new Dictionary<int, AudioBuffer>();

            AudioBuffer AllocAudioBuffer() {
                // Using `_audioContext` rather than `audioContext` is because the former captures `this`,
                // so the actual value subscribes to state updates.
                return new AudioBuffer(_audioContext);
            }

            void DeallocAudioBuffer(AudioBuffer buffer) {
                buffer.Dispose();
            }
        }

        /// <summary>
        /// Starts playback.
        /// </summary>
        internal void Play() {
            _audioSource.PlayDirect();
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        internal void Pause() {
            _audioSource.Pause();
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        internal void Stop() {
            _audioSource.Stop();
        }

        /// <summary>
        /// Gets/sets whether the playback is looped.
        /// </summary>
        internal bool IsLooped {
            get => _audioSource.IsLooped;
            set => _audioSource.IsLooped = value;
        }

        /// <summary>
        /// Gets/sets whether the playback is muted.
        /// </summary>
        internal bool IsMuted {
            get => _isMuted;
            set {
                _audioSource.Volume = value ? 0 : _originalVolume;
                _isMuted = value;
            }
        }

        /// <summary>
        /// Gets/sets the volume of audio output. The valid range is 0 to 1.
        /// </summary>
        internal float Volume {
            get => _audioSource.Volume;
            set {
                _audioSource.Volume = value;
                _originalVolume = _audioSource.Volume;
            }
        }

        /// <summary>
        /// Uses a new buffer.
        /// </summary>
        /// <returns>An <see cref="AudioBufferUser"/>. Call <see cref="AudioBufferUser.GetBuffer"/> to retrieve an <see cref="AudioBuffer"/>.</returns>
        internal AudioBufferUser UseNewBuffer() {
            var audioBuffer = _audioBufferPool.Acquire();

            _audioBufferMap[audioBuffer.NativeBuffer] = audioBuffer;

            return new AudioBufferUser(this, audioBuffer);
        }

        /// <summary>
        /// The playback state of internal <see cref="AudioSource"/>.
        /// </summary>
        internal AudioState SourceState => _audioSource.State;

        // Based on: https://developer.tizen.org/dev-guide/2.4/org.tizen.tutorials/html/native/multimedia/openal_tutorial_n.htm
        /// <summary>
        /// Update buffer states and releases buffers if they are drained.
        /// </summary>
        internal void UpdateBuffers() {
            var buffersProcessed = _audioSource.BuffersProcessed;

            while (buffersProcessed > 0) {
                var bufferID = _audioSource.UnqueueBuffer();
                var buffer = _audioBufferMap[bufferID];

                _audioBufferPool.Release(buffer);

                --buffersProcessed;
            }
        }

        /// <summary>
        /// Activates internal <see cref="AudioContext"/>.
        /// </summary>
        internal void ActivateContext() {
            _audioContext.SetAsCurrent();
        }

        /// <summary>
        /// Resets player state for restarting playback.
        /// </summary>
        internal void Reset() {
            _audioSource.Stop();
            _audioSource.UnqueueAllBuffers();
            _audioBufferMap.Clear();
            _audioBufferPool.Reset();
        }

        protected override void Dispose(bool disposing) {
            Reset();

            _audioBufferPool.Dispose();
            _audioSource.Dispose();
            _audioContext.Dispose();
            _audioDevice.Dispose();
        }

        private readonly AudioDevice _audioDevice;
        private readonly AudioContext _audioContext;
        private readonly AudioSource _audioSource;

        private readonly ObjectPool<AudioBuffer> _audioBufferPool;
        private readonly Dictionary<int, AudioBuffer> _audioBufferMap;

        private bool _isMuted;
        private float _originalVolume;

        private readonly VideoPlayerOptions _playerOptions;

    }
}
