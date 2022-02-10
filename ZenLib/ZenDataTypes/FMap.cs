﻿// <copyright file="FMap.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a simple finite list-backed map.
    /// </summary>
    public class FMap<TKey, TValue>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        public FSeq<Pair<TKey, TValue>> Values { get; set; } = new FSeq<Pair<TKey, TValue>>();

        /// <summary>
        /// Add a key and value to the Dict.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public void Set(TKey key, TValue value)
        {
            this.Values.Values.Insert(0, (key, value));
        }

        /// <summary>
        /// Check if the Dict contains the key.
        /// </summary>
        /// <param name="key">The given key.</param>
        /// <returns>True or false.</returns>
        public bool ContainsKey(TKey key)
        {
            return IndexOf(key) >= 0;
        }

        /// <summary>
        /// Get the element for a given key.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            var idx = IndexOf(key);

            if (idx < 0)
            {
                throw new System.IndexOutOfRangeException($"Missing key: {key} from Dict.");
            }

            return this.Values.Values[idx].Item2;
        }

        /// <summary>
        /// Gets the index of the given key.
        /// </summary>
        /// <param name="key">The given key.</param>
        /// <returns>The index, or -1 if none.</returns>
        private int IndexOf(TKey key)
        {
            for (int i = 0; i < this.Values.Values.Count; i++)
            {
                if (this.Values.Values[i].Item1.Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Convert the dict to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{" + string.Join(", ", this.Values.Values.Select(kv => $"{kv.Item1} => {kv.Item2}")) + "}";
        }
    }

    /// <summary>
    /// Static factory class for dictionary Zen objects.
    /// </summary>
    public static class FMap
    {
        /// <summary>
        /// The Zen value for an empty Dictionary.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<FMap<TKey, TValue>> Empty<TKey, TValue>()
        {
            return Create<FMap<TKey, TValue>>(("Values", FSeq.Empty<Pair<TKey, TValue>>()));
        }
    }

    /// <summary>
    /// Extension methods for Zen dictionary objects.
    /// </summary>
    public static class FMapExtensions
    {
        /// <summary>
        /// Add a value to a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FMap<TKey, TValue>> Set<TKey, TValue>(this Zen<FMap<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);
            CommonUtilities.ValidateNotNull(valueExpr);

            var l = mapExpr.GetField<FMap<TKey, TValue>, FSeq<Pair<TKey, TValue>>>("Values");
            return Create<FMap<TKey, TValue>>(("Values", l.AddFront(Pair.Create(keyExpr, valueExpr))));
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<TValue>> Get<TKey, TValue>(this Zen<FMap<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);

            var l = mapExpr.GetField<FMap<TKey, TValue>, FSeq<Pair<TKey, TValue>>>("Values");
            return l.SeqGet(keyExpr);
        }

        /// <summary>
        /// Check if a Zen map contains a key.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> ContainsKey<TKey, TValue>(this Zen<FMap<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(keyExpr);

            return mapExpr.Get(keyExpr).IsSome();
        }

        /// <summary>
        /// Get the first binding in a list for a given key.
        /// </summary>
        /// <param name="expr">Zen list expression.</param>
        /// <param name="key">The key.</param>
        /// <returns>Zen value.</returns>
        private static Zen<Option<TValue>> SeqGet<TKey, TValue>(this Zen<FSeq<Pair<TKey, TValue>>> expr, Zen<TKey> key)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(key);

            return expr.Case(
                empty: Option.Null<TValue>(),
                cons: (hd, tl) => If(hd.Item1() == key, Option.Create(hd.Item2()), tl.SeqGet(key)));
        }
    }
}
