// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
    {
        /// <summary>
        /// The solver.
        /// </summary>
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Solver;

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolicValue{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public SymbolicValue(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver)
        {
            this.Solver = solver;
        }

        /// <summary>
        /// Merge another symbolic value with respect to a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other symbolic value.</param>
        /// <returns>A merged symbolic value.</returns>
        internal abstract SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> other);

        /// <summary>
        /// Accept a visitor.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The return value.</returns>
        internal abstract TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal, TReturn, TParam> visitor,
            TParam parameter);
    }
}
