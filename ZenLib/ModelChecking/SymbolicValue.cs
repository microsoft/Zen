// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>
    {
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> Solver;

        public SymbolicValue(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> solver)
        {
            this.Solver = solver;
        }

        internal abstract SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> other);
    }
}
