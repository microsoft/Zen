// <copyright file="ExtensionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// Test the extensions APIs.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExtensionTests
    {
        /// <summary>
        /// Test that solve works as expected for booleans.
        /// </summary>
        [TestMethod]
        public void TestSolveBooleans()
        {
            var a = Symbolic<bool>();
            var b = Symbolic<bool>();
            var c = Symbolic<bool>();

            var expr = Or(a, And(b, c));
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(solution.Get(a) || (solution.Get(b) && solution.Get(c)));
        }

        /// <summary>
        /// Test that solve works for unsat.
        /// </summary>
        [TestMethod]
        public void TestSolveUnsat()
        {
            var a = Symbolic<bool>();
            var expr = And(a, Not(a));
            var solution = expr.Solve();

            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test that solve works as expected for bitvectors.
        /// </summary>
        [TestMethod]
        public void TestSolveBitvectors()
        {
            var a = Arbitrary<byte>();
            var b = Arbitrary<short>();
            var c = Arbitrary<ushort>();
            var d = Arbitrary<int>();
            var e = Arbitrary<uint>();
            var f = Arbitrary<long>();
            var g = Arbitrary<ulong>();

            var expr = And(a == 1, b == 2, c == 3, d == 4, e == 5, f == 6, g == 7);
            var solution = expr.Solve();

            Assert.AreEqual((byte)1, solution.Get(a));
            Assert.AreEqual((short)2, solution.Get(b));
            Assert.AreEqual((ushort)3, solution.Get(c));
            Assert.AreEqual((int)4, solution.Get(d));
            Assert.AreEqual(5U, solution.Get(e));
            Assert.AreEqual(6L, solution.Get(f));
            Assert.AreEqual(7UL, solution.Get(g));
        }

        /// <summary>
        /// Test that solve works as expected for unbounded integers.
        /// </summary>
        [TestMethod]
        public void TestSolveIntegers()
        {
            var a = Arbitrary<BigInteger>();
            var b = Arbitrary<BigInteger>();

            var expr = And(a == new BigInteger(1), b == new BigInteger(2));
            var solution = expr.Solve();

            Assert.AreEqual(new BigInteger(1), solution.Get(a));
            Assert.AreEqual(new BigInteger(2), solution.Get(b));
        }

        /// <summary>
        /// Test that solve works as expected for lists.
        /// </summary>
        [TestMethod]
        public void TestSolveLists()
        {
            var a = Arbitrary<FSeq<int>>();
            var b = Arbitrary<FSeq<int>>();

            var expr = And(a == FSeq.FromRange(new List<int> { 1, 2 }), b == FSeq.FromRange(new List<int> { 3 }));
            var solution = expr.Solve();

            var asol = solution.Get(a);
            var bsol = solution.Get(b);

            Assert.AreEqual(2, asol.Count());
            Assert.AreEqual(1, bsol.Count());

            Assert.AreEqual(1, asol.Values[0]);
            Assert.AreEqual(2, asol.Values[1]);
            Assert.AreEqual(3, bsol.Values[0]);
        }

        /// <summary>
        /// Test that solve works as expected for options and objects.
        /// </summary>
        [TestMethod]
        public void TestSolveOptions()
        {
            var a = Arbitrary<Option<int>>();
            var b = Arbitrary<Option<int>>();
            var expr = And(Not(a.IsSome()), b == Option.Some(1));
            var solution = expr.Solve();

            Assert.IsTrue(!solution.Get(a).HasValue);
            Assert.IsTrue(solution.Get(b).HasValue);
            Assert.AreEqual(1, solution.Get(b).Value);
        }

        /// <summary>
        /// Test that solve works as expected for fixed integers.
        /// </summary>
        [TestMethod]
        public void TestSolveFixedIntegers()
        {
            var a = Arbitrary<UInt128>();
            var b = Arbitrary<Int128>();
            var expr = And(a == new UInt128(1), b == new Int128(-3));
            var solution = expr.Solve();

            Assert.AreEqual(new UInt128(1), solution.Get(a));
            Assert.AreEqual(new Int128(-3), solution.Get(b));
        }

        /// <summary>
        /// Test that solve works as expected for fixed integers.
        /// </summary>
        [TestMethod]
        public void TestSolvePairs()
        {
            var a = Arbitrary<Pair<bool, int>>();
            var expr = And(a.Item1(), a.Item2() == 3);
            var solution = expr.Solve();

            Assert.AreEqual(true, solution.Get(a).Item1);
            Assert.AreEqual(3, solution.Get(a).Item2);
        }

        /// <summary>
        /// Test that solve works as expected for strings.
        /// </summary>
        [TestMethod]
        public void TestSolveStrings()
        {
            var a = Arbitrary<string>();
            var b = Arbitrary<string>();
            var expr = And(a == "hello", b.Contains(a), a != b);

            var solution = expr.Solve();

            Assert.AreEqual("hello", solution.Get(a));
            Assert.IsTrue(solution.Get(b).Contains("hello"));
        }

        /// <summary>
        /// Test that evaluate works as expected for booleans.
        /// </summary>
        [TestMethod]
        public void TestEvaluateBooleans()
        {
            var a = Arbitrary<bool>();
            var b = Arbitrary<bool>();
            var c = Arbitrary<bool>();
            var expr = Or(a, And(b, c));

            var assignment1 = new Dictionary<object, object>
            {
                { a, false },
                { b, true },
                { c, false },
            };

            var assignment2 = new Dictionary<object, object>
            {
                { a, false },
                { b, true },
                { c, true },
            };

            Assert.AreEqual(false, expr.Evaluate(assignment1));
            Assert.AreEqual(true, expr.Evaluate(assignment2));
        }

        /// <summary>
        /// Test that evaluate works as expected for booleans.
        /// </summary>
        [TestMethod]
        public void TestEvaluateList()
        {
            var a = Arbitrary<FSeq<int>>();
            var expr = a.Contains(3);

            var assignment = new Dictionary<object, object>
            {
                { a, new FSeq<int> { Values = ImmutableList.CreateRange(new List<int> { 3, 2, 1 }) } },
            };

            var l = expr.Evaluate(assignment);

            Assert.IsTrue(l);
        }

        /// <summary>
        /// Test that evaluate works with unassigned variables.
        /// </summary>
        [TestMethod]
        public void TestEvaluateMissingVariables()
        {
            try
            {
                var a = Arbitrary<bool>();
                var b = Arbitrary<bool>();
                var c = Arbitrary<bool>();
                var expr = Or(a, And(b, c));

                var assignment = new Dictionary<object, object>
                {
                    { a, false },
                    { b, true },
                };

                expr.Evaluate(assignment);
            }
            catch
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// Test that evaluate works with unassigned variables.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEvaluateWrongTypes1()
        {
            var a = Arbitrary<bool>();
            var b = Arbitrary<bool>();
            var c = Arbitrary<bool>();
            var expr = Or(a, And(b, c));

            var assignment = new Dictionary<object, object>
            {
                { a, 1 },
            };

            expr.Evaluate(assignment);
        }

        /// <summary>
        /// Test that evaluate works with unassigned variables.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEvaluateWrongTypes2()
        {
            var a = Arbitrary<bool>();
            var b = Arbitrary<bool>();
            var c = Arbitrary<bool>();
            var expr = Or(a, And(b, c));

            var assignment = new Dictionary<object, object>
            {
                { 1, a },
            };

            expr.Evaluate(assignment);
        }

        /// <summary>
        /// Test that evaluate works with unassigned variables.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEvaluateWrongTypes3()
        {
            var a = Arbitrary<FSeq<int>>();
            var expr = a.Select(x => x + 1);

            var assignment = new Dictionary<object, object>
            {
                { a, new LinkedList<int>(new List<int> { 3, 2, 1 }) },
            };

            expr.Evaluate(assignment);
        }
    }
}