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

        public static T2 CompileRunHelper<T1, T2>(
            Func<Zen<T1>, Zen<T2>> function,
            T1 value1,
            ImmutableDictionary<long, object> args)
        {
            var expression = function(Constant(value1));
            return Run(expression, new Dictionary<long, object>(args), false).Item1;
        }
    }
}
