// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray>
    {
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Solver;

        public SymbolicValue(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> solver)
        {
            this.Solver = solver;
        }

        internal abstract SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray> other);

        internal abstract TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TReturn, TParam> visitor,
            TParam parameter);
    }
}
