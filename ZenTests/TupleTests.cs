// <copyright file="TupleTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zen;
    using static Zen.Language;
    using static Zen.Tests.TestHelper;

    /// <summary>
    /// Tests for the Zen option type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TupleTests
    {
        /// <summary>
        /// Test getting items from a tuple.
        /// </summary>
        [TestMethod]
        public void TestTuple()
        {
            CheckValid<int, int>((i1, i2) => Tuple(i1, i2).Item1() == i1);
            CheckValid<int, int>((i1, i2) => Tuple(i1, i2).Item2() == i2);
        }

        /// <summary>
        /// Test getting items from a value tuple.
        /// </summary>
        [TestMethod]
        public void TestValueTuple()
        {
            CheckValid<int, int>((i1, i2) => ValueTuple(i1, i2).Item1() == i1);
            CheckValid<int, int>((i1, i2) => ValueTuple(i1, i2).Item2() == i2);
        }

        /// <summary>
        /// Test evaluating a tuple swap.
        /// </summary>
        [TestMethod]
        public void TestTupleEvaluateSwap()
        {
            var f = Function<int, int, Tuple<int, int>>((x, y) => Tuple(y, x));
            var r = f.Evaluate(1, 2);
            Assert.AreEqual(r.Item1, 2);
            Assert.AreEqual(r.Item2, 1);
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTupleEvaluateSwap()
        {
            var f = Function<int, int, (int, int)>((x, y) => ValueTuple(y, x));
            var r = f.Evaluate(1, 2);
            Assert.AreEqual(r.Item1, 2);
            Assert.AreEqual(r.Item2, 1);
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTupleEvaluateSwap2()
        {
            var f = Function<(int, int), (int, int)>(x => ValueTuple(x.Item2(), x.Item1()));
            var r = f.Evaluate((1, 2));
            Assert.AreEqual(r.Item1, 2);
            Assert.AreEqual(r.Item2, 1);
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTupleSymbolicSwap()
        {
            var f = Function<(int, int), (int, int)>(x => ValueTuple(x.Item2(), x.Item1()));
            var result = f.Find((x, o) => o.Item1() == 2);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(2, result.Value.Item2);
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTupleSymbolicSwapNested()
        {
            var f = Function<Tuple<int, (int, int)>, (int, int)>(x => ValueTuple(x.Item2().Item2(), x.Item2().Item1()));
            var result = f.Find((x, o) => o.Item1() == 2);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(2, result.Value.Item2.Item2);
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTupleEqualComponents()
        {
            CheckAgreement<(byte, byte)>(x => x.Item2() == x.Item1());
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTypeNull()
        {
            var f = Function<Option<(int, int)>>(() => Null<(int, int)>());
            Assert.AreEqual(f.Evaluate(false), Option.None<(int, int)>());
        }
    }
}
