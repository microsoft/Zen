// <copyright file="FBag.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple finite unordered multi-set.
    /// </summary>
    public class FBag<T> : IEquatable<FBag<T>>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        [ZenSize(enumerationType: EnumerationType.FixedSize)]
        public FSeq<Option<T>> Values { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FBag{T}"/> class.
        /// </summary>
        public FBag()
        {
            this.Values = new FSeq<Option<T>>();
        }

        private FBag(FSeq<Option<T>> values)
        {
            this.Values = values;
        }

        /// <summary>
        /// Get the elements of the bag as a set.
        /// </summary>
        /// <returns></returns>
        public ISet<T> ToSet()
        {
            return new HashSet<T>(this.Values.Values.Where(x => x.HasValue).Select(x => x.Value));
        }

        /// <summary>
        /// Get the elements of the bag as a list with duplicates.
        /// </summary>
        /// <returns></returns>
        public IList<T> ToList()
        {
            return new List<T>(this.Values.Values.Where(x => x.HasValue).Select(x => x.Value));
        }

        /// <summary>
        /// Convert this Zen Bag to a C# array.
        /// </summary>
        /// <returns>A C# array.</returns>
        public T[] ToArray()
        {
            return this.Values.Values.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        }

        /// <summary>
        /// Add an element to the multiset.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>A new multiset with the element added.</returns>
        public FBag<T> Add(T element)
        {
            return new FBag<T>(this.Values.AddFront(Option.Some(element)));
        }

        /// <summary>
        /// Remove an element from the multiset.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        /// <returns>A new multiset with the element removed.</returns>
        public FBag<T> Remove(T element)
        {
            var list = ImmutableList.Create<Option<T>>();
            foreach (var value in this.Values.Values)
            {
                list = list.Add(value.Where(x => !x.Equals(element)));
            }

            return new FBag<T>(new FSeq<Option<T>>(list));
        }

        /// <summary>
        /// Remove element from the multiset matching a predicate.
        /// </summary>
        /// <param name="function">The predicate over elements.</param>
        /// <returns>A new multiset with the matching elements removed.</returns>
        public FBag<T> Where(Func<T, bool> function)
        {
            var list = ImmutableList.Create<Option<T>>();
            foreach (var value in this.Values.Values)
            {
                list = list.Add(value.Where(function));
            }

            return new FBag<T>(new FSeq<Option<T>>(list));
        }

        /// <summary>
        /// Determines if any element of the bag satisfies some predicate.
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
        /// Determines if all elements of the bag satisfy some predicate.
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
        public FBag<TResult> Select<TResult>(Func<T, TResult> function)
        {
            var list = ImmutableList.Create<Option<TResult>>();
            foreach (var value in this.Values.Values)
            {
                list = list.Add(value.Select(function));
            }

            return new FBag<TResult>(new FSeq<Option<TResult>>(list));
        }

        /// <summary>
        /// Determines if the bag is empty.
        /// </summary>
        /// <returns>Whether the bag contains no elements.</returns>
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
            int count = 0;
            foreach (var value in this.Values.Values)
            {
                if (value.HasValue)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Convert the array to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var elements = this.Values.Values.Where(x => x.HasValue).Select(x => x.Value).ToArray();
            return "{" + string.Join(",", elements) + "}";
        }

        /// <summary>
        /// Equality for bags.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is FBag<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for bags.
        /// </summary>
        /// <param name="other">The other bag.</param>
        /// <returns>True or false.</returns>
        public bool Equals(FBag<T> other)
        {
            return this.Values.Equals(other.Values);
        }

        /// <summary>
        /// Hashcode for bags.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            return this.Values.GetHashCode();
        }

        /// <summary>
        /// Equality for bags.
        /// </summary>
        /// <param name="left">The left bag.</param>
        /// <param name="right">The right bag.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(FBag<T> left, FBag<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Equality for bags.
        /// </summary>
        /// <param name="left">The left bag.</param>
        /// <param name="right">The right bag.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(FBag<T> left, FBag<T> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for Bag Zen objects.
    /// </summary>
    public static class FBag
    {
        /// <summary>
        /// Convert a collection of items to a Bag.
        /// </summary>
        /// <param name="values">The items to add to the bag.</param>
        public static FBag<T> FromRange<T>(IEnumerable<T> values)
        {
            return new FBag<T> { Values = new FSeq<Option<T>> { Values = ImmutableList.CreateRange(values.Select(Option.Some)) } };
        }

        /// <summary>
        /// The Zen value for a bag from a sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<FBag<T>> Create<T>(Zen<FSeq<Option<T>>> seqExpr)
        {
            return Zen.Create<FBag<T>>(("Values", seqExpr));
        }

        /// <summary>
        /// Create an array from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FBag<T>> Create<T>(params Zen<T>[] elements)
        {
            CommonUtilities.ValidateNotNull(elements);

            var asOptions = elements.Select(Option.Create);
            return FBag.Create(FSeq.Create(asOptions));
        }

        /// <summary>
        /// Gets the underlying sequence for a bag.
        /// </summary>
        /// <param name="bagExpr">The bag expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<FSeq<Option<T>>> Values<T>(this Zen<FBag<T>> bagExpr)
        {
            CommonUtilities.ValidateNotNull(bagExpr);

            return bagExpr.GetField<FBag<T>, FSeq<Option<T>>>("Values");
        }

        /// <summary>
        /// Checks if the bag contains an element.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to check for containment.</param>
        /// <returns>Zen value indicating the containment.</returns>
        public static Zen<bool> Contains<T>(this Zen<FBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return bagExpr.Values().Any(o => And(o.IsSome(), o.Value() == value));
        }

        /// <summary>
        /// Add a value to a bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to add to the bag.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<FBag<T>> Add<T>(this Zen<FBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return FBag.Create(bagExpr.Values().AddFront(Option.Create(value)));
        }

        /// <summary>
        /// Add a value to a bag if space given the maximum size.
        /// If no space, then replace the last value.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to add to the bag.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<FBag<T>> AddIfSpace<T>(this Zen<FBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return FBag.Create(AddIfSpace(bagExpr.Values(), value));
        }

        /// <summary>
        /// Add a value to a sequence if there is space.
        /// Otherwise, replace the last value.
        /// </summary>
        /// <param name="seqExpr">Zen sequence expression.</param>
        /// <param name="value">The value to add to the sequence.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<FSeq<Option<T>>> AddIfSpace<T>(this Zen<FSeq<Option<T>>> seqExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(value);

            return seqExpr.Case(seqExpr, (hd, tl) =>
                If(hd.IsSome(), AddIfSpace(tl, value).AddFront(hd), tl.AddFront(Option.Create(value))));
        }

        /// <summary>
        /// Remove all occurences of a value from a bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="value">The value to add to the bag.</param>
        /// <returns>The new bag from adding the value..</returns>
        public static Zen<FBag<T>> Remove<T>(this Zen<FBag<T>> bagExpr, Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(value);

            return FBag.Create(bagExpr.Values().Select(o => If(And(o.IsSome(), o.Value() == value), Option.Null<T>(), o)));
        }

        /// <summary>
        /// Filters elements from the bag matching a given predicate.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>The new bag from removing the elements.</returns>
        public static Zen<FBag<T>> Where<T>(this Zen<FBag<T>> bagExpr, Func<Zen<T>, Zen<bool>> function)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(function);

            return FBag.Create(bagExpr.Values().Select(x => x.Where(function)));
        }

        /// <summary>
        /// Map a function over a bag to produce a new bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>The new bag from mapping over all the elements.</returns>
        public static Zen<FBag<TResult>> Select<T, TResult>(this Zen<FBag<T>> bagExpr, Func<Zen<T>, Zen<TResult>> function)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(function);

            return FBag.Create(bagExpr.Values().Select(x => x.Select(function)));
        }

        /// <summary>
        /// Determines if any element of the bag satisfies some predicate.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if an element satisfies the predicate.</returns>
        public static Zen<bool> Any<T>(this Zen<FBag<T>> bagExpr, Func<Zen<T>, Zen<bool>> function)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(function);

            return bagExpr.Values().Any(x => And(x.IsSome(), function(x.Value())));
        }

        /// <summary>
        /// Determines if all elements of the bag satisfy some predicate.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <param name="function">The predicate as a function.</param>
        /// <returns>A boolean indicating if all elements satisfy the predicate.</returns>
        public static Zen<bool> All<T>(this Zen<FBag<T>> bagExpr, Func<Zen<T>, Zen<bool>> function)
        {
            CommonUtilities.ValidateNotNull(bagExpr);
            CommonUtilities.ValidateNotNull(function);

            return bagExpr.Values().All(x => Implies(x.IsSome(), function(x.Value())));
        }

        /// <summary>
        /// Determines if the bag is empty.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <returns>A boolean indicating if the bag is empty.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<FBag<T>> bagExpr)
        {
            CommonUtilities.ValidateNotNull(bagExpr);

            return bagExpr.Values().All(o => o.IsNone());
        }

        /// <summary>
        /// Compute the size of the bag.
        /// </summary>
        /// <param name="bagExpr">Zen bag expression.</param>
        /// <returns>The new bag from adding the value.</returns>
        public static Zen<ushort> Size<T>(this Zen<FBag<T>> bagExpr)
        {
            CommonUtilities.ValidateNotNull(bagExpr);

            return Size(bagExpr.Values());
        }

        /// <summary>
        /// Compute the size of the bag sequence.
        /// </summary>
        /// <param name="seqExpr">Zen array expression.</param>
        /// <returns>The new bag from adding the value.</returns>
        private static Zen<ushort> Size<T>(Zen<FSeq<Option<T>>> seqExpr)
        {
            return seqExpr.Case(0, (hd, tl) => If<ushort>(hd.IsSome(), 1, 0) + Size(tl));
        }
    }
}
