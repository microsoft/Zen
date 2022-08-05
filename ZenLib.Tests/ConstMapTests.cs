// <copyright file="ConstMapTests.cs" company="Microsoft">
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
    /// Tests for the Zen const map type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConstMapTests
    {
        /// <summary>
        /// Test map evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestConstMapEvaluation()
        {
            var zf = new ZenFunction<ConstMap<int, int>, ConstMap<int, int>>(d => d.Set(10, 1));

            var d = zf.Evaluate(new ConstMap<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));

            d = zf.Evaluate(new ConstMap<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));

            zf.Compile();
            d = zf.Evaluate(new ConstMap<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));

            d = zf.Evaluate(new ConstMap<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));
        }

        /// <summary>
        /// Test map implementation.
        /// </summary>
        [TestMethod]
        public void TestConstMapImplementation()
        {
            Assert.AreEqual(0, new ConstMap<int, int>().Count());
            Assert.AreEqual(10, new ConstMap<int, int>().Set(1, 10).Get(1));
            Assert.AreEqual(0, new ConstMap<int, int>().Set(1, 10).Get(2));
            Assert.AreEqual(0, new ConstMap<int, int>().Set(1, 10).Set(1, 0).Count());
            Assert.IsTrue(new ConstMap<int, int>().Set(1, 2) == new ConstMap<int, int>().Set(1, 2));
            Assert.IsTrue(new ConstMap<int, int>().Set(1, 2) != new ConstMap<int, int>().Set(1, 3));
            Assert.IsTrue(new ConstMap<int, int>().Set(1, 2) != new ConstMap<int, int>());
            Assert.IsFalse(new ConstMap<int, int>().Equals(new object()));
        }

        /// <summary>
        /// Test that some basic map equations hold.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquations()
        {
            CheckValid<ConstMap<int, int>, int>((m, v) => m.Set(1, v).Get(1) == v, runBdds: false);
            CheckValid<ConstMap<int, int>, int, int>((m, v1, v2) => Zen.Implies(m.Get(2) == v2, m.Set(1, v1).Get(2) == v2), runBdds: false);
        }

        /// <summary>
        /// Test that map evaluation works.
        /// </summary>
        [TestMethod]
        public void TestConstMapEvaluation1()
        {
            var zf1 = new ZenFunction<ConstMap<int, int>, ConstMap<int, int>>(d => d.Set(10, 20));
            var zf2 = new ZenFunction<ConstMap<int, int>, bool>(d => d.Get(10) == 11);

            var result1 = zf1.Evaluate(new ConstMap<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5));
            Assert.AreEqual(20, result1.Get(10));

            zf1.Compile();
            result1 = zf1.Evaluate(new ConstMap<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5));
            Assert.AreEqual(20, result1.Get(10));

            var result2 = zf2.Evaluate(new ConstMap<int, int>().Set(10, 10));
            var result3 = zf2.Evaluate(new ConstMap<int, int>().Set(5, 5).Set(10, 11));
            var result4 = zf2.Evaluate(new ConstMap<int, int>().Set(5, 5));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsFalse(result4);

            zf2.Compile();
            result2 = zf2.Evaluate(new ConstMap<int, int>().Set(10, 10));
            result3 = zf2.Evaluate(new ConstMap<int, int>().Set(5, 5).Set(10, 11));
            result4 = zf2.Evaluate(new ConstMap<int, int>().Set(5, 5));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsFalse(result4);
        }

        /// <summary>
        /// Check that adding to a dictionary evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestConstMapEvaluation2()
        {
            var f = new ZenFunction<ConstMap<int, int>, ConstMap<int, int>>(d => d.Set(1, 1).Set(2, 2));

            var result = f.Evaluate(new ConstMap<int, int>());
            Assert.AreEqual(1, result.Get(1));
            Assert.AreEqual(2, result.Get(2));

            f.Compile();
            result = f.Evaluate(new ConstMap<int, int>());
            Assert.AreEqual(1, result.Get(1));
            Assert.AreEqual(2, result.Get(2));
        }

        /// <summary>
        /// Test map symbolic evaluation.
        /// </summary>
        [TestMethod]
        public void TestConstMapFind1()
        {
            var zf = new ZenFunction<ConstMap<int, int>, ConstMap<int, int>>(d => d.Set(10, 20));
            var result = zf.Find((d1, d2) => d2.Get(20) == 30);

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(30, result.Value.Get(20));
        }

        /// <summary>
        /// Test map symbolic evaluation.
        /// </summary>
        [TestMethod]
        public void TestConstMapFind2()
        {
            var zf = new ZenConstraint<ConstMap<int, int>>(d => Zen.And(d.Get(1) == 2, d.Get(0) == 1));
            var result = zf.Find();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(1, result.Value.Get(0));
            Assert.AreEqual(2, result.Value.Get(1));
        }

        /// <summary>
        /// Test map solve.
        /// </summary>
        [TestMethod]
        public void TestConstMapSolve()
        {
            var cm = Zen.Symbolic<ConstMap<string, bool>>();
            var expr = Zen.And(cm.Get("a"), Zen.Not(cm.Get("b")));
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new ConstMap<string, bool>().Set("a", true).Set("b", false), solution.Get(cm));
        }

        /// <summary>
        /// Test map solve.
        /// </summary>
        [TestMethod]
        public void TestConstMapIf()
        {
            var b = Zen.Symbolic<bool>();
            var cm = Zen.Symbolic<ConstMap<int, int>>();
            var expr = Zen.If(b, cm.Set(1, 2), cm.Set(1, 3)).Get(1) == 3;
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(false, solution.Get(b));
        }

        /// <summary>
        /// Test map with pairs.
        /// </summary>
        [TestMethod]
        public void TestConstMapPairs()
        {
            var cm = Zen.Symbolic<ConstMap<int, Pair<int, int>>>();
            var solution = (cm.Get(1).Item1() == 4).Solve();
            Assert.AreEqual(4, solution.Get(cm).Get(1).Item1);
        }

        /// <summary>
        /// Test map in an option.
        /// </summary>
        [TestMethod]
        public void TestConstMapOptionNone()
        {
            var o = Zen.Symbolic<Option<ConstMap<int, int>>>();
            var solution = o.IsNone().Solve();
            Assert.IsTrue(!solution.Get(o).HasValue);
        }

        /// <summary>
        /// Test map in an option.
        /// </summary>
        [TestMethod]
        public void TestConstMapOptionSome()
        {
            var o = Zen.Symbolic<Option<ConstMap<int, int>>>();
            var solution = Zen.And(o.IsSome(), o.Value().Get(1) == 5).Solve();
            Assert.IsTrue(solution.Get(o).HasValue);
            Assert.AreEqual(5, solution.Get(o).Value.Get(1));
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestConstMapConstants1()
        {
            Zen<ConstMap<int, int>> cm = new ConstMap<int, int>().Set(1, 4);
            var solution = (cm.Get(1) == 5).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestConstMapConstants2()
        {
            Zen<ConstMap<int, int>> cm = new ConstMap<int, int>();
            var solution = (cm.Get(1) == 5).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestConstMapConstants3()
        {
            Zen<ConstMap<int, int>> cm = new ConstMap<int, int>();
            var solution = (cm.Get(1) == 0).Solve();
            Assert.IsTrue(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestConstMapConstants4()
        {
            Zen<ConstMap<int, int>> cm = new ConstMap<int, int>();
            var solution = (cm.Set(1, 1).Get(1) == 0).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test maps that are nested.
        /// </summary>
        [TestMethod]
        public void TestConstMapNested1()
        {
            var cm = Zen.Symbolic<ConstMap<int, ConstMap<int, int>>>();
            var solution = (cm.Get(1).Get(1) == 4).Solve();
            Assert.AreEqual(4, solution.Get(cm).Get(1).Get(1));
        }

        /// <summary>
        /// Test maps that are nested.
        /// </summary>
        [TestMethod]
        public void TestConstMapNested2()
        {
            var cm = Zen.Symbolic<ConstMap<int, ConstMap<int, int>>>();
            var solution = Zen.And(cm.Get(1).Get(1) == 4, cm.Get(2).Get(2) == 5).Solve();
            Console.WriteLine(solution.Get(cm));
            Assert.AreEqual(4, solution.Get(cm).Get(1).Get(1));
            Assert.AreEqual(5, solution.Get(cm).Get(2).Get(2));
        }

        /// <summary>
        /// Test map within a map.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConstMapInMapValue()
        {
            var cm = Zen.Symbolic<Map<int, ConstMap<int, int>>>();
            (cm.Get(1).Value().Get(1) == 4).Solve();
        }

        /// <summary>
        /// Test map within a set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConstMapInMapKey()
        {
            var m = Zen.Symbolic<Map<ConstMap<int, int>, int>>();
            var cm = Zen.Symbolic<ConstMap<int, int>>();
            (m.Get(cm).Value() == 4).Solve();
        }

        /// <summary>
        /// Test map within a seq.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConstMapInSeq()
        {
            var s = Zen.Symbolic<Seq<ConstMap<int, int>>>();
            var cm = Zen.Symbolic<ConstMap<int, int>>();
            s.Contains(cm).Solve();
        }

        /// <summary>
        /// Test maps work in objects.
        /// </summary>
        [TestMethod]
        public void TestConstMapInPair()
        {
            var x = Zen.Symbolic<Pair<int, ConstMap<int, int>>>();
            var solution = Zen.And(x.Item1() == 1, x.Item2().Get(4) == 5).Solve();

            var result = solution.Get(x);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual(5, result.Item2.Get(4));
        }

        /// <summary>
        /// Test maps work as keys.
        /// </summary>
        [TestMethod]
        public void TestConstMapInKey()
        {
            var x = Zen.Symbolic<ConstMap<ConstMap<int, int>, int>>();
            var cm1 = new ConstMap<int, int>();
            var solution = (x.Get(cm1) == 2).Solve();

            var result = solution.Get(x);
            Assert.AreEqual(2, result.Get(cm1));
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality1()
        {
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var solution = (x == new ConstMap<int, int>().Set(0, 1)).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.Get(0) == 1);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality2()
        {
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var solution = (x == new ConstMap<int, int>()).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Count() == 0);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality3()
        {
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var solution = Zen.And(x.Set(1, 10) == new ConstMap<int, int>()).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality4()
        {
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var solution = Zen.And(x.Set(1, 10) == new ConstMap<int, int>().Set(1, 10).Set(2, 20)).Solve();
            var result = solution.Get(x);
            Assert.AreEqual(20, result.Get(2));
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality5()
        {
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var solution = (x != new ConstMap<int, int>().Set(0, 1)).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            var result = solution.Get(x);
            Assert.IsTrue(result.Get(0) != 1);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConstMapEquality6()
        {
            var x = Zen.Symbolic<ConstMap<int, ConstMap<string, bool>>>();
            (x == new ConstMap<int, ConstMap<string, bool>>().Set(1, new ConstMap<string, bool>().Set("a", true))).Solve();
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality7()
        {
            var x = Zen.Symbolic<ConstMap<int, ConstMap<int, int>>>();
            var y = Zen.Symbolic<ConstMap<int, ConstMap<int, int>>>();
            var solution = Zen.And(x.Get(0).Get(0) == 99, x == y).Solve();
            Assert.IsTrue(solution.Get(x).Get(0).Get(0) == 99);
            Assert.IsTrue(solution.Get(y).Get(0).Get(0) == 99);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestConstMapEquality8()
        {
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var solution = (new ConstMap<int, int>().Set(0, 1) == x.Set(3, 4).Set(1, 2)).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test maps work with strings.
        /// </summary>
        [TestMethod]
        public void TestConstMapStringValue()
        {
            var x = Zen.Symbolic<ConstMap<int, string>>();
            var solution = (x.Get(1) == "hi").Solve();
            var result = solution.Get(x);
            Assert.AreEqual("hi", result.Get(1));
        }

        /// <summary>
        /// Test maps work with strings.
        /// </summary>
        [TestMethod]
        public void TestConstMapMultiple()
        {
            var x = Zen.Symbolic<ConstMap<int, string>>();
            var y = Zen.Symbolic<ConstMap<int, string>>();
            var solution = Zen.And(x.Get(1) == "hi", x.Get(1) == y.Get(1)).Solve();
            Assert.AreEqual("hi", solution.Get(x).Get(1));
            Assert.AreEqual("hi", solution.Get(y).Get(1));
        }

        /// <summary>
        /// Test maps work with lists.
        /// </summary>
        [TestMethod]
        public void TestConstMapWithLists()
        {
            var x = Zen.Symbolic<ConstMap<int, FSeq<int>>>();
            var y = Zen.Symbolic<ConstMap<int, FSeq<int>>>();
            var solution = Zen.And(x.Get(1).Contains(3), x.Get(1) == y.Get(1), y.Get(2).Length() == 2).Solve();
            Console.WriteLine(solution.Get(x));
            Console.WriteLine(solution.Get(y));

            Assert.IsTrue(solution.Get(x).Get(1).Values.Contains(3));
            Assert.IsTrue(solution.Get(y).Get(1).Values.Contains(3));
            Assert.IsTrue(solution.Get(y).Get(2).Values.Count == 2);
        }

        /// <summary>
        /// Test maps work in objects.
        /// </summary>
        [TestMethod]
        public void TestConstMapInObject()
        {
            var x = Zen.Symbolic<TestMapObject>();
            var f = x.GetField<TestMapObject, ConstMap<int, bool>>("Edges");
            var solution = f.Get(1).Solve();
            Assert.IsTrue(solution.Get(x).Edges.Get(1));
        }

        /// <summary>
        /// Test maps work do not work with lists.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestConstMapInList()
        {
            var l = Zen.Symbolic<FSeq<int>>();
            var x = Zen.Symbolic<ConstMap<int, int>>();
            var i = l.Case(0, (hd, tl) => x.Get(1));
            (i == 2).Solve();
        }
    }

    /// <summary>
    /// A test object.
    /// </summary>
    public class TestMapObject
    {
        /// <summary>
        /// Some edge variables.
        /// </summary>
        public ConstMap<int, bool> Edges { get; set; }
    }
}
