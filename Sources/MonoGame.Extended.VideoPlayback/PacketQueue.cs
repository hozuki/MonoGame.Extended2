using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FFmpeg.AutoGen;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback {
    /// <inheritdoc />
    /// <summary>
    /// A simple packet queue for <see cref="Packet"/>s.
    /// The packet order satisfies that: 1. the packet with smaller primary key stays in the front;
    /// 2. if two primary keys are equal, secondary keys apply; 3. otherwise, first-in-first-out (FIFO).
    /// </summary>
    internal sealed class PacketQueue : IEnumerable<Packet> {

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="PacketQueue" /> instance using the default comparing method and default initial capacity.
        /// </summary>
        internal PacketQueue()
            : this(DefaultComparison) {
        }

        /// <summary>
        /// Creates a new <see cref="PacketQueue"/> instance using specified comparing method and default initial capacity.
        /// </summary>
        /// <param name="comparison">Comparing method.</param>
        internal PacketQueue(PacketQueueComparison comparison) {
            Comparison = comparison;
            _list = new LinkedList<Packet>();
            _comparer = CreateComparer(comparison);
        }

        /// <summary>
        /// Creates a new <see cref="PacketQueue"/> instance using the default comparing method and specified initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        internal PacketQueue(int capacity)
            : this(DefaultComparison, capacity) {
        }

        /// <summary>
        /// Creates a new <see cref="PacketQueue"/> instance using the specified comparing method and initial capacity.
        /// </summary>
        /// <param name="comparison">Comparing method.</param>
        /// <param name="capacity">Initial capacity.</param>
        internal PacketQueue(PacketQueueComparison comparison, int capacity) {
            Comparison = comparison;
            _list = new LinkedList<Packet>();
            _comparer = CreateComparer(comparison);
        }

        /// <summary>
        /// Default comparing method.
        /// </summary>
        internal const PacketQueueComparison DefaultComparison = PacketQueueComparison.FirstDtsThenPts;

        public IEnumerator<Packet> GetEnumerator() {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            var baseString = base.ToString();

            return $"{baseString} (Count = {Count.ToString()})";
        }

        /// <summary>
        /// Enqueues an <see cref="Packet"/>.
        /// </summary>
        /// <param name="packetToInsert">The packet to enqueue.</param>
        internal void Enqueue(Packet packetToInsert) {
            var list = _list;
            var originalListCount = list.Count;

            // If there is nothing in the queue, then welcome our first guest!
            if (originalListCount == 0) {
                list.AddLast(packetToInsert);

                return;
            }

            var comparer = _comparer;
            var lastPacket = list.First.Value;
            var compareResult = comparer.Compare(packetToInsert, lastPacket);

            // If there is only one packet in the queue, we only need to do one comparison.
            if (originalListCount == 1) {
                if (compareResult >= 0) {
                    list.AddLast(packetToInsert);
                } else {
                    list.AddFirst(packetToInsert);
                }

                return;
            }

            // Handling boundary situation: the packet is "smaller" than the first packet in this queue.
            if (compareResult < 0) {
                list.AddFirst(packetToInsert);

                return;
            }

            // Video streams sometimes contain packets with the same PTS or DTS. That's why there is a "secondary key" and "list of same primary keys".

            for (var groupStartNode = list.First; groupStartNode != null;) {
                var groupStartPacket = groupStartNode.Value;

                var groupEndNode = groupStartNode;
                var groupSize = 1;

                // Scan the whole group, mark its start and end.
                while (groupEndNode.Next != null && comparer.AreInSameGroup(groupEndNode.Value, groupEndNode.Next.Value)) {
                    groupEndNode = groupEndNode.Next;
                    ++groupSize;
                }

                // If the packet belongs to the group, find a place for it to insert.
                // Possible locations: before the first item in the group; between two group items; after the whole group.
                if (comparer.AreInSameGroup(packetToInsert, groupStartPacket)) {
                    var currentNode = groupStartNode;
                    var inserted = false;

                    for (var i = 0; i < groupSize; ++i) {
                        compareResult = comparer.CompareInSameGroup(packetToInsert, currentNode.Value);

                        Debug.Assert(compareResult != 0);

                        if (compareResult < 0) {
                            list.AddBefore(currentNode, packetToInsert);
                            inserted = true;

                            break;
                        }

                        currentNode = currentNode.Next;

                        Debug.Assert(currentNode != null);
                    }

                    if (!inserted) {
                        list.AddAfter(groupEndNode, packetToInsert);
                    }

                    return;
                } else {
                    // Otherwise it should fall before (smaller than) or after (larger than) the whole group.

                    compareResult = comparer.Compare(packetToInsert, groupStartPacket);

                    Debug.Assert(compareResult != 0);

                    // If the item is smaller than the whole group, just insert it before the group start.
                    if (compareResult < 0) {
                        list.AddBefore(groupStartNode, packetToInsert);

                        return;
                    }

                    // Otherwise, fall through and move to the next group.
                }

                groupStartNode = groupEndNode.Next;
            }

            // Handle boundary situation: the packet is "greater" than all packages in the queue.
            list.AddLast(packetToInsert);
        }

        /// <summary>
        /// Dequeues an <see cref="AVPacket"/> and returns it.
        /// </summary>
        /// <returns>The packet dequeued.</returns>
        internal Packet Dequeue() {
            var item = _list.First.Value;
            _list.RemoveFirst();

            return item;
        }

        /// <summary>
        /// Clears the <see cref="PacketQueue"/>.
        /// </summary>
        internal void Clear() {
            _list.Clear();
        }

        /// <summary>
        /// Gets the number of packets in the <see cref="PacketQueue"/>.
        /// </summary>
        internal int Count => _list.Count;

        /// <summary>
        /// The comparing method of this <see cref="PacketQueue"/>.
        /// </summary>
        internal PacketQueueComparison Comparison { get; }

        private static PacketComparer CreateComparer(PacketQueueComparison comparison) {
            switch (comparison) {
                case PacketQueueComparison.FirstDtsThenPts:
                    return PacketComparer.FirstDtsThenPts;
                case PacketQueueComparison.FirstPtsThenDts:
                    return PacketComparer.FirstPtsThenDts;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }
        }

        [NotNull]
        private readonly LinkedList<Packet> _list;

        [NotNull]
        private readonly PacketComparer _comparer;

    }
}
