// <copyright file="ZenAstCache.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
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
    }
}
