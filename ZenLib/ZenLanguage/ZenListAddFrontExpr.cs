// <copyright file="ZenListAddFrontExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a list add expression.
    /// </summary>
    internal sealed class ZenListAddFrontExpr<T> : Zen<FSeq<T>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<FSeq<T>>, Zen<Option<T>>), ZenListAddFrontExpr<T>> createFunc = (v) => new ZenListAddFrontExpr<T>(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenListAddFrontExpr.
        /// </summary>
        private static HashConsTable<(long, long), ZenListAddFrontExpr<T>> hashConsTable = new HashConsTable<(long, long), ZenListAddFrontExpr<T>>();

        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<FSeq<T>> Expr { get; }

        /// <summary>
        /// Gets the element to add.
        /// </summary>
        public Zen<Option<T>> ElementExpr { get; }

        /// <summary>
        /// Create a new ZenListAddFrontExpr.
        /// </summary>
        /// <param name="expr">The list expr.</param>
        /// <param name="element">The element expr.</param>
        /// <returns>The new expr.</returns>
        public static ZenListAddFrontExpr<T> Create(Zen<FSeq<T>> expr, Zen<Option<T>> element)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(element);

            var key = (expr.Id, element.Id);
            hashConsTable.GetOrAdd(key, (expr, element), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenListAddFrontExpr{T}"/> class.
        /// </summary>
        /// <param name="expr">The list expression.</param>
        /// <param name="element">The expression for the element to add.</param>
        private ZenListAddFrontExpr(Zen<FSeq<T>> expr, Zen<Option<T>> element)
        {
            this.Expr = expr;
            this.ElementExpr = element;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Cons({this.ElementExpr}, {this.Expr})";
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
            return visitor.VisitListAdd(this, parameter);
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
