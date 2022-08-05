// <copyright file="FMap.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Immutable;
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
        /// Creates a new instance of the <see cref="FMap{TKey, TValue}"/> class.
        /// </summary>
        public FMap()
        {
            this.Values = new FSeq<Pair<TKey, TValue>>();
        }

        private FMap(FSeq<Pair<TKey, TValue>> sequence)
        {
            this.Values = sequence;
        }

        /// <summary>
        /// The number of elements in the map.
        /// </summary>
        public int Count() { return this.Values.Values.Count; }

        /// <summary>
        /// Add a key and value to the finite map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public FMap<TKey, TValue> Set(TKey key, TValue value)
        {
            var newValues = this.Values.AddFront((key, value));
            return new FMap<TKey, TValue>(newValues);
        }

        /// <summary>
        /// Delete a key from the Map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        public FMap<TKey, TValue> Delete(TKey key)
        {
            var newList = ImmutableList<Pair<TKey, TValue>>.Empty.AddRange(this.Values.Values.Where(x => !x.Item1.Equals(key)));
            var newSeq = new FSeq<Pair<TKey, TValue>>(newList);
            return new FMap<TKey, TValue>(newSeq);
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
        public Option<TValue> Get(TKey key)
        {
            var idx = IndexOf(key);

            if (idx < 0)
            {
                return Option.None<TValue>();
            }

            return Option.Some(this.Values.Values[idx].Item2);
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

        /// <summary>
        /// Add a value to a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <param name="valueExpr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FMap<TKey, TValue>> Set<TKey, TValue>(this Zen<FMap<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);
            Contract.AssertNotNull(valueExpr);

            var l = mapExpr.GetField<FMap<TKey, TValue>, FSeq<Pair<TKey, TValue>>>("Values");
            return Create<FMap<TKey, TValue>>(("Values", l.AddFront(Pair.Create(keyExpr, valueExpr))));
        }

        /// <summary>
        /// Delete a key from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FMap<TKey, TValue>> Delete<TKey, TValue>(this Zen<FMap<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);

            var l = mapExpr.GetField<FMap<TKey, TValue>, FSeq<Pair<TKey, TValue>>>("Values");
            return Create<FMap<TKey, TValue>>(("Values", l.Where(x => x.Item1() != keyExpr)));
        }

        /// <summary>
        /// Get a value from a Zen map.
        /// </summary>
        /// <param name="mapExpr">Zen map expression.</param>
        /// <param name="keyExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<TValue>> Get<TKey, TValue>(this Zen<FMap<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);

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
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(keyExpr);

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
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(key);

            return expr.Case(
                empty: Option.Null<TValue>(),
                cons: (hd, tl) => If(hd.Item1() == key, Option.Create(hd.Item2()), tl.SeqGet(key)));
        }
    }
}
