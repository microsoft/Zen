// <copyright file="ZenSeqLengthExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a sequence length expression.
    /// </summary>
    internal sealed class ZenSeqLengthExpr<T> : Zen<BigInteger>
    {
        /// <summary>
        /// Gets the seq expr.
        /// </summary>
        public Zen<Seq<T>> SeqExpr { get; }

        /// <summary>
        /// Simplify and create a new ZenSeqLengthExpr.
        /// </summary>
        /// <param name="seqExpr">The seq expr.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<BigInteger> Simplify(Zen<Seq<T>> seqExpr) => new ZenSeqLengthExpr<T>(seqExpr);

        /// <summary>
        /// Create a new ZenSeqLengthExpr.
        /// </summary>
        /// <param name="seqExpr">The seq expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<BigInteger> Create(Zen<Seq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            var flyweight = ZenAstCache<ZenSeqLengthExpr<T>, Zen<BigInteger>>.Flyweight;
            flyweight.GetOrAdd(seqExpr.Id, seqExpr, Simplify, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqLengthExpr{T}"/> class.
        /// </summary>
        /// <param name="seqExpr">The seq expression.</param>
        private ZenSeqLengthExpr(Zen<Seq<T>> seqExpr)
        {
            this.SeqExpr = seqExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Length({this.SeqExpr})";
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
            return visitor.VisitSeqLength(this, parameter);
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
