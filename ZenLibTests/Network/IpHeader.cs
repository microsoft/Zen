// <copyright file="IpHeader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;

    /// <summary>
    /// An IP header object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class IpHeader
    {
        public Ip DstIp { get; set; }

        public Ip SrcIp { get; set; }

        public ushort DstPort { get; set; }

        public ushort SrcPort { get; set; }

        public byte Protocol { get; set; }

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

        public override string ToString()
        {
            return $"header(src={SrcIp}, dst={DstIp}, srcPort={SrcPort}, dstPort={DstPort}, proto={Protocol})";
        }
    }

    /// <summary>
    /// IP header extension methods.
    /// </summary>
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
