﻿// <copyright file="FlyweightTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the hash cons table.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FlyweightTests
    {
        /// <summary>
        /// Test that Zen ids are working.
        /// </summary>
        [TestMethod]
        public void TestZenIds()
        {
            var x = Zen.Constant(1);
            var y = Zen.Constant(2);
            var z = x + x;

            Assert.AreEqual(y.Id, z.Id);
            Assert.AreNotEqual(x.Id, y.Id);
            Assert.AreNotEqual(x.Id, z.Id);
        }

        /// <summary>
        /// Test that insertion is working.
        /// </summary>
        [TestMethod]
        public void TestInsertion()
        {
            var ht = new Flyweight<int, object>();
            Check(ht);
        }

        /// <summary>
        /// Test that insertion works with garbage collection.
        /// </summary>
        [TestMethod]
        public void TestGarbageCollection()
        {
            var ht = new Flyweight<int, object>();
            Check(ht);
            GC.Collect();
            Check(ht);
        }

        /// <summary>
        /// Checks insertion into the hash cons table.
        /// </summary>
        /// <param name="ht">The table to use.</param>
        private void Check(Flyweight<int, object> ht)
        {
            Assert.IsTrue(ht.GetOrAdd(1, 1, (v) => v, out var _));
            Assert.IsTrue(ht.GetOrAdd(2, 2, (v) => v, out var _));
            Assert.IsFalse(ht.GetOrAdd(1, 1, (v) => v, out var _));
        }

        /// <summary>
        /// Test that insertion works with garbage collection.
        /// </summary>
        [TestMethod]
        public void TestInsertMany()
        {
            var ht = new Flyweight<int, object>();

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(ht.GetOrAdd(i, i, (v) => v, out var _));
            }
        }
    }
}