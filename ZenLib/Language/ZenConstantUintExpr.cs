// <copyright file="ZenUintConstantExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a uint constant expression.
    /// </summary>
    internal sealed class ZenConstantUintExpr : Zen<uint>
    {
        private static Dictionary<uint, Zen<uint>> hashConsTable = new Dictionary<uint, Zen<uint>>();

        public static Zen<uint> Create(uint value)
        {
            if (hashConsTable.TryGetValue(value, out var v))
            {
                return v;
            }

            var ret = new ZenConstantUintExpr(value);
            hashConsTable[value] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstantUintExpr"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        private ZenConstantUintExpr(uint value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        internal uint Value { get; }

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
            return visitor.VisitZenConstantUintExpr(this, parameter);
        }
    }
}
