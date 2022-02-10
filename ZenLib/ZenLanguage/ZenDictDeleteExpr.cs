// <copyright file="ZenDictDeleteExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a dictionary delete expression.
    /// </summary>
    internal sealed class ZenDictDeleteExpr<TKey, TValue> : Zen<IDictionary<TKey, TValue>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<IDictionary<TKey, TValue>>, Zen<TKey>), ZenDictDeleteExpr<TKey, TValue>> createFunc = (v) =>
            new ZenDictDeleteExpr<TKey, TValue>(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenDictDeleteExpr.
        /// </summary>
        private static HashConsTable<(long, long), ZenDictDeleteExpr<TKey, TValue>> hashConsTable =
            new HashConsTable<(long, long), ZenDictDeleteExpr<TKey, TValue>>();

        /// <summary>
        /// Unroll a ZenDictDeleteExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<IDictionary<TKey, TValue>> Unroll()
        {
            return Create(this.DictExpr.Unroll(), this.KeyExpr.Unroll());
        }

        /// <summary>
        /// Create a new ZenDictDeleteExpr.
        /// </summary>
        /// <param name="dictExpr">The dictionary expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new expr.</returns>
        public static ZenDictDeleteExpr<TKey, TValue> Create(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> key)
        {
            CommonUtilities.ValidateNotNull(dictExpr);
            CommonUtilities.ValidateNotNull(key);
            CommonUtilities.ValidateIsMapElementType(typeof(TKey));
            CommonUtilities.ValidateIsMapElementType(typeof(TValue));

            var k = (dictExpr.Id, key.Id);
            hashConsTable.GetOrAdd(k, (dictExpr, key), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDictDeleteExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictExpr">The dictionary expression.</param>
        /// <param name="keyExpr">The key expression to add a value for.</param>
        private ZenDictDeleteExpr(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> keyExpr)
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
            return $"Delete({this.DictExpr}, {this.KeyExpr})";
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
            return visitor.VisitZenDictDeleteExpr(this, parameter);
        }
    }
}
