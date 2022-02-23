// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar>
    {
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Solver;

        public SymbolicValue(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> solver)
        {
            this.Solver = solver;
        }

        internal abstract SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar> other);

        internal abstract TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReturn, TParam> visitor,
            TParam parameter);
    }
}
