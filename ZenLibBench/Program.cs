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

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 50000; i++)
            {
                // zf.Transformer().InputSet((i, o) => o);
                var zf = new ZenFunction<uint, bool>(x => Language.And(x <= 90, x >= 30));
                zf.StateSet();
            }
            /* for (int i = 0; i < 32; i++)
            {
                var b = new AclBench();
                b.Backend = ZenLib.ModelChecking.Backend.DecisionDiagrams;
                b.NumLines = 2000;
                b.CreateAcl();
                b.VerifyAclProvenance();
            } */
            Console.WriteLine($"Time: {timer.ElapsedMilliseconds}ms");

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
