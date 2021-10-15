// <copyright file="ZenStringContainmentExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a string Containment expression.
    /// </summary>
    internal sealed class ZenStringContainmentExpr : Zen<bool>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<string>, Zen<string>, ContainmentType), Zen<bool>> createFunc = (v) => Simplify(v.Item1, v.Item2, v.Item3);

        /// <summary>
        /// Hash cons table for ZenStringContainmentExpr.
        /// </summary>
        private static HashConsTable<(long, long, int), Zen<bool>> hashConsTable = new HashConsTable<(long, long, int), Zen<bool>>();

        /// <summary>
        /// Constant functions for evaluating containment.
        /// </summary>
        private static Func<string, string, bool>[] constantFuncs = new Func<string, string, bool>[]
        {
            (s1, s2) => s1.StartsWith(s2),
            (s1, s2) => s1.EndsWith(s2),
            (s1, s2) => s1.Contains(s2),
        };

        /// <summary>
        /// Simplify and create a ZenStringContainmentExpr.
        /// </summary>
        /// <param name="e1">The string expr.</param>
        /// <param name="e2">The substring expr.</param>
        /// <param name="containmentType">The containment type.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<bool> Simplify(Zen<string> e1, Zen<string> e2, ContainmentType containmentType)
        {
            string x = ReflectionUtilities.GetConstantString(e1);
            string y = ReflectionUtilities.GetConstantString(e2);

            if (x != null && y != null)
            {
                var f = constantFuncs[(int)containmentType];
                return f(x, y);
            }

            if (y == string.Empty)
                return true;
            if (x == string.Empty)
                return false;

            return new ZenStringContainmentExpr(e1, e2, containmentType);
        }

        /// <summary>
        /// Create a new ZenStringContainmentExpr.
        /// </summary>
        /// <param name="expr1">The string expr.</param>
        /// <param name="expr2">The substring expr.</param>
        /// <param name="containmentType">The containment type.</param>
        /// <returns></returns>
        public static Zen<bool> Create(Zen<string> expr1, Zen<string> expr2, ContainmentType containmentType)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            var key = (expr1.Id, expr2.Id, (int)containmentType);
            hashConsTable.GetOrAdd(key, (expr1, expr2, containmentType), createFunc, out var value);
            return value;
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
