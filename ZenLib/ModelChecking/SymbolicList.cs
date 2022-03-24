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
        public SymbolicList(ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver, GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> value) : base(solver)
        {
            this.GuardedListGroup = value;
        }

        /// <summary>
        /// Gets the guarded lists representing the symbolic list.
        /// </summary>
        public GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> GuardedListGroup { get; }

        [ExcludeFromCodeCoverage]
        internal override TReturn Accept<TParam, TReturn>(
            ISymbolicValueVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal, TReturn, TParam> visitor,
            TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }

        /// <summary>
        /// Merge two symbolic lists together under a guard.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="other">The other list.</param>
        /// <returns>A new symbolic list.</returns>
        internal override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            TBool guard,
            SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> other)
        {
            var o = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)other;
            var result = Merge(guard, this.GuardedListGroup, o.GuardedListGroup);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.Solver, result);
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
        public GuardedListGroup(ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> mapping)
        {
            this.Mapping = mapping;
        }

        public ImmutableDictionary<int, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Mapping { get; }

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
        public GuardedList(TBool guard, ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> values)
        {
            this.Guard = guard;
            this.Values = values;
        }

        public TBool Guard { get; }

        public ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>> Values { get; }

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
