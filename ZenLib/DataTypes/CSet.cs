// <copyright file="CSet.cs" company="Microsoft">
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
    /// A set of values. This requires all elements of the set to be C# constants
    /// and can not be general Zen expressions. It may offer better performance than
    /// the more general Set type.
    /// </summary>
    public class CSet<T> : IEquatable<CSet<T>>
    {
        /// <summary>
        /// Gets the underlying values of the map.
        /// </summary>
        public CMap<T, bool> Map { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CSet{T}"/> class.
        /// </summary>
        public CSet()
        {
            this.Map = new CMap<T, bool>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CSet{TKey}"/> class.
        /// </summary>
        public CSet(params T[] values) : this((IEnumerable<T>)values)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CSet{TKey}"/> class.
        /// </summary>
        public CSet(IEnumerable<T> values)
        {
            this.Map = new CMap<T, bool>();
            foreach (var value in values)
            {
                this.Map = this.Map.Set(value, true);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CSet{T}"/> class.
        /// </summary>
        /// <param name="map">The map of values.</param>
        internal CSet(CMap<T, bool> map)
        {
            this.Map = map;
        }

        /// <summary>
        /// The number of elements in the set.
        /// </summary>
        public int Count() { return this.Map.Count(); }

        /// <summary>
        /// Add an element to the set.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public CSet<T> Add(T element)
        {
            return new CSet<T>(this.Map.Set(element, true));
        }

        /// <summary>
        /// Delete an element from the set.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public CSet<T> Delete(T element)
        {
            return new CSet<T>(this.Map.Set(element, false));
        }

        /// <summary>
        /// Determine if an element is in the set.
        /// </summary>
        /// <param name="element">The element.</param>
        public bool Contains(T element)
        {
            return this.Map.Get(element);
        }

        /// <summary>
        /// Convert the set to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{" + string.Join(", ", this.Map.Values.Keys) + "}";
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="obj">The other map.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is CSet<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for sets.
        /// </summary>
        /// <param name="other">The other map.</param>
        /// <returns>True or false.</returns>
        public bool Equals(CSet<T> other)
        {
            return this.Map.Equals(other.Map);
        }

        /// <summary>
        /// Hashcode for maps.
        /// </summary>
        /// <returns>Hashcode for maps.</returns>
        public override int GetHashCode()
        {
            return this.Map.GetHashCode();
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="left">The left map.</param>
        /// <param name="right">The right map.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(CSet<T> left, CSet<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for maps.
        /// </summary>
        /// <param name="left">The left map.</param>
        /// <param name="right">The right map.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(CSet<T> left, CSet<T> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for set Zen objects.
    /// </summary>
    public static class CSet
    {
        /// <summary>
        /// The underlying map for the set.
        /// </summary>
        /// <param name="setExpr">The set expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<T, bool>> Map<T>(this Zen<CSet<T>> setExpr)
        {
            return setExpr.GetField<CSet<T>, CMap<T, bool>>("Map");
        }

        /// <summary>
        /// Add an element to a Zen set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="element">The element to add.</param>
        /// <returns>Zen value.</returns>
        public static Zen<CSet<T>> Add<T>(this Zen<CSet<T>> setExpr, T element)
        {
            Contract.AssertNotNull(setExpr);

            return Create<CSet<T>>(("Map", setExpr.Map().Set(element, true)));
        }

        /// <summary>
        /// Delete an element from a Zen set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="element">The element to remove.</param>
        /// <returns>Zen value.</returns>
        public static Zen<CSet<T>> Delete<T>(this Zen<CSet<T>> setExpr, T element)
        {
            Contract.AssertNotNull(setExpr);

            return Create<CSet<T>>(("Map", setExpr.Map().Set(element, false)));
        }

        /// <summary>
        /// Check if a set contains an element.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="element">The element to check for.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<CSet<T>> setExpr, T element)
        {
            Contract.AssertNotNull(setExpr);

            return setExpr.Map().Get(element);
        }
    }
}
