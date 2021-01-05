// <copyright file="ZenAndExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an And expression.
    /// </summary>
    internal sealed class ZenAndExpr : Zen<bool>
    {
        /// <summary>
        /// Hash cons table for And terms.
        /// </summary>
        private static HashConsTable<(long, long), Zen<bool>> hashConsTable = new HashConsTable<(long, long), Zen<bool>>();

        /// <summary>
        /// Unroll a ZenAndExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<bool> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll());
        }

        /// <summary>
        /// Simplify a new ZenAndExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<bool> Simplify(Zen<bool> e1, Zen<bool> e2)
        {
            if (e1 is ZenConstantExpr<bool> x)
            {
                return (x.Value ? e2 : e1);
            }

            if (e2 is ZenConstantExpr<bool> y)
            {
                return (y.Value ? e1 : e2);
            }

            if (ReferenceEquals(e1, e2))
            {
                return e1;
            }

            return new ZenAndExpr(e1, e2);
        }

        /// <summary>
        /// Creates a new ZenAndExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<bool> Create(Zen<bool> expr1, Zen<bool> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var key = (expr1.Id, expr2.Id);
            hashConsTable.GetOrAdd(key, () => Simplify(expr1, expr2), out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAndExpr"/> class.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        private ZenAndExpr(Zen<bool> expr1, Zen<bool> expr2)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<bool> Expr1 { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<bool> Expr2 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"And({this.Expr1}, {this.Expr2})";
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
            return visitor.VisitZenAndExpr(this, parameter);
        }
    }
}
