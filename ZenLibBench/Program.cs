// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using BenchmarkDotNet.Running;

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<AclBench>();
            var ab = new AclBench();
            ab.CreateAcl();

            for (int i = 0; i < 50; i++)
            {
                var w = System.Diagnostics.Stopwatch.StartNew();
                ab.VerifyAcl();
                System.Console.WriteLine($"execute time: {w.ElapsedMilliseconds}ms");
            }
        }
    }
}
