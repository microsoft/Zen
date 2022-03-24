// <copyright file="OptimizationTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// Test the Optimization implementation.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class OptimizationTests
    {
        /// <summary>
        /// Test that maximize works for BigInteger.
        /// </summary>
        [TestMethod]
        public void TestMaximizeBigInteger()
        {
            var a = Zen.Symbolic<BigInteger>();
            var solution = Zen.Maximize(a, a <= new BigInteger(10));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new BigInteger(10), solution.Get(a));
        }

        /// <summary>
        /// Test that Minimize works for BigInteger.
        /// </summary>
        [TestMethod]
        public void TestMinimizeBigInteger()
        {
            var a = Zen.Symbolic<BigInteger>();
            var solution = Zen.Minimize(a, a >= new BigInteger(10));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new BigInteger(10), solution.Get(a));
        }

        /// <summary>
        /// Test that maximize works for Bitvectors.
        /// </summary>
        [TestMethod]
        public void TestMaximizeBitvector()
        {
            var a = Zen.Symbolic<byte>();
            var solution = Zen.Maximize(a, a != 254);

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(255, solution.Get(a));
        }

        /// <summary>
        /// Test that minimize works for Bitvectors.
        /// </summary>
        [TestMethod]
        public void TestMinimizeBitvector()
        {
            var a = Zen.Symbolic<byte>();
            var solution = Zen.Minimize(a, a != 254);

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(0, solution.Get(a));
        }

        /// <summary>
        /// Test that maximize works for Reals.
        /// </summary>
        [TestMethod]
        public void TestMaximizeReal()
        {
            var a = Zen.Symbolic<Real>();
            var solution = Zen.Maximize(a, a <= new Real(3, 4));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new Real(3, 4), solution.Get(a));
        }

        /// <summary>
        /// Test that minimize works for Reals.
        /// </summary>
        [TestMethod]
        public void TestMinimizeReal1()
        {
            var a = Zen.Symbolic<Real>();
            var solution = Zen.Minimize(a, a >= new Real(3, 4));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new Real(3, 4), solution.Get(a));
        }

        /// <summary>
        /// Test that minimize works for Reals.
        /// </summary>
        [TestMethod]
        public void TestMinimizeReal2()
        {
            var a = Zen.Symbolic<Real>();
            var solution = Zen.Minimize(a, a > new Real(3, 4));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new Real(7, 4), solution.Get(a));
        }

        /// <summary>
        /// Test that maximize works when there is no upper bound.
        /// </summary>
        [TestMethod]
        public void TestMaximizeNoUpper()
        {
            var a = Zen.Symbolic<Real>();
            var solution = Zen.Maximize(a, True());

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new Real(0), solution.Get(a));
        }

        /// <summary>
        /// Test that maximize works with addition.
        /// </summary>
        [TestMethod]
        public void TestMaximizeAddition()
        {
            var a = Zen.Symbolic<Real>();
            var b = Zen.Symbolic<Real>();
            var solution = Zen.Maximize(a + b, Zen.And(a <= (Real)10, b <= (Real)10, a + (Real)4 <= b));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new Real(6), solution.Get(a));
            Assert.AreEqual(new Real(10), solution.Get(b));
        }

        /// <summary>
        /// Test that maximize works with subtraction.
        /// </summary>
        [TestMethod]
        public void TestMaximizeDifference()
        {
            var a = Zen.Symbolic<Real>();
            var b = Zen.Symbolic<Real>();
            var solution = Zen.Maximize(a - b, Zen.And(a <= (Real)10, b <= (Real)10, b >= (Real)0, a >= (Real)0));

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new Real(10), solution.Get(a));
            Assert.AreEqual(new Real(0), solution.Get(b));
        }

        /// <summary>
        /// Test that maximize works with if-then-else.
        /// </summary>
        [TestMethod]
        public void TestMaximizeIfThenElse()
        {
            var a = Zen.Symbolic<byte>();
            var b = Zen.Symbolic<byte>();
            var solution = Zen.Maximize(If(a < b, b - a, 200), True());

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(0, solution.Get(a));
            Assert.AreEqual(255, solution.Get(b));
        }

        /// <summary>
        /// Test that maximize works with subtraction.
        /// </summary>
        [TestMethod]
        public void TestMaxMinUnsat()
        {
            var a = Zen.Symbolic<byte>();
            var solution = Zen.Maximize(a, False());
            Assert.IsFalse(solution.IsSatisfiable());

            solution = Zen.Minimize(a, False());
            Assert.IsFalse(solution.IsSatisfiable());
        }
    }
}