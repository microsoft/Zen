// <copyright file="ZenFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Zen extension methods for solving.
    /// </summary>
    public static class ZenExtensions
    {
        /// <summary>
        /// Solves for an assignment to Arbitrary variables in a boolean expression.
        /// </summary>
        /// <param name="expr">The boolean expression.</param>
        /// <param name="backend">The solver backend to use.</param>
        /// <returns>Mapping from arbitrary expressions to C# objects.</returns>
        public static Solution Solve(this Zen<bool> expr, Backend backend = Backend.Z3)
        {
            return new Solution(SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), backend));
        }
    }
}