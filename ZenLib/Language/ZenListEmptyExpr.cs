// <copyright file="ZenListEmptyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a True expression.
    /// </summary>
    internal sealed class ZenListEmptyExpr<T> : Zen<IList<T>>
    {
        /// <summary>
        /// The empty list instance.
        /// </summary>
        public static ZenListEmptyExpr<T> Instance = new ZenListEmptyExpr<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenListEmptyExpr{T}"/> class.
        /// </summary>
        private ZenListEmptyExpr()
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
            return visitor.VisitZenListEmptyExpr(this, parameter);
        }
    }
}
