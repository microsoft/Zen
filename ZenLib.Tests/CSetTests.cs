// <copyright file="CSetTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection.Metadata;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for the Zen const set type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CSetTests
    {
        /// <summary>
        /// Exception thrown when adding null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCSetAddNull()
        {
            new CSet<string>().Add(null);
        }

        /// <summary>
        /// Exception thrown when adding null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCSetDeleteNull()
        {
            new CSet<string>().Delete(null);
        }

        /// <summary>
        /// Exception thrown when adding null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCSetContainsNull()
        {
            new CSet<string>().Contains(null);
        }

        /// <summary>
        /// Exception thrown when adding null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCSetUnionNull()
        {
            new CSet<string>().Union(null);
        }

        /// <summary>
        /// Exception thrown when adding null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCSetIntersectNull()
        {
            new CSet<string>().Intersect(null);
        }

        /// <summary>
        /// Exception thrown when adding null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCSetDifferenceNull()
        {
            new CSet<string>().Difference(null);
        }

        /// <summary>
        /// Test set evaluation.
        /// </summary>
        [TestMethod]
        public void TestCSetEvaluate()
        {
            var zf = new ZenFunction<CSet<int>, CSet<int>>(d => d.Add(1).Delete(2));

            var d = zf.Evaluate(new CSet<int>(2, 3));
            Assert.AreEqual(2, d.Count());
            Assert.IsTrue(d.Contains(1));
            Assert.IsTrue(d.Contains(3));

            d = zf.Evaluate(new CSet<int>(2));
            Assert.AreEqual(1, d.Count());
            Assert.IsTrue(d.Contains(1));

            zf.Compile();
            d = zf.Evaluate(new CSet<int>(2, 3));
            Assert.AreEqual(2, d.Count());
            Assert.IsTrue(d.Contains(1));
            Assert.IsTrue(d.Contains(3));

            d = zf.Evaluate(new CSet<int>(2));
            Assert.AreEqual(1, d.Count());
            Assert.IsTrue(d.Contains(1));
        }

        /// <summary>
        /// Test set implementation.
        /// </summary>
        [TestMethod]
        public void TestCSetImplementation()
        {
            Assert.IsFalse(new CSet<int>().Contains(1));
            Assert.IsTrue(new CSet<int>(1).Contains(1));
            Assert.IsTrue(new CSet<int>(1, 2).Contains(1));
            Assert.IsFalse(new CSet<int>(1, 2).Contains(3));
            Assert.IsTrue(new CSet<int>(1).Add(2).Contains(2));
            Assert.IsFalse(new CSet<int>(1).Delete(1).Contains(2));
            Assert.IsFalse(new CSet<int>(1).Delete(1).Contains(1));
            Assert.IsTrue(new CSet<int>(1) == new CSet<int>().Add(1));
            Assert.IsTrue(new CSet<int>(1).Equals((object)new CSet<int>().Add(1)));
            Assert.IsTrue(new CSet<int>(1).GetHashCode() == new CSet<int>().Add(1).GetHashCode());
            Assert.IsTrue(new CSet<int>(1) != new CSet<int>().Add(2));
            Assert.IsFalse(new CSet<int>().Equals(new object()));
        }

        /// <summary>
        /// Test cset evaluation with intersect.
        /// </summary>
        [TestMethod]
        public void TestCSetIntersection()
        {
            var zf = new ZenFunction<CSet<int>, CSet<int>, CSet<int>>((d1, d2) => d1.Intersect(d2));

            // test interperter
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10)).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>(), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(1).Add(2), new CSet<int>().Add(2).Add(3)).Count());

            // test compiler
            zf.Compile();
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10)).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>(), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(1).Add(2), new CSet<int>().Add(2).Add(3)).Count());

            // test data structure
            Assert.AreEqual(0, new CSet<int>().Add(10).Intersect(new CSet<int>()).Count());
            Assert.AreEqual(1, new CSet<int>().Add(10).Intersect(new CSet<int>().Add(10)).Count());
            Assert.AreEqual(0, new CSet<int>().Add(10).Intersect(new CSet<int>().Add(11)).Count());
            Assert.AreEqual(0, new CSet<int>().Intersect(new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, new CSet<int>().Add(1).Add(2).Intersect(new CSet<int>().Add(2).Add(3)).Count());
        }

        /// <summary>
        /// Test set evaluation with union.
        /// </summary>
        [TestMethod]
        public void TestCSetUnion()
        {
            var zf = new ZenFunction<CSet<int>, CSet<int>, CSet<int>>((d1, d2) => d1.Union(d2));

            // test interperter
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10)).Count());
            Assert.AreEqual(2, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>(), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(3, zf.Evaluate(new CSet<int>().Add(1).Add(2), new CSet<int>().Add(2).Add(3)).Count());

            // test compiler
            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10)).Count());
            Assert.AreEqual(2, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>(), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(3, zf.Evaluate(new CSet<int>().Add(1).Add(2), new CSet<int>().Add(2).Add(3)).Count());

            // test data structure
            Assert.AreEqual(1, new CSet<int>().Add(10).Union(new CSet<int>()).Count());
            Assert.AreEqual(1, new CSet<int>().Add(10).Union(new CSet<int>().Add(10)).Count());
            Assert.AreEqual(2, new CSet<int>().Add(10).Union(new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, new CSet<int>().Union(new CSet<int>().Add(11)).Count());
            Assert.AreEqual(3, new CSet<int>().Add(1).Add(2).Union(new CSet<int>().Add(2).Add(3)).Count());
        }

        /// <summary>
        /// Test set evaluation with difference.
        /// </summary>
        [TestMethod]
        public void TestCSetDifference()
        {
            var zf = new ZenFunction<CSet<int>, CSet<int>, CSet<int>>((d1, d2) => d1.Difference(d2));

            // test interperter
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>(), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(1).Add(2), new CSet<int>().Add(2).Add(3)).Count());

            // test compiler
            zf.Compile();
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(0, zf.Evaluate(new CSet<int>(), new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, zf.Evaluate(new CSet<int>().Add(1).Add(2), new CSet<int>().Add(2).Add(3)).Count());

            // test data structure
            Assert.AreEqual(1, new CSet<int>().Add(10).Difference(new CSet<int>()).Count());
            Assert.AreEqual(0, new CSet<int>().Add(10).Difference(new CSet<int>().Add(10)).Count());
            Assert.AreEqual(1, new CSet<int>().Add(10).Difference(new CSet<int>().Add(11)).Count());
            Assert.AreEqual(0, new CSet<int>().Difference(new CSet<int>().Add(11)).Count());
            Assert.AreEqual(1, new CSet<int>().Add(1).Add(2).Difference(new CSet<int>().Add(2).Add(3)).Count());
        }

        /// <summary>
        /// Test set evaluation with issubsetof.
        /// </summary>
        [TestMethod]
        public void TestCSetIsSubset()
        {
            var zf = new ZenFunction<CSet<int>, CSet<int>, bool>((d1, d2) => d1.IsSubsetOf(d2));

            // test interperter
            Assert.IsFalse(zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()));
            Assert.IsTrue(zf.Evaluate(new CSet<int>(), new CSet<int>()));
            Assert.IsTrue(zf.Evaluate(new CSet<int>(), new CSet<int>().Add(10)));
            Assert.IsTrue(zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10).Add(11)));
            Assert.IsFalse(zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)));
            Assert.IsTrue(zf.Evaluate(new CSet<int>().Add(10).Add(11), new CSet<int>().Add(11).Add(10).Add(4)));

            // test compiler
            zf.Compile();
            Assert.IsFalse(zf.Evaluate(new CSet<int>().Add(10), new CSet<int>()));
            Assert.IsTrue(zf.Evaluate(new CSet<int>(), new CSet<int>()));
            Assert.IsTrue(zf.Evaluate(new CSet<int>(), new CSet<int>().Add(10)));
            Assert.IsTrue(zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(10).Add(11)));
            Assert.IsFalse(zf.Evaluate(new CSet<int>().Add(10), new CSet<int>().Add(11)));
            Assert.IsTrue(zf.Evaluate(new CSet<int>().Add(10).Add(11), new CSet<int>().Add(11).Add(10).Add(4)));

            // test data structure
            Assert.IsFalse(new CSet<int>().Add(10).IsSubsetOf(new CSet<int>()));
            Assert.IsTrue(new CSet<int>().IsSubsetOf(new CSet<int>()));
            Assert.IsTrue(new CSet<int>().IsSubsetOf(new CSet<int>().Add(10)));
            Assert.IsTrue(new CSet<int>().Add(10).IsSubsetOf(new CSet<int>().Add(10).Add(11)));
            Assert.IsFalse(new CSet<int>().Add(10).IsSubsetOf(new CSet<int>().Add(11)));
            Assert.IsTrue(new CSet<int>().Add(10).Add(11).IsSubsetOf(new CSet<int>().Add(11).Add(10).Add(4)));
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestCSetCombinations1()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();
            var s3 = Zen.Symbolic<CSet<int>>();
            var s4 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(s1.Contains(3), s1.Intersect(s2).Contains(5), s3.Add(4) == s2, s4 == s1.Union(s2));
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
        public void TestCSetCombinations2()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();
            var s3 = Zen.Symbolic<CSet<int>>();
            var s4 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(s1.Contains(3), s1.Union(s2).Contains(5), s3.Add(4) == s2, s4 == s1.Intersect(s2));
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
        public void TestCSetCombinations3()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(
                s1.Contains(3),
                Zen.Not(s1.Contains(5)),
                s2.Contains(5),
                Zen.Not(s2.Contains(3)),
                s1.Intersect(s2) == CSet.Empty<int>());

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
        public void TestCSetCombinations4()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(s1.Contains(3), s1.Difference(s2) == CSet.Empty<int>());
            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);

            Assert.IsTrue(r2.Contains(3));
            Assert.AreEqual(0, r1.Difference(r2).Count());
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestCSetCombinations5()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();
            var s3 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(s1.Contains(3), s2 != CSet.Empty<int>(), s1.Union(s2).Contains(4), s3.Difference(s2) == s1);
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
        public void TestCSetCombinations6()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();
            var s3 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(
                s1.Difference(s3) == s1.Union(s2).Difference(s3),
                s1 != CSet.Empty<int>(),
                s2 != CSet.Empty<int>(),
                s3 != CSet.Empty<int>(),
                s1 != s2,
                s2 != s3,
                Zen.Or(s1.Contains(1), s1.Contains(2), s1.Contains(3)),
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
        public void TestCSetCombinations7()
        {
            var s = Zen.Symbolic<CSet<int>>();

            var expr = Zen.Implies(Zen.Not(s.Contains(3)), s.Add(3).Difference(new CSet<int>(3)) == s);
            var solution = Zen.Not(expr).Solve();
            Assert.IsFalse(solution.IsSatisfiable());
        }

        /// <summary>
        /// Test set combine operations.
        /// </summary>
        [TestMethod]
        public void TestCSetCombinations8()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();
            var s3 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(
                s1.Union(s2).Difference(s3.Intersect(s1)) == s2,
                s3.Intersect(s1) != CSet.Empty<int>(),
                s1 != CSet.Empty<int>(),
                s2 != CSet.Empty<int>(),
                s3 != CSet.Empty<int>(),
                Zen.Or(s1.Contains(1), s1.Contains(2), s1.Contains(3)));

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
        public void TestCSetCombinations9()
        {
            var s1 = Zen.Symbolic<CSet<int>>();
            var s2 = Zen.Symbolic<CSet<int>>();

            var expr = Zen.And(
                Zen.Constant(new CSet<int>(1, 2, 3, 4)).Difference(s1.Difference(s2)) == new CSet<int>(1, 2),
                s2 != CSet.Empty<int>(),
                s1.Intersect(s2) != CSet.Empty<int>());

            var solution = expr.Solve();

            var r1 = solution.Get(s1);
            var r2 = solution.Get(s2);

            Assert.IsTrue(r1.Contains(3));
            Assert.IsTrue(r1.Contains(4));
            Assert.IsTrue(r1.Count() > 2);
            Assert.IsTrue(r2.Count() > 0);
        }

        /// <summary>
        /// Test set combine with constant.
        /// </summary>
        [TestMethod]
        public void TestCSetCombineWithConstants1()
        {
            var s = Zen.Symbolic<CSet<int>>();
            var e = s.Add(1).Add(2).Add(3).Intersect(new CSet<int>(4, 5)) == new CSet<int>(5);
            var solution = e.Solve();
            var r = solution.Get(s);
            Assert.IsTrue(r.Contains(5));
            Assert.IsFalse(r.Contains(4));
        }

        /// <summary>
        /// Test set combine with constant.
        /// </summary>
        [TestMethod]
        public void TestCSetCombineWithConstants2()
        {
            var s = Zen.Symbolic<CSet<int>>();
            var e = Zen.Constant(new CSet<int>(2)).Union(s.Add(1)) == new CSet<int>(1, 2);
            var solution = e.Solve();
            var r = solution.Get(s);
            Assert.IsTrue(r.Contains(5) || r.Count() == 0);
        }

        /// <summary>
        /// Test sets work with solve.
        /// </summary>
        [TestMethod]
        public void TestCSetSolve()
        {
            var x = Zen.Symbolic<CSet<int>>();
            var solution = x.Contains(3).Solve();
            var result = solution.Get(x);
            Assert.IsTrue(result.Contains(3));
        }

        /// <summary>
        /// Test sets work with solve.
        /// </summary>
        [TestMethod]
        public void TestCSetAddDelete()
        {
            var x = Zen.Symbolic<CSet<int>>();
            var y = Zen.Symbolic<CSet<int>>();
            var solution = Zen.And(x.Add(1).Add(2) == y, x.Contains(3)).Solve();
            var resx = solution.Get(x);
            var resy = solution.Get(y);
            Assert.IsTrue(resx.Contains(3));
            Assert.IsTrue(resy.Contains(1));
            Assert.IsTrue(resy.Contains(2));
            Assert.IsTrue(resy.Contains(3));
        }

        /// <summary>
        /// Test sets work with if.
        /// </summary>
        [TestMethod]
        public void TestCSetIf1()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<TestClass>();
            var f = x.GetField<TestClass, CSet<string>>("Strings");
            var e = Zen.If(b, x.WithField("Strings", f.Add("a").Delete("c")), x.WithField("Strings", f.Add("b").Delete("d")));
            var solution = (e == new TestClass { Strings = new CSet<string>("a", "b") }).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsFalse(solution.Get(b));
            Assert.AreEqual(new CSet<string>("a"), solution.Get(x).Strings);
        }

        /// <summary>
        /// Test sets work with if.
        /// </summary>
        [TestMethod]
        public void TestCSetIf2()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<TestClass>();
            var f = x.GetField<TestClass, CSet<string>>("Strings");
            var e = Zen.If(b, x.WithField("Strings", f.Add("a").Delete("b")), x.WithField("Strings", f.Add("b").Delete("d")));
            var g = e.GetField<TestClass, CSet<string>>("Strings");
            var expr = Zen.If<int>(g.Contains("b"), 1, 3);
            var solution = (expr == 1).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsFalse(solution.Get(b));
        }

        /// <summary>
        /// Test sets work with if.
        /// </summary>
        [TestMethod]
        public void TestCSetIf3()
        {
            var b = Zen.Symbolic<bool>();
            var x = Zen.Symbolic<TestClass>();
            var f = x.GetField<TestClass, CSet<string>>("Strings");
            var e = Zen.If(b, x.WithField("Strings", f.Add("a").Delete("b")), x.WithField("Strings", f.Add("b").Delete("d")));
            var g = e.GetField<TestClass, CSet<string>>("Strings");
            var expr = Zen.If<int>(g.Contains("b"), 1, 3);
            var solution = (expr == 1).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.IsFalse(solution.Get(b));
        }

        /// <summary>
        /// Test sets work with if.
        /// </summary>
        [TestMethod]
        public void TestCSetIf4()
        {
            var x = Zen.Symbolic<TestClass>();
            var y = Zen.Symbolic<TestClass>();
            var fx = x.GetField<TestClass, CSet<string>>("Strings");
            var fy = y.GetField<TestClass, CSet<string>>("Strings");
            var e = Zen.If<int>(fx == fy, 1, 2);
            var solution = Zen.And(fx.Contains("a"), Zen.Not(fx.Contains("b")), e == 1).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(new CSet<string>("a"), solution.Get(x).Strings);
            Assert.AreEqual(new CSet<string>("a"), solution.Get(y).Strings);
        }

        /// <summary>
        /// A test class.
        /// </summary>
        public class TestClass
        {
            /// <summary>
            /// The string values.
            /// </summary>
            public CSet<string> Strings;
        }
    }
}
