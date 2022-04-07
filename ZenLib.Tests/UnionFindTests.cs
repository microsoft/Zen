// <copyright file="UnionFindTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the union find implementation.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UnionFindTests
    {
        /// <summary>
        /// Test that basic add operations are working.
        /// </summary>
        [TestMethod]
        public void TestUnionFindAdd()
        {
            var uf = new UnionFind<int>();
            uf.Add(1);
            uf.Add(2);
            uf.Add(3);

            var sets = uf.GetDisjointSets();
            Assert.AreEqual(3, sets.Count);
            Assert.AreEqual(1, sets[0][0]);
            Assert.AreEqual(2, sets[1][0]);
            Assert.AreEqual(3, sets[2][0]);
        }

        /// <summary>
        /// Test that add does not insert duplicates.
        /// </summary>
        [TestMethod]
        public void TestUnionFindAddDuplicates()
        {
            var uf = new UnionFind<int>();
            uf.Add(1);
            uf.Add(2);
            uf.Add(3);
            uf.Add(3);
            uf.Add(1);

            var sets = uf.GetDisjointSets();
            Assert.AreEqual(3, sets.Count);
            Assert.AreEqual(1, sets[0][0]);
            Assert.AreEqual(2, sets[1][0]);
            Assert.AreEqual(3, sets[2][0]);
        }

        /// <summary>
        /// Test that basic add operations are working.
        /// </summary>
        [TestMethod]
        public void TestUnionFindUnion()
        {
            var uf = new UnionFind<int>();
            uf.Add(1);
            uf.Add(2);
            uf.Add(3);
            uf.Add(4);

            uf.Union(1, 3);
            Assert.AreEqual(uf.Find(1), uf.Find(3));
            Assert.AreNotEqual(uf.Find(1), uf.Find(2));
            Assert.AreNotEqual(uf.Find(1), uf.Find(4));
            Assert.AreNotEqual(uf.Find(2), uf.Find(4));

            var sets = uf.GetDisjointSets();
            Assert.AreEqual(3, sets.Count);
            Assert.AreEqual(1, sets[0][0]);
            Assert.AreEqual(3, sets[0][1]);
            Assert.AreEqual(2, sets[1][0]);
            Assert.AreEqual(4, sets[2][0]);
        }

        /// <summary>
        /// Test that basic find operations are working.
        /// </summary>
        [TestMethod]
        public void TestUnionFindFindWorks()
        {
            var uf = new UnionFind<int>();
            uf.Add(1);
            uf.Add(2);
            uf.Add(3);

            uf.Find(1);
            uf.Find(2);
            uf.Find(3);
        }

        /// <summary>
        /// Test that find throws an exception for non-existent items.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUnionFindFindException()
        {
            var uf = new UnionFind<int>();
            uf.Add(1);
            uf.Add(2);
            uf.Add(3);
            uf.Find(4);
        }

        /// <summary>
        /// Test that unions work correctly.
        /// </summary>
        [TestMethod]
        public void TestUnionFindManyUnions()
        {
            var uf = new UnionFind<int>();
            uf.Add(1);
            uf.Add(2);
            uf.Add(3);
            uf.Add(4);
            uf.Add(5);
            uf.Add(6);

            uf.Union(1, 2);
            uf.Union(2, 3);

            var sets = uf.GetDisjointSets();
            Assert.AreEqual(4, sets.Count);
            Assert.AreEqual(1, sets[0][0]);
            Assert.AreEqual(2, sets[0][1]);
            Assert.AreEqual(3, sets[0][2]);

            uf.Union(4, 5);
            uf.Union(5, 6);

            sets = uf.GetDisjointSets();
            Assert.AreEqual(2, sets.Count);
            Assert.AreEqual(1, sets[0][0]);
            Assert.AreEqual(2, sets[0][1]);
            Assert.AreEqual(3, sets[0][2]);
            Assert.AreEqual(4, sets[1][0]);
            Assert.AreEqual(5, sets[1][1]);
            Assert.AreEqual(6, sets[1][2]);

            uf.Union(1, 6);
            sets = uf.GetDisjointSets();
            Assert.AreEqual(1, sets.Count);
        }
    }
}