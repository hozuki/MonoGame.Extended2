namespace MonoGame.Extended.VideoPlayback;

/// <summary>
/// Comparing methods for <see cref="PacketQueue"/>.
/// </summary>
internal enum PacketQueueComparison
{

    /// <summary>
    /// Compare by DTS. If values are equal, compare by PTS.
    /// </summary>
    FirstDtsThenPts = 0,

    /// <summary>
    /// Compare by PTS. If values are equal, compare by DTS.
    /// </summary>
    FirstPtsThenDts = 1

}
