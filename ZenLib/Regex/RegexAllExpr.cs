// <copyright file="RegexAllExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Regex all expression.
    /// </summary>
    internal sealed class RegexAllExpr<T> : Regex<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// The all Regex instance.
        /// </summary>
        public static Regex<T> Instance = new RegexAllExpr<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexAllExpr{T}"/> class.
        /// </summary>
        private RegexAllExpr()
        {
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"All";
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
            return visitor.VisitRegexAllExpr(this, parameter);
        }
    }
}
