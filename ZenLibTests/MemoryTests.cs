// <copyright file="MemoryTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for memory reclaimation.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MemoryTests
    {
        /// <summary>
        /// Test that we are able to reclaim memory after GC.
        /// </summary>
        [TestMethod]
        public void TestMemoryDecreasesAfterGc()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var x = Basic.Constant(i);
            }

            var totalMemory1 = GC.GetTotalMemory(true) / 1000 / 1000;
            Console.WriteLine($"Using: {totalMemory1} MB");

            for (int i = 1000000; i < 2000000; i++)
            {
                var x = Basic.Constant(i);
            }

            var totalMemory2 = GC.GetTotalMemory(true) / 1000 / 1000;
            Console.WriteLine($"Using: {totalMemory2} MB");

            Assert.IsTrue(Math.Abs(totalMemory2 - totalMemory1) < 10);
        }

        /// <summary>
        /// Test that we are able to reuse expressions across functions.
        /// </summary>
        [TestMethod]
        public void TestMemoryReuseForSimilarFunctions()
        {
            var zf = new ZenFunction<int, int>(x => CreateFunction(x, 0));

            var totalMemory1 = GC.GetTotalMemory(true) / 1000 / 1000;
            Console.WriteLine($"Using: {totalMemory1} MB");

            var zfs = new ZenFunction<int, int>[2000];

            for (int i = 0; i < 2000; i++)
            {
                zfs[i] = new ZenFunction<int, int>(x => CreateFunction(x, 0));
            }

            var totalMemory2 = GC.GetTotalMemory(true) / 1000 / 1000;
            Console.WriteLine($"Using: {totalMemory2} MB");

            Assert.IsTrue(Math.Abs(totalMemory2 - totalMemory1) < 5);
        }

        private Zen<int> CreateFunction(Zen<int> x, int i)
        {
            if (i == 100)
            {
                return 100;
            }

            return Basic.If(x == i, 100 - i, CreateFunction(x, i + 1));
        }
    }
}