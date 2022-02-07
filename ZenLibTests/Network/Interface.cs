// <copyright file="ForwardingTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// A device interface object.
    /// </summary>
    [ExcludeFromCodeCoverage]
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

                packet = Packet.Create(oheader, Option.Create(uheader));
            }

            return packet;
        }

        public Zen<Packet> Decapsulate(Zen<Packet> packet)
        {
            if (this.GreTunnel.HasValue)
            {
                return Packet.Create(packet.GetOverlayHeader(), Option.Null<IpHeader>());
            }

            return packet;
        }
    }
}
