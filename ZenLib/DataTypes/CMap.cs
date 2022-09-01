// <copyright file="CMap.cs" company="Microsoft">
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
    /// A total map whose keys can only refer to C# constants rather than Zen values.
    /// this maps every key to some value, and the default value for every key is the
    /// default for that type (e.g., 0 for int, false for bool). Because these are total
    /// maps, there is no notion of deleting an element or an empty map.
    /// </summary>
    public class CMap<TKey, TValue> : IEquatable<CMap<TKey, TValue>>
    {
        /// <summary>
        /// The default value of the given type.
        /// </summary>
        private static TValue defaultValue = ReflectionUtilities.GetDefaultValue<TValue>();

        /// <summary>
        /// Gets the underlying values of the map.
        /// </summary>
        public IDictionary<TKey, TValue> Values { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CMap{TKey, TValue}"/> class.
        /// </summary>
        public CMap()
        {
            this.Values = ImmutableDictionary<TKey, TValue>.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CMap{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary of initial values.</param>
        internal CMap(ImmutableDictionary<TKey, TValue> dictionary)
        {
            Contract.AssertNotNull(dictionary);
            this.Values = dictionary;
        }

        /// <summary>
        /// The number of non-default elements in the map.
        /// </summary>
        public int Count() { return this.Values.Count; }

        /// <summary>
        /// Add a key and value to the Map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public CMap<TKey, TValue> Set(TKey key, TValue value)
        {
            Contract.AssertNotNull(key);
            Contract.AssertNotNull(value);

            var d = (ImmutableDictionary<TKey, TValue>)this.Values;
            d = value.Equals(defaultValue) ? d.Remove(key) : d.SetItem(key, value);
            return new CMap<TKey, TValue>(d);
        }

        /// <summary>
        /// Get the element for a given key.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            Contract.AssertNotNull(key);

            if (this.Values.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Convert the map to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var strings = this.Values.Select(kv => $"{kv.Key} => {kv.Value}").ToList();
            strings.Add("_ => " + defaultValue.ToString());
            return "{" + string.Join(", ", strings) + "}";
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="obj">The other map.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is CMap<TKey, TValue> o && Equals(o);
        }

        /// <summary>
        /// Equality for maps.
        /// </summary>
        /// <param name="other">The other map.</param>
        /// <returns>True or false.</returns>
        public bool Equals(CMap<TKey, TValue> other)
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
        public static bool operator ==(CMap<TKey, TValue> left, CMap<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for maps.
        /// </summary>
        /// <param name="left">The left map.</param>
        /// <param name="right">The right map.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(CMap<TKey, TValue> left, CMap<TKey, TValue> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for map Zen objects.
    /// </summary>
    public static class CMap
    {
        /// <summary>
        /// Add a value to a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<CMap<TKey, TValue>> Set<TKey, TValue>(this Zen<CMap<TKey, TValue>> mapExpr, TKey key, Zen<TValue> valueExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);
            Contract.AssertNotNull(valueExpr);

            return ConstMapSet(mapExpr, key, valueExpr);
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="key">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TValue> Get<TKey, TValue>(this Zen<CMap<TKey, TValue>> mapExpr, TKey key)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);

            return ConstMapGet(mapExpr, key);
        }
    }
}
