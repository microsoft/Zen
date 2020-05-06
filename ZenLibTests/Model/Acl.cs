// <copyright file="Acl.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// Simple packet class for testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Acl
    {
        /// <summary>
        /// Gets or sets the name of the ACL.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the lines of the ACL.
        /// </summary>
        public AclLine[] Lines { get; set; }

        /// <summary>
        /// Match a packet either accepting or rejecting.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Whether accepted.</returns>
        public Zen<bool> Match(Zen<Packet> packet)
        {
            return Match(packet, 0);
        }

        private Zen<bool> Match(Zen<Packet> packet, int i)
        {
            if (i == this.Lines.Length)
            {
                return false;
            }

            var line = this.Lines[i];

            return If(line.Match(packet), line.Permitted, this.Match(packet, i + 1));
        }

        /// <summary>
        /// Match a packet either accepting or rejecting.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Whether accepted.</returns>
        public bool Match(Packet packet)
        {
            return Match(packet, 0);
        }

        private bool Match(Packet packet, int i)
        {
            if (i == this.Lines.Length)
            {
                return false;
            }

            var line = this.Lines[i];

            return line.Match(packet) ? line.Permitted : this.Match(packet, i + 1);
        }

        /// <summary>
        /// Match a packet either accepting or rejecting.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Whether accepted.</returns>
        public bool MatchLoop(Packet packet)
        {
            for (int i = 0; i < this.Lines.Length; i++)
            {
                var line = this.Lines[i];
                if (line.Match(packet))
                {
                    return line.Permitted;
                }
            }

            return false;
        }

        /// <summary>
        /// Match a packet and get the line number of the match.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Whether accepting and the line number of the match.</returns>
        public Zen<Tuple<bool, ushort>> MatchProvenance(Zen<Packet> packet)
        {
            return TestHelper.ApplyOrderedRules(
                input: packet,
                deflt: Tuple<bool, ushort>(false, this.Lines.Length + 1),
                ruleMatch: (l, pkt, i) => l.Match(pkt),
                ruleAction: (l, pkt, i) => pkt,
                ruleReturn: (l, pkt, i) => Some(Tuple<bool, ushort>(l.Permitted, i)),
                rules: this.Lines);
        }
    }

    /// <summary>
    /// A line of an ACL.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AclLine
    {
        /// <summary>
        /// Gets a value indicating whether the packet is permitted or denied.
        /// </summary>
        public bool Permitted { get; set; }

        /// <summary>
        /// Gets the destination IP low match.
        /// </summary>
        public uint DstIpLow { get; set; }

        /// <summary>
        /// Gets the destination IP high match.
        /// </summary>
        public uint DstIpHigh { get; set; }

        /// <summary>
        /// Gets the source IP low match.
        /// </summary>
        public uint SrcIpLow { get; set; }

        /// <summary>
        /// Gets the source IP high match.
        /// </summary>
        public uint SrcIpHigh { get; set; }

        /// <summary>
        /// Match a packet with this acl.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Whether accepted.</returns>
        public Zen<bool> Match(Zen<Packet> packet)
        {
            var dstIp = packet.GetDstIp();
            var srcIp = packet.GetSrcIp();
            return And(dstIp >= this.DstIpLow,
                       dstIp <= this.DstIpHigh,
                       srcIp >= this.SrcIpLow,
                       srcIp <= this.SrcIpHigh);
        }

        /// <summary>
        /// Match a packet with this acl.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Whether accepted.</returns>
        public bool Match(Packet packet)
        {
            var dstIp = packet.DstIp;
            var srcIp = packet.SrcIp;
            return dstIp >= this.DstIpLow &&
                   dstIp <= this.DstIpHigh &&
                   srcIp >= this.SrcIpLow &&
                   srcIp <= this.SrcIpHigh;
        }
    }
}
