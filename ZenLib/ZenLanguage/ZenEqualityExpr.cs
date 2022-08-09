// <copyright file="ZenEqualityExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing an equality expr.
    /// </summary>
    internal sealed class ZenEqualityExpr<T> : Zen<bool>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<T>, Zen<T>), Zen<bool>> createFunc = (v) => Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenEqualityExpr.
        /// </summary>
        private static HashConsTable<(long, long), Zen<bool>> hashConsTable = new HashConsTable<(long, long), Zen<bool>>();

        /// <summary>
        /// Unroll a ZenEqualityExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<bool> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll());
        }

        /// <summary>
        /// Simplify and create a ZenEqualityExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <returns>A new ZenEqualityExpr.</returns>
        private static Zen<bool> Simplify(Zen<T> e1, Zen<T> e2)
        {
            if (e1 is ZenConstantExpr<BigInteger> be1 && e2 is ZenConstantExpr<BigInteger> be2)
            {
                return Zen.Constant(be1.Value == be2.Value);
            }

            if (e1 is ZenConstantExpr<ulong> ue1 && e2 is ZenConstantExpr<ulong> ue2)
            {
                return Zen.Constant(ue1.Value == ue2.Value);
            }

            if (e1 is ZenConstantExpr<bool> b1)
            {
                return b1.Value ? (Zen<bool>)(object)e2 : Zen.Not((Zen<bool>)(object)e2);
            }

            if (e2 is ZenConstantExpr<bool> b2)
            {
                return b2.Value ? (Zen<bool>)(object)e1 : Zen.Not((Zen<bool>)(object)e1);
            }

            var x = ReflectionUtilities.GetConstantIntegerValue(e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(e2);
            if (x.HasValue && y.HasValue)
            {
                return Zen.Constant(x.Value == y.Value);
            }

            return new ZenEqualityExpr<T>(e1, e2);
        }

        /// <summary>
        /// Create a new ZenEqualityExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <returns>A new Zen expr.</returns>
        public static Zen<bool> Create(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            var key = (expr1.Id, expr2.Id);
            hashConsTable.GetOrAdd(key, (expr1, expr2), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenEqualityExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        private ZenEqualityExpr(Zen<T> expr1, Zen<T> expr2)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
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
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Expr1} == {this.Expr2})";
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
            return visitor.VisitEquality(this, parameter);
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
}
