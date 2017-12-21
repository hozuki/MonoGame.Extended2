using System.IO;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Media;

namespace MonoGame.Extended.DesktopGL.VideoPlayback {

    /// <summary>
    /// Video helper class.
    /// </summary>
    public static class VideoHelper {

        /// <summary>
        /// Loads a video from file system using default decoding options.
        /// </summary>
        /// <param name="path">The path of the video file.</param>
        /// <returns>Loaded video.</returns>
        public static Video LoadFromFile([NotNull] string path) {
            return LoadFromFile(path, DecodingOptions.Default);
        }

        /// <summary>
        /// Loads a video from file system using specified decoding options.
        /// </summary>
        /// <param name="path">The path of the video file.</param>
        /// <param name="decodingOptions">The decoding options to use.</param>
        /// <returns>Loaded video.</returns>
        public static Video LoadFromFile([NotNull] string path, [NotNull] DecodingOptions decodingOptions) {
            var fullPath = Path.GetFullPath(path);

            return LoadFromUrl(fullPath, decodingOptions);
        }

        /// <summary>
        /// Loads a video from a URL using default decoding options.
        /// </summary>
        /// <param name="url">The URL of the video source.</param>
        /// <returns>Loaded video.</returns>
        public static Video LoadFromUrl([NotNull] string url) {
            return LoadFromFile(url, DecodingOptions.Default);
        }

        /// <summary>
        /// Loads a video from a URL using specified decoding options.
        /// </summary>
        /// <param name="url">The URL of the video source.</param>
        /// <param name="decodingOptions">The decoding options to use.</param>
        /// <returns>Loaded video.</returns>
        public static Video LoadFromUrl([NotNull] string url, [NotNull] DecodingOptions decodingOptions) {
            return new Video(url, decodingOptions);
        }

    }
}
