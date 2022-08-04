// <copyright file="ZenConstMapGetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map get expression.
    /// </summary>
    internal sealed class ZenConstMapGetExpr<TKey, TValue> : Zen<TValue>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<ConstMap<TKey, TValue>>, TKey), Zen<TValue>> createFunc = (v) =>
            Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenConstMapGetExpr.
        /// </summary>
        private static HashConsTable<(long, TKey), Zen<TValue>> hashConsTable =
            new HashConsTable<(long, TKey), Zen<TValue>>();

        /// <summary>
        /// Unroll a ZenConstMapGetExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<TValue> Unroll()
        {
            return Create(this.MapExpr.Unroll(), this.Key);
        }

        /// <summary>
        /// Simplify and create a new ZenConstMapGetExpr.
        /// </summary>
        /// <param name="map">The map expr.</param>
        /// <param name="key">The key.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<TValue> Simplify(Zen<ConstMap<TKey, TValue>> map, TKey key)
        {
            if (map is ZenConstMapSetExpr<TKey, TValue> e2 && e2.Key.Equals(key))
            {
                return e2.ValueExpr;
            }

            return new ZenConstMapGetExpr<TKey, TValue>(map, key);
        }

        /// <summary>
        /// Create a new ZenConstMapGetExpr.
        /// </summary>
        /// <param name="mapExpr">The map expr.</param>
        /// <param name="key">The key.</param>
        /// <returns>The new expr.</returns>
        public static Zen<TValue> Create(Zen<ConstMap<TKey, TValue>> mapExpr, TKey key)
        {
            CommonUtilities.ValidateNotNull(mapExpr);
            CommonUtilities.ValidateNotNull(key);

            var k = (mapExpr.Id, key);
            hashConsTable.GetOrAdd(k, (mapExpr, key), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstMapGetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="key">The key to add a value for.</param>
        private ZenConstMapGetExpr(Zen<ConstMap<TKey, TValue>> mapExpr, TKey key)
        {
            this.MapExpr = mapExpr;
            this.Key = key;
        }

        /// <summary>
        /// Gets the map expr.
        /// </summary>
        public Zen<ConstMap<TKey, TValue>> MapExpr { get; }

        /// <summary>
        /// Gets the key to add the value for.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Get({this.MapExpr}, {this.Key})";
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
