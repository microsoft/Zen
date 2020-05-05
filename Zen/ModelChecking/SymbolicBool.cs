// <copyright file="SymbolicBool.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.ModelChecking
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Representation of a symbolic boolean value.
    /// </summary>
    internal class SymbolicBool<TModel, TVar, TBool, TInt> : SymbolicValue<TModel, TVar, TBool, TInt>
    {
        public SymbolicBool(ISolver<TModel, TVar, TBool, TInt> solver, TBool value) : base(solver)
        {
            this.Value = value;
        }

        public TBool Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TInt> Merge(TBool guard, SymbolicValue<TModel, TVar, TBool, TInt> other)
        {
            var o = (SymbolicBool<TModel, TVar, TBool, TInt>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicBool<TModel, TVar, TBool, TInt>(this.Solver, value);
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
