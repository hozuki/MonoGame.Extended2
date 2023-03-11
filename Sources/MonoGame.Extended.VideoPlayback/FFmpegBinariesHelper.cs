using System;
using System.Diagnostics;
using System.IO;
using FFmpeg.AutoGen;

namespace MonoGame.Extended.VideoPlayback;

// Based on: https://github.com/Ruslan-B/FFmpeg.AutoGen/blob/master/FFmpeg.AutoGen.Example/FFmpegBinariesHelper.cs
/// <summary>
/// Provides service to initialize FFmpeg binaries its static context.
/// </summary>
public static class FFmpegBinariesHelper
{

    /// <summary>
    /// Initialize the FFmpeg binaries, and its static context if the previous step succeeds.
    /// </summary>
    /// <param name="path32">The path to 32-bit FFmpeg binaries.</param>
    /// <param name="path64">The path to 64-bit FFmpeg binaries.</param>
    public static void InitializeFFmpeg(string path32, string path64)
    {
        if (_pathsRegistered)
        {
            return;
        }

        _pathsRegistered = true;

        var loadSuccessful = false;

        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
                var current = Environment.CurrentDirectory;
                var probe = Environment.Is64BitProcess ? path64 : path32;

                while (current != null)
                {
                    var ffmpegDirectory = Path.Combine(current, probe);

                    if (Directory.Exists(ffmpegDirectory))
                    {
                        Debug.WriteLine($"FFmpeg binaries search path is set to: {ffmpegDirectory}");

                        RegisterLibrariesSearchPath(ffmpegDirectory);

                        loadSuccessful = true;

                        break;
                    }

                    current = Directory.GetParent(current)?.FullName;
                }

                break;
        }

        if (loadSuccessful)
        {
            // https://github.com/FFmpeg/FFmpeg/blob/70d25268c21cbee5f08304da95be1f647c630c15/doc/APIchanges#L86
            // https://github.com/leandromoreira/ffmpeg-libav-tutorial/issues/29

            var ver = ffmpeg.av_version_info();
            Debug.WriteLine($"Using FFmpeg version {ver}");
        }
    }

    internal static bool IsFFmpegVersion4OrAbove()
    {
        // libavformat version of FFmpeg 4.0
        const int ffmpegVersion4AvformatVersion = 58;
        var avformatVersion = ffmpeg.avformat_version();

        return avformatVersion >= ffmpegVersion4AvformatVersion;
    }

    private static void RegisterLibrariesSearchPath(string path)
    {
        ffmpeg.RootPath = path;
    }

    private static bool _pathsRegistered;

}
