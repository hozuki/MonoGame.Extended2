using System;
using Sdcb.FFmpeg.Raw;

namespace MonoGame.Extended.VideoPlayback;

/// <inheritdoc />
/// <summary>
/// Represents an exception caused by FFmpeg functions.
/// </summary>
public sealed class FFmpegException : ApplicationException
{

    /// <inheritdoc />
    /// <summary>
    /// Creates a new <see cref="FFmpegException" /> instance.
    /// </summary>
    public FFmpegException()
        : this(string.Empty)
    {
    }

    /// <inheritdoc />
    /// <summary>
    /// Creates a new <see cref="FFmpegException" /> instance and sets the error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public FFmpegException(string message)
        : this(message, ffmpeg.AVERROR_UNKNOWN)
    {
    }

    /// <summary>
    /// Creates a new <see cref="FFmpegException" /> instance, sets the error message and error number.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="avError">Error code.</param>
    public FFmpegException(string message, int avError)
        : base(message)
    {
        AvError = avError;
    }

    /// <summary>
    /// Gets FFmpeg error code.
    /// </summary>
    public int AvError { get; }

    /// <summary>
    /// Gets FFmpeg error description.
    /// </summary>
    public string AvErrorDescription
    {
        get
        {
            if (_avErrorDescription == null)
            {
                _avErrorDescription = FFmpegHelper.GetErrorString(AvError);
            }

            return _avErrorDescription;
        }
    }

    private string? _avErrorDescription;

}
