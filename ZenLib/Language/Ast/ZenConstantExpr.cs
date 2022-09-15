// <copyright file="ZenConstantExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a constant expression.
    /// </summary>
    internal sealed class ZenConstantExpr<T> : Zen<T>
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        internal T Value { get; }

        /// <summary>
        /// Simplify and create a new ZenBitwiseNot expr.
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <returns>The new expr.</returns>
        private static Zen<T> Simplify(T c) => new ZenConstantExpr<T>(c);

        /// <summary>
        /// Create a new ZenConstantExpr.
        /// </summary>
        /// <param name="value">The constant value.</param>
        /// <returns>The Zen expr.</returns>
        public static Zen<T> Create(T value)
        {
            var flyweight = ZenAstCache<ZenConstantExpr<T>, T, Zen<T>>.Flyweight;
            flyweight.GetOrAdd(value, value, Simplify, out var v);
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitConstant(this, parameter);
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
