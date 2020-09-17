// <copyright file="ZenComparisonExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Geq expression.
    /// </summary>
    internal sealed class ZenComparisonExpr<T> : Zen<bool>
    {
        private static Dictionary<(object, object, int), Zen<bool>> hashConsTable = new Dictionary<(object, object, int), Zen<bool>>();

        private string[] opStrings = new string[] { ">=", "<=", "==" };

        private static Func<long, long, long>[] constantFuncs = new Func<long, long, long>[]
        {
            (x, y) => x >= y ? 1L : 0L,
            (x, y) => x <= y ? 1L : 0L,
            (x, y) => x == y ? 1L : 0L,
        };

        internal override Zen<bool> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll(), this.ComparisonType);
        }

        private static Zen<bool> Simplify(Zen<T> e1, Zen<T> e2, ComparisonType comparisonType)
        {
            var x = ReflectionUtilities.GetConstantIntegerValue(e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(e2);

            if (x.HasValue && y.HasValue)
            {
                var f = constantFuncs[(int)comparisonType];
                return ReflectionUtilities.CreateConstantValue<bool>(f(x.Value, y.Value));
            }

            return new ZenComparisonExpr<T>(e1, e2, comparisonType);
        }

        public static Zen<bool> Create(Zen<T> expr1, Zen<T> expr2, ComparisonType comparisonType)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            if (comparisonType != ComparisonType.Eq)
            {
                CommonUtilities.ValidateIsIntegerType(typeof(T));
            }

            var key = (expr1, expr2, (int)comparisonType);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr1, expr2, comparisonType);
            hashConsTable[key] = ret;
            return ret;
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
