// <copyright file="SymbolicList.cs" company="Microsoft">
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
    internal class SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> : SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
    {
        /// <summary>
        /// The guarded lists representing the symbolic list.
        /// </summary>
        public GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> GuardedListGroup { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolicList{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="value">The symbolic value.</param>
        public SymbolicList(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver, GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> value) : base(solver)
        {
            this.GuardedListGroup = value;
        }

        /// <summary>
        /// Accept a visitor.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The return value.</returns>
        [ExcludeFromCodeCoverage]
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
            return this.GuardedListGroup.ToString();
        }
    }

    /// <summary>
    /// Represents a guarded group of lists, distinguishing by length.
    /// </summary>
    internal class GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
    {
        /// <summary>
        /// The mapping from list length to symbolic values.
        /// </summary>
        public ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Mapping { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="GuardedListGroup{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        public GuardedListGroup(ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> mapping)
        {
            this.Mapping = mapping;
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
            foreach (var kv in this.Mapping)
            {
                sb.Append($"{kv.Key} --> {kv.Value}, ");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A single list with a fixed length guarded by a predicate.
    /// </summary>
    internal class GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>
    {
        /// <summary>
        /// The symbolic guard.
        /// </summary>
        public TBool Guard { get; }

        /// <summary>
        /// The symbolic values for the list.
        /// </summary>
        public ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Values { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="GuardedList{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="guard">The symbolic guard.</param>
        /// <param name="values">The symbolic values.</param>
        public GuardedList(TBool guard, ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> values)
        {
            this.Guard = guard;
            this.Values = values;
        }

        /// <summary>
        /// Convert the object to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{this.Guard}: [");
            foreach (var value in this.Values)
            {
                sb.Append(value.ToString());
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}
