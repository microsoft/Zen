// <copyright file="ExtensionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Basic;

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
            var a = Arbitrary<IList<int>>();
            var b = Arbitrary<IList<int>>();

            var expr = And(a == new List<int> { 1, 2 }, b == new List<int> { 3 });
            var solution = expr.Solve();

            var asol = solution.Get(a);
            var bsol = solution.Get(b);

            Assert.AreEqual(2, asol.Count);
            Assert.AreEqual(1, bsol.Count);

            Assert.AreEqual(1, asol[0]);
            Assert.AreEqual(2, asol[1]);
            Assert.AreEqual(3, bsol[0]);
        }

        /// <summary>
        /// Test that solve works as expected for options and objects.
        /// </summary>
        [TestMethod]
        public void TestSolveOptions()
        {
            var a = Arbitrary<Option<int>>();
            var b = Arbitrary<Option<int>>();
            var expr = And(Not(a.HasValue()), b == Option.Some(1));
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
        /// Test that solve works as expected for strings.
        /// </summary>
        [TestMethod]
        public void TestSolveMany()
        {
            // create symbolic variables of different types
            var b = Arbitrary<bool>();
            var i = Arbitrary<int>();
            var s = Arbitrary<string>();
            var o = Arbitrary<Option<ulong>>();
            var l = Arbitrary<Seq<int>>(listSize: 10, checkSmallerLists: false);

            // build constraints on these variables
            var c1 = Or(b, i <= 10);
            var c2 = Or(Not(b), o == Option.Some(1UL));
            var c3 = Or(s.Contains("hello"), Not(o.HasValue()));
            var c4 = l.Where(x => x <= i).Length() == 5;
            var c5 = l.All(x => And(x >= 0, x <= 100));
            var expr = And(c1, c2, c3, c4, c5);

            // solve the constraints to get a solution
            var solution = expr.Solve();

            System.Console.WriteLine("b: " + solution.Get(b));
            System.Console.WriteLine("i: " + solution.Get(i));
            System.Console.WriteLine("s: " + solution.Get(s));
            System.Console.WriteLine("o: " + solution.Get(o));
            System.Console.WriteLine("l: " + string.Join(",", solution.Get(l)));
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
            var a = Arbitrary<Seq<int>>();
            var expr = a.Sort();

            var assignment = new Dictionary<object, object>
            {
                { a, new Seq<int> { Values = new List<int> { 3, 2, 1 } } },
            };

            var l = expr.Evaluate(assignment);

            Assert.AreEqual(3, l.Values.Count);
            Assert.AreEqual(1, l.Values[0]);
            Assert.AreEqual(2, l.Values[1]);
            Assert.AreEqual(3, l.Values[2]);
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
            var a = Arbitrary<Seq<int>>();
            var expr = a.Sort();

            var assignment = new Dictionary<object, object>
            {
                { a, new LinkedList<int>(new List<int> { 3, 2, 1 }) },
            };

            expr.Evaluate(assignment);
        }
    }
}