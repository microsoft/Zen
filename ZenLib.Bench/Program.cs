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

            var ts = new TransitionSystem<uint>
            {
                InitialStates = (s) => s <= 100,
                Invariants = (s) => true,
                NextRelation = (sOld, sNew) => sNew == sOld + 1,
                Specification = LTL.Always(LTL.Predicate<uint>(s => s < 105)),
            };

            var searchResults = ts.ModelCheck(2000).ToArray();
            Console.WriteLine(string.Join(",", searchResults.Last().CounterExample));

            // ZenBench.BenchmarkSets();
            // ZenBench.BenchmarkComparisons();
            // ZenBench.BenchmarkTransformers();
            // ZenBench.BenchmarkTransformerCache();
            // ZenBench.BenchmarkAllocation();
        }
    }
}
