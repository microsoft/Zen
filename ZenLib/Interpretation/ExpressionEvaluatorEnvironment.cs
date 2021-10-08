// <copyright file="InterpreterEnvironment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// An environment for the interpreter.
    /// </summary>
    internal class ExpressionEvaluatorEnvironment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluatorEnvironment"/> class.
        /// </summary>
        public ExpressionEvaluatorEnvironment(Dictionary<object, object> arbitraryAssignment)
        {
            this.ArbitraryAssignment = arbitraryAssignment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluatorEnvironment"/> class.
        /// </summary>
        /// <param name="argumentAssignment">The initial argument assignment.</param>
        public ExpressionEvaluatorEnvironment(Dictionary<long, object> argumentAssignment)
        {
            this.ArgumentAssignment = argumentAssignment;
        }

        /// <summary>
        /// Gets the argument assignment.
        /// </summary>
        public Dictionary<long, object> ArgumentAssignment { get; } = new Dictionary<long, object>();

        /// <summary>
        /// Get the arbitrary assignment.
        /// </summary>
        public Dictionary<object, object> ArbitraryAssignment { get; }
    }
}