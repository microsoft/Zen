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

            Assert.AreEqual(4, cache.Count);
            Assert.IsTrue(cache.TryGetValue(1, out var _));
            Assert.IsTrue(cache.TryGetValue(2, out var _));
            Assert.IsTrue(cache.TryGetValue(3, out var _));
            Assert.IsTrue(cache.TryGetValue(4, out var _));

            cache.Add(5, 5);
            Assert.AreEqual(4, cache.Count);

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
            Assert.AreEqual(1, cache.Count);

            cache.Add(3, 4);
            cache.TryGetValue(3, out var y);
            Assert.AreEqual(4, y);
            Assert.IsFalse(cache.TryGetValue(1, out var _));
            Assert.AreEqual(1, cache.Count);
        }

        /// <summary>
        /// Test that insertion is with 0 elements.
        /// </summary>
        [TestMethod]
        public void TestInsertionSizeZero()
        {
            var cache = new FiniteCache<int, int>(0);
            cache.Add(1, 1);
            Assert.IsFalse(cache.TryGetValue(1, out var _));
        }

        /// <summary>
        /// Test that insertion is with negative elements.
        /// </summary>
        [TestMethod]
        public void TestInsertionSizeNegative()
        {
            var cache = new FiniteCache<int, int>(-1);

            for (int i = 0; i < 1000; i++)
            {
                cache.Add(i, i);
            }

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(cache.TryGetValue(i, out var x));
                Assert.AreEqual(i, x);
            }
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

            Assert.AreEqual(10, cache.Count);
        }
    }
}