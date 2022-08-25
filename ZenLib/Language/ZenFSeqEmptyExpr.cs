﻿// <copyright file="ZenFSeqEmptyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an empty list expression.
    /// </summary>
    internal sealed class ZenFSeqEmptyExpr<T> : Zen<FSeq<T>>
    {
        /// <summary>
        /// The empty list instance.
        /// </summary>
        public static ZenFSeqEmptyExpr<T> Instance = new ZenFSeqEmptyExpr<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenFSeqEmptyExpr{T}"/> class.
        /// </summary>
        private ZenFSeqEmptyExpr()
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitListEmpty(this, parameter);
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