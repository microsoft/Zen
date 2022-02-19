// <copyright file="ZenDictEmptyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an empty dictionary expression.
    /// </summary>
    internal sealed class ZenDictEmptyExpr<TKey, TValue> : Zen<IDictionary<TKey, TValue>>
    {
        /// <summary>
        /// The empty dictionary instance.
        /// </summary>
        public static ZenDictEmptyExpr<TKey, TValue> Instance = new ZenDictEmptyExpr<TKey, TValue>();

        /// <summary>
        /// Unroll the expression.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<IDictionary<TKey, TValue>> Unroll()
        {
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenDictEmptyExpr{TKey, TValue}"/> class.
        /// </summary>
        private ZenDictEmptyExpr()
        {
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{}";
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
