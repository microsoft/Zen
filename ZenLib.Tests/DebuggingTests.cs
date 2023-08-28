// <copyright file="DebuggingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Solver;

    /// <summary>
    /// Tests for the Zen bag type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DebuggingTests
    {
        /// <summary>
        /// Test that debugging works with sat.
        /// </summary>
        [TestMethod]
        public void TestSolveDebugging()
        {
            string query = null;
            var solverConfig = new SolverConfig { Debug = (x) => query = x.SolverQuery };
            var b = Zen.Symbolic<bool>();
            var s = Zen.Symbolic<string>();
            var e = Zen.If(b, "hello", s);
            var sol = e.StartsWith("hi").Solve(solverConfig);
            Console.WriteLine(query);
            Assert.IsTrue(query.Contains("(declare-fun k!2 () String)"));
            Assert.IsTrue(query.Contains("(declare-fun k!1 () Bool)"));
            Assert.IsTrue(query.Contains("(assert (and (not k!1) (str.prefixof \"hi\" k!2)))"));
        }

        /// <summary>
        /// Test that debugging works with maximize.
        /// </summary>
        [TestMethod]
        public void TestMaximizeDebugging()
        {
            string query = null;
            var solverConfig = new SolverConfig { Debug = (x) => query = x.SolverQuery };
            var a = Zen.Symbolic<byte>();
            var solution = Zen.Maximize(a, Zen.True(), solverConfig);
            Assert.IsTrue(query.Contains("(declare-fun k!1 () (_ BitVec 8))"));
            Assert.IsTrue(query.Contains("(assert true)"));
            Assert.IsTrue(query.Contains("(maximize k!1)"));
            Assert.IsTrue(query.Contains("(check-sat)"));
        }

        /// <summary>
        /// Test that debugging works with minimize.
        /// </summary>
        [TestMethod]
        public void TestMinimizeDebugging()
        {
            string query = null;
            var solverConfig = new SolverConfig { Debug = (x) => query = x.SolverQuery };
            var a = Zen.Symbolic<byte>();
            var solution = Zen.Minimize(a, Zen.True(), solverConfig);
            Assert.IsTrue(query.Contains("(declare-fun k!1 () (_ BitVec 8))"));
            Assert.IsTrue(query.Contains("(assert true)"));
            Assert.IsTrue(query.Contains("(minimize k!1)"));
            Assert.IsTrue(query.Contains("(check-sat)"));
        }
    }
}
