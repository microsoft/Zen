// <copyright file="ZenStringSubstringExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a substring expression.
    /// </summary>
    internal sealed class ZenStringSubstringExpr : Zen<string>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<string>, Zen<BigInteger>, Zen<BigInteger>), Zen<string>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenStringSubstringExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<string>> hashConsTable = new HashConsTable<(long, long, long), Zen<string>>();

        /// <summary>
        /// Simplify and create a new ZenStringSubstringExpr.
        /// </summary>
        /// <param name="e1">The string expression.</param>
        /// <param name="e2">The offset expression.</param>
        /// <param name="e3">The length expression.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<string> Simplify(Zen<string> e1, Zen<BigInteger> e2, Zen<BigInteger> e3)
        {
            var x = ReflectionUtilities.GetConstantString(e1);

            if (x != null && e2 is ZenConstantExpr<BigInteger> be2 && e3 is ZenConstantExpr<BigInteger> be3)
                return CommonUtilities.Substring(x, be2.Value, be3.Value);

            return new ZenStringSubstringExpr(e1, e2, e3);
        }

        /// <summary>
        /// Create a new ZenStringSubstringExpr.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The offset expression.</param>
        /// <param name="expr3">The length expression.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<string> Create(Zen<string> expr1, Zen<BigInteger> expr2, Zen<BigInteger> expr3)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2, expr3), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringSubstringExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The subtring match.</param>
        /// <param name="expr3">The substituted string.</param>
        private ZenStringSubstringExpr(Zen<string> expr1, Zen<BigInteger> expr2, Zen<BigInteger> expr3)
        {
            this.StringExpr = expr1;
            this.OffsetExpr = expr2;
            this.LengthExpr = expr3;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> StringExpr { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<BigInteger> OffsetExpr { get; }

        /// <summary>
        /// Gets the third expression.
        /// </summary>
        internal Zen<BigInteger> LengthExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Substring({this.StringExpr}, {this.OffsetExpr}, {this.LengthExpr})";
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
            return visitor.VisitZenStringSubstringExpr(this, parameter);
        }
    }
}
