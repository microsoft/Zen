// <copyright file="ZenAndExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an And expression.
    /// </summary>
    internal sealed class ZenAndExpr : Zen<bool>
    {
        private static Dictionary<(object, object), Zen<bool>> hashConsTable = new Dictionary<(object, object), Zen<bool>>();

        private static Zen<bool> Simplify(Zen<bool> e1, Zen<bool> e2)
        {
            if (e1 is ZenConstantBoolExpr x)
            {
                return (x.Value ? e2 : e1);
            }

            if (e2 is ZenConstantBoolExpr y)
            {
                return (y.Value ? e1 : e2);
            }

            return new ZenAndExpr(e1, e2);
        }

        public static Zen<bool> Create(Zen<bool> expr1, Zen<bool> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            var key = (expr1, expr2);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr1, expr2);
            hashConsTable[key] = ret;
            return ret;
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
