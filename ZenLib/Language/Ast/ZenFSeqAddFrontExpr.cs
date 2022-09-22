// <copyright file="ZenFSeqAddFrontExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a list add expression.
    /// </summary>
    internal sealed class ZenFSeqAddFrontExpr<T> : Zen<FSeq<T>>
    {
        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<FSeq<T>> ListExpr { get; }

        /// <summary>
        /// Gets the element to add.
        /// </summary>
        public Zen<Option<T>> ElementExpr { get; }

        /// <summary>
        /// Simplify and create a ZenFSeqAddFrontExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A new expr.</returns>
        private static Zen<FSeq<T>> Simplify((Zen<FSeq<T>> e1, Zen<Option<T>> e2) args) => new ZenFSeqAddFrontExpr<T>(args.e1, args.e2);

        /// <summary>
        /// Create a new ZenListAddFrontExpr.
        /// </summary>
        /// <param name="expr">The list expr.</param>
        /// <param name="element">The element expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<FSeq<T>> Create(Zen<FSeq<T>> expr, Zen<Option<T>> element)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(element);

            var key = (expr.Id, element.Id);
            var flyweight = ZenAstCache<ZenFSeqAddFrontExpr<T>, Zen<FSeq<T>>>.Flyweight;
            flyweight.GetOrAdd(key, (expr, element), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenFSeqAddFrontExpr{T}"/> class.
        /// </summary>
        /// <param name="expr">The list expression.</param>
        /// <param name="element">The expression for the element to add.</param>
        private ZenFSeqAddFrontExpr(Zen<FSeq<T>> expr, Zen<Option<T>> element)
        {
            this.ListExpr = expr;
            this.ElementExpr = element;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Cons({this.ElementExpr}, {this.ListExpr})";
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
