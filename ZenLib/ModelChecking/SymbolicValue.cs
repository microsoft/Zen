// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>
    {
        public ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> Solver;

        public SymbolicValue(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> solver)
        {
            this.Solver = solver;
        }

        internal abstract SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> other);
    }
}
