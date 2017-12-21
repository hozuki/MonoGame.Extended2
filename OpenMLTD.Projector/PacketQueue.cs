using System;
using System.Collections;
using System.Collections.Generic;
using FFmpeg.AutoGen;

namespace OpenMLTD.Projector {
    /// <inheritdoc />
    /// <summary>
    /// A simple packet queue for <see cref="AVPacket"/>s.
    /// The packet order satisfies that: 1. the packet with smaller primary key stays in the front;
    /// 2. if two primary keys are equal, secondary keys apply; 3. otherwise, first-in-first-out (FIFO).
    /// </summary>
    internal sealed class PacketQueue : IEnumerable<AVPacket> {

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="T:OpenMLTD.Projector.PacketQueue" /> instance using the default comparing method and default initial capacity.
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
            _list = new List<AVPacket>();
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
            _list = new List<AVPacket>(capacity);
        }

        /// <summary>
        /// Default comparing method.
        /// </summary>
        internal const PacketQueueComparison DefaultComparison = PacketQueueComparison.FirstDtsThenPts;

        public IEnumerator<AVPacket> GetEnumerator() {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            var baseString = base.ToString();

            return $"{baseString} (Count = {Count})";
        }

        /// <summary>
        /// Enqueues an <see cref="AVPacket"/>.
        /// </summary>
        /// <param name="packet">The packet to enqueue.</param>
        internal void Enqueue(AVPacket packet) {
            var list = _list;
            var originalListCount = list.Count;

            // If there is nothing in the queue, then welcome our first guest!
            if (originalListCount == 0) {
                list.Add(packet);

                return;
            }

            Func<AVPacket, long> getKey1, getKey2;

            switch (Comparison) {
                case PacketQueueComparison.FirstDtsThenPts:
                    getKey1 = GetDts;
                    getKey2 = GetPts;
                    break;
                case PacketQueueComparison.FirstPtsThenDts:
                    getKey1 = GetPts;
                    getKey2 = GetDts;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var packetKey1 = getKey1(packet);
            var packetKey2 = getKey2(packet);

            var lastPacket = list[0];
            var lastPacketKey1 = getKey1(lastPacket);
            var lastPacketKey2 = getKey2(lastPacket);

            // If there is only one packet in the queue, we only need to do one comparison.
            if (originalListCount == 1) {
                if (packetKey1 > lastPacketKey1) {
                    list.Add(packet);
                } else if (packetKey1 < lastPacketKey1) {
                    list.Insert(0, packet);
                } else {
                    // Key #1s are equal, move on to key #2.
                    if (packetKey2 >= lastPacketKey2) {
                        list.Add(packet);
                    } else {
                        list.Insert(0, packet);
                    }
                }

                return;
            }

            // Handling boundary situation: the packet is "smaller" than the first packet in this queue.
            if (packetKey1 < lastPacketKey1) {
                list.Insert(0, packet);

                return;
            }

            // Index of the first packet in the list in which each packet has the same key #1 with its successor.
            // If the list only contains one packet, this index equals to the index of that packet in the packet queue.
            // Video streams sometimes contain packets with the same PTS or DTS. That's why there is a "secondary key" and "list of same primary keys".
            var sameKey1StartIndex = 0;

            for (var i = 1; i < originalListCount; ++i) {
                var currentPacket = list[i];
                var currentPacketKey1 = getKey1(currentPacket);

                // If we found a packet which is just "greater" than the packet we want to queue...
                if (packetKey1 < currentPacketKey1) {
                    // If `packet` is "greater" than the last packet we compared (last packet in the list of same key #1), we are feeling lucky.
                    if (packetKey1 > lastPacketKey1) {
                        list.Insert(i, packet);
                    } else {
                        // Otherwise, we have to scan through the list of same key #1 packets.
                        // Here, condition is packet_key1 == lastPacket_key1.

                        var sameKey1StartPacket = list[sameKey1StartIndex];
                        var sameKey1StartPacketKey2 = getKey2(sameKey1StartPacket);

                        // Again, we are feeling lucky...
                        if (packetKey2 < sameKey1StartPacketKey2) {
                            list.Insert(sameKey1StartIndex, packet);
                        } else {
                            var inserted = false;

                            // ... or scan through and find two fit packets, insert between them.
                            for (var j = sameKey1StartIndex; j < i - 1; ++j) {
                                var tmpPacket1 = list[j];
                                var tmpPacket2 = list[j + 1];
                                var tmpPacket1Key2 = getKey2(tmpPacket1);
                                var tmpPacket2Key2 = getKey2(tmpPacket2);

                                if (tmpPacket1Key2 <= packetKey2 && packetKey2 < tmpPacket2Key2) {
                                    list.Insert(j + 1, packet);
                                    inserted = true;

                                    break;
                                }
                            }

                            if (!inserted) {
                                // Those attemps failed, append the packet to the list of same key #1 packets.
                                list.Insert(i, packet);
                            }
                        }
                    }

                    return;
                }

                // If we still didn't find the "a little greater" packet, update sameKey1StartIndex...
                if (lastPacketKey1 != currentPacketKey1) {
                    sameKey1StartIndex = i;
                }

                // ... and assign the last packet to the packet we just compared.
                lastPacket = currentPacket;
                lastPacketKey1 = getKey1(lastPacket);
            }

            // Handle boundary situation: the packet is "greater" than all packages in the queue.
            list.Add(packet);

            // Performance optimization: should use readonly structs if using C# 7.2 or later...
            long GetDts(AVPacket pkt) {
                return pkt.dts;
            }

            long GetPts(AVPacket pkt) {
                return pkt.pts;
            }
        }

        /// <summary>
        /// Dequeues an <see cref="AVPacket"/> and returns it.
        /// </summary>
        /// <returns>The packet dequeued.</returns>
        internal AVPacket Dequeue() {
            var item = _list[0];
            _list.RemoveAt(0);

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

        private readonly List<AVPacket> _list;

    }
}
