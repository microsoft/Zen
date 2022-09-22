// <copyright file="ZenSeqSliceExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a sequence slice expression.
    /// </summary>
    internal sealed class ZenSeqSliceExpr<T> : Zen<Seq<T>>
    {
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
        /// Simplify and create a new ZenSeqSliceExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<Seq<T>> Simplify((Zen<Seq<T>> e1, Zen<BigInteger> e2, Zen<BigInteger> e3) args)
        {
            return new ZenSeqSliceExpr<T>(args.e1, args.e2, args.e3);
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
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.AssertNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            var flyweight = ZenAstCache<ZenSeqSliceExpr<T>, Zen<Seq<T>>>.Flyweight;
            flyweight.GetOrAdd(key, (expr1, expr2, expr3), Simplify, out var value);
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitSeqSlice(this, parameter);
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
