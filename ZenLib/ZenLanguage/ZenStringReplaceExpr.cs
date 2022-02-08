// <copyright file="ZenStringReplaceExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a string Replacement expression.
    /// </summary>
    internal sealed class ZenStringReplaceExpr : Zen<string>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<string>, Zen<string>, Zen<string>), Zen<string>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenStringReplaceExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<string>> hashConsTable = new HashConsTable<(long, long, long), Zen<string>>();

        /// <summary>
        /// Unroll a ZenStringReplaceExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<string> Unroll()
        {
            return Create(this.StringExpr.Unroll(), this.SubstringExpr.Unroll(), this.ReplaceExpr.Unroll());
        }

        /// <summary>
        /// Simplify and create a new ZenStringReplaceExpr.
        /// </summary>
        /// <param name="e1">The string expr.</param>
        /// <param name="e2">The substring expr.</param>
        /// <param name="e3">The replacement expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<string> Simplify(Zen<string> e1, Zen<string> e2, Zen<string> e3)
        {
            string x = ReflectionUtilities.GetConstantString(e1);
            string y = ReflectionUtilities.GetConstantString(e2);
            string z = ReflectionUtilities.GetConstantString(e3);

            if (x != null && y != null && z != null)
                return CommonUtilities.ReplaceFirst(x, y, z);
            if (x == string.Empty)
                return string.Empty;
            if (y == string.Empty)
                return e1 + e3;

            return new ZenStringReplaceExpr(e1, e2, e3);
        }

        /// <summary>
        /// Create a new ZenStringReplaceExpr.
        /// </summary>
        /// <param name="expr1">The string expr.</param>
        /// <param name="expr2">The substring expr.</param>
        /// <param name="expr3">The replacement expr.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<string> Create(Zen<string> expr1, Zen<string> expr2, Zen<string> expr3)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2, expr3), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringContainmentExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The subtring match.</param>
        /// <param name="expr3">The substituted string.</param>
        private ZenStringReplaceExpr(Zen<string> expr1, Zen<string> expr2, Zen<string> expr3)
        {
            this.StringExpr = expr1;
            this.SubstringExpr = expr2;
            this.ReplaceExpr = expr3;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> StringExpr { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<string> SubstringExpr { get; }

        /// <summary>
        /// Gets the containment type.
        /// </summary>
        internal Zen<string> ReplaceExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Replace({this.StringExpr}, {this.SubstringExpr}, {this.ReplaceExpr})";
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
