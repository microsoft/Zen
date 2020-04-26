// <copyright file="ZenAdapterExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Expression that allows for converting between types.
    /// </summary>
    internal sealed class ZenAdapterExpr<TTo, TFrom> : Zen<TTo>
    {
        private static Dictionary<object, ZenAdapterExpr<TTo, TFrom>> hashConsTable =
            new Dictionary<object, ZenAdapterExpr<TTo, TFrom>>();

        public static Zen<TTo> Create(Zen<TFrom> expr, Func<object, object> converter)
        {
            var converters = ImmutableList<Func<object, object>>.Empty.Add(converter);
            return CreateMulti(expr, converters);
        }

        public static Zen<TTo> CreateMulti(Zen<TFrom> expr, ImmutableList<Func<object, object>> converters)
        {
            CommonUtilities.Validate(expr);
            var key = (expr, converters);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenAdapterExpr<TTo, TFrom>(expr, converters);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAdapterExpr{TTo, TFrom}"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="converters">Converter between types.</param>
        private ZenAdapterExpr(Zen<TFrom> expr, ImmutableList<Func<object, object>> converters)
        {
            this.Expr = expr;
            this.Converters = converters;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Zen<TFrom> Expr { get; }

        /// <summary>
        /// Gets the converter.
        /// </summary>
        public ImmutableList<Func<object, object>> Converters { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Adapter({typeof(TTo)}, {typeof(TFrom)}, {this.Expr})";
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
            return visitor.VisitZenAdapterExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<TTo> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenAdapterExpr(this);
        }
    }
}
