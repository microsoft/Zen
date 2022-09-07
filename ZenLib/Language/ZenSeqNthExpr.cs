// <copyright file="ZenSeqNthExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a sequence nth expression.
    /// </summary>
    internal sealed class ZenSeqNthExpr<T> : Zen<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Seq<T>>, Zen<BigInteger>), Zen<T>> createFunc = (v) => Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenSeqAtExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<T>> hashConsTable = new HashConsTable<(long, long), Zen<T>>();

        /// <summary>
        /// Gets the seq expression.
        /// </summary>
        internal Zen<Seq<T>> SeqExpr { get; }

        /// <summary>
        /// Gets the index expression.
        /// </summary>
        internal Zen<BigInteger> IndexExpr { get; }

        /// <summary>
        /// Simplify and create a ZenSeqNthExpr.
        /// </summary>
        /// <param name="e1">The seq expr.</param>
        /// <param name="e2">The index expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<T> Simplify(Zen<Seq<T>> e1, Zen<BigInteger> e2)
        {
            return new ZenSeqNthExpr<T>(e1, e2);
        }

        /// <summary>
        /// Create a new ZenSeqAtExpr.
        /// </summary>
        /// <param name="expr1">The seq expr.</param>
        /// <param name="expr2">The index expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<T> Create(Zen<Seq<T>> expr1, Zen<BigInteger> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            var key = (expr1.Id, expr2.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqAtExpr{T}"/> class.
        /// </summary>
        /// <param name="seqExpr">The seq expression.</param>
        /// <param name="indexExpr">The index expression.</param>
        private ZenSeqNthExpr(Zen<Seq<T>> seqExpr, Zen<BigInteger> indexExpr)
        {
            this.SeqExpr = seqExpr;
            this.IndexExpr = indexExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Nth({this.SeqExpr}, {this.IndexExpr})";
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
            return visitor.VisitSeqNth(this, parameter);
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
