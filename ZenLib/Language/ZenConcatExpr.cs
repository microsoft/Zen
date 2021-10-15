// <copyright file="ZenConcatExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Concat expression.
    /// </summary>
    internal sealed class ZenConcatExpr : Zen<string>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<string>, Zen<string>), Zen<string>> createFunc = (v) => Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenConcatExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<string>> hashConsTable = new HashConsTable<(long, long), Zen<string>>();

        /// <summary>
        /// Simplify and create a new ZenConcatExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<string> Simplify(Zen<string> e1, Zen<string> e2)
        {
            string x = ReflectionUtilities.GetConstantString(e1);
            string y = ReflectionUtilities.GetConstantString(e2);

            if (x != null && y != null)
            {
                return ReflectionUtilities.CreateConstantString(x + y);
            }

            if (x == string.Empty)
            {
                return e2;
            }

            if (y == string.Empty)
            {
                return e1;
            }

            return new ZenConcatExpr(e1, e2);
        }

        /// <summary>
        /// Create a new ZenConcatExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <returns></returns>
        public static Zen<string> Create(Zen<string> expr1, Zen<string> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var key = (expr1.Id, expr2.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConcatExpr"/> class.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        private ZenConcatExpr(Zen<string> expr1, Zen<string> expr2)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
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
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"ZenConcatExpr({this.Expr1}, {this.Expr2})";
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
            return visitor.VisitZenConcatExpr(this, parameter);
        }
    }
}
