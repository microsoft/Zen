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
        private static Func<(Zen<Map<TKey, TValue>>, Zen<TKey>), Zen<Option<TValue>>> createFunc = (v) =>
            Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenDictAddExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<Option<TValue>>> hashConsTable =
            new HashConsTable<(long, long), Zen<Option<TValue>>>();

        /// <summary>
        /// Unroll a ZenDictGetExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<Option<TValue>> Unroll()
        {
            return Create(this.DictExpr.Unroll(), this.KeyExpr.Unroll());
        }

        /// <summary>
        /// Simplify and create a new ZenDictGetExpr.
        /// </summary>
        /// <param name="dict">The dictionary expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Option<TValue>> Simplify(Zen<Map<TKey, TValue>> dict, Zen<TKey> key)
        {
            if (dict is ZenDictEmptyExpr<TKey, TValue>)
            {
                return Option.Null<TValue>();
            }

            if (dict is ZenDictDeleteExpr<TKey, TValue> e1 && e1.KeyExpr.Equals(key))
            {
                return Option.Null<TValue>();
            }

            if (dict is ZenDictSetExpr<TKey, TValue> e2 && e2.KeyExpr.Equals(key))
            {
                return Option.Create(e2.ValueExpr);
            }

            return new ZenDictGetExpr<TKey, TValue>(dict, key);
        }

        /// <summary>
        /// Create a new ZenDictGetExpr.
        /// </summary>
        /// <param name="dictExpr">The dictionary expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Option<TValue>> Create(Zen<Map<TKey, TValue>> dictExpr, Zen<TKey> key)
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
        private ZenDictGetExpr(Zen<Map<TKey, TValue>> dictExpr, Zen<TKey> keyExpr)
        {
            this.DictExpr = dictExpr;
            this.KeyExpr = keyExpr;
        }

        /// <summary>
        /// Gets the dictionary expr.
        /// </summary>
        public Zen<Map<TKey, TValue>> DictExpr { get; }

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
            return visitor.Visit(this, parameter);
        }
    }
}
