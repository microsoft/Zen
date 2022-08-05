// <copyright file="ZenCastExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a cast expression.
    /// </summary>
    internal sealed class ZenCastExpr<TSource, TTarget> : Zen<TTarget>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<Zen<TSource>, Zen<TTarget>> createFunc = (v) => new ZenCastExpr<TSource, TTarget>(v);

        /// <summary>
        /// Hash cons table for ZenCastExpr.
        /// </summary>
        private static HashConsTable<long, Zen<TTarget>> hashConsTable = new HashConsTable<long, Zen<TTarget>>();

        /// <summary>
        /// Unroll a ZenCastExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<TTarget> Unroll()
        {
            return Create(this.SourceExpr.Unroll());
        }

        /// <summary>
        /// Create a new ZenCastExpr.
        /// </summary>
        /// <param name="sourceExpr">The source expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<TTarget> Create(Zen<TSource> sourceExpr)
        {
            Contract.AssertNotNull(sourceExpr);
            Contract.Assert(CommonUtilities.IsSafeCast(typeof(TSource), typeof(TTarget)), "Invalid cast");

            hashConsTable.GetOrAdd(sourceExpr.Id, sourceExpr, createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCastExpr{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="sourceExpr">The source expression.</param>
        private ZenCastExpr(Zen<TSource> sourceExpr)
        {
            this.SourceExpr = sourceExpr;
        }

        /// <summary>
        /// Gets the source expr.
        /// </summary>
        public Zen<TSource> SourceExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.SourceExpr} as {typeof(TTarget).Name})";
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
