// <copyright file="ZenStringContainmentExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a string Containment expression.
    /// </summary>
    internal sealed class ZenStringContainmentExpr : Zen<bool>
    {
        private static Dictionary<(object, object, int), Zen<bool>> hashConsTable = new Dictionary<(object, object, int), Zen<bool>>();

        private static Func<string, string, bool>[] constantFuncs = new Func<string, string, bool>[]
        {
            (s1, s2) => s1.StartsWith(s2),
            (s1, s2) => s1.EndsWith(s2),
            (s1, s2) => s1.Contains(s2),
        };

        public override Zen<bool> Unroll()
        {
            return Create(this.StringExpr.Unroll(), this.SubstringExpr.Unroll(), this.ContainmentType);
        }

        public static Zen<bool> Simplify(Zen<string> e1, Zen<string> e2, ContainmentType containmentType)
        {
            string x = ReflectionUtilities.GetConstantString(e1);
            string y = ReflectionUtilities.GetConstantString(e2);

            if (x != null && y != null)
            {
                var f = constantFuncs[(int)containmentType];
                return f(x, y);
            }

            if (y == "")
                return true;
            if (x == "")
                return false;

            return new ZenStringContainmentExpr(e1, e2, containmentType);
        }

        public static Zen<bool> Create(Zen<string> expr1, Zen<string> expr2, ContainmentType containmentType)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var key = (expr1, expr2, (int)containmentType);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr1, expr2, containmentType);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenStringContainmentExpr"/> class.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <param name="containmentType">The containment type.</param>
        private ZenStringContainmentExpr(Zen<string> expr1, Zen<string> expr2, ContainmentType containmentType)
        {
            this.StringExpr = expr1;
            this.SubstringExpr = expr2;
            this.ContainmentType = containmentType;
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
        /// Gets the containment type.
        /// </summary>
        internal ContainmentType ContainmentType { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Containment({this.StringExpr}, {this.SubstringExpr}, {this.ContainmentType})";
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
            return visitor.VisitZenStringContainmentExpr(this, parameter);
        }
    }

    internal enum ContainmentType
    {
        PrefixOf,
        SuffixOf,
        Contains,
    }
}
