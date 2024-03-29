﻿// <copyright file="Set.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing an arbitrary sized set.
    /// </summary>
    public class Set<T> : IEquatable<Set<T>>
    {
        /// <summary>
        /// Gets the underlying values of the backing map.
        /// </summary>
        public Map<T, SetUnit> Values { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{TKey}"/> class.
        /// </summary>
        public Set()
        {
            this.Values = new Map<T, SetUnit>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{TKey}"/> class.
        /// </summary>
        public Set(params T[] values) : this((IEnumerable<T>)values)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{TKey}"/> class.
        /// </summary>
        public Set(IEnumerable<T> values)
        {
            Contract.Assert(values != null);

            this.Values = new Map<T, SetUnit>();
            foreach (var value in values)
            {
                this.Values = this.Values.Set(value, new SetUnit());
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{T}"/> class.
        /// </summary>
        /// <param name="map">The backing map.</param>
        private Set(Map<T, SetUnit> map)
        {
            Contract.AssertNotNull(map);
            this.Values = map;
        }

        /// <summary>
        /// The number of elements in the map.
        /// </summary>
        public int Count() { return this.Values.Count(); }

        /// <summary>
        /// Add an element to the set.
        /// </summary>
        /// <param name="elt">The element to add.</param>
        public Set<T> Add(T elt)
        {
            Contract.AssertNotNull(elt);
            return new Set<T>(this.Values.Set(elt, new SetUnit()));
        }

        /// <summary>
        /// Delete an element from the Set.
        /// </summary>
        /// <param name="elt">The element to remove.</param>
        public Set<T> Delete(T elt)
        {
            Contract.AssertNotNull(elt);
            return new Set<T>(this.Values.Delete(elt));
        }

        /// <summary>
        /// Check if the set contains a value.
        /// </summary>
        /// <param name="elt">The given element.</param>
        /// <returns>True or false.</returns>
        public bool Contains(T elt)
        {
            Contract.AssertNotNull(elt);
            return this.Values.ContainsKey(elt);
        }

        /// <summary>
        /// Union this set with another.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>The union of the two sets.</returns>
        public Set<T> Union(Set<T> other)
        {
            Contract.AssertNotNull(other);
            return new Set<T>(CommonUtilities.MapUnion(this.Values, other.Values));
        }

        /// <summary>
        /// Intersect this set with another.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>The intersection of the two sets.</returns>
        public Set<T> Intersect(Set<T> other)
        {
            Contract.AssertNotNull(other);
            return new Set<T>(CommonUtilities.MapIntersect(this.Values, other.Values));
        }

        /// <summary>
        /// Difference of this set with another.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>The difference of the two sets.</returns>
        public Set<T> Difference(Set<T> other)
        {
            Contract.AssertNotNull(other);
            return new Set<T>(CommonUtilities.MapDifference(this.Values, other.Values));
        }

        /// <summary>
        /// Check if this set is a subset of another.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>True if this set is a subset of the other..</returns>
        public bool IsSubsetOf(Set<T> other)
        {
            Contract.AssertNotNull(other);
            return this.Equals(this.Intersect(other));
        }

        /// <summary>
        /// Convert the set to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{" + string.Join(", ", this.Values.Values.Select(kv => kv.Key)) + "}";
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="obj">The other set.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Set<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Set<T> other)
        {
            var count = this.Count();
            var otherCount = other.Count();
            return count == otherCount && this.Intersect(other).Count() == count;
        }

        /// <summary>
        /// Hashcode for sets.
        /// </summary>
        /// <returns>Hashcode for sets.</returns>
        public override int GetHashCode()
        {
            var hashCode = 1291433875;
            foreach (var kv in this.Values.Values)
            {
                hashCode += kv.Key.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="left">The left set.</param>
        /// <param name="right">The right set.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(Set<T> left, Set<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for sets.
        /// </summary>
        /// <param name="left">The left set.</param>
        /// <param name="right">The right set.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(Set<T> left, Set<T> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for map Zen objects.
    /// </summary>
    public static class Set
    {
        /// <summary>
        /// The Zen value for an empty set.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Empty<T>()
        {
            return Create<Set<T>>(("Values", Map.Empty<T, SetUnit>()));
        }

        /// <summary>
        /// The underlying map.
        /// </summary>
        /// <param name="setExpr">The set expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<T, SetUnit>> Values<T>(this Zen<Set<T>> setExpr)
        {
            return setExpr.GetField<Set<T>, Map<T, SetUnit>>("Values");
        }

        /// <summary>
        /// Add a value to a Zen set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="elementExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Add<T>(this Zen<Set<T>> setExpr, Zen<T> elementExpr)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(elementExpr);

            return Create<Set<T>>(("Values", setExpr.Values().Set(elementExpr, new SetUnit())));
        }

        /// <summary>
        /// Delete a value from a Zen set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="elementExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Delete<T>(this Zen<Set<T>> setExpr, Zen<T> elementExpr)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(elementExpr);

            return Create<Set<T>>(("Values", setExpr.Values().Delete(elementExpr)));
        }

        /// <summary>
        /// Check if a Zen set contains an element.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="elementExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<Set<T>> setExpr, Zen<T> elementExpr)
        {
            Contract.AssertNotNull(setExpr);
            Contract.AssertNotNull(elementExpr);
            return setExpr.Values().ContainsKey(elementExpr);
        }

        /// <summary>
        /// Union two sets together.
        /// </summary>
        /// <param name="setExpr1">Zen set expression.</param>
        /// <param name="setExpr2">Zen set expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Union<T>(this Zen<Set<T>> setExpr1, Zen<Set<T>> setExpr2)
        {
            Contract.AssertNotNull(setExpr1);
            Contract.AssertNotNull(setExpr2);

            return Create<Set<T>>(("Values", Zen.Union(setExpr1.Values(), setExpr2.Values())));
        }

        /// <summary>
        /// Intersect two sets.
        /// </summary>
        /// <param name="setExpr1">Zen set expression.</param>
        /// <param name="setExpr2">Zen set expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Intersect<T>(this Zen<Set<T>> setExpr1, Zen<Set<T>> setExpr2)
        {
            Contract.AssertNotNull(setExpr1);
            Contract.AssertNotNull(setExpr2);

            return Create<Set<T>>(("Values", Zen.Intersect(setExpr1.Values(), setExpr2.Values())));
        }

        /// <summary>
        /// Difference of two sets.
        /// </summary>
        /// <param name="setExpr1">Zen set expression.</param>
        /// <param name="setExpr2">Zen set expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Difference<T>(this Zen<Set<T>> setExpr1, Zen<Set<T>> setExpr2)
        {
            Contract.AssertNotNull(setExpr1);
            Contract.AssertNotNull(setExpr2);

            return Create<Set<T>>(("Values", Zen.Difference(setExpr1.Values(), setExpr2.Values())));
        }

        /// <summary>
        /// Check if one set is a subset of another.
        /// </summary>
        /// <param name="setExpr1">Zen set expression.</param>
        /// <param name="setExpr2">Zen set expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsSubsetOf<T>(this Zen<Set<T>> setExpr1, Zen<Set<T>> setExpr2)
        {
            Contract.AssertNotNull(setExpr1);
            Contract.AssertNotNull(setExpr2);

            return setExpr1 == setExpr1.Intersect(setExpr2);
        }
    }

    /// <summary>
    /// Unit class that is used to model sets as maps.
    /// </summary>
    public class SetUnit
    {
        /// <summary>
        /// Equality between unit types.
        /// </summary>
        /// <param name="obj">The other unit object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return true;
        }

        /// <summary>
        /// Hashcode for a unit type.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
