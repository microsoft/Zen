// <copyright file="SymbolicInteger.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Research.Zen.ModelChecking
{
    /// <summary>
    /// Representation of a symbolic boolean value.
    /// </summary>
    internal class SymbolicInteger<TModel, TVar, TBool, TInt> : SymbolicValue<TModel, TVar, TBool, TInt>
    {
        public SymbolicInteger(ISolver<TModel, TVar, TBool, TInt> solver, TInt value) : base(solver)
        {
            this.Value = value;
        }

        public TInt Value { get; }

        internal override SymbolicValue<TModel, TVar, TBool, TInt> Merge(TBool guard, SymbolicValue<TModel, TVar, TBool, TInt> other)
        {
            var o = (SymbolicInteger<TModel, TVar, TBool, TInt>)other;
            var value = this.Solver.Ite(guard, this.Value, o.Value);
            return new SymbolicInteger<TModel, TVar, TBool, TInt>(this.Solver, value);
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
