// <copyright file="ZenStringAtExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a substring expression.
    /// </summary>
    internal sealed class ZenStringAtExpr : Zen<string>
    {
        private static Dictionary<(object, object), Zen<string>> hashConsTable =
            new Dictionary<(object, object), Zen<string>>();

        internal override Zen<string> Unroll()
        {
            return Create(this.StringExpr.Unroll(), this.IndexExpr.Unroll());
        }

        public static Zen<string> Simplify(Zen<string> e1, Zen<BigInteger> e2)
        {
            var x = ReflectionUtilities.GetConstantString(e1);

            if (x != null && e2 is ZenConstantExpr<BigInteger> be)
            {
                return CommonUtilities.At(x, be.Value);
            }

            if (x == string.Empty)
            {
                return string.Empty;
            }

            return new ZenStringAtExpr(e1, e2);
        }

        public static Zen<string> Create(Zen<string> expr1, Zen<BigInteger> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var key = (expr1, expr2);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr1, expr2);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringAtExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The index expression.</param>
        private ZenStringAtExpr(Zen<string> expr1, Zen<BigInteger> expr2)
        {
            this.StringExpr = expr1;
            this.IndexExpr = expr2;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> StringExpr { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<BigInteger> IndexExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"At({this.StringExpr}, {this.IndexExpr})";
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
            return visitor.VisitZenStringAtExpr(this, parameter);
        }
    }
}
