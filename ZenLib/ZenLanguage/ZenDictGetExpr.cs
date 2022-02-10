// <copyright file="ZenDictGetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a dictionary get expression.
    /// </summary>
    internal sealed class ZenDictGetExpr<TKey, TValue> : Zen<Option<TValue>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<IDictionary<TKey, TValue>>, Zen<TKey>), ZenDictGetExpr<TKey, TValue>> createFunc = (v) =>
            new ZenDictGetExpr<TKey, TValue>(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenDictAddExpr.
        /// </summary>
        private static HashConsTable<(long, long), ZenDictGetExpr<TKey, TValue>> hashConsTable =
            new HashConsTable<(long, long), ZenDictGetExpr<TKey, TValue>>();

        /// <summary>
        /// Unroll a ZenDictGetExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<Option<TValue>> Unroll()
        {
            return Create(this.DictExpr.Unroll(), this.KeyExpr.Unroll());
        }

        /// <summary>
        /// Create a new ZenDictGetExpr.
        /// </summary>
        /// <param name="dictExpr">The dictionary expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new expr.</returns>
        public static ZenDictGetExpr<TKey, TValue> Create(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> key)
        {
            CommonUtilities.ValidateNotNull(dictExpr);
            CommonUtilities.ValidateNotNull(key);

            var k = (dictExpr.Id, key.Id);
            hashConsTable.GetOrAdd(k, (dictExpr, key), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDictSetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictExpr">The dictionary expression.</param>
        /// <param name="keyExpr">The key expression to add a value for.</param>
        private ZenDictGetExpr(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> keyExpr)
        {
            this.DictExpr = dictExpr;
            this.KeyExpr = keyExpr;
        }

        /// <summary>
        /// Gets the dictionary expr.
        /// </summary>
        public Zen<IDictionary<TKey, TValue>> DictExpr { get; }

        /// <summary>
        /// Gets the key to add the value for.
        /// </summary>
        public Zen<TKey> KeyExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Get({this.DictExpr}, {this.KeyExpr})";
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitZenDictGetExpr(this, parameter);
        }
    }
}
