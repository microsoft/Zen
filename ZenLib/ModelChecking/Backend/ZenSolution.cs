﻿// <copyright file="ZenSolution.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using ZenLib;
    using ZenLib.Interpretation;

    /// <summary>
    /// Represents a solution to a zen boolean expression.
    /// </summary>
    public class ZenSolution
    {
        /// <summary>
        /// Assignment from arbitrary variables to C# values.
        /// </summary>
        public Dictionary<object, object> VariableAssignment;

        /// <summary>
        /// Creates a new instance of the <see cref="ZenSolution"/> class.
        /// </summary>
        /// <param name="arbitraryAssignment">The arbitrary assignment.</param>
        internal ZenSolution(Dictionary<object, object> arbitraryAssignment)
        {
            VariableAssignment = arbitraryAssignment;
        }

        /// <summary>
        /// Returns if the solution is satisfiable.
        /// </summary>
        /// <returns>True if the solution exists.</returns>
        public bool IsSatisfiable()
        {
            return VariableAssignment != null;
        }

        /// <summary>
        /// Gets a value from the assignment.
        /// </summary>
        /// <typeparam name="T">The type of the expected value.</typeparam>
        /// <param name="expr">The expression to get in the solution.</param>
        /// <returns>The C# value associated with the expression.</returns>
        public T Get<T>(Zen<T> expr)
        {
            if (VariableAssignment != null && VariableAssignment.TryGetValue(expr, out var value))
            {
                return (T)value;
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(VariableAssignment) };
            var interpreter = new ExpressionEvaluatorVisitor(false);
            return (T)interpreter.Visit(expr, interpreterEnv);
        }
    }
}