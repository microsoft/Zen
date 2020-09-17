// <copyright file="ZenIntConstantExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a uint constant expression.
    /// </summary>
    internal sealed class ZenConstantIntExpr : Zen<int>
    {
        private static Dictionary<int, Zen<int>> hashConsTable = new Dictionary<int, Zen<int>>();

        internal override Zen<int> Unroll()
        {
            return this;
        }

        public static Zen<int> Create(int value)
        {
            if (hashConsTable.TryGetValue(value, out var v))
            {
                return v;
            }

            var ret = new ZenConstantIntExpr(value);
            hashConsTable[value] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstantIntExpr"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        private ZenConstantIntExpr(int value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        internal int Value { get; }

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
            return visitor.VisitZenConstantIntExpr(this, parameter);
        }
    }
}
