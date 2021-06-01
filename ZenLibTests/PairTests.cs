// <copyright file="PairTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for the Zen pair type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PairTests
    {
        /// <summary>
        /// Test getting items from a pair.
        /// </summary>
        [TestMethod]
        public void TestPair2()
        {
            CheckValid<int, int>((i1, i2) => Pair(i1, i2).Item1() == i1);
            CheckValid<int, int>((i1, i2) => Pair(i1, i2).Item2() == i2);
        }

        /// <summary>
        /// Test getting items from a pair.
        /// </summary>
        [TestMethod]
        public void TestPair3()
        {
            CheckValid<int, int, int>((i1, i2, i3) => Pair(i1, i2, i3).Item1() == i1);
            CheckValid<int, int, int>((i1, i2, i3) => Pair(i1, i2, i3).Item2() == i2);
            CheckValid<int, int, int>((i1, i2, i3) => Pair(i1, i2, i3).Item3() == i3);
        }

        /// <summary>
        /// Test getting items from a pair.
        /// </summary>
        [TestMethod]
        public void TestPair4()
        {
            CheckValid<int, int, int, int>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4).Item1() == i1);
            CheckValid<int, int, int, int>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4).Item2() == i2);
            CheckValid<int, int, int, int>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4).Item3() == i3);
            CheckValid<int, int, int, int>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4).Item4() == i4);
        }

        /// <summary>
        /// Test getting items from a pair.
        /// </summary>
        [TestMethod]
        public void TestPair5()
        {
            CheckValid<int, int, int, Pair<int, int>>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4.Item1(), i4.Item2()).Item1() == i1);
            CheckValid<int, int, int, Pair<int, int>>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4.Item1(), i4.Item2()).Item2() == i2);
            CheckValid<int, int, int, Pair<int, int>>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4.Item1(), i4.Item2()).Item3() == i3);
            CheckValid<int, int, int, Pair<int, int>>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4.Item1(), i4.Item2()).Item4() == i4.Item1());
            CheckValid<int, int, int, Pair<int, int>>((i1, i2, i3, i4) => Pair(i1, i2, i3, i4.Item1(), i4.Item2()).Item5() == i4.Item2());
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
        public void TestPairSymbolicSwapNested()
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
        public void TestPairEqualComponents()
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

        /// <summary>
        /// Test creating and evaluating a concrete pair.
        /// </summary>
        [TestMethod]
        public void TestConcretePair()
        {
            Pair<int, int> p1 = (1, 2);
            Pair<int, int, int> p2 = (1, 2, 3);
            Pair<int, int, int, int> p3 = (1, 2, 3, 4);
            Pair<int, int, int, int, int> p4 = (1, 2, 3, 4, 5);

            Assert.AreEqual(1, p1.Item1);
            Assert.AreEqual(2, p1.Item2);

            Assert.AreEqual(1, p2.Item1);
            Assert.AreEqual(2, p2.Item2);
            Assert.AreEqual(3, p2.Item3);

            Assert.AreEqual(1, p3.Item1);
            Assert.AreEqual(2, p3.Item2);
            Assert.AreEqual(3, p3.Item3);
            Assert.AreEqual(4, p3.Item4);

            Assert.AreEqual(1, p4.Item1);
            Assert.AreEqual(2, p4.Item2);
            Assert.AreEqual(3, p4.Item3);
            Assert.AreEqual(4, p4.Item4);
            Assert.AreEqual(5, p4.Item5);

            Assert.AreEqual((object)p1, (object)p1);
            Assert.AreEqual((object)p2, (object)p2);
            Assert.AreEqual((object)p3, (object)p3);
            Assert.AreEqual((object)p4, (object)p4);

            Assert.AreNotEqual(p1, p2);
            Assert.AreNotEqual(p2, p3);
            Assert.AreNotEqual(p3, p4);
            Assert.AreNotEqual(p1, new object());
            Assert.AreNotEqual(p1, null);
            Assert.AreNotEqual(p2, new object());
            Assert.AreNotEqual(p2, null);
            Assert.AreNotEqual(p3, new object());
            Assert.AreNotEqual(p3, null);
            Assert.AreNotEqual(p4, new object());
            Assert.AreNotEqual(p4, null);

            Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
            Assert.AreNotEqual(p2.GetHashCode(), p3.GetHashCode());
            Assert.AreNotEqual(p3.GetHashCode(), p4.GetHashCode());
        }
    }
}
