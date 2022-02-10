// <copyright file="SymbolicBool.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic boolean value.
    /// </summary>
    internal class SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>
    {
        public SymbolicBool(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> solver, TBool value) : base(solver)
        {
            this.Value = value;
        }

        public TBool Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> Merge(TBool guard, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> other)
        {
            var o = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>(this.Solver, value);
        }

        /// <summary>
        /// Convert the object to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (this.Value.Equals(this.Solver.False()))
            {
                return "false";
            }

            if (this.Value.Equals(this.Solver.True()))
            {
                return "true";
            }

            return "<symbool>";
        }
    }
}
