// <copyright file="ZenMapEmptyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an empty map expression.
    /// </summary>
    internal sealed class ZenMapEmptyExpr<TKey, TValue> : Zen<Map<TKey, TValue>>
    {
        /// <summary>
        /// The empty map instance.
        /// </summary>
        public static ZenMapEmptyExpr<TKey, TValue> Instance = new ZenMapEmptyExpr<TKey, TValue>();

        /// <summary>
        /// Unroll the expression.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<Map<TKey, TValue>> Unroll()
        {
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenMapEmptyExpr{TKey, TValue}"/> class.
        /// </summary>
        private ZenMapEmptyExpr()
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitMapEmpty(this, parameter);
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
