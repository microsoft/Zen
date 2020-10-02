// <copyright file="IpHeader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;

    /// <summary>
    /// An IP header object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class IpHeader
    {
        /// <summary>
        /// The destination Ip.
        /// </summary>
        public Ip DstIp { get; set; }

        /// <summary>
        /// The source Ip.
        /// </summary>
        public Ip SrcIp { get; set; }

        /// <summary>
        /// The destination port.
        /// </summary>
        public ushort DstPort { get; set; }

        /// <summary>
        /// The source port.
        /// </summary>
        public ushort SrcPort { get; set; }

        /// <summary>
        /// The IP protocol.
        /// </summary>
        public byte Protocol { get; set; }

        /// <summary>
        /// Create a Zen packet from a five tuple.
        /// </summary>
        /// <param name="dstIp">The destination Ip.</param>
        /// <param name="srcIp">The source Ip.</param>
        /// <param name="dstPort">The destination port.</param>
        /// <param name="srcPort">The source port.</param>
        /// <param name="protocol">The protocol.</param>
        /// <returns>A Zen packet.</returns>
        public static Zen<IpHeader> Create(
            Zen<Ip> dstIp,
            Zen<Ip> srcIp,
            Zen<ushort> dstPort,
            Zen<ushort> srcPort,
            Zen<byte> protocol)
        {
            return Language.Create<IpHeader>(
                ("DstIp", dstIp),
                ("SrcIp", srcIp),
                ("DstPort", dstPort),
                ("SrcPort", srcPort),
                ("Protocol", protocol));
        }

        /// <summary>
        /// Equality for ip headers.
        /// </summary>
        /// <param name="obj">The other header.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is IpHeader header &&
                   EqualityComparer<Ip>.Default.Equals(DstIp, header.DstIp) &&
                   EqualityComparer<Ip>.Default.Equals(SrcIp, header.SrcIp) &&
                   DstPort == header.DstPort &&
                   SrcPort == header.SrcPort &&
                   Protocol == header.Protocol;
        }

        /// <summary>
        /// Hashcode for ip headers.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(DstIp, SrcIp, DstPort, SrcPort, Protocol);
        }

        /// <summary>
        /// Convert a packet to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"header(src={SrcIp}, dst={DstIp}, srcPort={SrcPort}, dstPort={DstPort}, proto={Protocol})";
        }
    }

    /// <summary>
    /// IP header extension methods.
    /// </summary>
    [ExcludeFromCodeCoverage]
    static class IpHeaderExtensions
    {
        public static Zen<Ip> GetDstIp(this Zen<IpHeader> hdr)
        {
            return hdr.GetField<IpHeader, Ip>("DstIp");
        }

        public static Zen<Ip> GetSrcIp(this Zen<IpHeader> hdr)
        {
            return hdr.GetField<IpHeader, Ip>("SrcIp");
        }

        public static Zen<ushort> GetDstPort(this Zen<IpHeader> hdr)
        {
            return hdr.GetField<IpHeader, ushort>("DstPort");
        }

        public static Zen<ushort> GetSrcPort(this Zen<IpHeader> hdr)
        {
            return hdr.GetField<IpHeader, ushort>("SrcPort");
        }

        public static Zen<byte> GetProtocol(this Zen<IpHeader> hdr)
        {
            return hdr.GetField<IpHeader, byte>("Protocol");
        }
    }
}
