// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using ZenLib.SymbolicExecution;
    using static ZenLib.Zen;

    /// <summary>
    /// Helper functions to interpret a Zen function.
    /// </summary>
    internal static class Interpreter
    {
        /// <summary>
        /// Execute an expression with some arguments.
        /// </summary>
        /// <param name="expression">The function as an expression.</param>
        /// <param name="args">The arguments with values as C# values.</param>
        /// <param name="trackBranches">Whether to track branches.</param>
        /// <returns>The result and any path constraint.</returns>
        public static (T, PathConstraint) Run<T>(Zen<T> expression, Dictionary<long, object> args, bool trackBranches = false)
        {
            var environment = new ExpressionEvaluatorEnvironment { ArgumentAssignment = ImmutableDictionary<long, object>.Empty.AddRange(args) };
            var interpreter = new ExpressionEvaluatorVisitor(trackBranches);
            var result = (T)interpreter.Visit(expression, environment);
            return (result, interpreter.PathConstraint);
        }
    }
}
