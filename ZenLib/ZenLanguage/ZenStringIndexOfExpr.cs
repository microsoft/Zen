// <copyright file="ZenStringIndexOfExpr.cs" company="Microsoft">
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
    internal sealed class ZenStringIndexOfExpr : Zen<BigInteger>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<string>, Zen<string>, Zen<BigInteger>), Zen<BigInteger>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenStringIndexOfExpr.
        /// </summary>
        private static HashConsTable<(long, long, long), Zen<BigInteger>> hashConsTable = new HashConsTable<(long, long, long), Zen<BigInteger>>();

        /// <summary>
        /// Unroll a ZenStringIndexOfExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<BigInteger> Unroll()
        {
            return Create(this.StringExpr.Unroll(), this.SubstringExpr.Unroll(), this.OffsetExpr.Unroll());
        }

        /// <summary>
        /// Simplify and create a ZenStringIndexOfExpr.
        /// </summary>
        /// <param name="e1">The string expr.</param>
        /// <param name="e2">The substring expr.</param>
        /// <param name="e3">The offset expr.</param>
        /// <returns></returns>
        public static Zen<BigInteger> Simplify(Zen<string> e1, Zen<string> e2, Zen<BigInteger> e3)
        {
            var x = ReflectionUtilities.GetConstantString(e1);
            var y = ReflectionUtilities.GetConstantString(e2);

            if (x != null && y != null && e3 is ZenConstantExpr<BigInteger> be)
            {
                return CommonUtilities.IndexOf(x, y, be.Value);
            }

            return new ZenStringIndexOfExpr(e1, e2, e3);
        }

        /// <summary>
        /// Create a new ZenStringIndexOfExpr.
        /// </summary>
        /// <param name="expr1">the string expr.</param>
        /// <param name="expr2">The substring expr.</param>
        /// <param name="expr3">The offset expr.</param>
        /// <returns></returns>
        public static Zen<BigInteger> Create(Zen<string> expr1, Zen<string> expr2, Zen<BigInteger> expr3)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            var key = (expr1.Id, expr2.Id, expr3.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2, expr3), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringIndexOfExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The subtring expression.</param>
        /// <param name="expr3">The offset expression.</param>
        private ZenStringIndexOfExpr(Zen<string> expr1, Zen<string> expr2, Zen<BigInteger> expr3)
        {
            this.StringExpr = expr1;
            this.SubstringExpr = expr2;
            this.OffsetExpr = expr3;
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
        /// Gets the third expression.
        /// </summary>
        internal Zen<BigInteger> OffsetExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"IndexOf({this.StringExpr}, {this.SubstringExpr}, {this.OffsetExpr})";
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
            return visitor.VisitZenStringIndexOfExpr(this, parameter);
        }
    }
}
