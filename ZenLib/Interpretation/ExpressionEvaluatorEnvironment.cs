// <copyright file="InterpreterEnvironment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

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
        public ExpressionEvaluatorEnvironment(ImmutableDictionary<string, object> argumentAssignment)
        {
            this.ArgumentAssignment = argumentAssignment;
        }

        /// <summary>
        /// Gets the argument assignment.
        /// </summary>
        public ImmutableDictionary<string, object> ArgumentAssignment { get; } = ImmutableDictionary<string, object>.Empty;

        /// <summary>
        /// Get the arbitrary assignment.
        /// </summary>
        public Dictionary<object, object> ArbitraryAssignment { get; }

        /// <summary>
        /// Equality of type environments.
        /// </summary>
        /// <param name="obj">The other environment.</param>
        /// <returns>True or false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            var e = (ExpressionEvaluatorEnvironment)obj;

            return object.Equals(this.ArbitraryAssignment, e.ArbitraryAssignment) &&
                   this.ArgumentAssignment.Equals(e.ArgumentAssignment);
        }

        /// <summary>
        /// Hash code for an environment.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return (this.ArbitraryAssignment == null ? 0 : this.ArbitraryAssignment.GetHashCode()) +
                   this.ArgumentAssignment.GetHashCode();
        }
    }
}
