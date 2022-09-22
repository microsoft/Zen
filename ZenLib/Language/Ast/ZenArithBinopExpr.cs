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
        /// Simplify and create a new ZenArithBinopExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new expr.</returns>
        private static Zen<T> Simplify((Zen<T> e1, Zen<T> e2, ArithmeticOp op) args)
        {
            if (typeof(T) == typeof(BigInteger))
            {
                // constant folding for BigInteger.
                if (args.e1 is ZenConstantExpr<BigInteger> be1 && args.e2 is ZenConstantExpr<BigInteger> be2)
                {
                    return (Zen<T>)(object)ZenConstantExpr<BigInteger>.Create(constantBigIntFuncs[(int)args.op](be1.Value, be2.Value));
                }

                // arithmetic identities.
                switch (args.op)
                {
                    case ArithmeticOp.Addition:
                        if (args.e1 is ZenConstantExpr<BigInteger> a && a.Value == BigInteger.Zero)
                            return args.e2;
                        if (args.e2 is ZenConstantExpr<BigInteger> b && b.Value == BigInteger.Zero)
                            return args.e1;
                        break;

                    case ArithmeticOp.Subtraction:
                        if (args.e2 is ZenConstantExpr<BigInteger> e && e.Value == BigInteger.Zero)
                            return args.e1;
                        break;

                    case ArithmeticOp.Multiplication:
                        if (args.e1 is ZenConstantExpr<BigInteger> g && g.Value == BigInteger.Zero)
                            return (Zen<T>)(object)Zen.Constant(BigInteger.Zero);
                        if (args.e2 is ZenConstantExpr<BigInteger> h && h.Value == BigInteger.Zero)
                            return (Zen<T>)(object)Zen.Constant(BigInteger.Zero);
                        if (args.e1 is ZenConstantExpr<BigInteger> i && i.Value == BigInteger.One)
                            return args.e2;
                        if (args.e2 is ZenConstantExpr<BigInteger> j && j.Value == BigInteger.One)
                            return args.e1;
                        break;
                }
            }
            else if (typeof(T) == typeof(Real))
            {
                // constant folding for Real.
                if (args.e1 is ZenConstantExpr<Real> re1 && args.e2 is ZenConstantExpr<Real> re2)
                {
                    return (Zen<T>)(object)ZenConstantExpr<Real>.Create(constantRealFuncs[(int)args.op](re1.Value, re2.Value));
                }

                // arithmetic identities.
                switch (args.op)
                {
                    case ArithmeticOp.Addition:
                        if (args.e1 is ZenConstantExpr<Real> c && c.Value == new Real(0))
                            return args.e2;
                        if (args.e2 is ZenConstantExpr<Real> d && d.Value == new Real(0))
                            return args.e1;
                        break;

                    case ArithmeticOp.Subtraction:
                        if (args.e2 is ZenConstantExpr<Real> f && f.Value == new Real(0))
                            return args.e1;
                        break;

                    case ArithmeticOp.Multiplication:
                        if (args.e1 is ZenConstantExpr<Real> k && k.Value == new Real(0))
                            return (Zen<T>)(object)Zen.Constant(new Real(0));
                        if (args.e2 is ZenConstantExpr<Real> l && l.Value == new Real(0))
                            return (Zen<T>)(object)Zen.Constant(new Real(0));
                        if (args.e1 is ZenConstantExpr<Real> m && m.Value == new Real(1))
                            return args.e2;
                        if (args.e2 is ZenConstantExpr<Real> n && n.Value == new Real(1))
                            return args.e1;
                        break;
                }
            }
            else
            {
                // constant folding for other types.
                var x = ReflectionUtilities.GetConstantIntegerValue(args.e1);
                var y = ReflectionUtilities.GetConstantIntegerValue(args.e2);

                if (x.HasValue && y.HasValue)
                {
                    var f = constantFuncs[(int)args.op];
                    return ReflectionUtilities.CreateConstantIntegerValue<T>(f(x.Value, y.Value));
                }

                // arithmetic identities.
                switch (args.op)
                {
                    case ArithmeticOp.Addition:
                        if (x.HasValue && x.Value == 0)
                            return args.e2;
                        if (y.HasValue && y.Value == 0)
                            return args.e1;
                        break;

                    case ArithmeticOp.Subtraction:
                        if (y.HasValue && y.Value == 0)
                            return args.e1;
                        break;

                    case ArithmeticOp.Multiplication:
                        if ((x.HasValue && x.Value == 0) || (y.HasValue && y.Value == 0))
                            return ReflectionUtilities.CreateConstantIntegerValue<T>(0);
                        if (x.HasValue && x.Value == 1)
                            return args.e2;
                        if (y.HasValue && y.Value == 1)
                            return args.e1;
                        break;
                }
            }

            return new ZenArithBinopExpr<T>(args.e1, args.e2, args.op);
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
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);
            Contract.Assert(ReflectionUtilities.IsArithmeticType(typeof(T)));

            var type = typeof(T);

            if (ReflectionUtilities.IsFixedIntegerType(type) && op == ArithmeticOp.Multiplication)
            {
                throw new ZenException($"Operation: {op} is not supported for type {type}");
            }

            var key = (expr1.Id, expr2.Id, (int)op);
            var flyweight = ZenAstCache<ZenArithBinopExpr<T>, Zen<T>>.Flyweight;
            flyweight.GetOrAdd(key, (expr1, expr2, op), Simplify, out var value);
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
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitArithBinop(this, parameter);
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
    internal enum ArithmeticOp
    {
        Addition,
        Subtraction,
        Multiplication,
    }
}
