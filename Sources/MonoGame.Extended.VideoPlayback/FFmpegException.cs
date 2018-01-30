using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback {
    /// <inheritdoc />
    /// <summary>
    /// Represents an exception caused by FFmpeg functions.
    /// </summary>
    public sealed class FFmpegException : ApplicationException {

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="T:OpenMLTD.Projector.FFmpegException" /> instance.
        /// </summary>
        public FFmpegException() {
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="T:OpenMLTD.Projector.FFmpegException" /> instance and sets the error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public FFmpegException([NotNull] string message)
            : base(message) {
        }

    }
}
