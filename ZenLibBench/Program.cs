// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using BenchmarkDotNet.Running;
    using ZenLib;
    using ZenLib.Tests;
    using ZenLib.Tests.Network;

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var zf = new ZenFunction<int, int, bool>((x, y) => 
            {
                return Language.And(x <= 4, Language.Arbitrary<int>() <= 5);
            });


            var w = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var packetSet = zf.Transformer().InputSet((i, o) => o);
                var elt = packetSet.Element();
            }

            Console.WriteLine($"Time was: {w.ElapsedMilliseconds}");

            /* for (int i = 0; i < 1000; i++)
            {
                var b = new AclBench();
                b.Backend = ZenLib.ModelChecking.Backend.DecisionDiagrams;
                b.NumLines = 100;
                b.CreateAcl();
                b.VerifyAclProvenance();
            } */

            // _ = BenchmarkRunner.Run<AclBench>();
            // _ = BenchmarkRunner.Run<RouteBench>();
            
            /* var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var input in PfcModel.GenerateTests().Take(10))
            {
            }

            Console.WriteLine($"Time: {watch.ElapsedMilliseconds}"); */
        }
    }
}
