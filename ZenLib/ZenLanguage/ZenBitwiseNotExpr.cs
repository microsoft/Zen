// <copyright file="ZenBitwiseNotExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a BitwiseNot expression.
    /// </summary>
    internal sealed class ZenBitwiseNotExpr<T> : Zen<T>
    {
        /// <summary>
        /// Hash cons table for ZenBitwiseNot expr.
        /// </summary>
        private static HashConsTable<long, Zen<T>> hashConsTable = new HashConsTable<long, Zen<T>>();

        /// <summary>
        /// Unroll a ZenBitwiseNotExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<T> Unroll()
        {
            return Create(this.Expr.Unroll());
        }

        /// <summary>
        /// Simplify and create a new ZenBitwiseNot expr.
        /// </summary>
        /// <param name="e">The expr to bitwise negate.</param>
        /// <returns>The new expr.</returns>
        private static Zen<T> Simplify(Zen<T> e)
        {
            var x = ReflectionUtilities.GetConstantIntegerValue(e);

            if (x.HasValue)
            {
                return ReflectionUtilities.CreateConstantIntegerValue<T>(~x.Value);
            }

            if (e is ZenBitwiseNotExpr<T> y)
            {
                return y.Expr;
            }

            return new ZenBitwiseNotExpr<T>(e);
        }

        /// <summary>
        /// Create a new ZenBitwiseNot expr.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Zen<T> Create(Zen<T> expr)
        {
            Contract.AssertNotNull(expr);
            Contract.Assert(ReflectionUtilities.IsFiniteIntegerType(typeof(T)));

            hashConsTable.GetOrAdd(expr.Id, expr, Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenBitwiseNotExpr{T}"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        private ZenBitwiseNotExpr(Zen<T> expr)
        {
            this.Expr = expr;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        internal Zen<T> Expr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"~({this.Expr})";
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
            return visitor.VisitBitwiseNot(this, parameter);
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
