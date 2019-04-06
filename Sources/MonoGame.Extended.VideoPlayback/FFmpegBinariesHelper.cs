using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback {
    // Based on: https://github.com/Ruslan-B/FFmpeg.AutoGen/blob/master/FFmpeg.AutoGen.Example/FFmpegBinariesHelper.cs
    /// <summary>
    /// Provides service to initialize FFmpeg binaries its static context.
    /// </summary>
    public static class FFmpegBinariesHelper {

        /// <summary>
        /// Initialize the FFmpeg binaries, and its static context if the previous step succeeds.
        /// </summary>
        /// <param name="path32">The path to 32-bit FFmpeg binaries.</param>
        /// <param name="path64">The path to 64-bit FFmpeg binaries.</param>
        public static void InitializeFFmpeg([NotNull] string path32, [NotNull] string path64) {
            if (_pathsRegistered) {
                return;
            }

            _pathsRegistered = true;

            var loadSuccessful = false;

            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    var current = Environment.CurrentDirectory;
                    var probe = Environment.Is64BitProcess ? path64 : path32;

                    while (current != null) {
                        var ffmpegDirectory = Path.Combine(current, probe);

                        if (Directory.Exists(ffmpegDirectory)) {
                            Debug.Print("FFmpeg binaries search path is set to: {0}", ffmpegDirectory);

                            RegisterLibrariesSearchPath(ffmpegDirectory);

                            loadSuccessful = true;

                            break;
                        }

                        current = Directory.GetParent(current)?.FullName;
                    }

                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    var libraryPath = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);

                    RegisterLibrariesSearchPath(libraryPath);

                    loadSuccessful = true;
                    break;
            }

            if (loadSuccessful) {
                ffmpeg.av_register_all();

                var ver = ffmpeg.av_version_info();
                Debug.Print("Using FFmpeg version {0}", ver);
            }
        }

        private static void RegisterLibrariesSearchPath(string path) {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    SetDllDirectory(path);
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    var currentValue = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
                    if (!string.IsNullOrWhiteSpace(currentValue) && !currentValue.Contains(path)) {
                        var newValue = currentValue + Path.PathSeparator + path;
                        Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, newValue);
                    }

                    break;
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        // ReSharper disable once InconsistentNaming
        private const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

        private static bool _pathsRegistered;

    }
}
