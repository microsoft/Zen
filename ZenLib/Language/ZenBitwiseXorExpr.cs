// <copyright file="ZenBitwiseXorExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an And expression.
    /// </summary>
    internal sealed class ZenBitwiseXorExpr<T> : Zen<T>
    {
        private static Dictionary<(object, object), Zen<T>> hashConsTable = new Dictionary<(object, object), Zen<T>>();

        private static Zen<T> Simplify(Zen<T> e1, Zen<T> e2)
        {
            if (e1 is ZenConstantByteExpr xb && e2 is ZenConstantByteExpr yb)
            {
                return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)(xb.Value ^ yb.Value));
            }

            if (e1 is ZenConstantShortExpr xs && e2 is ZenConstantShortExpr ys)
            {
                return (Zen<T>)(object)ZenConstantShortExpr.Create((short)(xs.Value ^ ys.Value));
            }

            if (e1 is ZenConstantUshortExpr xus && e2 is ZenConstantUshortExpr yus)
            {
                return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)(xus.Value ^ yus.Value));
            }

            if (e1 is ZenConstantIntExpr xi && e2 is ZenConstantIntExpr yi)
            {
                return (Zen<T>)(object)ZenConstantIntExpr.Create(xi.Value ^ yi.Value);
            }

            if (e1 is ZenConstantUintExpr xui && e2 is ZenConstantUintExpr yui)
            {
                return (Zen<T>)(object)ZenConstantUintExpr.Create(xui.Value ^ yui.Value);
            }

            if (e1 is ZenConstantLongExpr xl && e2 is ZenConstantLongExpr yl)
            {
                return (Zen<T>)(object)ZenConstantLongExpr.Create(xl.Value ^ yl.Value);
            }

            if (e1 is ZenConstantUlongExpr xul && e2 is ZenConstantUlongExpr yul)
            {
                return (Zen<T>)(object)ZenConstantUlongExpr.Create(xul.Value ^ yul.Value);
            }

            return new ZenBitwiseXorExpr<T>(e1, e2);
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
        /// Initializes a new instance of the <see cref="ZenBitwiseXorExpr{T}"/> class.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">First Zen expression.</param>
        private ZenBitwiseXorExpr(Zen<T> expr1, Zen<T> expr2)
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
            return $"({this.Expr1.ToString()} ^ {this.Expr2.ToString()})";
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
            return visitor.VisitZenBitwiseXorExpr(this, parameter);
        }
    }
}
