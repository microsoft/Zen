﻿// <copyright file="SymbolicClass.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using ZenLib.Solver;

    /// <summary>
    /// Representation of a symbolic boolean value.
    /// </summary>
    internal class SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>
    {
        public SymbolicClass(ISolver<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> solver, ImmutableDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>> value) : base(solver)
        {
            this.Fields = value;
        }

        /// <summary>
        /// Gets the underlying decision diagram bitvector representation.
        /// </summary>
        public ImmutableDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>> Fields { get; set; }

        /// <summary>
        /// Merge two symbolic integers together under a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other integer.</param>
        /// <returns>A new symbolic integer.</returns>
        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray> other)
        {
            var o = (SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>)other;
            var newValue = ImmutableDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>>.Empty;
            foreach (var kv in this.Fields)
            {
                var value1 = kv.Value;
                var value2 = o.Fields[kv.Key];
                newValue = newValue.Add(kv.Key, value1.Merge(guard, value2));
            }

            return new SymbolicClass<TModel, TVar, TBool, TBitvec, TInt, TString, TArray>(this.Solver, newValue);
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
