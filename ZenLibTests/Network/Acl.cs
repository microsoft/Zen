// <copyright file="Acl.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// An access control list object.
    /// </summary>
    class Acl
    {
        public AclLine[] Lines { get; set; }

        public Zen<bool> Process(Zen<IpHeader> hdr, int i)
        {
            if (i == this.Lines.Length)
            {
                return false;
            }

            var line = this.Lines[i];
            return If(line.Matches(hdr), line.Permitted, Process(hdr, i + 1));
        }
    }

    /// <summary>
    /// An ACL line object.
    /// </summary>
    class AclLine
    {
        public bool Permitted { get; set; }
        public Ip? DstLow { get; set; }
        public Ip? DstHigh { get; set; }
        public Ip? SrcLow { get; set; }
        public Ip? SrcHigh { get; set; }
        public ushort? DstPortLow { get; set; }
        public ushort? DstPortHigh { get; set; }
        public ushort? SrcPortLow { get; set; }
        public ushort? SrcPortHigh { get; set; }
        public byte? ProtocolLow { get; set; }
        public byte? ProtocolHigh { get; set; }

        public Zen<bool> Matches(Zen<IpHeader> hdr)
        {
            Zen<bool> dstLowMatch = this.DstLow.HasValue ? hdr.GetDstIp().GetValue() >= this.DstLow.Value.Value : true;
            Zen<bool> dstHighMatch = this.DstHigh.HasValue ? hdr.GetDstIp().GetValue() <= this.DstHigh.Value.Value : true;
            Zen<bool> srcLowMatch = this.SrcLow.HasValue ? hdr.GetSrcIp().GetValue() >= this.SrcLow.Value.Value : true;
            Zen<bool> srcHighMatch = this.SrcHigh.HasValue ? hdr.GetSrcIp().GetValue() <= this.SrcHigh.Value.Value : true;

            Zen<bool> dstPortLowMatch = this.DstPortLow.HasValue ? hdr.GetDstPort() >= this.DstPortLow.Value : true;
            Zen<bool> dstPortHighMatch = this.DstPortHigh.HasValue ? hdr.GetDstPort() <= this.DstPortHigh.Value : true;

            Zen<bool> srcPortLowMatch = this.SrcPortLow.HasValue ? hdr.GetSrcPort() >= this.SrcPortLow.Value : true;
            Zen<bool> srcPortHighMatch = this.SrcPortHigh.HasValue ? hdr.GetSrcPort() <= this.SrcPortHigh.Value : true;

            Zen<bool> protoLowMatch = this.ProtocolLow.HasValue ? hdr.GetProtocol() >= this.ProtocolLow.Value : true;
            Zen<bool> protoHighMatch = this.ProtocolHigh.HasValue ? hdr.GetProtocol() <= this.ProtocolHigh.Value : true;

            return And(
                dstLowMatch, dstHighMatch,
                srcLowMatch, srcHighMatch,
                dstPortHighMatch, dstPortLowMatch,
                srcPortHighMatch, srcPortLowMatch,
                protoHighMatch, protoLowMatch);
        }
    }
}
