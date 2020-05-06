// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TInt>
    {
        public ISolver<TModel, TVar, TBool, TInt> Solver;

        public SymbolicValue(ISolver<TModel, TVar, TBool, TInt> solver)
        {
            this.Solver = solver;
        }

        internal abstract SymbolicValue<TModel, TVar, TBool, TInt> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TInt> other);
    }
}
