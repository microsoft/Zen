// <copyright file="LocatedPacket.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// A packet with a location.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LocatedPacket
    {
        /// <summary>
        /// Gets the node location as a byte.
        /// </summary>
        public byte Node { get; set; }

        /// <summary>
        /// Gets the Packet.
        /// </summary>
        public IpHeader Header { get; set; }
    }

    /// <summary>
    /// Helper class for LocatedPacket.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class LocatedPacketHelper
    {
        /// <summary>
        /// Create a LocatedPacket.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="header">The header.</param>
        /// <returns>A LocatedPacket.</returns>
        public static Zen<LocatedPacket> Create(Zen<byte> node, Zen<IpHeader> header)
        {
            return Create<LocatedPacket>(("Node", node), ("Header", header));
        }

        /// <summary>
        /// Get the node of a LocatedPacket.
        /// </summary>
        /// <param name="lp">The LocatedPacket.</param>
        /// <returns>The node.</returns>
        public static Zen<byte> GetNode(this Zen<LocatedPacket> lp)
        {
            return lp.GetField<LocatedPacket, byte>("Node");
        }

        /// <summary>
        /// Get the Packet of a LocatedPacket.
        /// </summary>
        /// <param name="lp">The LocatedPacket.</param>
        /// <returns>The packet.</returns>
        public static Zen<IpHeader> GetHeader(this Zen<LocatedPacket> lp)
        {
            return lp.GetField<LocatedPacket, IpHeader>("Header");
        }
    }
}
