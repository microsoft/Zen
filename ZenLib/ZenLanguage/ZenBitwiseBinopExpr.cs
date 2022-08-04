// <copyright file="ZenBitwiseBinopExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a binary bitvector operation expression.
    /// </summary>
    internal sealed class ZenBitwiseBinopExpr<T> : Zen<T>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<T>, Zen<T>, BitwiseOp), Zen<T>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// The operation strings for integer operations.
        /// </summary>
        private static string[] opStrings = new string[] { "&", "|", "^" };

        /// <summary>
        /// The evaluation functions for integer operations.
        /// </summary>
        private static Func<long, long, long>[] constantFuncs = new Func<long, long, long>[]
        {
            (x, y) => x & y,
            (x, y) => x | y,
            (x, y) => x ^ y,
        };

        /// <summary>
        /// Hash cons table for ZenIntegerBinopExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<T>> hashConsTable = new HashConsTable<(long, long, int), Zen<T>>();

        /// <summary>
        /// Unroll the ZenBitwiseBinopExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<T> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll(), this.Operation);
        }

        /// <summary>
        /// Simplify and create a new ZenBitwiseBinopExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <param name="op">The integer operation.</param>
        /// <returns>The new expr.</returns>
        private static Zen<T> Simplify(Zen<T> e1, Zen<T> e2, BitwiseOp op)
        {
            var x = ReflectionUtilities.GetConstantIntegerValue(e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(e2);

            if (x.HasValue && y.HasValue)
            {
                var f = constantFuncs[(int)op];
                return ReflectionUtilities.CreateConstantIntegerValue<T>(f(x.Value, y.Value));
            }

            return new ZenBitwiseBinopExpr<T>(e1, e2, op);
        }

        /// <summary>
        /// Create a new ZenBitwiseBinopExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <param name="op">The integer operation.</param>
        /// <returns>The new expr.</returns>
        public static Zen<T> Create(Zen<T> expr1, Zen<T> expr2, BitwiseOp op)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateIsFiniteIntegerType(typeof(T));

            var type = typeof(T);
            var key = (expr1.Id, expr2.Id, (int)op);
            hashConsTable.GetOrAdd(key, (expr1, expr2, op), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenBitwiseBinopExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <param name="op">The binary operation.</param>
        private ZenBitwiseBinopExpr(Zen<T> expr1, Zen<T> expr2, BitwiseOp op)
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
        internal BitwiseOp Operation { get; }

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
    /// A binary operation.
    /// </summary>
    internal enum BitwiseOp
    {
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
    }
}
