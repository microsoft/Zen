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
        /// Merge another symbolic value with respect to a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other symbolic value.</param>
        /// <returns>A merged symbolic value.</returns>
        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> other)
        {
            var o = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)other;
            var result = Merge(guard, this.GuardedListGroup, o.GuardedListGroup);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
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
        /// Merge two groups of guarded lists together.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="lists1">The first lists.</param>
        /// <param name="lists2">The second lists.</param>
        /// <returns>A new guarded group of lists.</returns>
        private GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> lists1,
            GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> lists2)
        {
            var result = CommonUtilities.Merge(lists1.Mapping, lists2.Mapping, (len, list1, list2) =>
            {
                if (list1.HasValue && !list2.HasValue)
                {
                    var newList = MapGuard(guard, list1.Value);
                    return Option.Some(newList);
                }

                if (!list1.HasValue && list2.HasValue)
                {
                    var newList = MapGuard(this.Solver.Not(guard), list2.Value);
                    return Option.Some(newList);
                }

                var merged = Merge(guard, list1.Value, list2.Value);

                /* if (merged.Guard.Equals(this.Solver.False()))
                {
                    return Option.None<GuardedList<TModel, TVar, TBool, TInt, TSeq, TArray, TChar, TReal>>();
                } */

                return Option.Some(merged);
            });

            return new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(result);
        }

        /// <summary>
        /// Map the guard over a list.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> MapGuard(TBool guard, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> list)
        {
            var newGuard = this.Solver.And(guard, list.Guard);
            return new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(newGuard, list.Values);
        }

        /// <summary>
        /// Merge two guarded lists together.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="list1">The first list.</param>
        /// <param name="list2">The second list.</param>
        /// <returns></returns>
        private GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> list1,
            GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> list2)
        {
            var newGuard = this.Solver.Ite(guard, list1.Guard, list2.Guard);
            var newValues = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            for (int i = 0; i < list1.Values.Count; i++)
            {
                var v1 = list1.Values[i];
                var v2 = list2.Values[i];
                newValues = newValues.Add(v1.Merge(guard, v2));
            }

            return new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(newGuard, newValues);
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
