// <copyright file="ZenAstCache.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A cache for Zen ast expressions.
    /// </summary>
    internal static class ZenAstCache<TObj, TKey, TValue> where TValue : class
    {
        /// <summary>
        /// A flyweight object that stores the expressions for each key.
        /// The object type TObj is to ensure that different flyweights are used
        /// for each AST node type. This ensures better parallelism.
        /// </summary>
        internal static Flyweight<TKey, TValue> Flyweight = new Flyweight<TKey, TValue>();

        /// <summary>
        /// Hash cons table for ZenCreateObjectExpr.
        /// </summary>
        internal static Flyweight<TKey[], TValue> FlyweightArray = new Flyweight<TKey[], TValue>(new ArrayComparer());

        /// <summary>
        /// Custom array comparer for ensuring hash consing uniqueness.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class ArrayComparer : IEqualityComparer<TKey[]>
        {
            /// <summary>
            /// Equality for key arrays.
            /// </summary>
            /// <param name="a1">The first array.</param>
            /// <param name="a2">The second array.</param>
            /// <returns>True or false.</returns>
            public bool Equals(TKey[] a1, TKey[] a2)
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }

                for (int i = 0; i < a1.Length; i++)
                {
                    if (!a1[i].Equals(a2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Hash code for the keys.
            /// </summary>
            /// <param name="array">The array of keys.</param>
            /// <returns>An integer.</returns>
            public int GetHashCode(TKey[] array)
            {
                int result = 31;
                for (int i = 0; i < array.Length; i++)
                {
                    result = result * 7 + array[i].GetHashCode();
                }

                return result;
            }
        }
    }
}
