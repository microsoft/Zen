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
    internal class SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString>
    {
        public SymbolicBitvec(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString> solver, TBitvec value) : base(solver)
        {
            this.Value = value;
        }

        public TBitvec Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> Merge(TBool guard, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString> other)
        {
            var o = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TString>(this.Solver, value);
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
