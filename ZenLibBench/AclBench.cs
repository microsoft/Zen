﻿// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using BenchmarkDotNet.Attributes;
    using ZenLib;
    using ZenLib.ModelChecking;
    using ZenLib.Tests.Network;

    using static ZenLib.Language;

    /// <summary>
    /// Benchmark for encoding ACLs of various sizes.
    /// </summary>
    [CsvExporter]
    [SimpleJob(targetCount: 30)]
    public class AclBench
    {
        /// <summary>
        /// The backend to use.
        /// </summary>
        [Params(Backend.DecisionDiagrams, Backend.Z3)]
        public Backend Backend { get; set; }

        /// <summary>
        /// The number of ACL lines to benchmark.
        /// </summary>
        [Params(0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000, 13000, 14000, 15000)]
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
                var line = new AclLine
                {
                    Permitted = (rnd.Next() & 1) == 0,
                    DstIp = Prefix.Random(24, 32),
                    SrcIp = Prefix.Random(24, 32),
                };

                lines.Add(line);
            }

            // add default deny
            var defaultPrefix = new Prefix { Length = 0, Address = 0U };
            lines.Add(new AclLine { DstIp = defaultPrefix, SrcIp = defaultPrefix });

            this.acl = new Acl { Lines = lines.ToArray() };
        }

        /// <summary>
        /// Find a packet that does not match any line of the ACL.
        /// </summary>
        [Benchmark]
        public void VerifyAclProvenance()
        {
            var f = Function<IpHeader, (bool, ushort)>(h => this.acl.ProcessProvenance(h));
            var packet = f.Find((p, o) => o.Item2() == (ushort)(this.acl.Lines.Length + 1), backend: this.Backend);
            f.Evaluate(packet.Value);
        }
    }
}
