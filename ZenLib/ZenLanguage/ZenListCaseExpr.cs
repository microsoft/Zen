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
        /// Unroll the ZenListCaseExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<TResult> Unroll()
        {
            return Create(this.ListExpr.Unroll(), this.EmptyExpr.Unroll(), this.ConsCase, true);
        }

        /// <summary>
        /// Simplify and create a new ZenListCaseExpr.
        /// </summary>
        /// <param name="e">The list expr.</param>
        /// <param name="emptyCase">The empty case.</param>
        /// <param name="consCase">The cons case.</param>
        /// <param name="unroll">Whether to unroll the expr.</param>
        /// <returns></returns>
        private static Zen<TResult> Simplify(Zen<FSeq<T>> e, Zen<TResult> emptyCase, Func<Zen<T>, Zen<FSeq<T>>, Zen<TResult>> consCase, bool unroll)
        {
            if (e is ZenListEmptyExpr<T> l1)
            {
                return emptyCase;
            }

            if (e is ZenListAddFrontExpr<T> l2)
            {
                return consCase(l2.ElementExpr, l2.Expr);
            }

            if (unroll && e is ZenIfExpr<FSeq<T>> l3)
            {
                var tbranch = Create(l3.TrueExpr, emptyCase, consCase);
                var fbranch = Create(l3.FalseExpr, emptyCase, consCase);
                return ZenIfExpr<TResult>.Create(l3.GuardExpr, tbranch.Unroll(), fbranch.Unroll());
            }

            return new ZenListCaseExpr<T, TResult>(e, emptyCase, consCase);
        }

        /// <summary>
        /// Create a new ZenListCaseExpr.
        /// </summary>
        /// <param name="listExpr">TThe list expr.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        /// <param name="unroll">Whether to unroll the expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<TResult> Create(
            Zen<FSeq<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<FSeq<T>>, Zen<TResult>> cons,
            bool unroll = false)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(empty);
            CommonUtilities.ValidateNotNull(cons);

            return Simplify(listExpr, empty, cons, unroll);
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
            Func<Zen<T>, Zen<FSeq<T>>, Zen<TResult>> cons)
        {
            this.ListExpr = listExpr;
            this.EmptyExpr = empty;
            this.ConsCase = cons;
        }

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
        public Func<Zen<T>, Zen<FSeq<T>>, Zen<TResult>> ConsCase { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Case({this.ListExpr}, {this.EmptyExpr}, {this.ConsCase.GetHashCode()})";
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
