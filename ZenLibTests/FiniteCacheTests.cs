// <copyright file="FiniteCacheTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the finite cache implementation.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FiniteCacheTests
    {
        /// <summary>
        /// Test that insertion is working.
        /// </summary>
        [TestMethod]
        public void TestInsertion()
        {
            var cache = new FiniteCache<int, int>(4);

            cache.Add(1, 1);
            cache.Add(2, 2);
            cache.Add(3, 3);
            cache.Add(4, 4);

            Assert.IsTrue(cache.TryGetValue(1, out var _));
            Assert.IsTrue(cache.TryGetValue(2, out var _));
            Assert.IsTrue(cache.TryGetValue(3, out var _));
            Assert.IsTrue(cache.TryGetValue(4, out var _));

            cache.Add(5, 5);

            var count = 0;
            if (cache.TryGetValue(1, out var _))
                count++;
            if (cache.TryGetValue(2, out var _))
                count++;
            if (cache.TryGetValue(3, out var _))
                count++;
            if (cache.TryGetValue(4, out var _))
                count++;

            Assert.AreEqual(3, count);
            Assert.IsTrue(cache.TryGetValue(5, out var _));
        }

        /// <summary>
        /// Test that insertion is working.
        /// </summary>
        [TestMethod]
        public void TestInsertionDuplicates()
        {
            var cache = new FiniteCache<int, int>(1);

            cache.Add(1, 1);
            cache.TryGetValue(1, out var x);
            Assert.AreEqual(1, x);

            cache.Add(1, 2);
            cache.TryGetValue(1, out var y);
            Assert.AreEqual(2, y);

            cache.Add(3, 4);
            cache.TryGetValue(3, out var z);
            Assert.AreEqual(4, z);
            Assert.IsFalse(cache.TryGetValue(1, out var _));
        }

        /// <summary>
        /// Test that insertion is working.
        /// </summary>
        [TestMethod]
        public void TestInsertionMany()
        {
            var cache = new FiniteCache<int, int>(10);

            for (int i = 0; i < 1001; i++)
            {
                cache.Add(i, i);
            }
        }
    }
}