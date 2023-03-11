using FFmpeg.AutoGen;

namespace MonoGame.Extended.VideoPlayback.VideoDecoding;

/// <summary>
/// Picture frame scaling methods.
/// </summary>
public enum FrameScalingMethod
{

    /// <summary>
    /// Use the default method.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Use fast bilinear interpolation.
    /// </summary>
    FastBilinear = ffmpeg.SWS_FAST_BILINEAR,

    /// <summary>
    /// Use bilinear interpolation.
    /// </summary>
    Bilinear = ffmpeg.SWS_BILINEAR,

    /// <summary>
    /// Use bicubic interpolation.
    /// </summary>
    Bicubic = ffmpeg.SWS_BICUBIC,

    /// <summary>
    /// Use experimental algorithm.
    /// The meaning is taken from <code>https://ffmpeg.org/ffmpeg-scaler.html</code>.
    /// </summary>
    Experimental = ffmpeg.SWS_X,

    /// <summary>
    /// Use neighbor approximation.
    /// </summary>
    Neighbor = ffmpeg.SWS_POINT,

    /// <summary>
    /// Use area averaging.
    /// </summary>
    Area = ffmpeg.SWS_AREA,

    /// <summary>
    /// Use bicubic interpolation for luma and bilinear for chroma.
    /// </summary>
    BicubicLinear = ffmpeg.SWS_BICUBLIN,

    /// <summary>
    /// Use Gaussian rescaling.
    /// </summary>
    Gauss = ffmpeg.SWS_GAUSS,

    /// <summary>
    /// Use sinc rescaling.
    /// </summary>
    Sinc = ffmpeg.SWS_SINC,

    /// <summary>
    /// Use Lanczos filter for rescaling.
    /// </summary>
    Lanczos = ffmpeg.SWS_LANCZOS,

    /// <summary>
    /// Use natural bicubic spline interpolation.
    /// </summary>
    Spline = ffmpeg.SWS_SPLINE

}
