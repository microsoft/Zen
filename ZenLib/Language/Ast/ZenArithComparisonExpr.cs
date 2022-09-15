// <copyright file="ZenComparisonExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing an arithmetic comparison expression.
    /// </summary>
    internal sealed class ZenArithComparisonExpr<T> : Zen<bool>
    {
        /// <summary>
        /// Hash cons table for ZenComparisonExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<bool>> hashConsTable = new HashConsTable<(long, long, int), Zen<bool>>();

        /// <summary>
        /// The strings for different comparison operations.
        /// </summary>
        private string[] opStrings = new string[] { ">=", "<=", ">", "<" };

        /// <summary>
        /// The constant functions for comparison operations.
        /// </summary>
        private static Func<long, long, long>[] constantFuncs = new Func<long, long, long>[]
        {
            (x, y) => x >= y ? 1L : 0L,
            (x, y) => x <= y ? 1L : 0L,
            (x, y) => x > y ? 1L : 0L,
            (x, y) => x < y ? 1L : 0L,
        };

        /// <summary>
        /// The constant functions for comparison operations for ulong values.
        /// </summary>
        private static Func<ulong, ulong, bool>[] constantUlongFuncs = new Func<ulong, ulong, bool>[]
        {
            (x, y) => x >= y,
            (x, y) => x <= y,
            (x, y) => x > y,
            (x, y) => x < y,
        };

        /// <summary>
        /// The interpretation functions for comparison operations.
        /// </summary>
        private static Func<BigInteger, BigInteger, bool>[] constantBigIntFuncs = new Func<BigInteger, BigInteger, bool>[]
        {
            (x, y) => x >= y,
            (x, y) => x <= y,
            (x, y) => x > y,
            (x, y) => x < y,
        };

        /// <summary>
        /// The interpretation functions for comparison operations.
        /// </summary>
        private static Func<Real, Real, bool>[] constantRealFuncs = new Func<Real, Real, bool>[]
        {
            (x, y) => x >= y,
            (x, y) => x <= y,
            (x, y) => x > y,
            (x, y) => x < y,
        };

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
        /// Simplify and create a ZenComparisonExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A new ZenComparisonExpr.</returns>
        private static Zen<bool> Simplify((Zen<T> e1, Zen<T> e2, ComparisonType comparisonType) args)
        {
            if (args.e1 is ZenConstantExpr<BigInteger> be1 && args.e2 is ZenConstantExpr<BigInteger> be2)
            {
                return Zen.Constant(constantBigIntFuncs[(int)args.comparisonType](be1.Value, be2.Value));
            }

            if (args.e1 is ZenConstantExpr<ulong> ue1 && args.e2 is ZenConstantExpr<ulong> ue2)
            {
                return Zen.Constant(constantUlongFuncs[(int)args.comparisonType](ue1.Value, ue2.Value));
            }

            if (args.e1 is ZenConstantExpr<Real> re1 && args.e2 is ZenConstantExpr<Real> re2)
            {
                return Zen.Constant(constantRealFuncs[(int)args.comparisonType](re1.Value, re2.Value));
            }

            var x = ReflectionUtilities.GetConstantIntegerValue(args.e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(args.e2);
            if (x.HasValue && y.HasValue)
            {
                return ReflectionUtilities.CreateConstantIntegerValue<bool>(constantFuncs[(int)args.comparisonType](x.Value, y.Value));
            }

            return new ZenArithComparisonExpr<T>(args.e1, args.e2, args.comparisonType);
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
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.Assert(ReflectionUtilities.IsArithmeticType(typeof(T)));

            var key = (expr1.Id, expr2.Id, (int)comparisonType);
            hashConsTable.GetOrAdd(key, (expr1, expr2, comparisonType), Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenArithComparisonExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <param name="comparisonType">The comparison type.</param>
        private ZenArithComparisonExpr(Zen<T> expr1, Zen<T> expr2, ComparisonType comparisonType)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
            this.ComparisonType = comparisonType;
        }

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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitArithComparison(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// The comparison type.
    /// </summary>
    internal enum ComparisonType
    {
        /// <summary>
        /// Greater than or equal to.
        /// </summary>
        Geq,

        /// <summary>
        /// Less than or equal to.
        /// </summary>
        Leq,

        /// <summary>
        /// Greater than.
        /// </summary>
        Gt,

        /// <summary>
        /// Less than.
        /// </summary>
        Lt,
    }
}
