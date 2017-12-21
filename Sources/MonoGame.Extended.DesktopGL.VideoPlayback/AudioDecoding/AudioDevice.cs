using System;
using System.Linq;
using JetBrains.Annotations;
using OpenAL;

namespace MonoGame.Extended.DesktopGL.VideoPlayback.AudioDecoding {
    /// <inheritdoc />
    /// <summary>
    /// A wrapper class for OpenAL audio devices.
    /// </summary>
    public sealed class AudioDevice : OpenALObject {

        /// <summary>
        /// Creates a new <see cref="AudioDevice"/> instance using given device name.
        /// </summary>
        /// <param name="deviceName">The device name. Use <see langword="null"/> to let OpenAL decide which device to use.</param>
        internal AudioDevice([CanBeNull] string deviceName) {
            DeviceName = deviceName;
            _device = Alc.OpenDevice(deviceName);
        }

        /// <summary>
        /// Creates a new <see cref="AudioDevice"/> instance using auto-detected device name.
        /// </summary>
        internal AudioDevice()
            : this(DetermineDriver()) {
        }

        /// <summary>
        /// The native ID of this <see cref="AudioDevice"/>.
        /// </summary>
        internal IntPtr NativeDevice => _device;

        /// <summary>
        /// The given name of this <see cref="AudioDevice"/>.
        /// </summary>
        internal string DeviceName { get; }

        protected override void Dispose(bool disposing) {
            if (_device == IntPtr.Zero) {
                return;
            }

            Alc.CloseDevice(_device);

            _device = IntPtr.Zero;
        }

        private static string DetermineDriver() {
            var platform = Environment.OSVersion.Platform;
            var availableDriverNames = Drivers.Where(d => d.Platform == platform).Select(d => d.Driver).ToArray();

            if (availableDriverNames.Length == 0) {
                return null;
            }

            foreach (var driverName in availableDriverNames) {
                var device = Alc.OpenDevice(driverName);

                if (device != IntPtr.Zero) {
                    Alc.CloseDevice(device);

                    return driverName;
                }
            }

            return null;
        }

        // https://www.openal.org/platforms/
        private static readonly (PlatformID Platform, string Driver)[] Drivers = {
            (PlatformID.Unix, "native"),
            (PlatformID.Unix, "OSS"),
            (PlatformID.Unix, "ALSA"),
            (PlatformID.MacOSX, "Core Audio"),
            (PlatformID.Win32NT, "DirectSound3D"), // actually WAS API?
            (PlatformID.Win32NT, "DirectSound"),
            (PlatformID.Win32NT, "MMSYSTEM")
        };

        private IntPtr _device;

    }
}
