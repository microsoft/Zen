// <copyright file="Packet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// Packet struct that contains IP headers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct Packet
    {
        /// <summary>
        /// The overlay header.
        /// </summary>
        public IpHeader OverlayHeader { get; set; }

        /// <summary>
        /// The underlay header.
        /// </summary>
        public Option<IpHeader> UnderlayHeader { get; set; }

        /// <summary>
        /// Create a Zen packet from headers.
        /// </summary>
        /// <param name="overlayHeader">Overlay header.</param>
        /// <param name="underlayHeader">Underlay header.</param>
        /// <returns>Zen packet.</returns>
        public static Zen<Packet> Create(
            Zen<IpHeader> overlayHeader,
            Zen<Option<IpHeader>> underlayHeader)
        {
            return Language.Create<Packet>(
                ("OverlayHeader", overlayHeader),
                ("UnderlayHeader", underlayHeader));
        }

        /// <summary>
        /// Convert a packet to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"packet({this.OverlayHeader}, {this.UnderlayHeader})";
        }
    }

    /// <summary>
    /// Packet extension methods.
    /// </summary>
    [ExcludeFromCodeCoverage]
    static class PacketExtensions
    {
        public static Zen<IpHeader> GetOverlayHeader(this Zen<Packet> packet)
        {
            return packet.GetField<Packet, IpHeader>("OverlayHeader");
        }

        public static Zen<Option<IpHeader>> GetUnderlayHeader(this Zen<Packet> packet)
        {
            return packet.GetField<Packet, Option<IpHeader>>("UnderlayHeader");
        }

        public static Zen<IpHeader> GetCurrentHeader(this Zen<Packet> pkt)
        {
            return pkt.GetUnderlayHeader().ValueOrDefault(pkt.GetOverlayHeader());
        }
    }
}
