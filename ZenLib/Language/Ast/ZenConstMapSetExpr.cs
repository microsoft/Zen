// <copyright file="ZenConstMapSetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map set expression.
    /// </summary>
    internal sealed class ZenConstMapSetExpr<TKey, TValue> : Zen<CMap<TKey, TValue>>
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
        /// Gets the value to add.
        /// </summary>
        public Zen<TValue> ValueExpr { get; }

        /// <summary>
        /// Simplify and create a new ZenMapSetExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<CMap<TKey, TValue>> Simplify((Zen<CMap<TKey, TValue>> map, TKey key, Zen<TValue> value) args)
        {
            if (args.map is ZenConstMapSetExpr<TKey, TValue> e1 && e1.Key.Equals(args.key))
            {
                return Create(e1.MapExpr, args.key, args.value);
            }

            return new ZenConstMapSetExpr<TKey, TValue>(args.map, args.key, args.value);
        }

        /// <summary>
        /// Create a new ZenConstMapSetExpr.
        /// </summary>
        /// <param name="mapExpr">The map expr.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<CMap<TKey, TValue>> Create(Zen<CMap<TKey, TValue>> mapExpr, TKey key, Zen<TValue> value)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);
            Contract.AssertNotNull(value);

            var k = (mapExpr.Id, key, value.Id);
            var flyweight = ZenAstCache<ZenConstMapSetExpr<TKey, TValue>, Zen<CMap<TKey, TValue>>>.Flyweight;
            flyweight.GetOrAdd(k, (mapExpr, key, value), Simplify, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapSetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="key">The key to add a value for.</param>
        /// <param name="valueExpr">The expression for the value.</param>
        private ZenConstMapSetExpr(Zen<CMap<TKey, TValue>> mapExpr, TKey key, Zen<TValue> valueExpr)
        {
            this.MapExpr = mapExpr;
            this.Key = key;
            this.ValueExpr = valueExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Set({this.MapExpr}, {this.Key}, {this.ValueExpr})";
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
            return visitor.VisitConstMapSet(this, parameter);
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
