﻿// <copyright file="ZenSeqConcatExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a sequence concatenation expression.
    /// </summary>
    internal sealed class ZenSeqConcatExpr<T> : Zen<Seq<T>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<Seq<T>>, Zen<Seq<T>>), Zen<Seq<T>>> createFunc = (v) =>
            Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenSeqConcatExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<Seq<T>>> hashConsTable =
            new HashConsTable<(long, long), Zen<Seq<T>>>();

        /// <summary>
        /// Unroll a ZenSeqConcatExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<Seq<T>> Unroll()
        {
            return Create(this.SeqExpr1.Unroll(), this.SeqExpr2.Unroll());
        }

        /// <summary>
        /// Simplify and create a new ZenSeqConcatExpr.
        /// </summary>
        /// <param name="seqExpr1">The seq expr.</param>
        /// <param name="seqExpr2">The seq expr.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Seq<T>> Simplify(Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2)
        {
            return new ZenSeqConcatExpr<T>(seqExpr1, seqExpr2);
        }

        /// <summary>
        /// Create a new ZenSeqConcatExpr.
        /// </summary>
        /// <param name="seqExpr1">The first seq expr.</param>
        /// <param name="seqExpr2">The second seq expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Seq<T>> Create(Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2)
        {
            CommonUtilities.ValidateNotNull(seqExpr1);
            CommonUtilities.ValidateNotNull(seqExpr2);

            var k = (seqExpr1.Id, seqExpr2.Id);
            hashConsTable.GetOrAdd(k, (seqExpr1, seqExpr2), createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqConcatExpr{T}"/> class.
        /// </summary>
        /// <param name="seqExpr1">The first seq expression.</param>
        /// <param name="seqExpr2">The second seq expression.</param>
        private ZenSeqConcatExpr(Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2)
        {
            this.SeqExpr1 = seqExpr1;
            this.SeqExpr2 = seqExpr2;
        }

        /// <summary>
        /// Gets the first seq expr.
        /// </summary>
        public Zen<Seq<T>> SeqExpr1 { get; }

        /// <summary>
        /// Gets the second seq expr.
        /// </summary>
        public Zen<Seq<T>> SeqExpr2 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Concat({this.SeqExpr1}, {this.SeqExpr2})";
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
            return visitor.VisitZenSeqConcatExpr(this, parameter);
        }
    }
}
