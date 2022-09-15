// <copyright file="ZenSeqIndexOfExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a substring expression.
    /// </summary>
    internal sealed class ZenSeqIndexOfExpr<T> : Zen<BigInteger>
    {
        /// <summary>
        /// Gets the seq expression.
        /// </summary>
        internal Zen<Seq<T>> SeqExpr { get; }

        /// <summary>
        /// Gets the subseq expression.
        /// </summary>
        internal Zen<Seq<T>> SubseqExpr { get; }

        /// <summary>
        /// Gets the third expression.
        /// </summary>
        internal Zen<BigInteger> OffsetExpr { get; }

        /// <summary>
        /// Simplify and create a ZenSeqIndexOfExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static Zen<BigInteger> Simplify((Zen<Seq<T>> e1, Zen<Seq<T>> e2, Zen<BigInteger> e3) args)
        {
            return new ZenSeqIndexOfExpr<T>(args.e1, args.e2, args.e3);
        }

        /// <summary>
        /// Create a new ZenSeqIndexOfExpr.
        /// </summary>
        /// <param name="expr1">the seq expr.</param>
        /// <param name="expr2">The subseq expr.</param>
        /// <param name="expr3">The offset expr.</param>
        /// <returns></returns>
        public static Zen<BigInteger> Create(Zen<Seq<T>> expr1, Zen<Seq<T>> expr2, Zen<BigInteger> expr3)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.AssertNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            var flyweight = ZenAstCache<ZenSeqIndexOfExpr<T>, (long, long, long), Zen<BigInteger>>.Flyweight;
            flyweight.GetOrAdd(key, (expr1, expr2, expr3), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqIndexOfExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The seq expression.</param>
        /// <param name="expr2">The subseq expression.</param>
        /// <param name="expr3">The offset expression.</param>
        private ZenSeqIndexOfExpr(Zen<Seq<T>> expr1, Zen<Seq<T>> expr2, Zen<BigInteger> expr3)
        {
            this.SeqExpr = expr1;
            this.SubseqExpr = expr2;
            this.OffsetExpr = expr3;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"IndexOf({this.SeqExpr}, {this.SubseqExpr}, {this.OffsetExpr})";
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
            return visitor.VisitSeqIndexOf(this, parameter);
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
