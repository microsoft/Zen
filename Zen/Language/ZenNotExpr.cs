// <copyright file="ZenNotExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Not expression.
    /// </summary>
    internal sealed class ZenNotExpr : Zen<bool>
    {
        private static Dictionary<object, ZenNotExpr> hashConsTable = new Dictionary<object, ZenNotExpr>();

        public static ZenNotExpr Create(Zen<bool> expr)
        {
            CommonUtilities.Validate(expr);

            if (hashConsTable.TryGetValue(expr, out var value))
            {
                return value;
            }

            var ret = new ZenNotExpr(expr);
            hashConsTable[expr] = ret;
            return ret;
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
            return $"Not({this.Expr.ToString()})";
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
            return visitor.VisitZenNotExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<bool> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenNotExpr(this);
        }
    }
}
