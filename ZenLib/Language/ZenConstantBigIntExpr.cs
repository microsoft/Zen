// <copyright file="ZenConstantBigIntExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// Class representing a big integer constant expression.
    /// </summary>
    internal sealed class ZenConstantBigIntExpr : Zen<BigInteger>
    {
        private static Dictionary<BigInteger, Zen<BigInteger>> hashConsTable = new Dictionary<BigInteger, Zen<BigInteger>>();

        internal override Zen<BigInteger> Unroll()
        {
            return this;
        }

        public static Zen<BigInteger> Create(BigInteger value)
        {
            if (hashConsTable.TryGetValue(value, out var v))
            {
                return v;
            }

            var ret = new ZenConstantBigIntExpr(value);
            hashConsTable[value] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstantBigIntExpr"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        private ZenConstantBigIntExpr(BigInteger value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        internal BigInteger Value { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return this.Value.ToString();
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
            return visitor.VisitZenConstantBigIntExpr(this, parameter);
        }
    }
}
