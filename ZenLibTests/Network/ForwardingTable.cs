// <copyright file="ForwardingTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// A forwarding table object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class ForwardingTable
    {
        public ForwardingRule[] Rules { get; set; }

        public Zen<byte> Forward(Zen<Packet> packet, int i)
        {
            if (i == this.Rules.Length)
            {
                return Constant<byte>(0); // 0 is the null interface
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
    [ExcludeFromCodeCoverage]
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
