// <copyright file="ZenMapSetExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a map set expression.
    /// </summary>
    internal sealed class ZenMapSetExpr<TKey, TValue> : Zen<Map<TKey, TValue>>
    {
        /// <summary>
        /// Gets the map expr.
        /// </summary>
        public Zen<Map<TKey, TValue>> MapExpr { get; }

        /// <summary>
        /// Gets the key to add the value for.
        /// </summary>
        public Zen<TKey> KeyExpr { get; }

        /// <summary>
        /// Gets the value to add.
        /// </summary>
        public Zen<TValue> ValueExpr { get; }

        /// <summary>
        /// Simplify and create a new ZenMapSetExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Map<TKey, TValue>> Simplify((Zen<Map<TKey, TValue>> map, Zen<TKey> key, Zen<TValue> value) args)
        {
            if (args.map is ZenMapSetExpr<TKey, TValue> e1 && e1.KeyExpr.Equals(args.key))
            {
                return Create(e1.MapExpr, args.key, args.value);
            }

            if (args.map is ZenMapDeleteExpr<TKey, TValue> e2 && e2.KeyExpr.Equals(args.key))
            {
                return Create(e2.MapExpr, args.key, args.value);
            }

            return new ZenMapSetExpr<TKey, TValue>(args.map, args.key, args.value);
        }

        /// <summary>
        /// Create a new ZenMapSetExpr.
        /// </summary>
        /// <param name="mapExpr">The map expr.</param>
        /// <param name="key">The key expr.</param>
        /// <param name="value">The value expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Map<TKey, TValue>> Create(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> key, Zen<TValue> value)
        {
            Contract.AssertNotNull(mapExpr);
            Contract.AssertNotNull(key);
            Contract.AssertNotNull(value);

            var k = (mapExpr.Id, key.Id, value.Id);
            var flyweight = ZenAstCache<ZenMapSetExpr<TKey, TValue>, (long, long, long), Zen<Map<TKey, TValue>>>.Flyweight;
            flyweight.GetOrAdd(k, (mapExpr, key, value), Simplify, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapSetExpr{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="keyExpr">The key expression to add a value for.</param>
        /// <param name="valueExpr">The expression for the value.</param>
        private ZenMapSetExpr(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            this.MapExpr = mapExpr;
            this.KeyExpr = keyExpr;
            this.ValueExpr = valueExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Set({this.MapExpr}, {this.KeyExpr}, {this.ValueExpr})";
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
            return visitor.VisitMapSet(this, parameter);
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
