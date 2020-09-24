// <copyright file="ZenConstantExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a constant expression.
    /// </summary>
    internal sealed class ZenConstantExpr<T> : Zen<T>
    {
        /// <summary>
        /// Hash cons table.
        /// </summary>
        private static Dictionary<T, Zen<T>> hashConsTable = new Dictionary<T, Zen<T>>();

        /// <summary>
        /// Gets the value.
        /// </summary>
        internal T Value { get; }

        /// <summary>
        /// Unroll the expression.
        /// </summary>
        /// <returns>The new unrolled expression.</returns>
        internal override Zen<T> Unroll()
        {
            return this;
        }

        public static Zen<T> Create(T value)
        {
            if (hashConsTable.TryGetValue(value, out var v))
            {
                return v;
            }

            var ret = new ZenConstantExpr<T>(value);
            hashConsTable[value] = ret;
            return ret;
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
