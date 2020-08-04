// <copyright file="ZenStringIndexOfExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a substring expression.
    /// </summary>
    internal sealed class ZenStringIndexOfExpr : Zen<short>
    {
        private static Dictionary<(object, object, object), Zen<short>> hashConsTable =
            new Dictionary<(object, object, object), Zen<short>>();

        public static Zen<short> Simplify(Zen<string> e1, Zen<string> e2, Zen<ushort> e3)
        {
            var x = ReflectionUtilities.GetConstantString(e1);
            var y = ReflectionUtilities.GetConstantString(e2);
            var z = ReflectionUtilities.GetConstantIntegerValue(e3);
            if (x != null && y != null && z.HasValue)
                return CommonUtilities.IndexOf(x, y, (ushort)z.Value);
            return new ZenStringIndexOfExpr(e1, e2, e3);
        }

        public static Zen<short> Create(Zen<string> expr1, Zen<string> expr2, Zen<ushort> expr3)
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
        /// Initializes a new instance of the <see cref="ZenStringIndexOfExpr"/> class.
        /// </summary>
        /// <param name="expr1">The string expression.</param>
        /// <param name="expr2">The subtring expression.</param>
        /// <param name="expr3">The offset expression.</param>
        private ZenStringIndexOfExpr(Zen<string> expr1, Zen<string> expr2, Zen<ushort> expr3)
        {
            this.StringExpr = expr1;
            this.SubstringExpr = expr2;
            this.OffsetExpr = expr3;
        }

        /// <summary>
        /// Gets the first expression.
        /// </summary>
        internal Zen<string> StringExpr { get; }

        /// <summary>
        /// Gets the second expression.
        /// </summary>
        internal Zen<string> SubstringExpr { get; }

        /// <summary>
        /// Gets the third expression.
        /// </summary>
        internal Zen<ushort> OffsetExpr { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"IndexOf({this.StringExpr}, {this.SubstringExpr}, {this.OffsetExpr})";
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
            return visitor.VisitZenStringIndexOfExpr(this, parameter);
        }
    }
}
