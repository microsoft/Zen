// <copyright file="ZenFSeqCaseExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a list case expression.
    /// </summary>
    internal sealed class ZenFSeqCaseExpr<T, TResult> : Zen<TResult>
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
        /// Gets the cons lambda.
        /// </summary>
        public ZenLambda<Pair<Option<T>, FSeq<T>>, TResult> ConsLambda { get; }

        /// <summary>
        /// Simplify and create a new ZenListCaseExpr.
        /// </summary>
        /// <param name="e">The list expr.</param>
        /// <param name="emptyCase">The empty case.</param>
        /// <param name="consCase">The cons case.</param>
        /// <returns></returns>
        private static Zen<TResult> Simplify(Zen<FSeq<T>> e, Zen<TResult> emptyCase, ZenLambda<Pair<Option<T>, FSeq<T>>, TResult> consCase)
        {
            if (e is ZenFSeqEmptyExpr<T>)
            {
                return emptyCase;
            }

            if (e is ZenFSeqAddFrontExpr<T> l2)
            {
                return consCase.Function(Pair.Create(l2.ElementExpr, l2.Expr));
            }

            return new ZenFSeqCaseExpr<T, TResult>(e, emptyCase, consCase);
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
            ZenLambda<Pair<Option<T>, FSeq<T>>, TResult> cons)
        {
            Contract.AssertNotNull(listExpr);
            Contract.AssertNotNull(empty);
            Contract.AssertNotNull(cons);

            return Simplify(listExpr, empty, cons);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenFSeqCaseExpr{TList, TResult}"/> class.
        /// </summary>
        /// <param name="listExpr">The list.</param>
        /// <param name="empty">The empty case.</param>
        /// <param name="cons">The cons case.</param>
        private ZenFSeqCaseExpr(
            Zen<FSeq<T>> listExpr,
            Zen<TResult> empty,
            ZenLambda<Pair<Option<T>, FSeq<T>>, TResult> cons)
        {
            this.ListExpr = listExpr;
            this.EmptyExpr = empty;
            this.ConsLambda = cons;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Case({this.ListExpr}, {this.EmptyExpr}, <lambda>={this.ConsLambda.GetHashCode()})";
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
