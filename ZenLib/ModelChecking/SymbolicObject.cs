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
        /// <summary>
        /// Gets the type of the object it represents.
        /// </summary>
        public Type ObjectType;

        /// <summary>
        /// Gets the underlying decision diagram bitvector representation.
        /// </summary>
        public ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Fields { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolicObject{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="solver">The solver.</param>
        /// <param name="value">The symbolic values.</param>
        public SymbolicObject(
            Type objectType,
            ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver,
            ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> value) : base(solver)
        {
            this.ObjectType = objectType;
            this.Fields = value;
        }

        /// <summary>
        /// Merge another symbolic value with respect to a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other symbolic value.</param>
        /// <returns>A merged symbolic value.</returns>
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
        /// Accept a visitor.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal, TReturn, TParam> visitor,
            TParam parameter)
        {
            return visitor.Visit(this, parameter);
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
