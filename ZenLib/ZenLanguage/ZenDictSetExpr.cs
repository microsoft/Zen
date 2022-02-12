// <copyright file="ZenDictSetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a dictionary set expression.
    /// </summary>
    internal sealed class ZenDictSetExpr<TKey, TValue> : Zen<IDictionary<TKey, TValue>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<IDictionary<TKey, TValue>>, Zen<TKey>, Zen<TValue>), ZenDictSetExpr<TKey, TValue>> createFunc = (v) =>
            new ZenDictSetExpr<TKey, TValue>(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenDictSetExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), ZenDictSetExpr<TKey, TValue>> hashConsTable =
            new HashConsTable<(long, long, long), ZenDictSetExpr<TKey, TValue>>();

        /// <summary>
        /// Unroll a ZenDictSetExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<IDictionary<TKey, TValue>> Unroll()
        {
            return Create(this.DictExpr.Unroll(), this.KeyExpr.Unroll(), this.ValueExpr.Unroll());
        }

        /// <summary>
        /// Create a new ZenDictSetExpr.
        /// </summary>
        /// <param name="dictExpr">The dictionary expr.</param>
        /// <param name="key">The key expr.</param>
        /// <param name="value">The value expr.</param>
        /// <returns>The new expr.</returns>
        public static ZenDictSetExpr<TKey, TValue> Create(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> key, Zen<TValue> value)
        {
            CommonUtilities.ValidateNotNull(dictExpr);
            CommonUtilities.ValidateNotNull(key);
            CommonUtilities.ValidateNotNull(value);

            var k = (dictExpr.Id, key.Id, value.Id);
            hashConsTable.GetOrAdd(k, (dictExpr, key, value), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDictSetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictExpr">The dictionary expression.</param>
        /// <param name="keyExpr">The key expression to add a value for.</param>
        /// <param name="valueExpr">The expression for the value.</param>
        private ZenDictSetExpr(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            this.DictExpr = dictExpr;
            this.KeyExpr = keyExpr;
            this.ValueExpr = valueExpr;
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
        /// Gets the value to add.
        /// </summary>
        public Zen<TValue> ValueExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Set({this.DictExpr}, {this.KeyExpr}, {this.ValueExpr})";
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
            return visitor.VisitZenDictSetExpr(this, parameter);
        }
    }
}
