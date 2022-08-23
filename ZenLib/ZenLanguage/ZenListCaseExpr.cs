// <copyright file="ZenListCaseExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a list case expression.
    /// </summary>
    internal sealed class ZenListCaseExpr<T, TResult> : Zen<TResult>
    {
        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<FSeq<T>> ListExpr { get; }

        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<TResult> EmptyExpr { get; }

        /// <summary>
        /// Gets the element to add.
        /// </summary>
        public Func<Zen<Option<T>>, Zen<FSeq<T>>, Zen<TResult>> ConsCase { get; }

        /// <summary>
        /// Simplify and create a new ZenListCaseExpr.
        /// </summary>
        /// <param name="e">The list expr.</param>
        /// <param name="emptyCase">The empty case.</param>
        /// <param name="consCase">The cons case.</param>
        /// <returns></returns>
        private static Zen<TResult> Simplify(Zen<FSeq<T>> e, Zen<TResult> emptyCase, Func<Zen<Option<T>>, Zen<FSeq<T>>, Zen<TResult>> consCase)
        {
            if (e is ZenListEmptyExpr<T>)
            {
                return emptyCase;
            }

            if (e is ZenListAddFrontExpr<T> l2)
            {
                return consCase(l2.ElementExpr, l2.Expr);
            }

            return new ZenListCaseExpr<T, TResult>(e, emptyCase, consCase);
        }

        /// <summary>
        /// Create a new ZenListCaseExpr.
        /// </summary>
        /// <param name="listExpr">TThe list expr.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        /// <returns>The new expr.</returns>
        public static Zen<TResult> Create(
            Zen<FSeq<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<Option<T>>, Zen<FSeq<T>>, Zen<TResult>> cons)
        {
            Contract.AssertNotNull(listExpr);
            Contract.AssertNotNull(empty);
            Contract.AssertNotNull(cons);

            return Simplify(listExpr, empty, cons);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenListCaseExpr{TList, TResult}"/> class.
        /// </summary>
        /// <param name="listExpr">The list.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        private ZenListCaseExpr(
            Zen<FSeq<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<Option<T>>, Zen<FSeq<T>>, Zen<TResult>> cons)
        {
            this.ListExpr = listExpr;
            this.EmptyExpr = empty;
            this.ConsCase = cons;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Case({this.ListExpr}, {this.EmptyExpr}, <lambda>)";
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
            return visitor.VisitListCase(this, parameter);
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
