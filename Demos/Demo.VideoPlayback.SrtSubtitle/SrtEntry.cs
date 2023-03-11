using System;

namespace Demo.VideoPlayback.SrtSubtitle;

internal sealed class SrtEntry : IComparable<SrtEntry>
{

    public SrtEntry(int sequenceNumber, TimeSpan start, TimeSpan end, string text)
    {
        SequenceNumber = sequenceNumber;
        Start = start;
        End = end;
        Text = text;
    }

    public int SequenceNumber { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }

    public string Text { get; }

    public int CompareTo(SrtEntry? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        return SequenceNumber.CompareTo(other.SequenceNumber);
    }

}
