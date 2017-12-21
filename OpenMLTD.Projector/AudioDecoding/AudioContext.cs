using System;
using JetBrains.Annotations;
using OpenAL;

namespace OpenMLTD.Projector.AudioDecoding {
    /// <inheritdoc />
    /// <summary>
    /// A wrapper class for OpenAL audio contexts.
    /// </summary>
    internal sealed class AudioContext : OpenALObject {

        /// <summary>
        /// Creates a new <see cref="AudioContext"/> instance.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="attribList"></param>
        internal AudioContext([NotNull] AudioDevice device, [CanBeNull] int[] attribList) {
            _context = Alc.CreateContext(device.NativeDevice, attribList);
            Device = device;
        }

        /// <summary>
        /// Creates a new <see cref="AudioContext"/> instance.
        /// </summary>
        /// <param name="device"></param>
        internal AudioContext([NotNull] AudioDevice device)
            : this(device, null) {
        }

        /// <summary>
        /// The <see cref="AudioDevice"/> used to create this <see cref="AudioContext"/>.
        /// </summary>
        internal AudioDevice Device { get; }

        /// <summary>
        /// Make this <see cref="AudioContext"/> to be the active context.
        /// </summary>
        internal void SetAsCurrent() {
            Alc.MakeContextCurrent(NativeContext);
        }

        /// <summary>
        /// Deactivate any <see cref="AudioContext"/>, if there is any one active.
        /// </summary>
        internal static void Reset() {
            Alc.MakeContextCurrent(IntPtr.Zero);
        }

        /// <summary>
        /// The native ID of this <see cref="AudioContext"/>.
        /// </summary>
        internal IntPtr NativeContext => _context;

        protected override void Dispose(bool disposing) {
            if (_context == IntPtr.Zero) {
                return;
            }

            var currentContext = Alc.GetCurrentContext();
            if (currentContext == _context) {
                Reset();
            }

            Alc.DestroyContext(_context);

            _context = IntPtr.Zero;
        }

        private IntPtr _context;

    }
}
