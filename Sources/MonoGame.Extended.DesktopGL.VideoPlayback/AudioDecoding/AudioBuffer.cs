using JetBrains.Annotations;
using OpenAL;

namespace MonoGame.Extended.DesktopGL.VideoPlayback.AudioDecoding {
    /// <inheritdoc />
    /// <summary>
    /// A wrapper class for native OpenAL audio buffers.
    /// </summary>
    internal sealed class AudioBuffer : OpenALObject {

        /// <summary>
        /// Creates a new <see cref="AudioBuffer"/> instance.
        /// </summary>
        /// <param name="context">The <see cref="AudioContext"/> used to create this <see cref="AudioBuffer"/>.</param>
        internal AudioBuffer([NotNull] AudioContext context) {
            Context = context;

            // Generate one buffer.
            var t = AL.GenBuffers(1);
            _buffer = t[0];
        }

        /// <summary>
        /// The <see cref="AudioContext"/> used to create this <see cref="AudioBuffer"/>.
        /// </summary>
        internal AudioContext Context { get; }

        /// <summary>
        /// Loads 16-bit stereo audio data into this <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="data">The audio data.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        internal void LoadStereo16Bit([NotNull] short[] data, int sampleRate) {
            LoadStereo16Bit(data, data.Length, sampleRate);
        }

        /// <summary>
        /// Loads 16-bit stereo audio data into this <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="data">The audio data.</param>
        /// <param name="count">Number of array items to load.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        internal void LoadStereo16Bit([NotNull] short[] data, int count, int sampleRate) {
            AL.BufferData(_buffer, ALFormat.Stereo16, data, count * sizeof(short), sampleRate);
        }

        /// <summary>
        /// Loads 16-bit stereo audio data into this <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="data">The audio data.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        internal void LoadStereo16Bit([NotNull] byte[] data, int sampleRate) {
            LoadStereo16Bit(data, data.Length, sampleRate);
        }

        /// <summary>
        /// Loads 16-bit stereo audio data into this <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="data">The audio data.</param>
        /// <param name="count">Number of array items to load.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        internal void LoadStereo16Bit([NotNull] byte[] data, int count, int sampleRate) {
            AL.BufferData(_buffer, ALFormat.Stereo16, data, count, sampleRate);
        }

        /// <summary>
        /// The native buffer ID of this <see cref="AudioBuffer"/>.
        /// </summary>
        internal int NativeBuffer => _buffer;

        protected override void Dispose(bool disposing) {
            if (_buffer == 0) {
                return;
            }

            var buffers = new[] { _buffer };
            AL.DeleteBuffers(buffers);
            _buffer = 0;
        }

        private int _buffer;

    }
}
