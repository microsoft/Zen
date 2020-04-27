// <copyright file="ZenListMatchExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a True expression.
    /// </summary>
    internal sealed class ZenListCaseExpr<T, TResult> : Zen<TResult>
    {
        public static ZenListCaseExpr<T, TResult> Create(
            Zen<IList<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> cons)
        {
            CommonUtilities.Validate(listExpr);
            CommonUtilities.Validate(empty);
            CommonUtilities.Validate(cons);

            return new ZenListCaseExpr<T, TResult>(listExpr, empty, cons);
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
            return $"Case<{typeof(T)}, {typeof(TResult)}>({this.ListExpr}, {this.EmptyCase}, {this.ConsCase.GetHashCode()})";
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

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<TResult> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenListCaseExpr(this);
        }
    }
}
