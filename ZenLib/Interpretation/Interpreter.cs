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
            var environment = new ExpressionEvaluatorEnvironment(args);
            var interpreter = new ExpressionEvaluator(trackBranches);
            var result = (T)interpreter.Evaluate(expression, environment);
            return (result, interpreter.PathConstraint);
        }

        public static T3 CompileRunHelper<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> function,
            T1 value1,
            T2 value2,
            ImmutableDictionary<long, object> args)
        {
            var expression = function(Constant(value1), Constant(value2));
            return Run(expression, new Dictionary<long, object>(args), false).Item1;
        }
    }
}
