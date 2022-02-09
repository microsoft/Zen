// <copyright file="UnionFind.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Simple union find data structure.
    /// </summary>
    internal class UnionFind<T>
    {
        /// <summary>
        /// Mapping from element to its index.
        /// </summary>
        private Dictionary<T, int> indexMap = new Dictionary<T, int>();

        /// <summary>
        /// The elements in the union find.
        /// </summary>
        private List<T> elements = new List<T>();

        /// <summary>
        /// The parent pointers.
        /// </summary>
        private List<int> parents = new List<int>();

        /// <summary>
        /// The sizes of the subtrees.
        /// </summary>
        private List<int> sizes = new List<int>();

        /// <summary>
        /// Adds a new element to the union find.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(T element)
        {
            if (indexMap.ContainsKey(element))
            {
                return;
            }

            indexMap[element] = parents.Count;
            elements.Add(element);
            parents.Add(parents.Count);
            sizes.Add(1);
        }

        /// <summary>
        /// Gets an integer representing the set id the element belongs to.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The set id for the element.</returns>
        public int Find(T element)
        {
            if (!this.indexMap.TryGetValue(element, out var index))
            {
                throw new ArgumentException("Find called for non-existent element in UnionFind");
            }

            return Find(index);
        }

        /// <summary>
        /// Gets an integer representing the set id the element belongs to.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The set id for the element.</returns>
        private int Find(int index)
        {
            while (index != parents[index])
            {
                index = parents[index];
            }

            return index;
        }

        /// <summary>
        /// Combine the sets that contain elements 1 and 2.
        /// </summary>
        /// <param name="element1">The first element.</param>
        /// <param name="element2">The second element.</param>
        public void Union(T element1, T element2)
        {
            var root1 = Find(element1);
            var root2 = Find(element2);

            if (root1 == root2)
            {
                return;
            }

            var size1 = sizes[root1];
            var size2 = sizes[root2];

            if (size1 < size2)
            {
                parents[root1] = root2;
                sizes[root2] += sizes[root1];
            }
            else
            {
                parents[root2] = root1;
                sizes[root1] += sizes[root2];
            }
        }

        /// <summary>
        /// Gets the current disjoint sets while preserving.
        /// The elements that were added first will show up first.
        /// </summary>
        /// <returns>The set of disjoint sets.</returns>
        public List<List<T>> GetDisjointSets()
        {
            var disjointSets = new List<List<T>>();
            var dict = new Dictionary<int, int>();
            for (int i = 0; i < this.elements.Count; i++)
            {
                var element = this.elements[i];
                var root = this.Find(i);

                if (dict.TryGetValue(root, out var index))
                {
                    disjointSets[index].Add(element);
                }
                else
                {
                    var newList = new List<T> { element };
                    dict[root] = disjointSets.Count;
                    disjointSets.Add(newList);
                }
            }

            return disjointSets;
        }
    }
}