// <copyright file="RegexBeginAnchorExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Regex anchor.
    /// </summary>
    internal sealed class RegexAnchorExpr<T> : Regex<T>
    {
        /// <summary>
        /// The begin anchor Regex instance.
        /// </summary>
        public static Regex<T> BeginInstance = new RegexAnchorExpr<T>(true);

        /// <summary>
        /// The end anchor Regex instance.
        /// </summary>
        public static Regex<T> EndInstance = new RegexAnchorExpr<T>(false);

        /// <summary>
        /// Whether this is a begin anchor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public bool IsBegin { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexAnchorExpr{T}"/> class.
        /// </summary>
        /// <param name="isBegin">Whether this is a begin anchor.</param>
        private RegexAnchorExpr(bool isBegin)
        {
            this.IsBegin = isBegin;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var anchorChar = this.IsBegin ? '^' : '$';
            return $"Anchor({anchorChar})";
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(IRegexExprVisitor<T, TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.Visit(this, parameter);
        }
    }
}
