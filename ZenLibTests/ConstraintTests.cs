// <copyright file="ConstraintTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Language;

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
    }
}