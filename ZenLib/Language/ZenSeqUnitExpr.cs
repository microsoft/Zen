// <copyright file="ZenSeqUnitExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a sequence unit expression.
    /// </summary>
    internal sealed class ZenSeqUnitExpr<T> : Zen<Seq<T>>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<Zen<T>, Zen<Seq<T>>> createFunc = (v) => new ZenSeqUnitExpr<T>(v);

        /// <summary>
        /// Hash cons table for ZenSeqUnitExpr.
        /// </summary>
        private static HashConsTable<long, Zen<Seq<T>>> hashConsTable = new HashConsTable<long, Zen<Seq<T>>>();

        /// <summary>
        /// Gets the value expr.
        /// </summary>
        public Zen<T> ValueExpr { get; }

        /// <summary>
        /// Create a new ZenSeqUnitExpr.
        /// </summary>
        /// <param name="valueExpr">The value expr.</param>
        /// <returns>The new expr.</returns>
        public static Zen<Seq<T>> Create(Zen<T> valueExpr)
        {
            Contract.AssertNotNull(valueExpr);

            hashConsTable.GetOrAdd(valueExpr.Id, valueExpr, createFunc, out var v);
            return v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSeqUnitExpr{T}"/> class.
        /// </summary>
        /// <param name="valueExpr">The value expression.</param>
        private ZenSeqUnitExpr(Zen<T> valueExpr)
        {
            this.ValueExpr = valueExpr;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Unit({this.ValueExpr})";
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
            return visitor.VisitSeqUnit(this, parameter);
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
