// <copyright file="ZenStringSubstringExpr.cs" company="Microsoft">
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
    internal sealed class ZenStringSubstringExpr : Zen<string>
    {
        private static Dictionary<(object, object, object), Zen<string>> hashConsTable =
            new Dictionary<(object, object, object), Zen<string>>();

        internal override Zen<string> Unroll()
        {
            return Create(this.StringExpr.Unroll(), this.OffsetExpr.Unroll(), this.LengthExpr.Unroll());
        }

        public static Zen<string> Simplify(Zen<string> e1, Zen<BigInteger> e2, Zen<BigInteger> e3)
        {
            var x = ReflectionUtilities.GetConstantString(e1);

            if (x != null && e2 is ZenConstantBigIntExpr be2 && e3 is ZenConstantBigIntExpr be3)
                return CommonUtilities.Substring(x, be2.Value, be3.Value);

            return new ZenStringSubstringExpr(e1, e2, e3);
        }

        public static Zen<string> Create(Zen<string> expr1, Zen<BigInteger> expr2, Zen<BigInteger> expr3)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            CommonUtilities.ValidateNotNull(expr3);

            var key = (expr1, expr2, expr3);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr1, expr2, expr3);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringSubstringExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The subtring match.</param>
        /// <param name="expr3">The substituted string.</param>
        private ZenStringSubstringExpr(Zen<string> expr1, Zen<BigInteger> expr2, Zen<BigInteger> expr3)
        {
            this.StringExpr = expr1;
            this.OffsetExpr = expr2;
            this.LengthExpr = expr3;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> StringExpr { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<BigInteger> OffsetExpr { get; }

        /// <summary>
        /// Gets the third expression.
        /// </summary>
        internal Zen<BigInteger> LengthExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Substring({this.StringExpr}, {this.OffsetExpr}, {this.LengthExpr})";
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
            return visitor.VisitZenStringSubstringExpr(this, parameter);
        }
    }
}
