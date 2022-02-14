// <copyright file="Set.cs" company="Microsoft">
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

        private Set(Map<T, SetUnit> map)
        {
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
            return new Set<T>(this.Values.Set(elt, new SetUnit()));
        }

        /// <summary>
        /// Delete an element from the Set.
        /// </summary>
        /// <param name="elt">The element to remove.</param>
        public Set<T> Delete(T elt)
        {
            return new Set<T>(this.Values.Delete(elt));
        }

        /// <summary>
        /// Check if the set contains a value.
        /// </summary>
        /// <param name="elt">The given element.</param>
        /// <returns>True or false.</returns>
        public bool Contains(T elt)
        {
            return this.Values.ContainsKey(elt);
        }

        /// <summary>
        /// Union this set with another.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>The union of the two sets.</returns>
        public Set<T> Union(Set<T> other)
        {
            return new Set<T>(new Map<T, SetUnit>(CommonUtilities.DictionaryUnion(this.Values.Values, other.Values.Values)));
        }

        /// <summary>
        /// Intersect this set with another.
        /// </summary>
        /// <param name="other">The other set.</param>
        /// <returns>The intersection of the two sets.</returns>
        public Set<T> Intersect(Set<T> other)
        {
            return new Set<T>(new Map<T, SetUnit>(CommonUtilities.DictionaryIntersect(this.Values.Values, other.Values.Values)));
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
            return 1291433875 + EqualityComparer<Map<T, SetUnit>>.Default.GetHashCode(Values);
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
    }

    /// <summary>
    /// Extension methods for Zen set objects.
    /// </summary>
    public static class SetExtensions
    {
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
            CommonUtilities.ValidateNotNull(setExpr);
            CommonUtilities.ValidateNotNull(elementExpr);

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
            CommonUtilities.ValidateNotNull(setExpr);
            CommonUtilities.ValidateNotNull(elementExpr);

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
            CommonUtilities.ValidateNotNull(setExpr);
            CommonUtilities.ValidateNotNull(elementExpr);
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
            CommonUtilities.ValidateNotNull(setExpr1);
            CommonUtilities.ValidateNotNull(setExpr2);

            var map = Create<Map<T, SetUnit>>(("Values", Zen.Union(setExpr1.Values().Values(), setExpr2.Values().Values())));
            return Create<Set<T>>(("Values", map));
        }

        /// <summary>
        /// Union two sets together.
        /// </summary>
        /// <param name="setExpr1">Zen set expression.</param>
        /// <param name="setExpr2">Zen set expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Intersect<T>(this Zen<Set<T>> setExpr1, Zen<Set<T>> setExpr2)
        {
            CommonUtilities.ValidateNotNull(setExpr1);
            CommonUtilities.ValidateNotNull(setExpr2);

            var map = Create<Map<T, SetUnit>>(("Values", Zen.Intersect(setExpr1.Values().Values(), setExpr2.Values().Values())));
            return Create<Set<T>>(("Values", map));
        }
    }

    /// <summary>
    /// Unit class that is hidden from the user to not interfere with maps that use Unit.
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
