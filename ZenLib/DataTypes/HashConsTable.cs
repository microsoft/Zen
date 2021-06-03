// <copyright file="HashConsTable.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A hash cons table that is able to reclaim memory.
    /// It uses weak references to Zen values and overrides
    /// values that have been garbage collected when inserting.
    /// </summary>
    internal sealed class HashConsTable<TKey, TValue> where TValue : class
    {
        /// <summary>
        /// Lock object for the hash cons table.
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// The table of hash consed elements.
        /// </summary>
        private Dictionary<TKey, WeakReference<TValue>> table = new Dictionary<TKey, WeakReference<TValue>>();

        /// <summary>
        /// Creates a new HashConsTable.
        /// </summary>
        /// <param name="comparer">An optional comparer.</param>
        public HashConsTable(IEqualityComparer<TKey> comparer = null)
        {
            if (comparer == null)
            {
                this.table = new Dictionary<TKey, WeakReference<TValue>>();
            }
            else
            {
                this.table = new Dictionary<TKey, WeakReference<TValue>>(comparer);
            }
        }

        /// <summary>
        /// Gets an element if it exists, or adds a new one if not.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="createFunc">The callback to create a fresh value.</param>
        /// <param name="result">The value either existing or added.</param>
        /// <returns>True if the element was added.</returns>
        public bool GetOrAdd(TKey key, Func<TValue> createFunc, out TValue result)
        {
            lock (this.lockObj)
            {
                // we need to occasionally cull the dead elements in the table to avoid
                // growing in an unbounded way when there are many values that become dead.
                // when we reach a power of 2 size, we count the dead entries, and clean them up
                // only if they exceed half the elements in the table.
                // if this never happens, then it means that the dead entries are a constant fraction
                // of the entries.
                if ((this.table.Count & (this.table.Count - 1)) == 0)
                {
                    int numDead = 0;

                    foreach (var v in this.table.Values)
                    {
                        if (!v.TryGetTarget(out var _))
                        {
                            numDead++;
                        }
                    }

                    if (numDead >= this.table.Count / 2)
                    {
                        var newCount = (this.table.Count - numDead) * 2;
                        var newTable = new Dictionary<TKey, WeakReference<TValue>>(newCount, this.table.Comparer);

                        foreach (var kv in this.table)
                        {
                            if (kv.Value.TryGetTarget(out var _))
                            {
                                newTable[kv.Key] = kv.Value;
                            }
                        }

                        this.table = newTable;
                    }
                }

                if (this.table.TryGetValue(key, out var wref))
                {
                    if (wref.TryGetTarget(out var target))
                    {
                        result = target;
                        return false;
                    }

                    result = createFunc();
                    wref.SetTarget(result);
                    return true;
                }

                result = createFunc();
                this.table[key] = new WeakReference<TValue>(result);
                return true;
            }
        }
    }
}
