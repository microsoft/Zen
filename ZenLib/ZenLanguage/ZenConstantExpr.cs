// <copyright file="ZenConstantExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a constant expression.
    /// </summary>
    internal sealed class ZenConstantExpr<T> : Zen<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<T, Zen<T>> createFunc = (v) => new ZenConstantExpr<T>(v);

        /// <summary>
        /// Hash cons table.
        /// </summary>
        private static HashConsTable<T, Zen<T>> hashConsTable = new HashConsTable<T, Zen<T>>();

        /// <summary>
        /// Gets the value.
        /// </summary>
        internal T Value { get; }

        /// <summary>
        /// Unroll the expression.
        /// </summary>
        /// <returns>The new unrolled expression.</returns>
        public override Zen<T> Unroll()
        {
            return this;
        }

        /// <summary>
        /// Create a new ZenConstantExpr.
        /// </summary>
        /// <param name="value">The constant value.</param>
        /// <returns>The Zen expr.</returns>
        public static Zen<T> Create(T value)
        {
            hashConsTable.GetOrAdd(value, value, createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstantExpr{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        private ZenConstantExpr(T value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return this.Value.ToString();
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
            return visitor.VisitZenConstantExpr(this, parameter);
        }
    }
}
