// <copyright file="ZenSeqSliceExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a sequence slice expression.
    /// </summary>
    internal sealed class ZenSeqSliceExpr<T> : Zen<Seq<T>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Seq<T>>, Zen<BigInteger>, Zen<BigInteger>), Zen<Seq<T>>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenSeqSliceExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<Seq<T>>> hashConsTable = new HashConsTable<(long, long, long), Zen<Seq<T>>>();

        /// <summary>
        /// Unroll a ZenSeqSliceExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<Seq<T>> Unroll()
        {
            return Create(this.SeqExpr.Unroll(), this.OffsetExpr.Unroll(), this.LengthExpr.Unroll());
        }

        /// <summary>
        /// Simplify and create a new ZenSeqSliceExpr.
        /// </summary>
        /// <param name="e1">The seq expression.</param>
        /// <param name="e2">The offset expression.</param>
        /// <param name="e3">The length expression.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<Seq<T>> Simplify(Zen<Seq<T>> e1, Zen<BigInteger> e2, Zen<BigInteger> e3)
        {
            return new ZenSeqSliceExpr<T>(e1, e2, e3);
        }

        /// <summary>
        /// Create a new ZenSeqSliceExpr.
        /// </summary>
        /// <param name="expr1">The seq expression.</param>
        /// <param name="expr2">The offset expression.</param>
        /// <param name="expr3">The length expression.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<Seq<T>> Create(Zen<Seq<T>> expr1, Zen<BigInteger> expr2, Zen<BigInteger> expr3)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2, expr3), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqSliceExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The seq expression.</param>
        /// <param name="expr2">The offset expression.</param>
        /// <param name="expr3">The length expression.</param>
        private ZenSeqSliceExpr(Zen<Seq<T>> expr1, Zen<BigInteger> expr2, Zen<BigInteger> expr3)
        {
            this.SeqExpr = expr1;
            this.OffsetExpr = expr2;
            this.LengthExpr = expr3;
        }

        /// <summary>
        /// Gets the seq expression.
        /// </summary>
        internal Zen<Seq<T>> SeqExpr { get; }

        /// <summary>
        /// Gets the offset expression.
        /// </summary>
        internal Zen<BigInteger> OffsetExpr { get; }

        /// <summary>
        /// Gets the length expression.
        /// </summary>
        internal Zen<BigInteger> LengthExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Slice({this.SeqExpr}, {this.OffsetExpr}, {this.LengthExpr})";
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
    }
}
