// <copyright file="ZenContainmentExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a string Replacement expression.
    /// </summary>
    internal sealed class ZenStringReplaceExpr : Zen<string>
    {
        private static Dictionary<(object, object, object), Zen<string>> hashConsTable =
            new Dictionary<(object, object, object), Zen<string>>();

        public static Zen<string> Simplify(Zen<string> e1, Zen<string> e2, Zen<string> e3)
        {
            string x = ReflectionUtilities.GetConstantString(e1);
            string y = ReflectionUtilities.GetConstantString(e2);
            string z = ReflectionUtilities.GetConstantString(e3);
            if (x != null && y != null && z != null)
                return CommonUtilities.ReplaceFirst(x, y, z);
            if (x == "")
                return "";
            if (y == "")
                return e1 + e3;
            return new ZenStringReplaceExpr(e1, e2, e3);
        }

        public static Zen<string> Create(Zen<string> expr1, Zen<string> expr2, Zen<string> expr3)
        {
            CommonUtilities.Validate(expr1);
            CommonUtilities.Validate(expr2);
            CommonUtilities.Validate(expr3);

            var key = (expr1, expr2, expr3);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr1, expr2, expr3);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenContainmentExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The subtring match.</param>
        /// <param name="expr3">The substituted string.</param>
        private ZenStringReplaceExpr(Zen<string> expr1, Zen<string> expr2, Zen<string> expr3)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
            this.Expr3 = expr3;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> Expr1 { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<string> Expr2 { get; }

        /// <summary>
        /// Gets the containment type.
        /// </summary>
        internal Zen<string> Expr3 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Replace({this.Expr1}, {this.Expr2}, {this.Expr3})";
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
            return visitor.VisitZenStringReplaceExpr(this, parameter);
        }
    }
}
