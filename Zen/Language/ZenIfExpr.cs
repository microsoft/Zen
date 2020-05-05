// <copyright file="ZenIfExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an If expression.
    /// </summary>
    internal sealed class ZenIfExpr<T> : Zen<T>
    {
        private static Dictionary<(object, object, object), ZenIfExpr<T>> hashConsTable =
            new Dictionary<(object, object, object), ZenIfExpr<T>>();

        public static ZenIfExpr<T> Create(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            CommonUtilities.Validate(guardExpr);
            CommonUtilities.Validate(trueExpr);
            CommonUtilities.Validate(falseExpr);

            var key = (guardExpr, trueExpr, falseExpr);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenIfExpr<T>(guardExpr, trueExpr, falseExpr);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenIfExpr{T}"/> class.
        /// </summary>
        /// <param name="guardExpr">The guard expression.</param>
        /// <param name="trueExpr">The true expression.</param>
        /// <param name="falseExpr">The false expression.</param>
        private ZenIfExpr(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            this.GuardExpr = guardExpr;
            this.TrueExpr = trueExpr;
            this.FalseExpr = falseExpr;
        }

        /// <summary>
        /// Gets the guard expression.
        /// </summary>
        internal Zen<bool> GuardExpr { get; }

        /// <summary>
        /// Gets the true expression.
        /// </summary>
        internal Zen<T> TrueExpr { get; }

        /// <summary>
        /// Gets the false expression.
        /// </summary>
        internal Zen<T> FalseExpr { get; }

        /// <summary>
        /// Convert the condition to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"If({this.GuardExpr.ToString()}, {this.TrueExpr.ToString()}, {this.FalseExpr.ToString()})";
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
            return visitor.VisitZenIfExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<T> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenIfExpr(this);
        }
    }
}
