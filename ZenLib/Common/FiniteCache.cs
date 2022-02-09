// <copyright file="UnorderedList.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Simple cache that holds only a fixed number of entries.
    /// </summary>
    internal class FiniteCache<TKey, TValue>
    {
        /// <summary>
        /// The maximum number of elements allowed.
        /// </summary>
        private int maxCount;

        /// <summary>
        /// Internal cache of values.
        /// </summary>
        private Dictionary<TKey, TValue> cache;

        /// <summary>
        /// All the entries.
        /// </summary>
        private List<TKey> entries;

        /// <summary>
        /// Random number generator for a random eviction policy.
        /// </summary>
        private Random random;

        /// <summary>
        ///     Creates a new instance of the <see cref="FiniteCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="count">
        ///     The maximum number of elements stored in the cache. A negative number indicates
        ///     that the cache has no upper limit.
        /// </param>
        public FiniteCache(int count)
        {
            this.maxCount = count;
            this.cache = count >= 0 ? new Dictionary<TKey, TValue>(count) : new Dictionary<TKey, TValue>();
            this.entries = new List<TKey>();
            this.random = new Random(0);
        }

        /// <summary>
        /// Gets the number of entries in the cache.
        /// </summary>
        public int Count { get => this.entries.Count; }

        /// <summary>
        ///     Adds a key and value to the cache. Assumes no duplicate key exists
        ///     and that the user checks with TryGetValue prior to adding.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value for the key.</param>
        public void Add(TKey key, TValue value)
        {
            if (this.maxCount == 0)
            {
                return;
            }

            if (this.entries.Count == maxCount)
            {
                var toEvictIndex = this.random.Next(0, maxCount);
                var toEvictKey = this.entries[toEvictIndex];
                this.cache.Remove(toEvictKey);
                this.cache[key] = value;
                this.entries[toEvictIndex] = key;
            }
            else
            {
                this.cache[key] = value;
                this.entries.Add(key);
            }
        }

        /// <summary>
        ///     Try to get a value from the cache.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="value">The value if it exists.</param>
        /// <returns>Whether the key is in the cache.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.cache.TryGetValue(key, out var v))
            {
                value = v;
                return true;
            }

            value = default(TValue);
            return false;
        }
    }
}