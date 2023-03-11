using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MonoGame.Extended.VideoPlayback.Extensions;

namespace MonoGame.Extended.VideoPlayback;

internal abstract class PacketComparer : IComparer<Packet>
{

    static PacketComparer()
    {
        FirstDtsThenPts = new DtsThenPts();
        FirstPtsThenDts = new PtsThenDts();
    }

    public int Compare(Packet? x, Packet? y)
    {
        if (x is null)
        {
            if (y is null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (y is null)
            {
                return 1;
            }
        }

        var compareResult = x.LoopNumber.CompareTo(y.LoopNumber);

        if (compareResult != 0)
        {
            return compareResult;
        }

        var xk = GetKey1(x);
        var yk = GetKey1(y);

        compareResult = xk.CompareTo(yk);

        if (compareResult != 0)
        {
            return compareResult;
        }

        xk = GetKey2(x);
        yk = GetKey2(y);

        return xk.CompareTo(yk);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AreInSameGroup(Packet packet1, Packet packet2)
    {
        return packet1.LoopNumber == packet2.LoopNumber && GetKey1(packet1) == GetKey1(packet2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareInSameGroup(Packet x, Packet y)
    {
        Debug.Assert(AreInSameGroup(x, y), "Packets to compare are not in the same group");
        return GetKey2(x).CompareTo(GetKey2(y));
    }

    protected abstract long GetKey1(Packet packet);

    protected abstract long GetKey2(Packet packet);

    public static readonly PacketComparer FirstDtsThenPts;

    public static readonly PacketComparer FirstPtsThenDts;

    private sealed class DtsThenPts : PacketComparer
    {

        protected override unsafe long GetKey1(Packet packet)
        {
            return packet.RawPacket->dts;
        }

        protected override long GetKey2(Packet packet)
        {
            return packet.GetRobustTimestamp();
        }

    }

    private sealed class PtsThenDts : PacketComparer
    {

        protected override long GetKey1(Packet packet)
        {
            return packet.GetRobustTimestamp();
        }

        protected override unsafe long GetKey2(Packet packet)
        {
            return packet.RawPacket->dts;
        }

    }

}
