// <copyright file="SymbolicDict.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic dictionary value.
    /// </summary>
    internal class SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>
    {
        public SymbolicDict(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> solver, TArray value) : base(solver)
        {
            this.Value = value;
        }

        public TArray Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> other)
        {
            var o = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, value);
        }

        internal override TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TReturn, TParam> visitor,
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
            return "<symdict>";
        }
    }
}
