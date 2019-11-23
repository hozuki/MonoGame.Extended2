using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback {
    internal abstract class PacketComparer : IComparer<Packet> {

        static PacketComparer() {
            FirstDtsThenPts = new DtsThenPts();
            FirstPtsThenDts = new PtsThenDts();
        }

        public int Compare(Packet x, Packet y) {
            Debug.Assert(x != null);
            Debug.Assert(y != null);

            var compareResult = x.LoopNumber.CompareTo(y.LoopNumber);

            if (compareResult != 0) {
                return compareResult;
            }

            var xk = GetKey1(x);
            var yk = GetKey1(y);

            compareResult = xk.CompareTo(yk);

            if (compareResult != 0) {
                return compareResult;
            }

            xk = GetKey2(x);
            yk = GetKey2(y);

            return xk.CompareTo(yk);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreInSameGroup([NotNull] Packet packet1, [NotNull] Packet packet2) {
            return packet1.LoopNumber == packet2.LoopNumber && GetKey1(packet1) == GetKey1(packet2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareInSameGroup([NotNull] Packet x, [NotNull] Packet y) {
            return GetKey2(x).CompareTo(GetKey2(y));
        }

        internal abstract long GetKey1(Packet packet);

        internal abstract long GetKey2(Packet packet);

        [NotNull]
        public static readonly PacketComparer FirstDtsThenPts;

        [NotNull]
        public static readonly PacketComparer FirstPtsThenDts;

        private sealed class DtsThenPts : PacketComparer {

            internal override unsafe long GetKey1(Packet packet) {
                return packet.RawPacket->dts;
            }

            internal override unsafe long GetKey2(Packet packet) {
                return packet.RawPacket->pts;
            }

        }

        private sealed class PtsThenDts : PacketComparer {

            internal override unsafe long GetKey1(Packet packet) {
                return packet.RawPacket->pts;
            }

            internal override unsafe long GetKey2(Packet packet) {
                return packet.RawPacket->dts;
            }

        }

    }
}
