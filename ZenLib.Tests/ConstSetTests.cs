// <copyright file="ConstSetTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for the Zen const set type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConstSetTests
    {
        /// <summary>
        /// Test set evaluation.
        /// </summary>
        [TestMethod]
        public void TestConstSetEvaluate()
        {
            var zf = new ZenFunction<ConstSet<int>, ConstSet<int>>(d => d.Add(1).Delete(2));

            var d = zf.Evaluate(new ConstSet<int>(2, 3));
            Assert.AreEqual(2, d.Count());
            Assert.IsTrue(d.Contains(1));
            Assert.IsTrue(d.Contains(3));

            d = zf.Evaluate(new ConstSet<int>(2));
            Assert.AreEqual(1, d.Count());
            Assert.IsTrue(d.Contains(1));

            zf.Compile();
            d = zf.Evaluate(new ConstSet<int>(2, 3));
            Assert.AreEqual(2, d.Count());
            Assert.IsTrue(d.Contains(1));
            Assert.IsTrue(d.Contains(3));

            d = zf.Evaluate(new ConstSet<int>(2));
            Assert.AreEqual(1, d.Count());
            Assert.IsTrue(d.Contains(1));
        }

        /// <summary>
        /// Test set implementation.
        /// </summary>
        [TestMethod]
        public void TestConstSetImplementation()
        {
            Assert.IsFalse(new ConstSet<int>().Contains(1));
            Assert.IsTrue(new ConstSet<int>(1).Contains(1));
            Assert.IsTrue(new ConstSet<int>(1, 2).Contains(1));
            Assert.IsFalse(new ConstSet<int>(1, 2).Contains(3));
            Assert.IsTrue(new ConstSet<int>(1).Add(2).Contains(2));
            Assert.IsFalse(new ConstSet<int>(1).Delete(1).Contains(2));
            Assert.IsFalse(new ConstSet<int>(1).Delete(1).Contains(1));
            Assert.IsTrue(new ConstSet<int>(1) == new ConstSet<int>().Add(1));
            Assert.IsTrue(new ConstSet<int>(1).Equals((object)new ConstSet<int>().Add(1)));
            Assert.IsTrue(new ConstSet<int>(1).GetHashCode() == new ConstSet<int>().Add(1).GetHashCode());
            Assert.IsTrue(new ConstSet<int>(1) != new ConstSet<int>().Add(2));
            Assert.IsFalse(new ConstSet<int>().Equals(new object()));
        }

        /// <summary>
        /// Test sets work with solve.
        /// </summary>
        [TestMethod]
        public void TestConstSetSolve()
        {
            var x = Zen.Symbolic<ConstSet<int>>();
            var solution = x.Contains(3).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Contains(3));
        }

        /// <summary>
        /// Test sets work with solve.
        /// </summary>
        [TestMethod]
        public void TestConstSetAddDelete()
        {
            var x = Zen.Symbolic<ConstSet<int>>();
            var y = Zen.Symbolic<ConstSet<int>>();
            var solution = Zen.And(x.Add(1).Add(2) == y, x.Contains(3)).Solve();
            var resx = solution.Get(x);
            var resy = solution.Get(y);
            Assert.IsTrue(resx.Contains(3));
            Assert.IsTrue(resy.Contains(1));
            Assert.IsTrue(resy.Contains(2));
            Assert.IsTrue(resy.Contains(3));
        }
    }
}
