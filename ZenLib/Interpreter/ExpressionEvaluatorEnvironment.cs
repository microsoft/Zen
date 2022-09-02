// <copyright file="InterpreterEnvironment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System.Collections.Immutable;

    /// <summary>
    /// An environment for the interpreter.
    /// </summary>
    internal class ExpressionEvaluatorEnvironment
    {
        /// <summary>
        /// Gets the argument assignment.
        /// </summary>
        public ImmutableDictionary<long, object> ArgumentAssignment { get; set; } = ImmutableDictionary<long, object>.Empty;

        /// <summary>
        /// Get the arbitrary assignment.
        /// </summary>
        public ImmutableDictionary<object, object> ArbitraryAssignment { get; set; }

        /// <summary>
        /// Creates a new environment with the binding added.
        /// </summary>
        /// <returns></returns>
        public ExpressionEvaluatorEnvironment AddBinding(long id, object value)
        {
            return new ExpressionEvaluatorEnvironment
            {
                ArgumentAssignment = this.ArgumentAssignment.SetItem(id, value),
                ArbitraryAssignment = this.ArbitraryAssignment,
            };
        }
    }
}