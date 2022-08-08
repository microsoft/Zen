// <copyright file="ZenMapDeleteExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map delete expression.
    /// </summary>
    internal sealed class ZenMapDeleteExpr<TKey, TValue> : Zen<Map<TKey, TValue>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Map<TKey, TValue>>, Zen<TKey>), ZenMapDeleteExpr<TKey, TValue>> createFunc = (v) =>
            new ZenMapDeleteExpr<TKey, TValue>(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenMapDeleteExpr.
        /// </summary>
        private static HashConsTable<(long, long), ZenMapDeleteExpr<TKey, TValue>> hashConsTable =
            new HashConsTable<(long, long), ZenMapDeleteExpr<TKey, TValue>>();

        /// <summary>
        /// Unroll a ZenMapDeleteExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<Map<TKey, TValue>> Unroll()
        {
            return Create(this.MapExpr.Unroll(), this.KeyExpr.Unroll());
        }

        /// <summary>
        /// Create a new ZenMapDeleteExpr.
        /// </summary>
        /// <param name="mapExpr">The map expr.</param>
        /// <param name="key">The key expr.</param>
        /// <returns>The new expr.</returns>
        public static ZenMapDeleteExpr<TKey, TValue> Create(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> key)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);

            var k = (mapExpr.Id, key.Id);
            hashConsTable.GetOrAdd(k, (mapExpr, key), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapDeleteExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="keyExpr">The key expression to add a value for.</param>
        private ZenMapDeleteExpr(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            this.MapExpr = mapExpr;
            this.KeyExpr = keyExpr;
        }

        /// <summary>
        /// Gets the map expr.
        /// </summary>
        public Zen<Map<TKey, TValue>> MapExpr { get; }

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
            return $"Delete({this.MapExpr}, {this.KeyExpr})";
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
            return visitor.VisitMapDelete(this, parameter);
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
