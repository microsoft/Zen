// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using ZenLib;
    using ZenLib.ModelChecking;
    using ZenLib.Tests;

    using static ZenLib.Language;

    /// <summary>
    /// Benchmark for encoding ACLs of various sizes.
    /// </summary>
    public class AclBench
    {
        /// <summary>
        /// The backend to use.
        /// </summary>
        [Params(Backend.DecisionDiagrams, Backend.Z3)]
        public Backend Backend { get; set; } = Backend.DecisionDiagrams;

        /// <summary>
        /// The number of ACL lines to benchmark.
        /// </summary>
        [Params(100, 500, 1000)]
        public int NumLines { get; set; }

        private Acl acl;

        /// <summary>
        /// Create a deterministic random ACL of a given size.
        /// </summary>
        [GlobalSetup]
        public void CreateAcl()
        {
            var rnd = new Random(42);
            var lines = new List<AclLine>();
            for (int i = 0; i < this.NumLines; i++)
            {
                var d = (uint)rnd.Next();
                var s = (uint)rnd.Next();
                var line = new AclLine
                {
                    Permitted = (rnd.Next() & 1) == 0,
                    DstIpLow = (d & 0xFFFFFF00),
                    SrcIpLow = (s & 0xFFFFFF00),
                    DstIpHigh = (d | 0x000000FF),
                    SrcIpHigh = (s | 0x000000FF),
                };

                lines.Add(line);
            }

            this.acl = new Acl { Name = "BenchAcl", Lines = lines.ToArray() };
        }

        /// <summary>
        /// Find a packet that does not match any line of the ACL.
        /// </summary>
        [Benchmark]
        public void VerifyAcl()
        {
            var f = Function<Packet, Tuple<bool, ushort>>(this.acl.MatchProvenance);
            var packet = f.Find((p, o) => o.Item2() == (this.acl.Lines.Length + 1), backend: this.Backend);
        }
    }
}
