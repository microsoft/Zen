// <copyright file="ZenArithBinopExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a binary arithmetic operation expression.
    /// </summary>
    internal sealed class ZenArithBinopExpr<T> : Zen<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<T>, Zen<T>, ArithmeticOp), Zen<T>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// The operation strings for integer operations.
        /// </summary>
        private static string[] opStrings = new string[] { "+", "-", "*" };

        /// <summary>
        /// The evaluation functions for integer operations.
        /// </summary>
        private static Func<long, long, long>[] constantFuncs = new Func<long, long, long>[]
        {
            (x, y) => x + y,
            (x, y) => x - y,
            (x, y) => x * y,
        };

        /// <summary>
        /// The evaluation functions for integer operations.
        /// </summary>
        private static Func<BigInteger, BigInteger, BigInteger>[] constantBigIntFuncs = new Func<BigInteger, BigInteger, BigInteger>[]
        {
            (x, y) => x + y,
            (x, y) => x - y,
            (x, y) => x * y,
        };

        /// <summary>
        /// The evaluation functions for real operations.
        /// </summary>
        private static Func<Real, Real, Real>[] constantRealFuncs = new Func<Real, Real, Real>[]
        {
            (x, y) => x + y,
            (x, y) => x - y,
            (x, y) => x * y,
        };

        /// <summary>
        /// Hash cons table for ZenArithBinopExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<T>> hashConsTable = new HashConsTable<(long, long, int), Zen<T>>();

        /// <summary>
        /// Unroll the ZenArithBinopExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<T> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll(), this.Operation);
        }

        /// <summary>
        /// Simplify and create a new ZenArithBinopExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <param name="op">The operation.</param>
        /// <returns>The new expr.</returns>
        private static Zen<T> Simplify(Zen<T> e1, Zen<T> e2, ArithmeticOp op)
        {
            if (e1 is ZenConstantExpr<BigInteger> be1 && e2 is ZenConstantExpr<BigInteger> be2)
            {
                return (Zen<T>)(object)ZenConstantExpr<BigInteger>.Create(constantBigIntFuncs[(int)op](be1.Value, be2.Value));
            }

            if (e1 is ZenConstantExpr<Real> re1 && e2 is ZenConstantExpr<Real> re2)
            {
                return (Zen<T>)(object)ZenConstantExpr<Real>.Create(constantRealFuncs[(int)op](re1.Value, re2.Value));
            }

            var x = ReflectionUtilities.GetConstantIntegerValue(e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(e2);

            if (x.HasValue && y.HasValue)
            {
                var f = constantFuncs[(int)op];
                return ReflectionUtilities.CreateConstantIntegerValue<T>(f(x.Value, y.Value));
            }

            switch (op)
            {
                case ArithmeticOp.Addition:
                    if (x.HasValue && x.Value == 0)
                        return e2;
                    if (y.HasValue && y.Value == 0)
                        return e1;
                    break;

                case ArithmeticOp.Subtraction:
                    if (y.HasValue && y.Value == 0)
                        return e1;
                    break;

                case ArithmeticOp.Multiplication:
                    if ((x.HasValue && x.Value == 0) || (y.HasValue && y.Value == 0))
                        return ReflectionUtilities.CreateConstantIntegerValue<T>(0);
                    if (x.HasValue && x.Value == 1)
                        return e2;
                    if (y.HasValue && y.Value == 1)
                        return e1;
                    break;
            }

            return new ZenArithBinopExpr<T>(e1, e2, op);
        }

        /// <summary>
        /// Create a new ZenArithBinopExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <param name="op">The operation.</param>
        /// <returns>The new expr.</returns>
        public static Zen<T> Create(Zen<T> expr1, Zen<T> expr2, ArithmeticOp op)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateIsArithmeticType(typeof(T));

            var type = typeof(T);

            if (ReflectionUtilities.IsFixedIntegerType(type) && op == ArithmeticOp.Multiplication)
            {
                throw new ZenException($"Operation: {op} is not supported for type {type}");
            }

            var key = (expr1.Id, expr2.Id, (int)op);
            hashConsTable.GetOrAdd(key, (expr1, expr2, op), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenArithBinopExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <param name="op">The binary operation.</param>
        private ZenArithBinopExpr(Zen<T> expr1, Zen<T> expr2, ArithmeticOp op)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
            this.Operation = op;
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
        /// Gets the operation.
        /// </summary>
        internal ArithmeticOp Operation { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Expr1} {opStrings[(int)this.Operation]} {this.Expr2})";
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
            return visitor.Visit(this, parameter);
        }
    }

    /// <summary>
    /// A binary operation.
    /// </summary>
    internal enum ArithmeticOp
    {
        Addition,
        Subtraction,
        Multiplication,
    }
}
