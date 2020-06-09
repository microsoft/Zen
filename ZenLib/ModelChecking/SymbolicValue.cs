// <copyright file="SymbolicValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    /// <summary>
    /// Representation of a symbolic value.
    /// </summary>
    internal abstract class SymbolicValue<TModel, TVar, TBool, TInt, TString>
    {
        public ISolver<TModel, TVar, TBool, TInt, TString> Solver;

        public SymbolicValue(ISolver<TModel, TVar, TBool, TInt, TString> solver)
        {
            this.Solver = solver;
        }

        internal abstract SymbolicValue<TModel, TVar, TBool, TInt, TString> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TInt, TString> other);
    }
}
