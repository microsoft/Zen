// <copyright file="ZenSeqEmptyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an empty sequence expression.
    /// </summary>
    internal sealed class ZenSeqEmptyExpr<T> : Zen<Seq<T>>
    {
        /// <summary>
        /// The empty seq instance.
        /// </summary>
        public static ZenSeqEmptyExpr<T> Instance = new ZenSeqEmptyExpr<T>();

        /// <summary>
        /// Unroll the expression.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<Seq<T>> Unroll()
        {
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqEmptyExpr{T}"/> class.
        /// </summary>
        private ZenSeqEmptyExpr()
        {
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"[]";
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
