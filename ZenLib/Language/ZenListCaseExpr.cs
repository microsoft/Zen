// <copyright file="ZenListMatchExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a True expression.
    /// </summary>
    internal sealed class ZenListCaseExpr<T, TResult> : Zen<TResult>
    {
        private static Zen<TResult> Simplify(Zen<IList<T>> e, Zen<TResult> emptyCase, Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> consCase)
        {
            if (e is ZenListEmptyExpr<T> l1)
            {
                return emptyCase;
            }

            if (e is ZenListAddFrontExpr<T> l2)
            {
                return consCase(l2.Element, l2.Expr);
            }

            if (Settings.SimplifyRecursive)
            {
                if (e is ZenIfExpr<IList<T>> l3)
                {
                    var tbranch = ZenListCaseExpr<T, TResult>.Create(l3.TrueExpr, emptyCase, consCase);
                    var fbranch = ZenListCaseExpr<T, TResult>.Create(l3.FalseExpr, emptyCase, consCase);
                    return ZenIfExpr<TResult>.Create(l3.GuardExpr, tbranch, fbranch);
                }
            }

            return new ZenListCaseExpr<T, TResult>(e, emptyCase, consCase);
        }

        public static Zen<TResult> Create(
            Zen<IList<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> cons)
        {
            CommonUtilities.ValidateNotNull(listExpr);
            CommonUtilities.ValidateNotNull(empty);
            CommonUtilities.ValidateNotNull(cons);

            return Simplify(listExpr, empty, cons);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenListCaseExpr{TList, TResult}"/> class.
        /// </summary>
        /// <param name="listExpr">The list.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        private ZenListCaseExpr(
            Zen<IList<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> cons)
        {
            this.ListExpr = listExpr;
            this.EmptyCase = empty;
            this.ConsCase = cons;
        }

        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<IList<T>> ListExpr { get; }

        /// <summary>
        /// Gets the list expr.
        /// </summary>
        public Zen<TResult> EmptyCase { get; }

        /// <summary>
        /// Gets the element to add.
        /// </summary>
        public Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> ConsCase { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"case({this.ListExpr}, {this.EmptyCase}, {this.ConsCase.GetHashCode()})";
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
            return visitor.VisitZenListCaseExpr(this, parameter);
        }
    }
}
