// <copyright file="ZenSeqRegexExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a seq regex match expression.
    /// </summary>
    internal sealed class ZenSeqRegexExpr<T> : Zen<bool>
    {
        /// <summary>
        /// Hash cons table for ZenSeqContainsExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<bool>> hashConsTable = new HashConsTable<(long, long), Zen<bool>>();

        /// <summary>
        /// Gets the seq expression.
        /// </summary>
        internal Zen<Seq<T>> SeqExpr { get; }

        /// <summary>
        /// Gets the Regex expression.
        /// </summary>
        internal Regex<T> Regex { get; }

        /// <summary>
        /// Simplify and create a ZenSeqRegexExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<bool> Simplify((Zen<Seq<T>> e1, Regex<T> e2) args) => new ZenSeqRegexExpr<T>(args.e1, args.e2);

        /// <summary>
        /// Create a new ZenSeqRegexExpr.
        /// </summary>
        /// <param name="expr1">The seq expr.</param>
        /// <param name="expr2">The Regex expr.</param>
        /// <returns></returns>
        public static Zen<bool> Create(Zen<Seq<T>> expr1, Regex<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            var key = (expr1.Id, expr2.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqRegexExpr{T}"/> class.
        /// </summary>
        /// <param name="seqExpr">The seq expression.</param>
        /// <param name="regex">The Regex expression.</param>
        private ZenSeqRegexExpr(Zen<Seq<T>> seqExpr, Regex<T> regex)
        {
            this.SeqExpr = seqExpr;
            this.Regex = regex;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"InRe({this.SeqExpr}, {this.Regex})";
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
            return visitor.VisitSeqRegex(this, parameter);
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
