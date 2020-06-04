// <copyright file="ZenArgumentExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A function argument placeholder expression..
    /// </summary>
    /// <typeparam name="T">Type of an underlying C# value.</typeparam>
    internal sealed class ZenArgumentExpr<T> : Zen<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZenArgumentExpr{T}"/> class.
        /// </summary>
        public ZenArgumentExpr()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the unique id for the object.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Arg({this.Id})";
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
            return visitor.VisitZenArgumentExpr(this, parameter);
        }
    }
}
