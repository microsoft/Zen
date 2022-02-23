// <copyright file="ModelCheckerEnvironment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// An environment for the symbolic evaluator.
    /// </summary>
    internal class SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SymbolicEvaluationEnvironment(Dictionary<long, object> argumentsToExpr)
        {
            this.ArgumentsToExpr = argumentsToExpr;
            this.ArgumentsToValue = ImmutableDictionary<long, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the  class.
        /// </summary>
        /// <param name="argumentsToExpr">The arguments to expr assignment.</param>
        /// <param name="argumentAssignment">The initial argument assignment.</param>
        public SymbolicEvaluationEnvironment(
            Dictionary<long, object> argumentsToExpr,
            ImmutableDictionary<long, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>> argumentAssignment)
        {
            this.ArgumentsToValue = argumentAssignment;
            this.ArgumentsToExpr = argumentsToExpr;
        }

        /// <summary>
        /// Gets the argument to value assignment.
        /// </summary>
        public ImmutableDictionary<long, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>> ArgumentsToValue { get; }

        /// <summary>
        /// Gets the argument to expression assignment.
        /// </summary>
        public Dictionary<long, object> ArgumentsToExpr { get; }
    }
}
