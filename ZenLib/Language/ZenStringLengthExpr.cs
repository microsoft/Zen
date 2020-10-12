// <copyright file="ZenStringLengthExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a string length expression.
    /// </summary>
    internal sealed class ZenStringLengthExpr : Zen<BigInteger>
    {
        private static Dictionary<object, Zen<BigInteger>> hashConsTable = new Dictionary<object, Zen<BigInteger>>();

        public override Zen<BigInteger> Unroll()
        {
            return Create(this.Expr.Unroll());
        }

        public static Zen<BigInteger> Simplify(Zen<string> e1)
        {
            var x = ReflectionUtilities.GetConstantString(e1);

            if (x != null)
            {
                return new BigInteger(x.Length);
            }

            return new ZenStringLengthExpr(e1);
        }

        public static Zen<BigInteger> Create(Zen<string> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

            if (hashConsTable.TryGetValue(expr, out var value))
            {
                return value;
            }

            var ret = Simplify(expr);
            hashConsTable[expr] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringLengthExpr"/> class.
        /// </summary>
        /// <param name="expr">The string expression.</param>
        private ZenStringLengthExpr(Zen<string> expr)
        {
            this.Expr = expr;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> Expr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Length({this.Expr})";
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
            return visitor.VisitZenStringLengthExpr(this, parameter);
        }
    }
}
