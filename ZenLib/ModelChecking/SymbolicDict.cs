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
    internal class SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>
    {
        public SymbolicDict(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> solver, TArray value) : base(solver)
        {
            this.Value = value;
        }

        public TArray Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> other)
        {
            var o = (SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicDict<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>(this.Solver, value);
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
