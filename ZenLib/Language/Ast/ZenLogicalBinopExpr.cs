// <copyright file="ZenAndExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an And expression.
    /// </summary>
    internal sealed class ZenLogicalBinopExpr : Zen<bool>
    {
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
        /// Simplify a new ZenAndExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        private static Zen<bool> Simplify((Zen<bool> e1, Zen<bool> e2, LogicalOp op) args)
        {
            if (ReferenceEquals(args.e1, args.e2))
            {
                return args.e1;
            }

            if (args.op == LogicalOp.And)
            {
                if (args.e1 is ZenConstantExpr<bool> x)
                {
                    return x.Value ? args.e2 : args.e1;
                }

                if (args.e2 is ZenConstantExpr<bool> y)
                {
                    return y.Value ? args.e1 : args.e2;
                }

                if (args.e1 is ZenNotExpr n1 && args.e2 is ZenNotExpr n2)
                {
                    return Zen.Not(Zen.Or(n1.Expr, n2.Expr));
                }
            }
            else
            {
                Contract.Assert(args.op == LogicalOp.Or);
                if (args.e1 is ZenConstantExpr<bool> x)
                {
                    return x.Value ? args.e1 : args.e2;
                }

                if (args.e2 is ZenConstantExpr<bool> y)
                {
                    return y.Value ? args.e2 : args.e1;
                }

                if (args.e1 is ZenNotExpr n1 && args.e2 is ZenNotExpr n2)
                {
                    return Zen.Not(Zen.And(n1.Expr, n2.Expr));
                }
            }

            return new ZenLogicalBinopExpr(args.e1, args.e2, args.op);
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
            var flyweight = ZenAstCache<ZenLogicalBinopExpr, (long, long, int), Zen<bool>>.Flyweight;
            flyweight.GetOrAdd(key, (expr1, expr2, op), Simplify, out var value);
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
