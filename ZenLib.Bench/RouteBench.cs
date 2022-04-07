// <copyright file="AclBench.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using ZenLib;
    using ZenLib.ModelChecking;
    using ZenLib.Tests.Network;

    using static ZenLib.Zen;

    /// <summary>
    /// Benchmark for encoding ACLs of various sizes.
    /// </summary>
    [CsvExporter]
    [SimpleJob(targetCount: 30)]
    public class RouteBench
    {
        /// <summary>
        /// Gets or sets the backend to use.
        /// </summary>
        [Params(Backend.DecisionDiagrams, Backend.Z3)]
        public Backend Backend { get; set; }

        /// <summary>
        /// The maximum bounded list input size.
        /// </summary>
        [Params(3)]
        public int ListSize { get; set; }

        /// <summary>
        /// The number of ACL lines to benchmark.
        /// </summary>
        [Params(10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]
        public int NumLines { get; set; }

        private RouteMap routeMap;

        /// <summary>
        /// Create a deterministic random ACL of a given size.
        /// </summary>
        [GlobalSetup]
        public void CreateRouteMap()
        {
            var rnd = new Random(42);

            var lines = new List<RouteMapLine>();
            for (int i = 0; i < this.NumLines; i++)
            {
                var l1 = (byte)rnd.Next() % 25 + 8;
                var l2 = (byte)rnd.Next() % 25 + 8;
                var llow = (byte)Math.Min(l1, l2);
                var lhigh = (byte)Math.Max(l1, l2);
                var addr = (uint)rnd.Next();
                var protoGuard = rnd.Next() % 2 == 0 ? new List<byte>() : new List<byte> { (byte)(rnd.Next() % 4) };
                var commGuard = rnd.Next() % 2 == 0 ? new List<uint>() : new List<uint> { (uint)(rnd.Next() % 4) };
                var commAdds = rnd.Next() % 10 != 0 ? new List<uint>() : new List<uint> { (uint)(rnd.Next() % 4) };
                var commDeletes = rnd.Next() % 10 != 0 ? new List<uint>() : new List<uint> { (uint)(rnd.Next() % 4) };
                var x = rnd.Next() % 5;
                var disposition = x < 2 ? Disposition.Allow : (x < 4 ? Disposition.Deny : Disposition.NextTerm);

                var line = new RouteMapLine
                {
                    PrefixGuard = (addr, llow, lhigh),
                    ProtocolGuard = protoGuard,
                    AsPathContainsGuard = new List<uint>(),
                    CommunityGuard = commGuard,
                    CommunityAdds = commAdds,
                    CommunityDeletes = commDeletes,
                    AsPathPrepends = new List<uint>(),
                    Disposition = disposition,
                };

                lines.Add(line);
            }

            this.routeMap = new RouteMap { Lines = lines };
        }

        /// <summary>
        /// Find a route that does not match any line of the route map.
        /// </summary>
        [Benchmark]
        public void VerifyRouteMapProvenance()
        {
            var f = new ZenFunction<Route, Pair<Option<Route>, int>>(this.routeMap.ProcessProvenance);
            var packet = f.Find(
                (p, o) => o.Item2() == (this.routeMap.Lines.Count + 1),
                depth: this.ListSize,
                backend: this.Backend);
        }

        /// <summary>
        /// Find a route that does not match any line of the route map.
        /// </summary>
        [Benchmark]
        public void VerifyRouteMap()
        {
            var f = new ZenFunction<Route, Option<Route>>(this.routeMap.Process);
            var packet = f.Find(
                (p, o) => Not(o.IsSome()),
                depth: this.ListSize,
                backend: this.Backend);
        }
    }
}
