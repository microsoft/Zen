// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ZenLib;

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkTransformers();
            // BenchmarkTransformerCache();
            // BenchmarkAllocation();
        }

        private static void BenchmarkTransformers()
        {
            Benchmark(nameof(BenchmarkAllocation), 1, () =>
            {
                var f = new Func<Zen<TestObject>, Zen<Option<TestObject>>>(p =>
                {
                    return Language.If(
                        p.GetField<TestObject, uint>("DstIp") == 1,
                        Language.Some(p.WithField<TestObject, uint>("DstIp", 2)),
                        Language.If(
                            p.GetField<TestObject, uint>("DstIp") == 3,
                            Language.Some(p.WithField<TestObject, uint>("DstIp", 4)),
                            Language.Some(p)
                            ));
                });

                var zf1 = new ZenFunction<TestObject, bool>(p => f(p).HasValue());
                var zf2 = new ZenFunction<TestObject, TestObject>(p => f(p).Value().Unroll());

                // var set1 = zf1.Transformer().InputSet((i, o) => o);
                var t = zf2.Transformer();

                /* var rnd = new Random(0);
                var randoms = new HashSet<(uint, uint)>();
                for (int i = 0; i < 2000; i++)
                {
                    randoms.Add(((uint)rnd.Next(0, 65000), (uint)rnd.Next(0, 65000)));
                }

                var set2 = new ZenFunction<Packet, bool>(p =>
                {
                    var constraints = randoms.Select(x => Language.And(p.GetField<Packet, uint>("DstIp") == x.Item1, p.GetField<Packet, uint>("SrcIp") == x.Item2));
                    return Language.Or(constraints.ToArray());
                }).StateSet();

                var filtered = set1.Intersect(set2);
                var forward = t.TransformForward(filtered);

                t.TransformBackwards(set2); */
            });
        }

        private static void BenchmarkTransformerCache()
        {
            Benchmark(nameof(BenchmarkAllocation), 20000, () =>
            {
                var zf = new ZenFunction<uint, bool>(x => Language.And(x <= 90, x >= 30));
                zf.StateSet();
                zf.Transformer();
            });
        }

        private static void BenchmarkAllocation()
        {
            Benchmark(nameof(BenchmarkAllocation), 30, () =>
            {
                var b = new AclBench();
                b.Backend = ZenLib.ModelChecking.Backend.DecisionDiagrams;
                b.NumLines = 10000;
                b.CreateAcl();
                b.Acl.ProcessProvenance(Language.Arbitrary<ZenLib.Tests.Network.IpHeader>());
            });
        }

        private static void Benchmark(string name, int iterations, Action action)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }

            timer.Stop();
            Console.WriteLine($"[{name}]: Total: {timer.ElapsedMilliseconds}ms, Avg {timer.ElapsedMilliseconds / iterations}ms");
        }
    }

    /// <summary>
    ///     A test class.
    /// </summary>
    public class TestObject
    {
        /// <summary>
        ///     Gets or sets the destination IP address.
        /// </summary>
        public uint DstIp { get; set; }

        /// <summary>
        ///     Gets or sets the source IP address.
        /// </summary>
        public uint SrcIp { get; set; }
    }
}
