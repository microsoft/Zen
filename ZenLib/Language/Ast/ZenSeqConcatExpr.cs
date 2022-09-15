// <copyright file="ZenSeqConcatExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a sequence concatenation expression.
    /// </summary>
    internal sealed class ZenSeqConcatExpr<T> : Zen<Seq<T>>
    {
        /// <summary>
        /// Hash cons table for ZenSeqConcatExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<Seq<T>>> hashConsTable =
            new HashConsTable<(long, long), Zen<Seq<T>>>();

        /// <summary>
        /// Gets the first seq expr.
        /// </summary>
        public Zen<Seq<T>> SeqExpr1 { get; }

        /// <summary>
        /// Gets the second seq expr.
        /// </summary>
        public Zen<Seq<T>> SeqExpr2 { get; }

        /// <summary>
        /// Simplify and create a new ZenSeqConcatExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<Seq<T>> Simplify((Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2) args)
        {
            if (args.seqExpr1 is ZenConstantExpr<Seq<T>> e1 && e1.Value.Length() == 0)
            {
                return args.seqExpr2;
            }

            if (args.seqExpr2 is ZenConstantExpr<Seq<T>> e2 && e2.Value.Length() == 0)
            {
                return args.seqExpr1;
            }

            return new ZenSeqConcatExpr<T>(args.seqExpr1, args.seqExpr2);
        }

        /// <summary>
        /// Create a new ZenSeqConcatExpr.
        /// </summary>
        /// <param name="seqExpr1">The first seq expr.</param>
        /// <param name="seqExpr2">The second seq expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Seq<T>> Create(Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2)
        {
            Contract.AssertNotNull(seqExpr1);
            Contract.AssertNotNull(seqExpr2);

            var k = (seqExpr1.Id, seqExpr2.Id);
            hashConsTable.GetOrAdd(k, (seqExpr1, seqExpr2), Simplify, out var v);
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitSeqConcat(this, parameter);
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
