// <copyright file="TimeoutTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.Solver;

    /// <summary>
    /// Tests that timeouts work with the solvers.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TimeoutTests
    {
        /// <summary>
        /// Test that a timeout works.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenSolverTimeoutException))]
        public void TestTimeout()
        {
            var variables = new Zen<BigInteger>[2000];

            var constraints = new List<Zen<bool>>();
            var sum = Zen.Constant(BigInteger.Zero);
            for (int i = 0; i < 2000; i++)
            {
                variables[i] = Zen.Symbolic<BigInteger>();
                constraints.Add(variables[i] >= BigInteger.Zero);
                sum += variables[i];
            }
            constraints.Add(sum == new BigInteger(3490));

            // try to solve a big problem in a millisecond.
            var solverConfig = new SolverConfig
            {
                SolverType = SolverType.Z3,
                SolverTimeout = TimeSpan.FromMilliseconds(1),
            };

            Zen.And(constraints.ToArray()).Solve(solverConfig);
        }
    }
}