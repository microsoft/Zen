// <copyright file="ForwardingTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// A device objet.
    /// </summary>
    class Device
    {
        public string Name { get; set; }

        public ForwardingTable Table { get; set; }

        public Interface[] Interfaces { get; set; }
    }

    /// <summary>
    /// A device interface object.
    /// </summary>
    class Interface
    {
        public string Name { get; set; }

        public Device Owner { get; set; }

        public Interface Neighbor { get; set; }

        public byte PortNumber { get; set; }

        public Acl InboundAcl { get; set; }

        public Acl OutboundACl { get; set; }

        public (Ip, Ip)? GreTunnel { get; set; }

        public Zen<Packet> Encapsulate(Zen<Packet> packet)
        {
            if (this.GreTunnel.HasValue)
            {
                var oheader = packet.GetOverlayHeader();
                var newSrcIp = this.GreTunnel.Value.Item1;
                var newDstIp = this.GreTunnel.Value.Item2;
                var uheader = IpHeader.Create(
                    Ip.Create(newDstIp.Value),
                    Ip.Create(newSrcIp.Value),
                    oheader.GetDstPort(),
                    oheader.GetSrcPort(),
                    oheader.GetProtocol());

                packet = Packet.Create(oheader, Some(uheader));
            }

            return packet;
        }

        public Zen<Packet> Decapsulate(Zen<Packet> packet)
        {
            if (this.GreTunnel.HasValue)
            {
                return Packet.Create(packet.GetOverlayHeader(), Null<IpHeader>());
            }

            return packet;
        }
    }

    /// <summary>
    /// A forwarding table object.
    /// </summary>
    class ForwardingTable
    {
        public ForwardingRule[] Rules { get; set; }

        public Zen<byte> Forward(Zen<Packet> packet, int i)
        {
            if (i == this.Rules.Length)
            {
                return Byte(0); // 0 is the null interface
            }

            var rule = this.Rules[i];

            return If(
                rule.Matches(packet.GetCurrentHeader()),
                rule.Interface.PortNumber,
                Forward(packet, i + 1));
        }
    }

    /// <summary>
    /// A forwarding rule object.
    /// </summary>
    class ForwardingRule
    {
        public Ip DstIpLow { get; set; }

        public Ip DstIpHigh { get; set; }

        public Interface Interface { get; set; }

        public Zen<bool> Matches(Zen<IpHeader> hdr)
        {
            Zen<bool> dstLowMatch = hdr.GetDstIp().GetValue() >= this.DstIpLow.Value;
            Zen<bool> dstHighMatch = hdr.GetDstIp().GetValue() <= this.DstIpHigh.Value;
            return And(dstLowMatch, dstHighMatch);
        }
    }
}
