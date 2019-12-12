using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback {
    /// <summary>
    /// Options for <see cref="Framework.Media.VideoPlayer"/>.
    /// </summary>
    public sealed class VideoPlayerOptions {

        /// <summary>
        /// Creates a new <see cref="VideoPlayerOptions"/> instance.
        /// </summary>
        /// <param name="decodingThreadSleepInterval">The sleeping interval of decoding thread, in milliseconds.</param>
        /// <param name="minimumAudioBufferCount">Minimum number of audio buffers.</param>
        public VideoPlayerOptions(int decodingThreadSleepInterval, int minimumAudioBufferCount) {
            DecodingThreadSleepInterval = decodingThreadSleepInterval;
            MinimumAudioBufferCount = minimumAudioBufferCount;
        }

        /// <summary>
        /// The sleeping interval of decoding thread, in milliseconds.
        /// </summary>
        public int DecodingThreadSleepInterval { get; }

        /// <summary>
        /// Minimum number of audio buffers.
        /// </summary>
        public int MinimumAudioBufferCount { get; }

        /// <summary>
        /// Default video player options.
        /// </summary>
        [NotNull]
        public static readonly VideoPlayerOptions Default
            = new VideoPlayerOptions(
                5, // If we want a 60 fps display (~16.67 ms/frame), sleeping for ~5 ms is OK. If you suffer from a low frame rate, decrease this value.
                15
            );

    }
}
