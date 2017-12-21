using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.DesktopGL.VideoPlayback.AudioDecoding {
    partial class AudioPlayer {

        /// <inheritdoc cref="IDisposable" />
        /// <summary>
        /// A simple <see cref="AudioBuffer"/> acquire manager taking advantage of <see langword="using"/> syntax.
        /// </summary>
        internal struct AudioBufferUser : IDisposable {

            /// <summary>
            /// Creates a new <see cref="AudioBufferUser"/> instance.
            /// </summary>
            /// <param name="player">The <see cref="AudioPlayer"/> which creates this <see cref="AudioBufferUser"/>.</param>
            /// <param name="buffer">The <see cref="AudioBuffer"/> to use.</param>
            internal AudioBufferUser([NotNull] AudioPlayer player, [NotNull] AudioBuffer buffer) {
                _player = player;
                _buffer = buffer;
                _bufferUsed = false;
            }

            /// <summary>
            /// Gets the <see cref="AudioBuffer"/> attached to this user.
            /// </summary>
            /// <returns>Retrieved <see cref="AudioBuffer"/>.</returns>
            internal AudioBuffer GetBuffer() {
                _bufferUsed = true;

                return _buffer;
            }

            public void Dispose() {
                // If the buffer has not been acquired before exiting `using` block, it can be put back to the pool.
                // Otherwise, the buffer has meaningful data and it should be queued.
                if (_bufferUsed) {
                    _player._audioSource.QueueBuffer(_buffer);
                } else {
                    _player._audioBufferPool.Release(_buffer);
                }
            }

            private readonly AudioPlayer _player;
            private readonly AudioBuffer _buffer;

            private bool _bufferUsed;

        }

    }
}
