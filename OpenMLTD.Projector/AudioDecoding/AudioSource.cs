using System;
using JetBrains.Annotations;
using OpenAL;

namespace OpenMLTD.Projector.AudioDecoding {
    /// <inheritdoc />
    /// <summary>
    /// A wrapper class for OpenAL audio sources.
    /// </summary>
    internal sealed class AudioSource : OpenALObject {

        /// <summary>
        /// Creates a new <see cref="AudioSource"/> instance.
        /// </summary>
        /// <param name="context">The <see cref="AudioContext"/> used to create this <see cref="AudioSource"/>.</param>
        internal AudioSource([NotNull] AudioContext context) {
            Context = context;

            Alc.MakeContextCurrent(context.NativeContext);

            var t = GenSourcesBuffer;
            AL.GenSources(t);
            _source = t[0];
        }

        /// <summary>
        /// The <see cref="AudioContext"/> used to create this <see cref="AudioSource"/>.
        /// </summary>
        internal AudioContext Context { get; }

        /// <summary>
        /// Gets or sets whether this <see cref="AudioSource"/> is looped.
        /// </summary>
        internal bool IsLooped {
            get => _isLooped;
            set {
                if (NativeSource == InvalidObjectID) {
                    return;
                }

                EnsureNotDisposed();

                AL.Source(NativeSource, ALSourceb.Looping, value);
                _isLooped = value;
            }
        }

        /// <summary>
        /// Gets or sets the volume of this <see cref="AudioSource"/>. The valid range is 0 to 1.
        /// </summary>
        internal float Volume {
            get => _gain;
            set {
                if (NativeSource == InvalidObjectID) {
                    return;
                }

                EnsureNotDisposed();

                AL.Source(NativeSource, ALSourcef.Gain, value);
                _gain = value < 0 ? 0 : (value > 1 ? 1 : value);
            }
        }

        /// <summary>
        /// Starts playback, not looped.
        /// </summary>
        internal void Play() {
            EnsureNotDisposed();

            IsLooped = false;
            PlayDirect();
        }

        /// <summary>
        /// Starts playback, maintaining looping status.
        /// </summary>
        internal void PlayDirect() {
            EnsureNotDisposed();

            AL.SourcePlay(NativeSource);
        }

        /// <summary>
        /// Starts playback, looped.
        /// </summary>
        internal void PlayLooped() {
            EnsureNotDisposed();

            IsLooped = true;
            PlayDirect();
        }

        /// <summary>
        /// Pauses the playback.
        /// </summary>
        internal void Pause() {
            EnsureNotDisposed();

            AL.SourcePause(NativeSource);
        }

        /// <summary>
        /// Stops the playback.
        /// </summary>
        internal void Stop() {
            EnsureNotDisposed();

            AL.SourceStop(NativeSource);
        }

        /// <summary>
        /// Queues an <see cref="AudioBuffer"/> for streaming.
        /// </summary>
        /// <param name="buffer"></param>
        internal void QueueBuffer([NotNull] AudioBuffer buffer) {
            EnsureNotDisposed();

            AL.SourceQueueBuffer(NativeSource, buffer.NativeBuffer);
        }

        /// <summary>
        /// Unqueues an <see cref="AudioBuffer"/> and returns the its ID.
        /// </summary>
        /// <returns>The unqueued buffer ID. May be <see cref="OpenALObject.InvalidObjectID"/> when there are no queued buffers.</returns>
        internal int UnqueueBuffer() {
            EnsureNotDisposed();

            var bids = new int[1];

            AL.SourceUnqueueBuffers(_source, 1, bids);

            return bids[0];
        }

        /// <summary>
        /// Unqueue all queued <see cref="AudioBuffer"/>s.
        /// </summary>
        internal void UnqueueAllBuffers() {
            var queued = BuffersQueued;

            if (queued == 0) {
                return;
            }

            var bids = new int[queued];

            AL.SourceUnqueueBuffers(NativeSource, queued, bids);
        }

        /// <summary>
        /// Gets the playback state of this <see cref="AudioSource"/>.
        /// </summary>
        internal AudioState State {
            get {
                if (NativeSource == InvalidObjectID) {
                    return AudioState.Unknown;
                }

                var state = AlState;
                switch (state) {
                    case ALSourceState.Initial:
                        return AudioState.Loaded;
                    case ALSourceState.Playing:
                        return AudioState.Playing;
                    case ALSourceState.Paused:
                        return AudioState.Paused;
                    case ALSourceState.Stopped:
                        return AudioState.Stopped;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// The native ID of this <see cref="AudioSource"/>.
        /// </summary>
        internal int NativeSource => _source;

        /// <summary>
        /// Gets the number of <see cref="AudioBuffer"/>s processed (ready to be unqueued).
        /// </summary>
        internal int BuffersProcessed {
            get {
                EnsureNotDisposed();

                AL.GetSource(NativeSource, ALGetSourcei.BuffersProcessed, out var value);
                return value;
            }
        }

        /// <summary>
        /// Gets the number of <see cref="AudioBuffer"/>s queued (to be played).
        /// </summary>
        internal int BuffersQueued {
            get {
                EnsureNotDisposed();

                AL.GetSource(NativeSource, ALGetSourcei.BuffersQueued, out var value);
                return value;
            }
        }

        protected override void Dispose(bool disposing) {
            if (_source == 0) {
                return;
            }

            AL.DeleteSource(_source);
            _source = 0;
        }

        private ALSourceState AlState {
            get {
                if (_source == 0) {
                    throw new InvalidOperationException();
                }

                EnsureNotDisposed();

                var state = AL.GetSourceState(_source);
                return state;
            }
        }

        private static int[] GenSourcesBuffer => _genSourcesBuffer ?? (_genSourcesBuffer = new int[1]);

        [ThreadStatic]
        private static int[] _genSourcesBuffer;

        private int _source;

        private float _gain = 1f;
        private bool _isLooped;

    }
}
