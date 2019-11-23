using FFmpeg.AutoGen;

namespace MonoGame.Extended.VideoPlayback {
    /// <summary>
    /// <see cref="AVPacket"/> with some custom properties.
    /// </summary>
    internal struct Packet {

        /// <summary>
        /// Creates a new <see cref="Packet"/> instance.
        /// </summary>
        /// <param name="loopNumber">Loop number.</param>
        public Packet(int loopNumber) {
            LoopNumber = loopNumber;
            RawPacket = new AVPacket();
        }

        /// <summary>
        /// Creates a new <see cref="Packet"/> instance.
        /// </summary>
        /// <param name="rawPacket">Raw <see cref="AVPacket"/>.</param>
        /// <param name="loopNumber">Loop number.</param>
        public Packet(AVPacket rawPacket, int loopNumber) {
            RawPacket = rawPacket;
            LoopNumber = loopNumber;
        }

        /// <summary>
        /// Loop number of the packet.
        /// </summary>
        public readonly int LoopNumber;

        /// <summary>
        /// Raw <see cref="AVPacket"/>.
        /// </summary>
        /// <remarks>Cannot be read-only because this field is used in address retrievals.</remarks>
        public AVPacket RawPacket;

    }
}
