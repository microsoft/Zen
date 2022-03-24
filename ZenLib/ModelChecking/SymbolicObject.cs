// <copyright file="SymbolicObject.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic boolean value.
    /// </summary>
    internal class SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
    {
        public SymbolicObject(
            Type objectType,
            ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver,
            ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> value) : base(solver)
        {
            this.ObjectType = objectType;
            this.Fields = value;
        }

        /// <summary>
        /// Gets the type of the object it represents.
        /// </summary>
        public Type ObjectType;

        /// <summary>
        /// Gets the underlying decision diagram bitvector representation.
        /// </summary>
        public ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Fields { get; set; }

        internal override TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal, TReturn, TParam> visitor,
            TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }

        /// <summary>
        /// Merge two symbolic integers together under a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other integer.</param>
        /// <returns>A new symbolic integer.</returns>
        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> other)
        {
            var o = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)other;
            var newValue = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            foreach (var kv in this.Fields)
            {
                var value1 = kv.Value;
                var value2 = o.Fields[kv.Key];
                newValue = newValue.Add(kv.Key, value1.Merge(guard, value2));
            }

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.ObjectType, this.Solver, newValue);
        }

        /// <summary>
        /// Convert the object to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (var kv in this.Fields)
            {
                sb.Append($"{kv.Key} = {kv.Value}");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
