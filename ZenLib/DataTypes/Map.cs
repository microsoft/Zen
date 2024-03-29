﻿// <copyright file="Map.cs" company="Microsoft">
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
    /// A class representing a map from keys to values that is supported by Zen.
    /// This class is handled symbolically using the SMT theory of arrays.
    /// When the keys to the map are constants, take a look at the <see cref="CMap{TKey, TValue}"/> class.
    /// </summary>
    public class Map<TKey, TValue> : IEquatable<Map<TKey, TValue>>
    {
        /// <summary>
        /// Gets the underlying values of the map.
        /// </summary>
        public IDictionary<TKey, TValue> Values { get; set; }

        /// <summary>
        /// Used to indicate that the map is negated.
        /// </summary>
        internal bool Negated = false;

        /// <summary>
        /// Creates a new instance of the <see cref="Map{TKey, TValue}"/> class.
        /// </summary>
        public Map()
        {
            this.Values = ImmutableDictionary<TKey, TValue>.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="negated">Whether this map is negated for sets.</param>
        internal Map(bool negated)
        {
            if (negated)
            {
                Contract.Assert(typeof(TValue) == typeof(SetUnit));
            }

            this.Values = ImmutableDictionary<TKey, TValue>.Empty;
            this.Negated = negated;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary of initial values.</param>
        internal Map(ImmutableDictionary<TKey, TValue> dictionary)
        {
            this.Values = dictionary;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary of initial values.</param>
        /// <param name="negated">Whether this map is negated for sets.</param>
        internal Map(ImmutableDictionary<TKey, TValue> dictionary, bool negated)
        {
            if (negated)
            {
                Contract.Assert(typeof(TValue) == typeof(SetUnit));
            }

            this.Values = dictionary;
            this.Negated = negated;
        }

        /// <summary>
        /// The number of elements in the map.
        /// </summary>
        public int Count() { return this.Values.Count; }

        /// <summary>
        /// Add a key and value to the Map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public Map<TKey, TValue> Set(TKey key, TValue value)
        {
            Contract.AssertNotNull(key);
            Contract.AssertNotNull(value);

            var d = (ImmutableDictionary<TKey, TValue>)this.Values;
            return new Map<TKey, TValue>(d.SetItem(key, value), this.Negated);
        }

        /// <summary>
        /// Delete a key from the Map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        public Map<TKey, TValue> Delete(TKey key)
        {
            Contract.AssertNotNull(key);

            var d = (ImmutableDictionary<TKey, TValue>)this.Values;
            return new Map<TKey, TValue>(d.Remove(key), this.Negated);
        }

        /// <summary>
        /// Check if the Map contains the key.
        /// </summary>
        /// <param name="key">The given key.</param>
        /// <returns>True or false.</returns>
        public bool ContainsKey(TKey key)
        {
            Contract.AssertNotNull(key);

            return this.Values.ContainsKey(key);
        }

        /// <summary>
        /// Get the element for a given key.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <returns></returns>
        public Option<TValue> Get(TKey key)
        {
            Contract.AssertNotNull(key);

            if (this.Values.TryGetValue(key, out var value))
            {
                return Option.Some(value);
            }

            return Option.None<TValue>();
        }

        /// <summary>
        /// Convert the dict to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{" + string.Join(", ", this.Values.Select(kv => $"{kv.Key} => {kv.Value}")) + "}";
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="obj">The other map.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Map<TKey, TValue> o && Equals(o);
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="other">The other map.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Map<TKey, TValue> other)
        {
            var count = this.Count();
            var otherCount = other.Count();
            var both = this.Values.Intersect(other.Values);
            return count == otherCount && both.Count() == count;
        }

        /// <summary>
        /// Hashcode for maps.
        /// </summary>
        /// <returns>Hashcode for maps.</returns>
        public override int GetHashCode()
        {
            var hashCode = 1291433875;
            foreach (var kv in this.Values.Values)
            {
                hashCode += kv.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="left">The left map.</param>
        /// <param name="right">The right map.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(Map<TKey, TValue> left, Map<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for maps.
        /// </summary>
        /// <param name="left">The left map.</param>
        /// <param name="right">The right map.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(Map<TKey, TValue> left, Map<TKey, TValue> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for map Zen objects.
    /// </summary>
    public static class Map
    {
        /// <summary>
        /// The Zen value for an empty map.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Map<TKey, TValue>> Empty<TKey, TValue>()
        {
            return EmptyMap<TKey, TValue>();
        }

        /// <summary>
        /// Add a value to a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Map<TKey, TValue>> Set<TKey, TValue>(this Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);
            Contract.AssertNotNull(valueExpr);

            return MapSet(mapExpr, keyExpr, valueExpr);
        }

        /// <summary>
        /// Delete a key from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Map<TKey, TValue>> Delete<TKey, TValue>(this Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);

            return MapDelete(mapExpr, keyExpr);
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<TValue>> Get<TKey, TValue>(this Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);

            return MapGet(mapExpr, keyExpr);
        }

        /// <summary>
        /// Check if a Zen map contains a key.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> ContainsKey<TKey, TValue>(this Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);

            return Get(mapExpr, keyExpr).IsSome();
        }
    }
}
