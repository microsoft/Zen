// <copyright file="ZenConstantBoolExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a True expression.
    /// </summary>
    internal sealed class ZenConstantBoolExpr : Zen<bool>
    {
        public static ZenConstantBoolExpr False = new ZenConstantBoolExpr(false);

        public static ZenConstantBoolExpr True = new ZenConstantBoolExpr(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenConstantBoolExpr"/> class.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        private ZenConstantBoolExpr(bool value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        public bool Value { get; }

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
            return visitor.VisitZenConstantBoolExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<bool> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenConstantBoolExpr(this);
        }
    }
}
