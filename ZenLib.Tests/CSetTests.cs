// <copyright file="CSetTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
