// <copyright file="ZenConstMapGetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map get expression.
    /// </summary>
    internal sealed class ZenConstMapGetExpr<TKey, TValue> : Zen<TValue>
    {
        /// <summary>
        /// Gets the map expr.
        /// </summary>
        public Zen<CMap<TKey, TValue>> MapExpr { get; }

        /// <summary>
        /// Gets the key to add the value for.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// Simplify and create a new ZenConstMapGetExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<TValue> Simplify((Zen<CMap<TKey, TValue>> map, TKey key) args)
        {
            if (args.map is ZenConstMapSetExpr<TKey, TValue> e2 && e2.Key.Equals(args.key))
            {
                return e2.ValueExpr;
            }

            return new ZenConstMapGetExpr<TKey, TValue>(args.map, args.key);
        }

        /// <summary>
        /// Create a new ZenConstMapGetExpr.
        /// </summary>
        /// <param name="mapExpr">The map expr.</param>
        /// <param name="key">The key.</param>
        /// <returns>The new expr.</returns>
        public static Zen<TValue> Create(Zen<CMap<TKey, TValue>> mapExpr, TKey key)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);

            var k = (mapExpr.Id, key);
            var flyweight = ZenAstCache<ZenConstMapGetExpr<TKey, TValue>, (long, TKey), Zen<TValue>>.Flyweight;
            flyweight.GetOrAdd(k, (mapExpr, key), Simplify, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstMapGetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="key">The key to add a value for.</param>
        private ZenConstMapGetExpr(Zen<CMap<TKey, TValue>> mapExpr, TKey key)
        {
            this.MapExpr = mapExpr;
            this.Key = key;
        }

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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitConstMapGet(this, parameter);
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
