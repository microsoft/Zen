// <copyright file="ZenMultiplyExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a multiply expression.
    /// </summary>
    internal sealed class ZenMultiplyExpr<T> : Zen<T>
    {
        private static Dictionary<(object, object), Zen<T>> hashConsTable = new Dictionary<(object, object), Zen<T>>();

        public static Zen<T> Simplify(Zen<T> e1, Zen<T> e2)
        {
            var x = ReflectionUtilities.GetConstantIntegerValue(e1);
            var y = ReflectionUtilities.GetConstantIntegerValue(e2);

            if (x.HasValue && y.HasValue)
            {
                return ReflectionUtilities.CreateConstantValue<T>(x.Value * y.Value);
            }

            if ((x.HasValue && x.Value == 0) || (y.HasValue && y.Value == 0))
            {
                return ReflectionUtilities.CreateConstantValue<T>(0);
            }

            if (x.HasValue && x.Value == 1)
            {
                return e2;
            }

            if (y.HasValue && y.Value == 1)
            {
                return e1;
            }

            return new ZenMultiplyExpr<T>(e1, e2);
        }

        public static Zen<T> Create(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.Validate(expr1);
            CommonUtilities.Validate(expr2);
            CommonUtilities.ValidateIsIntegerType(typeof(T));

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
        /// Initializes a new instance of the <see cref="ZenMultiplyExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        private ZenMultiplyExpr(Zen<T> expr1, Zen<T> expr2)
        {
            this.Expr1 = expr1;
            this.Expr2 = expr2;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<T> Expr1 { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<T> Expr2 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"ZenMultiplyExpr({this.Expr1}, {this.Expr2})";
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
            return visitor.VisitZenMultiplyExpr(this, parameter);
        }
    }
}
