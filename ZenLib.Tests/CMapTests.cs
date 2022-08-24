// <copyright file="CMapTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen const map type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CMapTests
    {
        /// <summary>
        /// Test map evaluation with set.
        /// </summary>
        [TestMethod]
        public void TestCMapEvaluation()
        {
            var zf = new ZenFunction<CMap<int, int>, CMap<int, int>>(d => d.Set(10, 1));

            var d = zf.Evaluate(new CMap<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));

            d = zf.Evaluate(new CMap<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));

            zf.Compile();
            d = zf.Evaluate(new CMap<int, int>().Set(10, 100));
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));

            d = zf.Evaluate(new CMap<int, int>());
            Assert.AreEqual(1, d.Count());
            Assert.AreEqual(1, d.Get(10));
        }

        /// <summary>
        /// Test map implementation.
        /// </summary>
        [TestMethod]
        public void TestCMapImplementation()
        {
            Assert.AreEqual(0, new CMap<int, int>().Count());
            Assert.AreEqual(10, new CMap<int, int>().Set(1, 10).Get(1));
            Assert.AreEqual(0, new CMap<int, int>().Set(1, 10).Get(2));
            Assert.AreEqual(0, new CMap<int, int>().Set(1, 10).Set(1, 0).Count());
            Assert.IsTrue(new CMap<int, int>().Set(1, 2) == new CMap<int, int>().Set(1, 2));
            Assert.IsTrue(new CMap<int, int>().Set(1, 2) != new CMap<int, int>().Set(1, 3));
            Assert.IsTrue(new CMap<int, int>().Set(1, 2) != new CMap<int, int>());
            Assert.IsFalse(new CMap<int, int>().Equals(new object()));
        }

        /// <summary>
        /// Test that some basic map equations hold.
        /// </summary>
        [TestMethod]
        public void TestCMapEquations()
        {
            CheckValid<CMap<int, int>, int>((m, v) => m.Set(1, v).Get(1) == v, runBdds: false);
            CheckValid<CMap<int, int>, int, int>((m, v1, v2) => Zen.Implies(m.Get(2) == v2, m.Set(1, v1).Get(2) == v2), runBdds: false);
        }

        /// <summary>
        /// Test that map evaluation works.
        /// </summary>
        [TestMethod]
        public void TestCMapEvaluation1()
        {
            var zf1 = new ZenFunction<CMap<int, int>, CMap<int, int>>(d => d.Set(10, 20));
            var zf2 = new ZenFunction<CMap<int, int>, bool>(d => d.Get(10) == 11);

            var result1 = zf1.Evaluate(new CMap<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5));
            Assert.AreEqual(20, result1.Get(10));

            zf1.Compile();
            result1 = zf1.Evaluate(new CMap<int, int>().Set(5, 5));
            Assert.AreEqual(2, result1.Count());
            Assert.AreEqual(5, result1.Get(5));
            Assert.AreEqual(20, result1.Get(10));

            var result2 = zf2.Evaluate(new CMap<int, int>().Set(10, 10));
            var result3 = zf2.Evaluate(new CMap<int, int>().Set(5, 5).Set(10, 11));
            var result4 = zf2.Evaluate(new CMap<int, int>().Set(5, 5));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsFalse(result4);

            zf2.Compile();
            result2 = zf2.Evaluate(new CMap<int, int>().Set(10, 10));
            result3 = zf2.Evaluate(new CMap<int, int>().Set(5, 5).Set(10, 11));
            result4 = zf2.Evaluate(new CMap<int, int>().Set(5, 5));
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
            Assert.IsFalse(result4);
        }

        /// <summary>
        /// Check that adding to a dictionary evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestCMapEvaluation2()
        {
            var f = new ZenFunction<CMap<int, int>, CMap<int, int>>(d => d.Set(1, 1).Set(2, 2));

            var result = f.Evaluate(new CMap<int, int>());
            Assert.AreEqual(1, result.Get(1));
            Assert.AreEqual(2, result.Get(2));

            f.Compile();
            result = f.Evaluate(new CMap<int, int>());
            Assert.AreEqual(1, result.Get(1));
            Assert.AreEqual(2, result.Get(2));
        }

        /// <summary>
        /// Test map symbolic evaluation.
        /// </summary>
        [TestMethod]
        public void TestCMapFind1()
        {
            var zf = new ZenFunction<CMap<int, int>, CMap<int, int>>(d => d.Set(10, 20));
            var result = zf.Find((d1, d2) => d2.Get(20) == 30);

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(30, result.Value.Get(20));
        }

        /// <summary>
        /// Test map symbolic evaluation.
        /// </summary>
        [TestMethod]
        public void TestCMapFind2()
        {
            var zf = new ZenConstraint<CMap<int, int>>(d => Zen.And(d.Get(1) == 2, d.Get(0) == 1));
            var result = zf.Find();

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(1, result.Value.Get(0));
            Assert.AreEqual(2, result.Value.Get(1));
        }

        /// <summary>
        /// Test map solve.
        /// </summary>
        [TestMethod]
        public void TestCMapSolve()
        {
            var cm = Zen.Symbolic<CMap<string, bool>>();
            var expr = Zen.And(cm.Get("a"), Zen.Not(cm.Get("b")));
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new CMap<string, bool>().Set("a", true).Set("b", false), solution.Get(cm));
        }

        /// <summary>
        /// Test map solve.
        /// </summary>
        [TestMethod]
        public void TestCMapIf()
        {
            var b = Zen.Symbolic<bool>();
            var cm = Zen.Symbolic<CMap<int, int>>();
            var expr = Zen.If(b, cm.Set(1, 2), cm.Set(1, 3)).Get(1) == 3;
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(false, solution.Get(b));
        }

        /// <summary>
        /// Test map with pairs.
        /// </summary>
        [TestMethod]
        public void TestCMapPairs()
        {
            var cm = Zen.Symbolic<CMap<int, Pair<int, int>>>();
            var solution = (cm.Get(1).Item1() == 4).Solve();
            Assert.AreEqual(4, solution.Get(cm).Get(1).Item1);
        }

        /// <summary>
        /// Test map in an option.
        /// </summary>
        [TestMethod]
        public void TestCMapOptionNone()
        {
            var o = Zen.Symbolic<Option<CMap<int, int>>>();
            var solution = o.IsNone().Solve();
            Assert.IsTrue(!solution.Get(o).HasValue);
        }

        /// <summary>
        /// Test map in an option.
        /// </summary>
        [TestMethod]
        public void TestCMapOptionSome()
        {
            var o = Zen.Symbolic<Option<CMap<int, int>>>();
            var solution = Zen.And(o.IsSome(), o.Value().Get(1) == 5).Solve();
            Assert.IsTrue(solution.Get(o).HasValue);
            Assert.AreEqual(5, solution.Get(o).Value.Get(1));
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestCMapConstants1()
        {
            Zen<CMap<int, int>> cm = new CMap<int, int>().Set(1, 4);
            var solution = (cm.Get(1) == 5).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestCMapConstants2()
        {
            Zen<CMap<int, int>> cm = new CMap<int, int>();
            var solution = (cm.Get(1) == 5).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestCMapConstants3()
        {
            Zen<CMap<int, int>> cm = new CMap<int, int>();
            var solution = (cm.Get(1) == 0).Solve();
            Assert.IsTrue(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test map with constants.
        /// </summary>
        [TestMethod]
        public void TestCMapConstants4()
        {
            Zen<CMap<int, int>> cm = new CMap<int, int>();
            var solution = (cm.Set(1, 1).Get(1) == 0).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test maps that are nested.
        /// </summary>
        [TestMethod]
        public void TestCMapNested1()
        {
            var cm = Zen.Symbolic<CMap<int, CMap<int, int>>>();
            var solution = (cm.Get(1).Get(1) == 4).Solve();
            Assert.AreEqual(4, solution.Get(cm).Get(1).Get(1));
        }

        /// <summary>
        /// Test maps that are nested.
        /// </summary>
        [TestMethod]
        public void TestCMapNested2()
        {
            var cm = Zen.Symbolic<CMap<int, CMap<int, int>>>();
            var solution = Zen.And(cm.Get(1).Get(1) == 4, cm.Get(2).Get(2) == 5).Solve();
            Assert.AreEqual(4, solution.Get(cm).Get(1).Get(1));
            Assert.AreEqual(5, solution.Get(cm).Get(2).Get(2));
        }

        /// <summary>
        /// Test map within a map.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCMapInMapValue()
        {
            var cm = Zen.Symbolic<Map<int, CMap<int, int>>>();
            (cm.Get(1).Value().Get(1) == 4).Solve();
        }

        /// <summary>
        /// Test map within a set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCMapInMapKey()
        {
            var m = Zen.Symbolic<Map<CMap<int, int>, int>>();
            var cm = Zen.Symbolic<CMap<int, int>>();
            (m.Get(cm).Value() == 4).Solve();
        }

        /// <summary>
        /// Test map within a seq.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCMapInSeq()
        {
            var s = Zen.Symbolic<Seq<CMap<int, int>>>();
            var cm = Zen.Symbolic<CMap<int, int>>();
            s.Contains(cm).Solve();
        }

        /// <summary>
        /// Test maps work in objects.
        /// </summary>
        [TestMethod]
        public void TestCMapInPair()
        {
            var x = Zen.Symbolic<Pair<int, CMap<int, int>>>();
            var solution = Zen.And(x.Item1() == 1, x.Item2().Get(4) == 5).Solve();

            var result = solution.Get(x);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual(5, result.Item2.Get(4));
        }

        /// <summary>
        /// Test maps work as keys.
        /// </summary>
        [TestMethod]
        public void TestCMapInKey()
        {
            var x = Zen.Symbolic<CMap<CMap<int, int>, int>>();
            var cm1 = new CMap<int, int>();
            var solution = (x.Get(cm1) == 2).Solve();

            var result = solution.Get(x);
            Assert.AreEqual(2, result.Get(cm1));
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality1()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            var solution = (x == new CMap<int, int>().Set(0, 1)).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.Get(0) == 1);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality2()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            var solution = (x == new CMap<int, int>()).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Count() == 0);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality3()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            var solution = Zen.And(x.Set(1, 10) == new CMap<int, int>()).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality4()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            var solution = Zen.And(x.Set(1, 10) == new CMap<int, int>().Set(1, 10).Set(2, 20)).Solve();
            var result = solution.Get(x);
            Assert.AreEqual(20, result.Get(2));
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality5()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            var solution = (x != new CMap<int, int>().Set(0, 1)).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Get(0) != 1);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality6()
        {
            var x = Zen.Symbolic<CMap<int, CMap<string, bool>>>();
            var solution = (x == new CMap<int, CMap<string, bool>>().Set(1, new CMap<string, bool>().Set("a", true))).Solve();
            var result = solution.Get(x);
            Assert.AreEqual(true, result.Get(1).Get("a"));
            Assert.AreEqual(false, result.Get(1).Get("b"));
            Assert.AreEqual(false, result.Get(2).Get("a"));
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality7()
        {
            var x = Zen.Symbolic<CMap<int, Map<string, bool>>>();
            var solution = (x == new CMap<int, Map<string, bool>>().Set(1, new Map<string, bool>().Set("a", true))).Solve();
            var result = solution.Get(x);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1, result.Get(1).Count());
            Assert.AreEqual(true, result.Get(1).Get("a").Value);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality8()
        {
            var x = Zen.Symbolic<CMap<int, TestHelper.Object2Different>>();
            var o = new TestHelper.Object2Different { Field1 = 7, Field2 = 9 };
            var solution = (x == new CMap<int, TestHelper.Object2Different>().Set(1, o)).Solve();
            var result = solution.Get(x);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(7, result.Get(1).Field1);
            Assert.AreEqual((short)9, result.Get(1).Field2);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality9()
        {
            var x = Zen.Symbolic<CMap<int, FSeq<int>>>();
            var l = new FSeq<int>().AddFront(1).AddFront(2);
            var solution = (x == new CMap<int, FSeq<int>>().Set(1, l)).Solve();
            var result = solution.Get(x);
            Assert.AreEqual(2, result.Get(1).Count());
            Assert.AreEqual(2, result.Get(1).ToList()[0]);
            Assert.AreEqual(1, result.Get(1).ToList()[1]);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality10()
        {
            var x = Zen.Symbolic<CMap<int, CMap<int, int>>>();
            var y = Zen.Symbolic<CMap<int, CMap<int, int>>>();
            var solution = Zen.And(x.Get(0).Get(0) == 99, x == y).Solve();
            Assert.IsTrue(solution.Get(x).Get(0).Get(0) == 99);
            Assert.IsTrue(solution.Get(y).Get(0).Get(0) == 99);
        }

        /// <summary>
        /// Test maps work with equality.
        /// </summary>
        [TestMethod]
        public void TestCMapEquality11()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            var solution = (new CMap<int, int>().Set(0, 1) == x.Set(3, 4).Set(1, 2)).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test maps work with strings.
        /// </summary>
        [TestMethod]
        public void TestCMapStringValue()
        {
            var x = Zen.Symbolic<CMap<int, string>>();
            var solution = (x.Get(1) == "hi").Solve();
            var result = solution.Get(x);
            Assert.AreEqual("hi", result.Get(1));
        }

        /// <summary>
        /// Test maps work with strings.
        /// </summary>
        [TestMethod]
        public void TestCMapMultiple()
        {
            var x = Zen.Symbolic<CMap<int, string>>();
            var y = Zen.Symbolic<CMap<int, string>>();
            var solution = Zen.And(x.Get(1) == "hi", x.Get(1) == y.Get(1)).Solve();
            Assert.AreEqual("hi", solution.Get(x).Get(1));
            Assert.AreEqual("hi", solution.Get(y).Get(1));
        }

        /// <summary>
        /// Test maps work with lists.
        /// </summary>
        [TestMethod]
        public void TestCMapWithLists()
        {
            var x = Zen.Symbolic<CMap<int, FSeq<int>>>();
            var y = Zen.Symbolic<CMap<int, FSeq<int>>>();
            var solution = Zen.And(x.Get(1).Contains(3), x.Get(1) == y.Get(1), y.Get(2).Length() == 2).Solve();

            Assert.IsTrue(solution.Get(x).Get(1).ToList().Contains(3));
            Assert.IsTrue(solution.Get(y).Get(1).ToList().Contains(3));
            Assert.IsTrue(solution.Get(y).Get(2).ToList().Count == 2);
        }

        /// <summary>
        /// Test maps work in objects.
        /// </summary>
        [TestMethod]
        public void TestCMapInObject()
        {
            var x = Zen.Symbolic<TestMapObject>();
            var f = x.GetField<TestMapObject, CMap<int, bool>>("Edges");
            var solution = f.Get(1).Solve();
            Assert.IsTrue(solution.Get(x).Edges.Get(1));
        }

        /// <summary>
        /// Test maps work with lists.
        /// </summary>
        [TestMethod]
        public void TestCMapInFSeq()
        {
            var l = Zen.Symbolic<FSeq<int>>();
            var x = Zen.Symbolic<CMap<int, int>>();
            var i = Zen.If(l.IsEmpty(), 0, x.Get(1));
            var sol = (i == 2).Solve();
            Assert.IsTrue(sol.Get(l).Count() > 0);
        }

        /// <summary>
        /// Test maps fail with lists.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCMapInFSeqFail()
        {
            var zf = Zen.Constraint<CMap<string, int>, FSeq<int>>((m, l) => l.Select(x => m.Get("a")).Fold<int, int>(0, Zen.Plus) == 4);
            var res = zf.Find();
        }

        /// <summary>
        /// Test maps work in if conditions.
        /// </summary>
        [TestMethod]
        public void TestCMapInIf1()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<CMap<string, int>>();
            var expr = Zen.If(b, x.Set("c", 3), new CMap<string, int>().Set("a", 1).Set("b", 2)).Get("a") == 4;
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(solution.Get(b));
            Assert.AreEqual(4, solution.Get(x).Get("a"));
        }

        /// <summary>
        /// Test maps work in if conditions.
        /// </summary>
        [TestMethod]
        public void TestCMapInIf2()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<CMap<string, int>>();
            var expr = Zen.If(b, new CMap<string, int>().Set("a", 1).Set("b", 2), x.Set("c", 3)).Get("a") == 4;
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsFalse(solution.Get(b));
            Assert.AreEqual(4, solution.Get(x).Get("a"));
        }

        /// <summary>
        /// Test maps work in if conditions.
        /// </summary>
        [TestMethod]
        public void TestCMapInIf3()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<CMap<string, TestHelper.Object2>>();
            var o1 = new TestHelper.Object2 { Field1 = 1, Field2 = 2 };
            var o2 = new TestHelper.Object2 { Field1 = 3, Field2 = 4 };
            var expr = Zen.If(b, x.Set("c", o1), new CMap<string, TestHelper.Object2>().Set("a", o2).Set("b", o2)).Get("a").GetField<Object2, int>("Field1") == 1;
            var solution = expr.Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsTrue(solution.Get(b));
            Assert.AreEqual(1, solution.Get(x).Get("a").Field1);
        }

        /// <summary>
        /// Test maps work in if conditions.
        /// </summary>
        [TestMethod]
        public void TestCMapInIf4()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<TestMapObject>();
            var o = new TestMapObject { Edges = new CMap<int, bool>().Set(1, true) };
            var expr = Zen.If(b, o, x);
            var solution = Zen.Not(expr.GetField<TestMapObject, CMap<int, bool>>("Edges").Get(1)).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsFalse(solution.Get(b));
            Assert.IsFalse(solution.Get(x).Edges.Get(1));
        }

        /// <summary>
        /// Test maps work when there are no constants.
        /// </summary>
        [TestMethod]
        public void TestCMapNoConstants()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<CMap<int, bool>>();
            var y = Zen.Symbolic<CMap<int, bool>>();
            var e = Zen.If(b, Create<TestMapObject>(("Edges", x), ("Field", Zen.True())), Create<TestMapObject>(("Edges", y), ("Field", Zen.False())));
            var solution = e.GetField<TestMapObject, bool>("Field").Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Console.WriteLine(solution.Get(b));
            Console.WriteLine(solution.Get(x));
            Console.WriteLine(solution.Get(y));
        }
    }

    /// <summary>
    /// A test object.
    /// </summary>
    public class TestMapObject
    {
        /// <summary>
        /// A field.
        /// </summary>
        public bool Field { get; set; }

        /// <summary>
        /// Some edge variables.
        /// </summary>
        public CMap<int, bool> Edges { get; set; }
    }
}
