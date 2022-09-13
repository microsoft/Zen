// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Linq;
    using ZenLib;
    using ZenLib.TransitionSystem;
 
    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line args.</param>
        static void Main(string[] args)
        {
            ZenSettings.UseLargeStack = true;

            // ZenBench.BenchmarkSets();
            // ZenBench.BenchmarkComparisons();
            // ZenBench.BenchmarkTransformers();
            // ZenBench.BenchmarkTransformerCache();
            // ZenBench.BenchmarkAllocation();
        }
    }
}
