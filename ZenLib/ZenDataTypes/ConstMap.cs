// <copyright file="ConstMap.cs" company="Microsoft">
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
    /// A class representing a const map.
    /// </summary>
    public class ConstMap<TKey, TValue> : IEquatable<ConstMap<TKey, TValue>>
    {
        /// <summary>
        /// Gets the underlying values of the map.
        /// </summary>
        public IDictionary<TKey, TValue> Values { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ConstMap{TKey, TValue}"/> class.
        /// </summary>
        public ConstMap()
        {
            this.Values = ImmutableDictionary<TKey, TValue>.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ConstMap{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary of initial values.</param>
        internal ConstMap(ImmutableDictionary<TKey, TValue> dictionary)
        {
            this.Values = dictionary;
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
        public ConstMap<TKey, TValue> Set(TKey key, TValue value)
        {
            var d = (ImmutableDictionary<TKey, TValue>)this.Values;
            return new ConstMap<TKey, TValue>(d.SetItem(key, value));
        }

        /// <summary>
        /// Delete a key from the Map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        public ConstMap<TKey, TValue> Delete(TKey key)
        {
            var d = (ImmutableDictionary<TKey, TValue>)this.Values;
            return new ConstMap<TKey, TValue>(d.Remove(key));
        }

        /// <summary>
        /// Check if the Map contains the key.
        /// </summary>
        /// <param name="key">The given key.</param>
        /// <returns>True or false.</returns>
        public bool ContainsKey(TKey key)
        {
            return this.Values.ContainsKey(key);
        }

        /// <summary>
        /// Get the element for a given key.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            if (this.Values.TryGetValue(key, out var value))
            {
                return value;
            }

            return ReflectionUtilities.GetDefaultValue<TValue>();
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
            return obj is ConstMap<TKey, TValue> o && Equals(o);
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="other">The other map.</param>
        /// <returns>True or false.</returns>
        public bool Equals(ConstMap<TKey, TValue> other)
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
        public static bool operator ==(ConstMap<TKey, TValue> left, ConstMap<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for maps.
        /// </summary>
        /// <param name="left">The left map.</param>
        /// <param name="right">The right map.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(ConstMap<TKey, TValue> left, ConstMap<TKey, TValue> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for map Zen objects.
    /// </summary>
    public static class ConstMap
    {
        /// <summary>
        /// Add a value to a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ConstMap<TKey, TValue>> Set<TKey, TValue>(this Zen<ConstMap<TKey, TValue>> mapExpr, TKey key, Zen<TValue> valueExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(key);
            CommonUtilities.ValidateNotNull(valueExpr);

            return ConstMapSet(mapExpr, key, valueExpr);
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="key">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TValue> Get<TKey, TValue>(this Zen<ConstMap<TKey, TValue>> mapExpr, TKey key)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(key);

            return ConstMapGet(mapExpr, key);
        }
    }
}
