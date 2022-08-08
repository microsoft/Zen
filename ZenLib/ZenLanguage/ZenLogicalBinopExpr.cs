// <copyright file="ZenAndExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an And expression.
    /// </summary>
    internal sealed class ZenLogicalBinopExpr : Zen<bool>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<bool>, Zen<bool>, LogicalOp), Zen<bool>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for And terms.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<bool>> hashConsTable = new HashConsTable<(long, long, int), Zen<bool>>();

        /// <summary>
        /// Unroll a ZenAndExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<bool> Unroll()
        {
            return Create(this.Expr1.Unroll(), this.Expr2.Unroll(), this.Operation);
        }

        /// <summary>
        /// Simplify a new ZenAndExpr.
        /// </summary>
        /// <param name="e1">The first expr.</param>
        /// <param name="e2">The second expr.</param>
        /// <param name="op">The operation.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<bool> Simplify(Zen<bool> e1, Zen<bool> e2, LogicalOp op)
        {
            if (ReferenceEquals(e1, e2))
            {
                return e1;
            }

            if (op == LogicalOp.And)
            {
                if (e1 is ZenConstantExpr<bool> x)
                {
                    return (x.Value ? e2 : e1);
                }

                if (e2 is ZenConstantExpr<bool> y)
                {
                    return (y.Value ? e1 : e2);
                }
            }
            else
            {
                Contract.Assert(op == LogicalOp.Or);
                if (e1 is ZenConstantExpr<bool> x)
                {
                    return (x.Value ? e1 : e2);
                }

                if (e2 is ZenConstantExpr<bool> y)
                {
                    return (y.Value ? e2 : e1);
                }
            }

            return new ZenLogicalBinopExpr(e1, e2, op);
        }

        /// <summary>
        /// Creates a new ZenAndExpr.
        /// </summary>
        /// <param name="expr1">The first expr.</param>
        /// <param name="expr2">The second expr.</param>
        /// <param name="op">The operation.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<bool> Create(Zen<bool> expr1, Zen<bool> expr2, LogicalOp op)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            var key = (expr1.Id, expr2.Id, (int)op);
            hashConsTable.GetOrAdd(key, (expr1, expr2, op), createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenLogicalBinopExpr"/> class.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <param name="op">The operation.</param>
        private ZenLogicalBinopExpr(Zen<bool> expr1, Zen<bool> expr2, LogicalOp op)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
            this.Operation = op;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<bool> Expr1 { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<bool> Expr2 { get; }

        /// <summary>
        /// Gets the operation.
        /// </summary>
        internal LogicalOp Operation { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (this.Operation == LogicalOp.And)
            {
                return $"And({this.Expr1}, {this.Expr2})";
            }
            else
            {
                Contract.Assert(this.Operation == LogicalOp.Or);
                return $"Or({this.Expr1}, {this.Expr2})";
            }
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
            return visitor.VisitLogicalBinop(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <summary>
        /// A logical binary operation.
        /// </summary>
        internal enum LogicalOp
        {
            And,
            Or,
        }
    }
}
