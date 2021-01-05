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
                var x = Language.Constant(i);
            }

            var totalMemory1 = GC.GetTotalMemory(true) / 1000 / 1000;
            Console.WriteLine($"Using: {totalMemory1} MB");

            for (int i = 1000000; i < 2000000; i++)
            {
                var x = Language.Constant(i);
            }

            var totalMemory2 = GC.GetTotalMemory(true) / 1000 / 1000;
            Console.WriteLine($"Using: {totalMemory2} MB");

            Assert.IsTrue(Math.Abs(totalMemory2 - totalMemory1) < 5);
        }
    }
}