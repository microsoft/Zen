// <copyright file="Dict.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A class representing a simple list-backed dictionary.
    /// </summary>
    public class Dict<TKey, TValue>
    {
        /// <summary>
        /// Gets the underlying values with more recent values at the front.
        /// </summary>
        public IList<Pair<TKey, TValue>> Values { get; set; } = new List<Pair<TKey, TValue>>();

        /// <summary>
        /// Add a key and value to the Dict.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public void Add(TKey key, TValue value)
        {
            this.Values.Insert(0, (key, value));
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

            return this.Values[idx].Item2;
        }

        /// <summary>
        /// Gets the index of the given key.
        /// </summary>
        /// <param name="key">The given key.</param>
        /// <returns>The index, or -1 if none.</returns>
        private int IndexOf(TKey key)
        {
            for (int i = 0; i < this.Values.Count; i++)
            {
                if (this.Values[i].Item1.Equals(key))
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
            return "{" + string.Join(", ", this.Values.Select(kv => $"{kv.Item1} => {kv.Item2}")) + "}";
        }
    }
}
