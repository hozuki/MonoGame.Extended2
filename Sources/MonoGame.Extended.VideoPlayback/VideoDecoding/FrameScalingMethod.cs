using Sdcb.FFmpeg.Raw;

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
    FastBilinear = (int)SWS.FastBilinear,

    /// <summary>
    /// Use bilinear interpolation.
    /// </summary>
    Bilinear = (int)SWS.Bilinear,

    /// <summary>
    /// Use bicubic interpolation.
    /// </summary>
    Bicubic = (int)SWS.Bicubic,

    /// <summary>
    /// Use experimental algorithm.
    /// The meaning is taken from <code>https://ffmpeg.org/ffmpeg-scaler.html</code>.
    /// </summary>
    Experimental = (int)SWS.X,

    /// <summary>
    /// Use neighbor approximation.
    /// </summary>
    Neighbor = (int)SWS.Point,

    /// <summary>
    /// Use area averaging.
    /// </summary>
    Area = (int)SWS.Area,

    /// <summary>
    /// Use bicubic interpolation for luma and bilinear for chroma.
    /// </summary>
    BicubicLinear = (int)SWS.Bicublin,

    /// <summary>
    /// Use Gaussian rescaling.
    /// </summary>
    Gauss = (int)SWS.Gauss,

    /// <summary>
    /// Use sinc rescaling.
    /// </summary>
    Sinc = (int)SWS.Sinc,

    /// <summary>
    /// Use Lanczos filter for rescaling.
    /// </summary>
    Lanczos = (int)SWS.Lanczos,

    /// <summary>
    /// Use natural bicubic spline interpolation.
    /// </summary>
    Spline = (int)SWS.Spline,

}
