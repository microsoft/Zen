// <copyright file="SetTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen set type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SetTests
    {
        /// <summary>
        /// Test set symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestSetDelete()
        {
            var zf = new ZenFunction<Set<int>, Set<int>>(d => d.Delete(10).Add(10));

            var d = zf.Evaluate(new Set<int>().Add(10));
            Assert.AreEqual(1, d.Count());

            d = zf.Evaluate(new Set<int>());
            Assert.AreEqual(1, d.Count());

            zf.Compile();
            d = zf.Evaluate(new Set<int>().Add(10));
            Assert.AreEqual(1, d.Count());

            d = zf.Evaluate(new Set<int>());
            Assert.AreEqual(1, d.Count());
        }

        /// <summary>
        /// Test set symbolic evaluation with delete.
        /// </summary>
        [TestMethod]
        public void TestSetDeleteImplementation()
        {
            Assert.AreEqual(0, new Set<int>().Delete(10).Count());
            Assert.AreEqual(1, new Set<int>().Add(1).Delete(10).Count());
            Assert.AreEqual(0, new Set<int>().Add(1).Delete(1).Count());
            Assert.AreEqual(1, new Set<int>().Add(1).Add(10).Delete(1).Count());
        }

        /// <summary>
        /// Test that some basic set equations hold.
        /// </summary>
        [TestMethod]
        public void TestSetEquations()
        {
            CheckValid<Set<byte>, byte>((d, e) => d.Add(e).Delete(e) == d.Delete(e), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => d.Delete(e).Add(e) == d.Add(e), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => Implies(d.Contains(e), d.Add(e) == d), runBdds: false);
            CheckValid<Set<byte>, byte>((d, e) => Implies(Not(d.Contains(e)), d.Delete(e) == d), runBdds: false);
            CheckValid<Set<UInt3>, Set<UInt3>, UInt3>((s1, s2, e) => And(s1.Contains(e), s2.Contains(e)) == s1.Intersect(s2).Contains(e), runBdds: false);
            CheckValid<Set<UInt3>, Set<UInt3>, UInt3>((s1, s2, e) => Or(s1.Contains(e), s2.Contains(e)) == s1.Union(s2).Contains(e), runBdds: false);
            CheckValid<Set<UInt3>, Set<UInt3>, UInt3>((s1, s2, e) => And(s1.Contains(e), Not(s2.Contains(e))) == s1.Difference(s2).Contains(e), runBdds: false);
        }

        /// <summary>
        /// Test that set evaluation works.
        /// </summary>
        [TestMethod]
        public void TestSetEvaluation1()
        {
            var zf1 = new ZenFunction<Set<int>, Set<int>>(d => d.Add(10));
            var zf2 = new ZenFunction<Set<int>, bool>(d => d.Contains(10));
            var zf3 = new ZenFunction<Set<int>>(() => Set.Empty<int>());

            var result1 = zf1.Evaluate(new Set<int>().Add(5));
            Assert.AreEqual(2, result1.Count());
            Assert.IsTrue(result1.Contains(5));
            Assert.IsTrue(result1.Contains(10));

            zf1.Compile();
            result1 = zf1.Evaluate(new Set<int>().Add(5));
            Assert.AreEqual(2, result1.Count());
            Assert.IsTrue(result1.Contains(5));
            Assert.IsTrue(result1.Contains(10));

            var result2 = zf2.Evaluate(new Set<int>().Add(5));
            var result3 = zf2.Evaluate(new Set<int>().Add(10));
            var result4 = zf2.Evaluate(new Set<int>().Add(5).Add(10));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsTrue(result4);

            zf2.Compile();
            result2 = zf2.Evaluate(new Set<int>().Add(5));
            result3 = zf2.Evaluate(new Set<int>().Add(10));
            result4 = zf2.Evaluate(new Set<int>().Add(5).Add(10));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsTrue(result4);

            var result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count());

            zf3.Compile();
            result5 = zf3.Evaluate();
            Assert.AreEqual(0, result5.Count());
        }

        /// <summary>
        /// Test that set symbolic evaluation with equality and empty set.
        /// </summary>
        [TestMethod]
        public void TestSetEqualsEmpty()
        {
            var zf = new ZenConstraint<Set<int>>(d => d == Set.Empty<int>());
            var result = zf.Find();

            Assert.AreEqual(0, result.Value.Count());
        }

        /// <summary>
        /// Test symbolic evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestSetAdd()
        {
            var zf = new ZenFunction<Set<int>, Set<int>>(d => d.Add(10));
            var result = zf.Find((d1, d2) => d2 == Set.Empty<int>());

            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Test set symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestSetContains()
        {
            var zf = new ZenConstraint<Set<int>>(d => d.Contains(10));
            var result = zf.Find();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(1, result.Value.Count());
            Assert.IsTrue(result.Value.Contains(10));
        }

        /// <summary>
        /// Test set symbolic evaluation with get.
        /// </summary>
        [TestMethod]
        public void TestSetEquals()
        {
            CheckAgreement<Set<ushort>>(d => d.Add(1).Add(3) == Set.Empty<ushort>().Add(1), runBdds: false);
            CheckAgreement<Set<ushort>>(d => d.Add(1).Add(3) == Set.Empty<ushort>().Add(1).Add(3), runBdds: false);

            var zf1 = new ZenConstraint<Set<int>>(d => d.Add(1).Add(3) == Set.Empty<int>().Add(1));
            var zf2 = new ZenConstraint<Set<int>>(d => d.Add(1).Add(3) == Set.Empty<int>().Add(1).Add(3));

            Assert.IsFalse(zf1.Evaluate(new Set<int>()));
            Assert.IsTrue(zf2.Evaluate(new Set<int>()));

            zf1.Compile();
            zf2.Compile();
            Assert.IsFalse(zf1.Evaluate(new Set<int>()));
            Assert.IsTrue(zf2.Evaluate(new Set<int>()));
        }

        /// <summary>
        /// Test that If works with sets.
        /// </summary>
        [TestMethod]
        public void TestSetIte()
        {
            CheckValid<Set<long>, bool>((d, b) =>
                Implies(d == Set.Empty<long>(),
                        Not(If(b, d.Add(1), d.Add(2)).Contains(3))), runBdds: false);
        }

        /// <summary>
        /// Test that adding and then checking containment works.
        /// </summary>
        [TestMethod]
        public void TestAddThenContainsIsTrue()
        {
            CheckValid<Set<int>>(d => d.Add(1).Contains(1), runBdds: false);
        }

        /// <summary>
        /// Test that the empty set does not contain anything.
        /// </summary>
        [TestMethod]
        public void TestSetEmpty()
        {
            RandomBytes(x => CheckAgreement<Set<int>>(d => Not(Set.Empty<int>().Contains(x)), runBdds: false));
        }

        /// <summary>
        /// Test that the evaluation is working.
        /// </summary>
        [TestMethod]
        public void TestSetEvaluateInput()
        {
            CheckAgreement<Set<int>>(d => d.Contains(1), runBdds: false);

            var f = new ZenFunction<Set<int>, bool>(d => d.Contains(1));

            var d1 = new Set<int>().Add(1);
            Assert.AreEqual(true, f.Evaluate(d1));

            var d2 = new Set<int>().Add(2);
            Assert.AreEqual(false, f.Evaluate(d2));
        }

        /// <summary>
        /// Test that the set contains operation works.
        /// </summary>
        [TestMethod]
        public void TestSetContainsKey()
        {
            var d = new Set<int>().Add(1).Add(2).Add(1);

            Assert.IsTrue(d.Contains(1));
            Assert.IsTrue(d.Contains(2));
            Assert.IsFalse(d.Contains(3));
        }

        /// <summary>
        /// Test that the set works with strings.
        /// </summary>
        [TestMethod]
        public void TestSetStrings1()
        {
            var f = new ZenFunction<Set<string>, bool>(d => true);
            var sat = f.Find((d, allowed) =>
            {
                var v1 = d.Contains("k1");
                var v2 = d.Contains("k2");
                var v3 = d.Contains("k3");
                return And(v1, v2, v3);
            });

            Assert.IsTrue(sat.HasValue);
            Assert.IsTrue(sat.Value.Contains("k1"));
            Assert.IsTrue(sat.Value.Contains("k2"));
            Assert.IsTrue(sat.Value.Contains("k3"));
        }

        /// <summary>
        /// Test that the set works with strings.
        /// </summary>
        [TestMethod]
        public void TestSetStrings2()
        {
            var result = new ZenConstraint<Set<string>>(s => If(s.Contains("hello"), s.Contains("hi"), s.Contains("hey"))).Find();
            Assert.IsTrue(result.Value.Contains("hi") || result.Value.Contains("hello"));
        }

        /// <summary>
        /// Test that the set contains operation works.
        /// </summary>
        [TestMethod]
        public void TestSetContainsMissing()
        {
            var d = new Set<int>().Add(1).Add(2);
            Assert.IsFalse(d.Contains(3));
        }

        /// <summary>
        /// Test that the set tostring operation works.
        /// </summary>
        [TestMethod]
        public void TestSetToString()
        {
            var d = new Set<int>().Add(1).Add(2);
            Assert.AreEqual("{1, 2}", d.ToString());
        }

        /// <summary>
        /// Test that the set works with pairs.
        /// </summary>
        [TestMethod]
        public void TestSetPairs()
        {
            var f = new ZenFunction<Set<Pair<int, bool>>, bool>(d => d.Contains(Pair.Create<int, bool>(1, true)));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual("{(1, True)}", sat.Value.ToString());
        }

        /// <summary>
        /// Test that the set works with pairs.
        /// </summary>
        [TestMethod]
        public void TestSetNestedObjects()
        {
            var f = new ZenFunction<Set<Pair<int, Pair<int, int>>>, bool>(d => d.Contains(Pair.Create<int, Pair<int, int>>(2, Pair.Create<int, int>(3, 4))));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual(1, sat.Value.Count());
        }

        /// <summary>
        /// Test that the set works with chars.
        /// </summary>
        [TestMethod]
        public void TestSetWithChar()
        {
            var res = new ZenConstraint<Set<char>, bool>((c, b) => If(c.Contains('a'), b, c.Contains('b'))).Find();
            Assert.IsTrue(res.Value.Item1.Contains('a') || res.Value.Item1.Contains('b'));
        }

        /// <summary>
        /// Test that the set works with options.
        /// </summary>
        [TestMethod]
        public void TestSetOptions()
        {
            var f = new ZenFunction<Set<Option<int>>, bool>(d => d.Contains(Option.Null<int>()));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual("{None}", sat.Value.ToString());
        }

        /// <summary>
        /// Test that the set works with options.
        /// </summary>
        [TestMethod]
        public void TestSetUnit()
        {
            var f = new ZenFunction<Set<Unit>, bool>(d => d.Contains(new Unit()));
            var sat = f.Find((d, allowed) => allowed);
            Assert.AreEqual("{ZenLib.Unit}", sat.Value.ToString());
        }

        /// <summary>
        /// Test that all primitive types work with sets.
        /// </summary>
        [TestMethod]
        public void TestSetPrimitiveTypes()
        {
            Assert.IsTrue(new ZenConstraint<Set<bool>>(m => m.Contains(true)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<byte>>(m => m.Contains(1)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<short>>(m => m.Contains(2)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<ushort>>(m => m.Contains(3)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<int>>(m => m.Contains(4)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<uint>>(m => m.Contains(5)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<long>>(m => m.Contains(6)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<ulong>>(m => m.Contains(7)).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<BigInteger>>(m => m.Contains(new BigInteger(8))).Find().HasValue);
            Assert.IsTrue(new ZenConstraint<Set<string>>(m => m.Contains("9")).Find().HasValue);
        }

        /// <summary>
        /// Test that set works with fixed integers.
        /// </summary>
        [TestMethod]
        public void TestSetWithFixedInteger1()
        {
            var zf = new ZenConstraint<Set<UInt3>>(d => d.Contains(new UInt3(2)));
            var result = zf.Find();

            Assert.AreEqual(1, result.Value.Count());
            Assert.IsTrue(result.Value.Contains(new UInt3(2)));
        }

        /// <summary>
        /// Test that set works with fixed integers.
        /// </summary>
        [TestMethod]
        public void TestSetWithFixedInteger2()
        {
            var zf = new ZenConstraint<Set<Int10>>(d => d.Contains(new Int10(-2)));
            var result = zf.Find();

            Assert.AreEqual(1, result.Value.Count());
            Assert.IsTrue(result.Value.Contains(new Int10(-2)));
        }

        /// <summary>
        /// Test set evaluation with union.
        /// </summary>
        [TestMethod]
        public void TestSetUnion()
        {
            var zf = new ZenFunction<Set<int>, Set<int>, Set<int>>((d1, d2) => d1.Union(d2));

            // test interperter
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10)).Count());
            Assert.AreEqual(2, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>(), new Set<int>().Add(11)).Count());
            Assert.AreEqual(3, zf.Evaluate(new Set<int>().Add(1).Add(2), new Set<int>().Add(2).Add(3)).Count());

            // test compiler
            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10)).Count());
            Assert.AreEqual(2, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>(), new Set<int>().Add(11)).Count());
            Assert.AreEqual(3, zf.Evaluate(new Set<int>().Add(1).Add(2), new Set<int>().Add(2).Add(3)).Count());

            // test data structure
            Assert.AreEqual(1, new Set<int>().Add(10).Union(new Set<int>()).Count());
            Assert.AreEqual(1, new Set<int>().Add(10).Union(new Set<int>().Add(10)).Count());
            Assert.AreEqual(2, new Set<int>().Add(10).Union(new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, new Set<int>().Union(new Set<int>().Add(11)).Count());
            Assert.AreEqual(3, new Set<int>().Add(1).Add(2).Union(new Set<int>().Add(2).Add(3)).Count());
        }

        /// <summary>
        /// Test set evaluation with intersect.
        /// </summary>
        [TestMethod]
        public void TestSetIntersection()
        {
            var zf = new ZenFunction<Set<int>, Set<int>, Set<int>>((d1, d2) => d1.Intersect(d2));

            // test interperter
            Assert.AreEqual(0, zf.Evaluate(new Set<int>().Add(10), new Set<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10)).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>(), new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(1).Add(2), new Set<int>().Add(2).Add(3)).Count());

            // test compiler
            zf.Compile();
            Assert.AreEqual(0, zf.Evaluate(new Set<int>().Add(10), new Set<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10)).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>(), new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(1).Add(2), new Set<int>().Add(2).Add(3)).Count());

            // test data structure
            Assert.AreEqual(0, new Set<int>().Add(10).Intersect(new Set<int>()).Count());
            Assert.AreEqual(1, new Set<int>().Add(10).Intersect(new Set<int>().Add(10)).Count());
            Assert.AreEqual(0, new Set<int>().Add(10).Intersect(new Set<int>().Add(11)).Count());
            Assert.AreEqual(0, new Set<int>().Intersect(new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, new Set<int>().Add(1).Add(2).Intersect(new Set<int>().Add(2).Add(3)).Count());
        }

        /// <summary>
        /// Test set evaluation with difference.
        /// </summary>
        [TestMethod]
        public void TestSetDifference()
        {
            var zf = new ZenFunction<Set<int>, Set<int>, Set<int>>((d1, d2) => d1.Difference(d2));

            // test interperter
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>()).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>(), new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(1).Add(2), new Set<int>().Add(2).Add(3)).Count());

            // test compiler
            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>()).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new Set<int>(), new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new Set<int>().Add(1).Add(2), new Set<int>().Add(2).Add(3)).Count());

            // test data structure
            Assert.AreEqual(1, new Set<int>().Add(10).Difference(new Set<int>()).Count());
            Assert.AreEqual(0, new Set<int>().Add(10).Difference(new Set<int>().Add(10)).Count());
            Assert.AreEqual(1, new Set<int>().Add(10).Difference(new Set<int>().Add(11)).Count());
            Assert.AreEqual(0, new Set<int>().Difference(new Set<int>().Add(11)).Count());
            Assert.AreEqual(1, new Set<int>().Add(1).Add(2).Difference(new Set<int>().Add(2).Add(3)).Count());
        }

        /// <summary>
        /// Test set evaluation with issubsetof.
        /// </summary>
        [TestMethod]
        public void TestSetIsSubset()
        {
            var zf = new ZenFunction<Set<int>, Set<int>, bool>((d1, d2) => d1.IsSubsetOf(d2));

            // test interperter
            Assert.IsFalse(zf.Evaluate(new Set<int>().Add(10), new Set<int>()));
            Assert.IsTrue(zf.Evaluate(new Set<int>(), new Set<int>()));
            Assert.IsTrue(zf.Evaluate(new Set<int>(), new Set<int>().Add(10)));
            Assert.IsTrue(zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10).Add(11)));
            Assert.IsFalse(zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)));
            Assert.IsTrue(zf.Evaluate(new Set<int>().Add(10).Add(11), new Set<int>().Add(11).Add(10).Add(4)));

            // test compiler
            zf.Compile();
            Assert.IsFalse(zf.Evaluate(new Set<int>().Add(10), new Set<int>()));
            Assert.IsTrue(zf.Evaluate(new Set<int>(), new Set<int>()));
            Assert.IsTrue(zf.Evaluate(new Set<int>(), new Set<int>().Add(10)));
            Assert.IsTrue(zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(10).Add(11)));
            Assert.IsFalse(zf.Evaluate(new Set<int>().Add(10), new Set<int>().Add(11)));
            Assert.IsTrue(zf.Evaluate(new Set<int>().Add(10).Add(11), new Set<int>().Add(11).Add(10).Add(4)));

            // test data structure
            Assert.IsFalse(new Set<int>().Add(10).IsSubsetOf(new Set<int>()));
            Assert.IsTrue(new Set<int>().IsSubsetOf(new Set<int>()));
            Assert.IsTrue(new Set<int>().IsSubsetOf(new Set<int>().Add(10)));
            Assert.IsTrue(new Set<int>().Add(10).IsSubsetOf(new Set<int>().Add(10).Add(11)));
            Assert.IsFalse(new Set<int>().Add(10).IsSubsetOf(new Set<int>().Add(11)));
            Assert.IsTrue(new Set<int>().Add(10).Add(11).IsSubsetOf(new Set<int>().Add(11).Add(10).Add(4)));
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations1()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();
            var s3 = Symbolic<Set<int>>();
            var s4 = Symbolic<Set<int>>();

            var expr = And(s1.Contains(3), s1.Intersect(s2).Contains(5), s3.Add(4) == s2, s4 == s1.Union(s2));
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);
            var r3 = solution.Get(s3);
            var r4 = solution.Get(s4);

            Assert.IsTrue(r1.Contains(3));
            Assert.IsTrue(r1.Intersect(r2).Contains(5));
            Assert.IsTrue(r3.Add(4) == r2);
            Assert.IsTrue(r4 == r1.Union(r2));
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations2()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();
            var s3 = Symbolic<Set<int>>();
            var s4 = Symbolic<Set<int>>();

            var expr = And(s1.Contains(3), s1.Union(s2).Contains(5), s3.Add(4) == s2, s4 == s1.Intersect(s2));
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);
            var r3 = solution.Get(s3);
            var r4 = solution.Get(s4);

            Assert.IsTrue(r1.Contains(3));
            Assert.IsTrue(r1.Union(r2).Contains(5));
            Assert.IsTrue(r3.Add(4) == r2);
            Assert.IsTrue(r4 == r1.Intersect(r2));
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations3()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();

            var expr = And(s1.Contains(3), Not(s1.Contains(5)), s2.Contains(5), Not(s2.Contains(3)), s1.Intersect(s2) == Set.Empty<int>());
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);

            Assert.IsTrue(r1.Contains(3));
            Assert.IsFalse(r1.Contains(5));
            Assert.IsFalse(r2.Contains(3));
            Assert.IsTrue(r2.Contains(5));
            Assert.IsTrue(r1.Intersect(r2).Count() == 0);
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations4()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();

            var expr = And(s1.Contains(3), s1.Difference(s2) == Set.Empty<int>());
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);
            Console.WriteLine(r1 + ", " + r2);

            Assert.IsTrue(r2.Contains(3));
            Assert.AreEqual(0, r1.Difference(r2).Count());
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations5()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();
            var s3 = Symbolic<Set<int>>();

            var expr = And(s1.Contains(3), s2 != Set.Empty<int>(), s1.Union(s2).Contains(4), s3.Difference(s2) == s1);
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);
            var r3 = solution.Get(s3);

            Assert.IsTrue(r1.Contains(3));
            Assert.IsTrue(r2.Count() > 0);
            Assert.IsTrue(r1.Union(r2).Contains(4));
            Assert.IsTrue(r3.Difference(r2) == r1);
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations6()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();
            var s3 = Symbolic<Set<int>>();

            var expr = And(
                s1.Difference(s3) == s1.Union(s2).Difference(s3),
                s1 != Set.Empty<int>(),
                s2 != Set.Empty<int>(),
                s3 != Set.Empty<int>(),
                s1 != s2,
                s2 != s3,
                Zen.Not(s2.IsSubsetOf(s1)));
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);
            var r3 = solution.Get(s3);

            Assert.IsTrue(r1.Count() > 0);
            Assert.IsTrue(r2.Count() > 0);
            Assert.IsTrue(r3.Count() > 0);
            Assert.IsTrue(r1 != r2);
            Assert.IsTrue(r1 != r3);
            Assert.IsTrue(r2 != r3);
            Assert.IsTrue(r1.Difference(r3) == r1.Union(r2).Difference(r3));
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations7()
        {
            var s = Symbolic<Set<int>>();

            var expr = Implies(Zen.Not(s.Contains(3)), s.Add(3).Difference(new Set<int>(3)) == s);
            var solution = Not(expr).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations8()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();
            var s3 = Symbolic<Set<int>>();

            var expr = And(
                s1.Union(s2).Difference(s3.Intersect(s1)) == s2,
                s3.Intersect(s1) != Set.Empty<int>(),
                s1 != Set.Empty<int>(),
                s2 != Set.Empty<int>(),
                s3 != Set.Empty<int>());

            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);
            var r3 = solution.Get(s3);

            Assert.IsTrue(r1.Intersect(r3).Count() > 0);
            Assert.IsTrue(r1.Count() > 0);
            Assert.IsTrue(r2.Count() > 0);
            Assert.IsTrue(r3.Count() > 0);
            Assert.IsTrue(r1.Union(r2).Difference(r1.Intersect(r3)) == r2);
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetCombinations9()
        {
            var s1 = Symbolic<Set<int>>();
            var s2 = Symbolic<Set<int>>();

            var expr = And(
                Constant(new Set<int>(1, 2, 3, 4)).Difference(s1.Difference(s2)) == new Set<int>(1, 2),
                s2 != Set.Empty<int>(),
                s1.Intersect(s2) != Set.Empty<int>());

            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);

            Assert.IsTrue(r1.Contains(3));
            Assert.IsTrue(r1.Contains(4));
            Assert.IsTrue(r1.Count() > 2);
            Assert.IsTrue(r2.Count() > 0);
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestSetWorksWithRealsAndChars()
        {
            var s1 = Symbolic<Set<Pair<char, Real>>>();
            var s2 = Symbolic<Set<Pair<char, Real>>>();

            var p1 = Pair.Create<char, Real>('a', new Real(3));
            var expr = And(s1.Contains(p1), s1.IsSubsetOf(s2));
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);

            Assert.IsTrue(r1.Contains(new Pair<char, Real> { Item1 = 'a', Item2 = new Real(3) }));
            Assert.IsTrue(r1.Count() <= r2.Count());
            Assert.IsTrue(r1.IsSubsetOf(r2));
        }

        /// <summary>
        /// Test set equality and hashcode.
        /// </summary>
        [TestMethod]
        public void TestSetEqualsHashcode()
        {
            var s1 = new Set<int>(11, 10);
            var s2 = new Set<int>(10, 11);
            var s3 = new Set<int>(1, 2);
            var s4 = new Set<int>();
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.Equals((object)s2));
            Assert.IsFalse(s1.Equals(10));
            Assert.IsFalse(s1 == s3);
            Assert.IsFalse(s1 == s4);
            Assert.IsTrue(s1 != s3);
            Assert.IsTrue(s1.GetHashCode() != s3.GetHashCode());
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
            Assert.AreEqual(0, new SetUnit().GetHashCode());
        }

        /// <summary>
        /// Test that set work with other sets.
        /// </summary>
        [TestMethod]
        public void TestSetWithOtherSet1()
        {
            var result = new ZenConstraint<Set<Set<int>>>(d => d.Contains(Set.Empty<int>())).Find();
            Assert.IsTrue(result.Value.Contains(new Set<int>()));
        }

        /// <summary>
        /// Test that set work with other sets.
        /// </summary>
        [TestMethod]
        public void TestSetWithOtherSet2()
        {
            var result = new ZenConstraint<Set<Set<int>>>(d => d.Delete(Set.Empty<int>().Add(1)).Contains(Set.Empty<int>().Add(3))).Find();
            Assert.IsTrue(result.Value.Contains(new Set<int>().Add(3)));
        }

        /// <summary>
        /// Test that sets work with objects.
        /// </summary>
        [TestMethod]
        public void TestSetWithObjects()
        {
            var result = new ZenConstraint<Set<ObjectWithSet>>(
                d => d.Contains(new ObjectWithSet { Set = new Set<int>().Add(1) })).Find();
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual("{[Set={1}]}", result.Value.ToString());
        }

        /* /// <summary>
        /// Test that adding many elements to a set works.
        /// </summary>
        [TestMethod]
        public void TestSetPerformance()
        {
            var x = Zen.Symbolic<Set<string>>();
            var set = x;

            for (int i = 0; i < 100; i++)
            {
                set = set.Add(i.ToString());
            }

            for (int i = 0; i < 100; i++)
            {
                set = set.Delete(i.ToString());
            }

            var solution = (set == Set.Empty<string>()).Solve();
            Console.WriteLine(solution.Get(x));
        } */

        /// <summary>
        /// Object with a set field.
        /// </summary>
        public struct ObjectWithSet
        {
            /// <summary>
            /// The set value.
            /// </summary>
            public Set<int> Set { get; set; }

            /// <summary>
            /// To string for object.
            /// </summary>
            /// <returns>A string.</returns>
            public override string ToString()
            {
                return $"[Set={this.Set}]";
            }
        }
    }
}
