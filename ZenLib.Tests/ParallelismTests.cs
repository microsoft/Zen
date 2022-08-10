// <copyright file="ParallelismTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for checking for thread-safety bugs.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ParallelismTests
    {
        /// <summary>
        /// How many times to try to trigger thread safety issues.
        /// </summary>
        private static readonly int numIterations = 100;

        /// <summary>
        /// Solve multiple problems in parallel repeatedly.
        /// </summary>
        [TestMethod]
        public void TestMultithreadingFind()
        {
            Parallel.For(0, numIterations, (i) =>
            {
                var zf = new ZenConstraint<int>(x => x == i);
                var result = zf.Find();
                Assert.AreEqual(i, result.Value);
            });
        }

        /// <summary>
        /// Solve multiple problems in parallel repeatedly.
        /// </summary>
        [TestMethod]
        public void TestMultithreadingFindAll()
        {
            Parallel.For(0, numIterations, (i) =>
            {
                var zf = new ZenConstraint<int>(x => Zen.Or(x == i, x == i + 1));
                var result = zf.FindAll().ToList();
                Assert.AreEqual(2, result.Count);
            });
        }
    }
}