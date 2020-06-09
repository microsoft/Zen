// <copyright file="SymbolicInteger.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Representation of a symbolic boolean value.
    /// </summary>
    internal class SymbolicInteger<TModel, TVar, TBool, TInt, TString> : SymbolicValue<TModel, TVar, TBool, TInt, TString>
    {
        public SymbolicInteger(ISolver<TModel, TVar, TBool, TInt, TString> solver, TInt value) : base(solver)
        {
            this.Value = value;
        }

        public TInt Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TInt, TString> Merge(TBool guard, SymbolicValue<TModel, TVar, TBool, TInt, TString> other)
        {
            var o = (SymbolicInteger<TModel, TVar, TBool, TInt, TString>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicInteger<TModel, TVar, TBool, TInt, TString>(this.Solver, value);
        }

        /// <summary>
        /// Convert the object to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "<symint>";
        }
    }
}
