// <copyright file="ZenSeqContainsExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a seq contains expression.
    /// </summary>
    internal sealed class ZenSeqContainsExpr<T> : Zen<bool>
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
        /// Gets the containment type.
        /// </summary>
        internal SeqContainmentType ContainmentType { get; }

        /// <summary>
        /// Simplify and create a ZenSeqContainsExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<bool> Simplify((Zen<Seq<T>> e1, Zen<Seq<T>> e2, SeqContainmentType containmentType) args)
        {
            return new ZenSeqContainsExpr<T>(args.e1, args.e2, args.containmentType);
        }

        /// <summary>
        /// Create a new ZenSeqContainsExpr.
        /// </summary>
        /// <param name="expr1">The seq expr.</param>
        /// <param name="expr2">The subseq expr.</param>
        /// <param name="containmentType">The containment type.</param>
        /// <returns></returns>
        public static Zen<bool> Create(Zen<Seq<T>> expr1, Zen<Seq<T>> expr2, SeqContainmentType containmentType)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            var key = (expr1.Id, expr2.Id, (int)containmentType);
            var flyweight = ZenAstCache<ZenSeqContainsExpr<T>, Zen<bool>>.Flyweight;
            flyweight.GetOrAdd(key, (expr1, expr2, containmentType), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqContainsExpr{T}"/> class.
        /// </summary>
        /// <param name="seqExpr">The seq expression.</param>
        /// <param name="subseqExpr">The subseq expression.</param>
        /// <param name="containmentType">The containment type.</param>
        private ZenSeqContainsExpr(Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr, SeqContainmentType containmentType)
        {
            this.SeqExpr = seqExpr;
            this.SubseqExpr = subseqExpr;
            this.ContainmentType = containmentType;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            switch (this.ContainmentType)
            {
                case SeqContainmentType.Contains:
                    return $"Contains({this.SeqExpr}, {this.SubseqExpr})";
                case SeqContainmentType.HasPrefix:
                    return $"PrefixOf({this.SeqExpr}, {this.SubseqExpr})";
                case SeqContainmentType.HasSuffix:
                    return $"SuffixOf({this.SeqExpr}, {this.SubseqExpr})";
                default:
                    throw new ZenUnreachableException();
            }
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
            return visitor.VisitSeqContains(this, parameter);
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

    /// <summary>
    /// The sequence containment type.
    /// </summary>
    internal enum SeqContainmentType
    {
        HasPrefix,
        HasSuffix,
        Contains,
    }
}
