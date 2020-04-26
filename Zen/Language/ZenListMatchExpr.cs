// <copyright file="ZenListMatchExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a True expression.
    /// </summary>
    internal sealed class ZenListMatchExpr<T, TResult> : Zen<TResult>
    {
        private static Dictionary<object, ZenListMatchExpr<T, TResult>> hashConsTable =
            new Dictionary<object, ZenListMatchExpr<T, TResult>>();

        public static ZenListMatchExpr<T, TResult> Create(
            object uniqueId,
            Zen<IList<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> cons)
        {
            CommonUtilities.Validate(uniqueId);
            CommonUtilities.Validate(listExpr);
            CommonUtilities.Validate(empty);
            CommonUtilities.Validate(cons);

            var key = (uniqueId, listExpr, empty, cons);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenListMatchExpr<T, TResult>(uniqueId, listExpr, empty, cons);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenListMatchExpr{TList, TResult}"/> class.
        /// </summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <param name="listExpr">The list.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        private ZenListMatchExpr(
            object uniqueId,
            Zen<IList<T>> listExpr,
            Zen<TResult> empty,
            Func<Zen<T>, Zen<IList<T>>, Zen<TResult>> cons)
        {
            this.UniqueId = uniqueId;
            this.ListExpr = listExpr;
            this.EmptyCase = empty;
            this.ConsCase = cons;
        }

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        public object UniqueId { get; }

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
            return $"Match<{typeof(T)}, {typeof(TResult)}>({this.UniqueId}, {this.ListExpr}, {this.EmptyCase}, {this.ConsCase.GetHashCode()})";
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
            return visitor.VisitZenListMatchExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<TResult> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenListMatchExpr(this);
        }
    }
}
