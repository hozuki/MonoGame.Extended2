using System;
using System.Diagnostics;
using FFmpeg.AutoGen;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback.Extensions {
    internal static unsafe class PacketExtensions {

        public static long GetRobustTimestamp([NotNull] this Packet packet) {
            var rawPacket = packet.RawPacket;

            if (rawPacket == null) {
                throw new NullReferenceException("The containing AVPacket is null.");
            }

            var timestamp = rawPacket->pts;

            if (timestamp == ffmpeg.AV_NOPTS_VALUE) {
                // It usually happens in WMV/ASF (with ASF container).
                //
                // From FFmpeg docs for `AVPacket.pts` field:
                //
                //     ... Can be AV_NOPTS_VALUE if it is not stored in the file.
                //
                // So we have to assume that for these packets their desired PTS equal DTS, a reasonable assumption.
                timestamp = rawPacket->dts;
            }

            Debug.Assert(timestamp != ffmpeg.AV_NOPTS_VALUE, nameof(timestamp) + " != " + nameof(ffmpeg) + "." + nameof(ffmpeg.AV_NOPTS_VALUE));

            return timestamp;
        }

    }
}
