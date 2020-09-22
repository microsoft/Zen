// <copyright file="Acl.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// An access control list object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Acl
    {
        /// <summary>
        /// The acl line.
        /// </summary>
        public AclLine[] Lines { get; set; }

        /// <summary>
        /// Process a header on an acl.
        /// </summary>
        /// <param name="hdr">The header.</param>
        /// <param name="i">The line number.</param>
        /// <returns>Whether permitted.</returns>
        public Zen<bool> Process(Zen<IpHeader> hdr, int i)
        {
            if (i == this.Lines.Length)
            {
                return false;
            }

            var line = this.Lines[i];
            return If(line.Matches(hdr), line.Permitted, Process(hdr, i + 1));
        }

        /// <summary>
        /// Process a header on an acl.
        /// </summary>
        /// <param name="hdr">The header.</param>
        /// <returns>Whether permitted.</returns>
        public Zen<(bool, ushort)> ProcessProvenance(Zen<IpHeader> hdr)
        {
            var acc = ValueTuple<bool, ushort>(false, (ushort)(this.Lines.Length + 1));

            for (int i = this.Lines.Length - 1; i >= 0; i--)
            {
                var line = this.Lines[i];
                acc = If(line.Matches(hdr), ValueTuple<bool, ushort>(line.Permitted, (ushort)(i + 1)), acc);
            }

            return acc;
        }

        /// <summary>
        /// Process a header on an acl.
        /// </summary>
        /// <param name="hdr">The header.</param>
        /// <param name="i">The line number.</param>
        /// <returns>Whether permitted.</returns>
        public Zen<(bool, ushort)> ProcessProvenance(Zen<IpHeader> hdr, int i)
        {
            if (i == this.Lines.Length)
            {
                return ValueTuple<bool, ushort>(false, (ushort)(i + 1));
            }

            var line = this.Lines[i];
            return If(
                line.Matches(hdr),
                ValueTuple<bool, ushort>(line.Permitted, (ushort)(i + 1)),
                ProcessProvenance(hdr, i + 1));
        }

        /// <summary>
        /// Create a string from an acl.
        /// </summary>
        /// <returns></returns>
        public string Display(string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"ip access-list {name}\n");
            for (int i = 0; i < this.Lines.Length; i++)
            {
                sb.Append($"  {this.Lines[i].Display(i * 10)}\n");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// An ACL line object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AclLine
    {
        /// <summary>
        /// Whether to permit or deny.
        /// </summary>
        public bool Permitted { get; set; }

        /// <summary>
        /// The destination match.
        /// </summary>
        public Prefix DstIp { get; set; }

        /// <summary>
        /// The source match.
        /// </summary>
        public Prefix SrcIp { get; set; }

        /// <summary>
        /// The destination port low match.
        /// </summary>
        public ushort? DstPortLow { get; set; }

        /// <summary>
        /// The destination port high match.
        /// </summary>
        public ushort? DstPortHigh { get; set; }

        /// <summary>
        /// The source port low match.
        /// </summary>
        public ushort? SrcPortLow { get; set; }

        /// <summary>
        /// The source port high match.
        /// </summary>
        public ushort? SrcPortHigh { get; set; }

        /// <summary>
        /// The protocol lowm atch.
        /// </summary>
        public byte? ProtocolLow { get; set; }

        /// <summary>
        /// The protocol high match.
        /// </summary>
        public byte? ProtocolHigh { get; set; }

        /// <summary>
        /// Match a header against an acl line.
        /// </summary>
        /// <param name="hdr">The header.</param>
        /// <returns>Whether matched.</returns>
        public Zen<bool> Matches(Zen<IpHeader> hdr)
        {
            Zen<bool> dstMatch = this.DstIp != null ? this.DstIp.Match(hdr.GetDstIp()) : true;
            Zen<bool> srcMatch = this.SrcIp != null ? this.SrcIp.Match(hdr.GetSrcIp()) : true;

            Zen<bool> dstPortLowMatch = this.DstPortLow.HasValue ? hdr.GetDstPort() >= this.DstPortLow.Value : true;
            Zen<bool> dstPortHighMatch = this.DstPortHigh.HasValue ? hdr.GetDstPort() <= this.DstPortHigh.Value : true;

            Zen<bool> srcPortLowMatch = this.SrcPortLow.HasValue ? hdr.GetSrcPort() >= this.SrcPortLow.Value : true;
            Zen<bool> srcPortHighMatch = this.SrcPortHigh.HasValue ? hdr.GetSrcPort() <= this.SrcPortHigh.Value : true;

            Zen<bool> protoLowMatch = this.ProtocolLow.HasValue ? hdr.GetProtocol() >= this.ProtocolLow.Value : true;
            Zen<bool> protoHighMatch = this.ProtocolHigh.HasValue ? hdr.GetProtocol() <= this.ProtocolHigh.Value : true;

            return And(
                dstMatch, srcMatch,
                dstPortHighMatch, dstPortLowMatch,
                srcPortHighMatch, srcPortLowMatch,
                protoHighMatch, protoLowMatch);
        }

        /// <summary>
        /// Create a string from an acl line.
        /// </summary>
        /// <returns></returns>
        public string Display(int i)
        {
            var permitted = this.Permitted ? "permit" : "deny";
            var dstIp = this.DstIp == null ? "any" : this.DstIp.ToString();
            var srcIp = this.DstIp == null ? "any" : this.SrcIp.ToString();
            return $"{i} {permitted} ip {dstIp} {srcIp}";
        }
    }
}
