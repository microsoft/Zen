// <copyright file="ZenMapGetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map get expression.
    /// </summary>
    internal sealed class ZenMapGetExpr<TKey, TValue> : Zen<Option<TValue>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Map<TKey, TValue>>, Zen<TKey>), Zen<Option<TValue>>> createFunc = (v) =>
            Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenMapAddExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<Option<TValue>>> hashConsTable =
            new HashConsTable<(long, long), Zen<Option<TValue>>>();

        /// <summary>
        /// Gets the map expr.
        /// </summary>
        public Zen<Map<TKey, TValue>> MapExpr { get; }

        /// <summary>
        /// Gets the key to add the value for.
        /// </summary>
        public Zen<TKey> KeyExpr { get; }

        /// <summary>
        /// Simplify and create a new ZenMapGetExpr.
        /// </summary>
        /// <param name="map">The map expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Option<TValue>> Simplify(Zen<Map<TKey, TValue>> map, Zen<TKey> key)
        {
            if (map is ZenMapEmptyExpr<TKey, TValue>)
            {
                return Option.Null<TValue>();
            }

            if (map is ZenMapDeleteExpr<TKey, TValue> e1 && e1.KeyExpr.Equals(key))
            {
                return Option.Null<TValue>();
            }

            if (map is ZenMapSetExpr<TKey, TValue> e2 && e2.KeyExpr.Equals(key))
            {
                return Option.Create(e2.ValueExpr);
            }

            return new ZenMapGetExpr<TKey, TValue>(map, key);
        }

        /// <summary>
        /// Create a new ZenMapGetExpr.
        /// </summary>
        /// <param name="mapExpr">The map expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Option<TValue>> Create(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> key)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);

            var k = (mapExpr.Id, key.Id);
            hashConsTable.GetOrAdd(k, (mapExpr, key), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapSetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="keyExpr">The key expression to add a value for.</param>
        private ZenMapGetExpr(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            this.MapExpr = mapExpr;
            this.KeyExpr = keyExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Get({this.MapExpr}, {this.KeyExpr})";
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitMapGet(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
