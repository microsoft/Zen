// <copyright file="TupleTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

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
            CheckValid<int, int>((i1, i2) => Pair(i1, i2).Item1() == i1);
            CheckValid<int, int>((i1, i2) => Pair(i1, i2).Item2() == i2);
        }

        /// <summary>
        /// Test getting items from a value tuple.
        /// </summary>
        [TestMethod]
        public void TestValueTuple()
        {
            CheckValid<int, int>((i1, i2) => Pair(i1, i2).Item1() == i1);
            CheckValid<int, int>((i1, i2) => Pair(i1, i2).Item2() == i2);
        }

        /// <summary>
        /// Test evaluating a tuple swap.
        /// </summary>
        [TestMethod]
        public void TestTupleEvaluateSwap()
        {
            var f = new ZenFunction<int, int, Pair<int, int>>((x, y) => Pair(y, x));
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
            var f = new ZenFunction<int, int, Pair<int, int>>((x, y) => Pair(y, x));
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
            var f = new ZenFunction<Pair<int, int>, Pair<int, int>>(x => Pair(x.Item2(), x.Item1()));
            var r = f.Evaluate(new Pair<int, int> { Item1 = 1, Item2 = 2 });
            Assert.AreEqual(r.Item1, 2);
            Assert.AreEqual(r.Item2, 1);
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTupleSymbolicSwap()
        {
            var f = new ZenFunction<Pair<int, int>, Pair<int, int>>(x => Pair(x.Item2(), x.Item1()));
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
            var f = new ZenFunction<Pair<int, Pair<int, int>>, Pair<int, int>>(x => Pair(x.Item2().Item2(), x.Item2().Item1()));
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
            CheckAgreement<Pair<byte, byte>>(x => x.Item2() == x.Item1());
        }

        /// <summary>
        /// Test evaluating a value tuple swap.
        /// </summary>
        [TestMethod]
        public void TestValueTypeNull()
        {
            var f = new ZenFunction<Option<Pair<int, int>>>(() => Null<Pair<int, int>>());
            Assert.AreEqual(f.Evaluate(), Option.None<Pair<int, int>>());
        }
    }
}
