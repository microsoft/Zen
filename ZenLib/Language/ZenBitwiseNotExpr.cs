// <copyright file="ZenBitwiseNotExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Not expression.
    /// </summary>
    internal sealed class ZenBitwiseNotExpr<T> : Zen<T>
    {
        private static Dictionary<object, Zen<T>> hashConsTable = new Dictionary<object, Zen<T>>();

        private static Zen<T> Simplify(Zen<T> e)
        {
            if (e is ZenConstantByteExpr xb)
            {
                return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)(~xb.Value));
            }

            if (e is ZenConstantShortExpr xs)
            {
                return (Zen<T>)(object)ZenConstantShortExpr.Create((short)(~xs.Value));
            }

            if (e is ZenConstantUshortExpr xus)
            {
                return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)(~xus.Value));
            }

            if (e is ZenConstantIntExpr xi)
            {
                return (Zen<T>)(object)ZenConstantIntExpr.Create(~xi.Value);
            }

            if (e is ZenConstantUintExpr xui)
            {
                return (Zen<T>)(object)ZenConstantUintExpr.Create(~xui.Value);
            }

            if (e is ZenConstantLongExpr xl)
            {
                return (Zen<T>)(object)ZenConstantLongExpr.Create(~xl.Value);
            }

            if (e is ZenConstantUlongExpr xul)
            {
                return (Zen<T>)(object)ZenConstantUlongExpr.Create(~xul.Value);
            }

            return new ZenBitwiseNotExpr<T>(e);
        }

        public static Zen<T> Create(Zen<T> expr)
        {
            CommonUtilities.Validate(expr);
            CommonUtilities.ValidateIsIntegerType(typeof(T));

            if (hashConsTable.TryGetValue(expr, out var value))
            {
                return value;
            }

            var ret = Simplify(expr);
            hashConsTable[expr] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenBitwiseNotExpr{T}"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        private ZenBitwiseNotExpr(Zen<T> expr)
        {
            this.Expr = expr;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        internal Zen<T> Expr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"~({this.Expr.ToString()})";
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
            return visitor.VisitZenBitwiseNotExpr(this, parameter);
        }
    }
}
