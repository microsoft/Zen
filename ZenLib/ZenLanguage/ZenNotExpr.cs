// <copyright file="ZenNotExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Not expression.
    /// </summary>
    internal sealed class ZenNotExpr : Zen<bool>
    {
        /// <summary>
        /// Hash cons table for ZenNotExpr.
        /// </summary>
        private static HashConsTable<long, Zen<bool>> hashConsTable = new HashConsTable<long, Zen<bool>>();

        /// <summary>
        /// Unroll a ZenNotExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<bool> Unroll()
        {
            return Create(this.Expr.Unroll());
        }

        /// <summary>
        /// Simplify and create a ZenNotExpr.
        /// </summary>
        /// <param name="e">The expr.</param>
        /// <returns>The negated expr.</returns>
        private static Zen<bool> Simplify(Zen<bool> e)
        {
            if (e is ZenConstantExpr<bool> x)
            {
                return x.Value ? ZenConstantExpr<bool>.Create(false) : ZenConstantExpr<bool>.Create(true);
            }

            if (e is ZenNotExpr y)
            {
                return y.Expr;
            }

            return new ZenNotExpr(e);
        }

        /// <summary>
        /// Create a new ZenNotExpr.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>The negated expr.</returns>
        public static Zen<bool> Create(Zen<bool> expr)
        {
            Contract.AssertNotNull(expr);

            hashConsTable.GetOrAdd(expr.Id, expr, Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenNotExpr"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        private ZenNotExpr(Zen<bool> expr)
        {
            this.Expr = expr;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        internal Zen<bool> Expr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Not({this.Expr})";
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
            return visitor.VisitNot(this, parameter);
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
