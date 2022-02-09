// <copyright file="ConstraintTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// Test the ZenConstraint implementation.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConstraintTests
    {
        /// <summary>
        /// Test that find works for constraints.
        /// </summary>
        [TestMethod]
        public void TestFind()
        {
            var a = new ZenConstraint<int>(x => x == 11).Find();
            var b = new ZenConstraint<int, int>((x, y) => And(x == 11, y == 11)).Find();
            var c = new ZenConstraint<int, int, int>((x, y, z) => And(x == 11, y == 11, z == 11)).Find();
            var d = new ZenConstraint<int, int, int, int>((w, x, y, z) => And(w == 11, x == 11, y == 11, z == 11)).Find();

            Assert.AreEqual(11, a.Value);
            Assert.AreEqual(11, b.Value.Item1);
            Assert.AreEqual(11, b.Value.Item2);
            Assert.AreEqual(11, c.Value.Item1);
            Assert.AreEqual(11, c.Value.Item2);
            Assert.AreEqual(11, c.Value.Item3);
            Assert.AreEqual(11, d.Value.Item1);
            Assert.AreEqual(11, d.Value.Item2);
            Assert.AreEqual(11, d.Value.Item3);
            Assert.AreEqual(11, d.Value.Item4);
        }

        /// <summary>
        /// Test that find works for constraints.
        /// </summary>
        [TestMethod]
        public void TestFindAll()
        {
            Assert.AreEqual(1, new ZenConstraint<int>(x => x == 11).FindAll().Count());
            Assert.AreEqual(1, new ZenConstraint<int, int>((x, y) => And(x == 11, y == 11)).FindAll().Count());
            Assert.AreEqual(1, new ZenConstraint<int, int, int>((x, y, z) => And(x == 11, y == 11, z == 11)).FindAll().Count());
            Assert.AreEqual(1, new ZenConstraint<int, int, int, int>((w, x, y, z) => And(w == 11, x == 11, y == 11, z == 11)).FindAll().Count());
        }

        /// <summary>
        /// Test that find works for constraints.
        /// </summary>
        [TestMethod]
        public void TestStateSet1()
        {
            var c = new ZenConstraint<int>(i => And(i < 10, i >= 1));
            var set = c.StateSet();
            Assert.IsFalse(set.IsEmpty());
            Assert.IsFalse(set.IsFull());
            Assert.IsTrue(set.Element() < 10 && set.Element() >= 1);
        }

        /// <summary>
        /// Test that transformers work with multiple inputs.
        /// </summary>
        [TestMethod]
        public void TestStateSet2()
        {
            var c1 = new ZenConstraint<uint>(x => x == 1);
            var c2 = new ZenConstraint<uint, uint>((x, y) => And(x == 1, y == 1));
            var c3 = new ZenConstraint<uint, uint, uint>((x, y, z) => And(x == 1, y == 1, z == 1));
            var c4 = new ZenConstraint<uint, uint, uint, uint>((w, x, y, z) => And(w == 1, x == 1, y == 1, z == 1));

            var v1 = c1.StateSet().Element();
            var v2 = c2.StateSet().Element();
            var v3 = c3.StateSet().Element();
            var v4 = c4.StateSet().Element();

            Assert.AreEqual(1U, v1);
            Assert.AreEqual(1U, v2.Item1);
            Assert.AreEqual(1U, v2.Item2);
            Assert.AreEqual(1U, v3.Item1);
            Assert.AreEqual(1U, v3.Item2);
            Assert.AreEqual(1U, v3.Item3);
            Assert.AreEqual(1U, v4.Item1);
            Assert.AreEqual(1U, v4.Item2);
            Assert.AreEqual(1U, v4.Item3);
            Assert.AreEqual(1U, v4.Item4);
        }
    }
}