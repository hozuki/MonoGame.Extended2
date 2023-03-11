using System.Diagnostics;
using Sdcb.FFmpeg.Raw;

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
    public static void InitializeFFmpeg()
    {
        // https://github.com/FFmpeg/FFmpeg/blob/70d25268c21cbee5f08304da95be1f647c630c15/doc/APIchanges#L86
        // https://github.com/leandromoreira/ffmpeg-libav-tutorial/issues/29

        var ver = ffmpeg.av_version_info();
        Debug.WriteLine($"Using FFmpeg version {ver}");
    }

    internal static bool IsFFmpegVersion4OrAbove()
    {
        // libavformat version of FFmpeg 4.0
        const int ffmpegVersion4AvformatVersion = 58;

        if (!_avformatVersion.HasValue)
        {
            _avformatVersion = ffmpeg.avformat_version();
        }

        return _avformatVersion.Value >= ffmpegVersion4AvformatVersion;
    }

    private static uint? _avformatVersion;

}
