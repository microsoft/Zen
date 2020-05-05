// <copyright file="Packet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Zen;
    using static Zen.Language;

    /// <summary>
    /// Simple packet class for testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Packet
    {
        /// <summary>
        /// The destination IP address.
        /// </summary>
        public uint DstIp { get; set; }

        /// <summary>
        /// The source IP address.
        /// </summary>
        public uint SrcIp { get; set; }

        /// <summary>
        /// Convert a packet to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"DstIp={DstIp}, SrcIp={SrcIp}";
        }

        /// <summary>
        /// Create a packet object.
        /// </summary>
        /// <param name="dstIp">The destination IP.</param>
        /// <param name="srcIp">The source IP.</param>
        /// <returns>The packet.</returns>
        public static Zen<Packet> Create(Zen<uint> dstIp, Zen<uint> srcIp)
        {
            return Create<Packet>(("DstIp", dstIp), ("SrcIp", srcIp));
        }
    }

    /// <summary>
    /// Helper class for Packets.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PacketHelper
    {
        /// <summary>
        /// Get the destination IP.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The ip.</returns>
        public static Zen<uint> GetDstIp(this Zen<Packet> packet)
        {
            return packet.GetField<Packet, uint>("DstIp");
        }

        /// <summary>
        /// Get the source IP.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The ip.</returns>
        public static Zen<uint> GetSrcIp(this Zen<Packet> packet)
        {
            return packet.GetField<Packet, uint>("SrcIp");
        }
    }
}
