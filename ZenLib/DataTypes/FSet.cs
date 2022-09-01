// <copyright file="FSet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple finite set.
    /// </summary>
    public class FSet<T> : IEquatable<FSet<T>>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        public FSeq<T> Values { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FSet{T}"/> class.
        /// </summary>
        public FSet()
        {
            this.Values = new FSeq<T>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSet{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public FSet(params T[] values)
        {
            this.Values = new FSeq<T>(values);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSet{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public FSet(IEnumerable<T> values)
        {
            this.Values = new FSeq<T>(values);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FSet{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        private FSet(FSeq<T> values)
        {
            this.Values = values;
        }

        /// <summary>
        /// Get the elements of the set.
        /// </summary>
        /// <returns></returns>
        public ISet<T> ToSet()
        {
            return new HashSet<T>(this.Values.Values.Where(x => x.HasValue).Select(x => x.Value));
        }

        /// <summary>
        /// Add an element to the set.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>A new multiset with the element added.</returns>
        public FSet<T> Add(T element)
        {
            return new FSet<T>(this.Values.AddFront(element));
        }

        /// <summary>
        /// Remove an element from the multiset.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        /// <returns>A new multiset with the element removed.</returns>
        public FSet<T> Remove(T element)
        {
            var list = ImmutableList.Create<T>();
            foreach (var value in this.Values.Values)
            {
                if (value.HasValue && !value.Value.Equals(element))
                {
                    list = list.Add(value.Value);
                }
            }

            return new FSet<T>(new FSeq<T>(list));
        }

        /// <summary>
        /// Remove element from the multiset matching a predicate.
        /// </summary>
        /// <param name="function">The predicate over elements.</param>
        /// <returns>A new multiset with the matching elements removed.</returns>
        public FSet<T> Where(Func<T, bool> function)
        {
            var list = ImmutableList.Create<T>();
            foreach (var value in this.Values.Values)
            {
                if (value.HasValue && function(value.Value))
                {
                    list = list.Add(value.Value);
                }
            }

            return new FSet<T>(new FSeq<T>(list));
        }

        /// <summary>
        /// Determines if any element of the set satisfies some predicate.
        /// </summary>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if an element satisfies the predicate.</returns>
        public bool Any(Func<T, bool> function)
        {
            foreach (var value in this.Values.Values)
            {
                if (value.HasValue && function(value.Value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if all elements of the set satisfy some predicate.
        /// </summary>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if all elements satisfy the predicate.</returns>
        public bool All(Func<T, bool> function)
        {
            foreach (var value in this.Values.Values)
            {
                if (value.HasValue && !function(value.Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove element from the multiset matching a predicate.
        /// </summary>
        /// <param name="function">The predicate over elements.</param>
        /// <returns>A new multiset with the matching elements removed.</returns>
        public FSet<TResult> Select<TResult>(Func<T, TResult> function)
        {
            var list = ImmutableList.Create<TResult>();
            foreach (var value in this.Values.Values)
            {
                if (value.HasValue)
                {
                    list = list.Add(function(value.Value));
                }
            }

            return new FSet<TResult>(new FSeq<TResult>(list));
        }

        /// <summary>
        /// Determines if the set is empty.
        /// </summary>
        /// <returns>Whether the set contains no elements.</returns>
        public bool IsEmpty()
        {
            foreach (var element in this.Values.Values)
            {
                if (element.HasValue)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the size of the multiset.
        /// </summary>
        /// <returns>The number of elements in the multiset.</returns>
        public int Size()
        {
            return this.ToSet().Count;
        }

        /// <summary>
        /// Convert the array to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var elements = this.Values.Values.Where(x => x.HasValue).Select(x => x.Value).ToImmutableHashSet();
            return "{" + string.Join(",", elements) + "}";
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is FSet<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>True or false.</returns>
        public bool Equals(FSet<T> other)
        {
            var s1 = this.ToSet();
            var s2 = other.ToSet();
            return s1.SetEquals(s2);
        }

        /// <summary>
        /// Hashcode for sets.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            return this.Values.GetHashCode();
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="left">The left set.</param>
        /// <param name="right">The right set.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(FSet<T> left, FSet<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="left">The left set.</param>
        /// <param name="right">The right set.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(FSet<T> left, FSet<T> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for FSet Zen objects.
    /// </summary>
    public static class FSet
    {
        /// <summary>
        /// The Zen value for a set from a sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<FSet<T>> Create<T>(Zen<FSeq<T>> seqExpr)
        {
            return Zen.Create<FSet<T>>(("Values", seqExpr));
        }

        /// <summary>
        /// Create an array from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSet<T>> Create<T>(params Zen<T>[] elements)
        {
            Contract.AssertNotNull(elements);

            return FSet.Create<T>(FSeq.Create(elements));
        }

        /// <summary>
        /// Gets the underlying sequence for a set.
        /// </summary>
        /// <param name="setExpr">The set expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<FSeq<T>> Values<T>(this Zen<FSet<T>> setExpr)
        {
            Contract.AssertNotNull(setExpr);

            return setExpr.GetField<FSet<T>, FSeq<T>>("Values");
        }

        /// <summary>
        /// Checks if the set contains an element.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="value">The value to check for containment.</param>
        /// <returns>Zen value indicating the containment.</returns>
        public static Zen<bool> Contains<T>(this Zen<FSet<T>> setExpr, Zen<T> value)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(value);

            return setExpr.Values().Any(o => o == value);
        }

        /// <summary>
        /// Add a value to a set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="value">The value to add to the set.</param>
        /// <returns>The new set from adding the value.</returns>
        public static Zen<FSet<T>> Add<T>(this Zen<FSet<T>> setExpr, Zen<T> value)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(value);

            return FSet.Create(setExpr.Values().AddFront(value));
        }

        /// <summary>
        /// Union two sets together.
        /// </summary>
        /// <param name="setExpr1">Zen first set expression.</param>
        /// <param name="setExpr2">Zen second set expression.</param>
        /// <returns>The new set from the union.</returns>
        public static Zen<FSet<T>> Union<T>(this Zen<FSet<T>> setExpr1, Zen<FSet<T>> setExpr2)
        {
            Contract.AssertNotNull(setExpr1);
            Contract.AssertNotNull(setExpr2);

            return FSet.Create(setExpr1.Values().Append(setExpr2.Values()));
        }

        /// <summary>
        /// Determines if a set is a subset of another set.
        /// </summary>
        /// <param name="setExpr1">Zen first set expression.</param>
        /// <param name="setExpr2">Zen second set expression.</param>
        /// <returns>True if all elements of the furst set are contained in the second.</returns>
        public static Zen<bool> IsSubsetOf<T>(this Zen<FSet<T>> setExpr1, Zen<FSet<T>> setExpr2)
        {
            Contract.AssertNotNull(setExpr1);
            Contract.AssertNotNull(setExpr2);

            return setExpr1.Values().All(x => setExpr2.Contains(x));
        }

        /// <summary>
        /// Remove all occurences of a value from a set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="value">The value to add to the set.</param>
        /// <returns>The new set from adding the value.</returns>
        public static Zen<FSet<T>> Remove<T>(this Zen<FSet<T>> setExpr, Zen<T> value)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(value);

            return FSet.Create(setExpr.Values().RemoveAll(value));
        }

        /// <summary>
        /// Filters elements from the set matching a given predicate.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>The new set from removing the elements.</returns>
        public static Zen<FSet<T>> Where<T>(this Zen<FSet<T>> setExpr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(function);

            return FSet.Create(setExpr.Values().Where(function));
        }

        /// <summary>
        /// Map a function over a set to produce a new set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>The new set from mapping over all the elements.</returns>
        public static Zen<FSet<TResult>> Select<T, TResult>(this Zen<FSet<T>> setExpr, Func<Zen<T>, Zen<TResult>> function)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(function);

            return FSet.Create(setExpr.Values().Select(function));
        }

        /// <summary>
        /// Determines if any element of the set satisfies some predicate.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if an element satisfies the predicate.</returns>
        public static Zen<bool> Any<T>(this Zen<FSet<T>> setExpr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(function);

            return setExpr.Values().Any(function);
        }

        /// <summary>
        /// Determines if all elements of the set satisfy some predicate.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if all elements satisfy the predicate.</returns>
        public static Zen<bool> All<T>(this Zen<FSet<T>> setExpr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(function);

            return setExpr.Values().All(function);
        }

        /// <summary>
        /// Determines if the set is empty.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <returns>A boolean indicating if the set is empty.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<FSet<T>> setExpr)
        {
            Contract.AssertNotNull(setExpr);

            return setExpr.Values().IsEmpty();
        }

        /// <summary>
        /// Compute the size of the set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <returns>The new set from adding the value.</returns>
        public static Zen<ushort> Size<T>(this Zen<FSet<T>> setExpr)
        {
            Contract.AssertNotNull(setExpr);

            return Size(setExpr.Values());
        }

        /// <summary>
        /// Compute the size of the set sequence.
        /// </summary>
        /// <param name="seqExpr">Zen set expression.</param>
        /// <returns>The count of unique elements.</returns>
        private static Zen<ushort> Size<T>(Zen<FSeq<T>> seqExpr)
        {
            return seqExpr.Case(0, (hd, tl) =>
            {
                return Size(tl) + If<ushort>(And(hd.IsSome(), Not(tl.Contains(hd.Value()))), 1, 0);
            });
        }
    }
}