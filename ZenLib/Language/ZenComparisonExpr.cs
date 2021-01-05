// <copyright file="ZenComparisonExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a comparison expression.
    /// </summary>
    internal sealed class ZenComparisonExpr<T> : Zen<bool>
    {
        /// <summary>
        /// Hash cons table for ZenComparisonExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<bool>> hashConsTable = new HashConsTable<(long, long, int), Zen<bool>>();

        /// <summary>
        /// The strings for different comparison operations.
        /// </summary>
        private string[] opStrings = new string[] { ">=", "<=", "==" };

        /// <summary>
        /// The constant functions for comparison operations.
        /// </summary>
        private static Func<long, long, long>[] constantFuncs = new Func<long, long, long>[]
        {
            (x, y) => x >= y ? 1L : 0L,
            (x, y) => x <= y ? 1L : 0L,
            (x, y) => x == y ? 1L : 0L,
        };

        /// <summary>
        /// The interpretation functions for comparison operations.
        /// </summary>
        private static Func<BigInteger, BigInteger, bool>[] constantBigIntFuncs = new Func<BigInteger, BigInteger, bool>[]
        {
            (x, y) => x >= y,
            (x, y) => x <= y,
            (x, y) => x == y,
        };

        /// <summary>
        /// Unroll a ZenComparisonExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<bool> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll(), this.ComparisonType);
        }

        /// <summary>
        /// Simplify and create a ZenComparisonExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <param name="comparisonType">The comparison type.</param>
        /// <returns>A new ZenComparisonExpr.</returns>
        private static Zen<bool> Simplify(Zen<T> e1, Zen<T> e2, ComparisonType comparisonType)
        {
            if (e1 is ZenConstantExpr<BigInteger> be1 && e2 is ZenConstantExpr<BigInteger> be2)
            {
                return Language.Constant(constantBigIntFuncs[(int)comparisonType](be1.Value, be2.Value));
            }

            var x = ReflectionUtilities.GetConstantIntegerValue(e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(e2);

            if (x.HasValue && y.HasValue)
            {
                var f = constantFuncs[(int)comparisonType];
                return ReflectionUtilities.CreateConstantIntegerValue<bool>(f(x.Value, y.Value));
            }

            return new ZenComparisonExpr<T>(e1, e2, comparisonType);
        }

        /// <summary>
        /// Create a new ZenComparisonExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <param name="comparisonType">The comparison type.</param>
        /// <returns>A new Zen expr.</returns>
        public static Zen<bool> Create(Zen<T> expr1, Zen<T> expr2, ComparisonType comparisonType)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            if (comparisonType != ComparisonType.Eq)
            {
                CommonUtilities.ValidateIsIntegerType(typeof(T));
            }

            var key = (expr1.Id, expr2.Id, (int)comparisonType);
            hashConsTable.GetOrAdd(key, () => Simplify(expr1, expr2, comparisonType), out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenComparisonExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <param name="comparisonType">The comparison type.</param>
        private ZenComparisonExpr(Zen<T> expr1, Zen<T> expr2, ComparisonType comparisonType)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
            this.ComparisonType = comparisonType;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<T> Expr1 { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<T> Expr2 { get; }

        /// <summary>
        /// Gets the comparison type.
        /// </summary>
        internal ComparisonType ComparisonType { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Expr1} {opStrings[(int)this.ComparisonType]} {this.Expr2})";
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
            return visitor.VisitZenComparisonExpr(this, parameter);
        }
    }

    internal enum ComparisonType
    {
        Geq,
        Leq,
        Eq,
    }
}
