// <copyright file="ZenListAddFrontExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a list add expression.
    /// </summary>
    internal sealed class ZenListAddFrontExpr<T> : Zen<IList<T>>
    {
        private static Dictionary<(object, object), ZenListAddFrontExpr<T>> hashConsTable =
            new Dictionary<(object, object), ZenListAddFrontExpr<T>>();

        internal override Zen<IList<T>> Unroll()
        {
            return new ZenListAddFrontExpr<T>(this.Expr.Unroll(), this.Element.Unroll());
        }

        public static ZenListAddFrontExpr<T> Create(Zen<IList<T>> expr, Zen<T> element)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(element);

            var key = (expr, element);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenListAddFrontExpr<T>(expr, element);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenListAddFrontExpr{T}"/> class.
        /// </summary>
        /// <param name="expr">The list expression.</param>
        /// <param name="element">The expression for the element to add.</param>
        private ZenListAddFrontExpr(Zen<IList<T>> expr, Zen<T> element)
        {
            this.Expr = expr;
            this.Element = element;
        }

        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<IList<T>> Expr { get; }

        /// <summary>
        /// Gets the element to add.
        /// </summary>
        public Zen<T> Element { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Element} :: {this.Expr})";
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
            return visitor.VisitZenListAddFrontExpr(this, parameter);
        }
    }
}
