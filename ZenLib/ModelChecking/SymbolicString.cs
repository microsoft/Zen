// <copyright file="SymbolicString.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic string value.
    /// </summary>
    internal class SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>
    {
        public SymbolicString(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> solver, TSeq value) : base(solver)
        {
            this.Value = value;
        }

        public TSeq Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> other)
        {
            var o = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>(this.Solver, value);
        }

        internal override TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TReturn, TParam> visitor,
            TParam parameter)
        {
            return visitor.VisitSymbolicString(this, parameter);
        }

        /// <summary>
        /// Convert the object to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "<symstring>";
        }
    }
}
