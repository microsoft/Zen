// <copyright file="ZenArbitraryExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A placeholder for an abtrary value of a given type.
    /// </summary>
    /// <typeparam name="T">Type of an underlying C# value.</typeparam>
    internal sealed class ZenArbitraryExpr<T> : Zen<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZenArbitraryExpr{T}"/> class.
        /// </summary>
        public ZenArbitraryExpr() { }

        public override Zen<T> Unroll()
        {
            return this;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Var({this.GetHashCode()})";
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
            return visitor.VisitZenArbitraryExpr(this, parameter);
        }
    }
}
