﻿// <copyright file="InterleavingHeuristicTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.ModelChecking;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the interleaving heuristic.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class InterleavingHeuristicTests
    {
        /// <summary>
        /// Test that the heuristic works with a simple equality.
        /// </summary>
        [TestMethod]
        public void TestSimpleEquality()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var expr = (a == b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works with a simple arithmetic.
        /// </summary>
        [TestMethod]
        public void TestSimpleArithmeticPlus()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var expr = (a + b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works with a simple arithmetic.
        /// </summary>
        [TestMethod]
        public void TestSimpleArithmeticMinus()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var expr = (a - b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works with a simple arithmetic.
        /// </summary>
        [TestMethod]
        public void TestSimpleArithmeticXor()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var expr = (a ^ b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works with a simple arithmetic.
        /// </summary>
        [TestMethod]
        public void TestCharInterleaving()
        {
            var a = Arbitrary<char>();
            var b = Arbitrary<char>();
            var expr = (a == b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works with a simple arithmetic.
        /// </summary>
        [TestMethod]
        public void TestSimpleArithmeticBand()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var expr = (a & b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works for unrelated variables.
        /// </summary>
        [TestMethod]
        public void TestUnrelatedVariables()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var c = Arbitrary<ushort>();
            var expr = And(a == 1, Or(b == 2, c == 3));
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(3, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(a));
            Assert.IsTrue(disjointSets[1].Contains(b));
            Assert.IsTrue(disjointSets[2].Contains(c));
        }

        /// <summary>
        /// Test that the heuristic works for if conditions.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithIfCondition1()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var c = Arbitrary<ushort>();
            var expr = If(c == 2, And(a == b), Not(a == 3));
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);

            Assert.AreEqual(2, disjointSets.Count);
            Assert.IsTrue(disjointSets[1].Contains(a));
            Assert.IsTrue(disjointSets[1].Contains(b));
            Assert.IsTrue(disjointSets[0].Contains(c));
        }

        /// <summary>
        /// Test that the heuristic works for if conditions.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithIfCondition2()
        {
            var a = Arbitrary<int>();
            var b = Arbitrary<int>();
            var c = Arbitrary<int>();
            var expr = If(c == 2, a, b) == If(c == 3, b, a);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(2, disjointSets.Count);
            Assert.IsTrue(disjointSets[1].Contains(a));
            Assert.IsTrue(disjointSets[1].Contains(b));
            Assert.IsTrue(disjointSets[0].Contains(c));
        }

        /// <summary>
        /// Test that the heuristic works for if conditions.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithBooleans()
        {
            var a = Arbitrary<bool>();
            var b = Arbitrary<int>();
            var expr = (b == 3) == a;
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);

            Assert.AreEqual(2, disjointSets.Count);
            Assert.IsTrue(disjointSets[1].Contains(a));
            Assert.IsTrue(disjointSets[0].Contains(b));
        }

        /// <summary>
        /// Test that the heuristic works for if conditions.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithConstants()
        {
            var c = Arbitrary<char>();
            var expr = (c == 'a');
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);

            Assert.AreEqual(1, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Contains(c));
        }

        /// <summary>
        /// Test that the heuristic works for if conditions.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithObjects1()
        {
            var a = Arbitrary<Object2>();
            var b = Arbitrary<Object2>();
            var expr = a == b;
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);

            Assert.AreEqual(2, disjointSets.Count);

            // each field should be equal.
            foreach (var disjointSet in disjointSets)
            {
                Assert.AreEqual(2, disjointSet.Count);
            }
        }

        /// <summary>
        /// Test that the heuristic works for if conditions.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithObjects2()
        {
            var a = Arbitrary<Object2>();
            var b = Arbitrary<Object2>();
            var c = Arbitrary<bool>();
            var d = Arbitrary<int>();
            var expr = If(c, a, b).GetField<Object2, int>("Field2") == d;
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);
            Assert.AreEqual(4, disjointSets.Count);
            Assert.AreEqual(3, disjointSets[2].Count);
            Assert.IsTrue(disjointSets[2].Contains(d));
        }

        /// <summary>
        /// Test that the heuristic works for objects.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithObjects3()
        {
            var a = Arbitrary<TestHelper.Object2>();
            var b = Arbitrary<TestHelper.Object2>();
            var expr = (a == b);
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);

            Assert.AreEqual(2, disjointSets.Count);
            Assert.IsTrue(disjointSets[0].Count == 2);
            Assert.IsTrue(disjointSets[1].Count == 2);
        }

        /// <summary>
        /// Test that the heuristic works for objects.
        /// </summary>
        [TestMethod]
        public void TestHeuristicWithObjects4()
        {
            var a = Arbitrary<Pair<int, Object2>>();
            var b = Arbitrary<Pair<int, Object2>>();
            var g = Arbitrary<bool>();
            var expr = Zen.If(g, a, b) == new Pair<int, Object2>(1, new Object2 { Field1 = 0, Field2 = 1 });
            var i = new InterleavingHeuristicVisitor();
            var disjointSets = i.GetInterleavedVariables(expr, ImmutableDictionary<long, object>.Empty);

            Assert.AreEqual(7, disjointSets.Count);
        }
    }
}
