// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using ZenLib;

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            /* var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 50000; i++)
            {
                var zf = new ZenFunction<uint, bool>(x => Language.And(x <= 90, x >= 30));
                zf.StateSet();
            }
            Console.WriteLine($"Time: {timer.ElapsedMilliseconds}ms"); */

            /* var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 50; i++)
            {
                var b = new AclBench();
                b.Backend = ZenLib.ModelChecking.Backend.DecisionDiagrams;
                b.NumLines = 10000;
                b.CreateAcl();
                b.Acl.ProcessProvenance(Language.Arbitrary<ZenLib.Tests.Network.IpHeader>());
                // b.VerifyAclProvenance();
            }
            Console.WriteLine($"Time: {timer.ElapsedMilliseconds / 50}ms");
            Console.WriteLine($"Memory: {GC.GetTotalMemory(false) / 1000 / 1000}mb"); */

            var timer = System.Diagnostics.Stopwatch.StartNew();

            var zf = new ZenFunction<IList<byte>, IList<byte>>(Language.Sort);

            foreach (var input in zf.GenerateInputs(listSize: 3, checkSmallerLists: false))
            {
                Console.WriteLine(string.Join(",", input));
            }

            Console.WriteLine($"Time: {timer.ElapsedMilliseconds / 50}ms");
            Console.WriteLine($"Memory: {GC.GetTotalMemory(false) / 1000 / 1000}mb");
        }
    }
}
