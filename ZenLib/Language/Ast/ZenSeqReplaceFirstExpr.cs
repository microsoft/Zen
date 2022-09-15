// <copyright file="ZenSeqReplaceFirstExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a seq replacement expression.
    /// </summary>
    internal sealed class ZenSeqReplaceFirstExpr<T> : Zen<Seq<T>>
    {
        /// <summary>
        /// Hash cons table for ZenSeqReplaceFirstExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<Seq<T>>> hashConsTable = new HashConsTable<(long, long, long), Zen<Seq<T>>>();

        /// <summary>
        /// Gets the seq expression.
        /// </summary>
        internal Zen<Seq<T>> SeqExpr { get; }

        /// <summary>
        /// Gets the subseq expression.
        /// </summary>
        internal Zen<Seq<T>> SubseqExpr { get; }

        /// <summary>
        /// Gets the replacement expression.
        /// </summary>
        internal Zen<Seq<T>> ReplaceExpr { get; }

        /// <summary>
        /// Simplify and create a new ZenSeqReplaceFirstExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<Seq<T>> Simplify((Zen<Seq<T>> e1, Zen<Seq<T>> e2, Zen<Seq<T>> e3) args)
        {
            return new ZenSeqReplaceFirstExpr<T>(args.e1, args.e2, args.e3);
        }

        /// <summary>
        /// Create a new ZenSeqReplaceFirstExpr.
        /// </summary>
        /// <param name="expr1">The seq expr.</param>
        /// <param name="expr2">The subseq expr.</param>
        /// <param name="expr3">The replacement expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<Seq<T>> Create(Zen<Seq<T>> expr1, Zen<Seq<T>> expr2, Zen<Seq<T>> expr3)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.AssertNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2, expr3), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqReplaceFirstExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The seq expression.</param>
        /// <param name="expr2">The subseq expression.</param>
        /// <param name="expr3">The replacement expression.</param>
        private ZenSeqReplaceFirstExpr(Zen<Seq<T>> expr1, Zen<Seq<T>> expr2, Zen<Seq<T>> expr3)
        {
            this.SeqExpr = expr1;
            this.SubseqExpr = expr2;
            this.ReplaceExpr = expr3;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"ReplaceFirst({this.SeqExpr}, {this.SubseqExpr}, {this.ReplaceExpr})";
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
            return visitor.VisitSeqReplaceFirst(this, parameter);
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
