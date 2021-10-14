// <copyright file="ZenFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using ZenLib.Interpretation;

    /// <summary>
    /// Represents a solution to a zen boolean expression.
    /// </summary>
    public class Solution
    {
        /// <summary>
        /// Assignment from arbitrary variables to C# values.
        /// </summary>
        internal Dictionary<object, object> ArbitraryAssignment;

        /// <summary>
        /// Creates a new instance of the <see cref="Solution"/> class.
        /// </summary>
        /// <param name="arbitraryAssignment">The arbitrary assignment.</param>
        internal Solution(Dictionary<object, object> arbitraryAssignment)
        {
            this.ArbitraryAssignment = arbitraryAssignment;
        }

        /// <summary>
        /// Gets a value from the assignment.
        /// </summary>
        /// <typeparam name="T">The type of the expected value.</typeparam>
        /// <param name="expr">The expression to get in the solution.</param>
        /// <returns>The C# value associated with the expression.</returns>
        public T Get<T>(Zen<T> expr)
        {
            if (this.ArbitraryAssignment.TryGetValue(expr, out var value))
            {
                return (T)value;
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(this.ArbitraryAssignment);
            var result = expr.Accept(new ExpressionEvaluator(false), interpreterEnv);
            return CommonUtilities.ConvertSymbolicResultToCSharp<T>(result);
        }
    }
}