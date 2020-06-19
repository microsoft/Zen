// <copyright file="Option.cs" company="Microsoft">
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
        [Params(250, 500, 750, 1000, 1250, 1500, 1750, 2000, 2250, 2500, 2750, 3000, 3250, 3500, 3750, 4000, 4250, 4500, 4750, 5000)]
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
            var packet = f.Find((p, o) => o.Item2() == (this.acl.Lines.Length + 1), backend: this.Backend);
        }

        /// <summary>
        /// Find a packet that does not match any line of the ACL.
        /// </summary>
        [Benchmark]
        public void VerifyAcl()
        {
            var f = Function<IpHeader, bool>(h => this.acl.Process(h, 0));
            var packet = f.Find((p, o) => o, backend: this.Backend);
        }
    }
}
