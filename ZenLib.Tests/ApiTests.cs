// <copyright file="ApiTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for the Zen object type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ApiTests
    {
        /// <summary>
        /// Test that the solve method is working with evaluate.
        /// </summary>
        [TestMethod]
        public void TestSolve()
        {
            var x = Zen.Symbolic<int>();
            var solution = (x + 1 == 5).Solve();

            Assert.IsTrue(solution.IsSatisfiable());
            Assert.AreEqual(4, solution.VariableAssignment[x]);
        }

        /// <summary>
        /// Test that the evaluation function is working.
        /// </summary>
        [TestMethod]
        public void TestEvaluate()
        {
            Assert.AreEqual(int.MinValue, Zen.Evaluate((a) => a + 1, int.MaxValue));

            Assert.AreEqual(1, Zen.Evaluate(() => Zen.Constant(1)));
            Assert.AreEqual(2, Zen.Evaluate((a) => a + 1, 1));
            Assert.AreEqual(3, Zen.Evaluate((a, b) => a + b, 1, 2));
            Assert.AreEqual(6, Zen.Evaluate((a, b, c) => a + b + c, 1, 2, 3));
            Assert.AreEqual(10, Zen.Evaluate((a, b, c, d) => a + b + c + d, 1, 2, 3, 4));
        }

        /// <summary>
        /// Test that the compilation function is working.
        /// </summary>
        [TestMethod]
        public void TestCompile()
        {
            var f1 = Zen.Compile<int>(() => 1);
            var f2 = Zen.Compile<int, int>((a) => a + 1);
            var f3 = Zen.Compile<int, int, int>((a, b) => a + b);
            var f4 = Zen.Compile<int, int, int, int>((a, b, c) => a + b + c);
            var f5 = Zen.Compile<int, int, int, int, int>((a, b, c, d) => a + b + c + d);

            Assert.AreEqual(int.MinValue, f2(int.MaxValue));

            Assert.AreEqual(1, f1());
            Assert.AreEqual(2, f2(1));
            Assert.AreEqual(3, f3(1, 2));
            Assert.AreEqual(6, f4(1, 2, 3));
            Assert.AreEqual(10, f5(1, 2, 3, 4));
        }
    }
}
