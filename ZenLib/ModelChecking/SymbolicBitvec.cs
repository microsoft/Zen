// <copyright file="SymbolicInteger.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic integer value.
    /// </summary>
    internal class SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
    {
        /// <summary>
        /// The symbolic bitvector value.
        /// </summary>
        public TBitvec Value { get; }

        /// <summary>
        /// Create a new instance of the <see cref="SymbolicBitvec{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="value">The bitvector value.</param>
        public SymbolicBitvec(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver, TBitvec value) : base(solver)
        {
            this.Value = value;
        }

        /// <summary>
        /// Merge another symbolic value with respect to a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other symbolic value.</param>
        /// <returns>A merged symbolic value.</returns>
        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> other)
        {
            var o = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, value);
        }

        /// <summary>
        /// Accept a visitor.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal, TReturn, TParam> visitor,
            TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }

        /// <summary>
        /// Convert the object to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "<symbitvec>";
        }
    }
}
