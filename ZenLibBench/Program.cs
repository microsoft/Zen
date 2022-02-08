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
                    return Basic.If(
                        p.GetField<TestObject, uint>("DstIp") == 1,
                        Option.Create(p.WithField<TestObject, uint>("DstIp", 2)),
                        Basic.If(
                            p.GetField<TestObject, uint>("DstIp") == 3,
                            Option.Create(p.WithField<TestObject, uint>("DstIp", 4)),
                            Option.Create(p)
                            ));
                });

                var zf1 = new ZenFunction<TestObject, bool>(p => f(p).HasValue());
                var zf2 = new ZenFunction<TestObject, TestObject>(p => f(p).Value().Unroll());
                var full = new ZenFunction<TestObject, bool>(x => true).StateSet();

                var rnd = new Random(0);
                var randoms = new HashSet<(uint, uint)>();
                for (int i = 0; i < 400; i++)
                {
                    randoms.Add(((uint)rnd.Next(0, 65000), (uint)rnd.Next(0, 65000)));
                }

                var largeSet = new ZenFunction<TestObject, bool>(p =>
                {
                    var constraints = randoms.Select(x => Basic.And(p.GetField<TestObject, uint>("DstIp") == x.Item1, p.GetField<TestObject, uint>("SrcIp") == x.Item2));
                    return Basic.Or(constraints.ToArray());
                }).StateSet();

                var t = zf2.Transformer();

                t.TransformForward(largeSet);
                t.TransformBackwards(largeSet);
            });
        }

        private static void BenchmarkTransformerCache()
        {
            Benchmark(nameof(BenchmarkAllocation), 20000, () =>
            {
                var zf = new ZenFunction<uint, bool>(x => Basic.And(x <= 90, x >= 30));
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
                b.Acl.ProcessProvenance(Basic.Arbitrary<ZenLib.Tests.Network.IpHeader>());
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

        /// <summary>
        /// Foo field.
        /// </summary>
        public uint Foo { get; set; }

        /// <summary>
        /// Bar field.
        /// </summary>
        public uint Bar { get; set; }

        /// <summary>
        /// Baz field.
        /// </summary>
        public uint Baz { get; set; }
    }
}
