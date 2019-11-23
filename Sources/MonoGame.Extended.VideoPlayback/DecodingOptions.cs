using JetBrains.Annotations;
using MonoGame.Extended.VideoPlayback.VideoDecoding;

namespace MonoGame.Extended.VideoPlayback {
    /// <summary>
    /// Video/audio decoding options.
    /// </summary>
    public sealed class DecodingOptions {

        /// <summary>
        /// Creates a new <see cref="DecodingOptions"/> instance.
        /// </summary>
        /// <param name="videoPacketQueueCapacity">The capacity of video packet queue.</param>
        /// <param name="audioPacketQueueCapacity">The capacity of audio packet queue.</param>
        /// <param name="videoPacketQueueSizeThreshold">The minimum size of the video packet queue.</param>
        /// <param name="audioPacketQueueSizeThreshold">The minimum size of the audio packet queue.</param>
        /// <param name="frameScalingMethod">The method to scale a video frame.</param>
        /// /// <param name="extraAudioBufferingTime">Extra buffering time of audio data, in milliseconds.</param>
        public DecodingOptions(int videoPacketQueueCapacity, int audioPacketQueueCapacity, int videoPacketQueueSizeThreshold, int audioPacketQueueSizeThreshold,
            FrameScalingMethod frameScalingMethod, int extraAudioBufferingTime) {
            VideoPacketQueueCapacity = videoPacketQueueCapacity;
            AudioPacketQueueCapacity = audioPacketQueueCapacity;
            VideoPacketQueueSizeThreshold = videoPacketQueueSizeThreshold;
            AudioPacketQueueSizeThreshold = audioPacketQueueSizeThreshold;
            FrameScalingMethod = frameScalingMethod;
            ExtraAudioBufferingTime = extraAudioBufferingTime;
        }

        /// <summary>
        /// The capacity of video packet queue.
        /// </summary>
        public int VideoPacketQueueCapacity { get; }

        /// <summary>
        /// The capacity of audio packet queue.
        /// </summary>
        public int AudioPacketQueueCapacity { get; }

        /// <summary>
        /// The minimum size of the video packet queue.
        /// </summary>
        public int VideoPacketQueueSizeThreshold { get; }

        /// <summary>
        /// The minimum size of the audio packet queue.
        /// </summary>
        public int AudioPacketQueueSizeThreshold { get; }

        /// <summary>
        /// The method to scale a video frame.
        /// </summary>
        public FrameScalingMethod FrameScalingMethod { get; }

        /// <summary>
        /// Delay of audio data, in milliseconds.
        /// </summary>
        public int ExtraAudioBufferingTime { get; }

        /// <summary>
        /// Default decoding options.
        /// </summary>
        [NotNull]
        public static readonly DecodingOptions Default
            = new DecodingOptions(
                512,
                512,
                15, // 15 packets should be enough for usual videos; if not, increase this number.
                30, // Audio packets are usually more than video packets.
                FrameScalingMethod.Default,
                800
            );

    }
}
