// <copyright file="ZenFieldExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Field expression.
    /// </summary>
    internal sealed class ZenGetFieldExpr<T1, T2> : Zen<T2>
    {
        private static Dictionary<(object, object), ZenGetFieldExpr<T1, T2>> hashConsTable =
            new Dictionary<(object, object), ZenGetFieldExpr<T1, T2>>();

        public static ZenGetFieldExpr<T1, T2> Create(Zen<T1> expr, string fieldName)
        {
            CommonUtilities.Validate(expr);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(T1), fieldName);

            var key = (expr, fieldName);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenGetFieldExpr<T1, T2>(expr, fieldName);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenGetFieldExpr{T1, T2}"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="fieldName">The field name.</param>
        private ZenGetFieldExpr(Zen<T1> expr, string fieldName)
        {
            this.Expr = expr;
            this.FieldName = fieldName;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Zen<T1> Expr { get; }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Field({this.FieldName}, {this.Expr})";
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
            return visitor.VisitZenGetFieldExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<T2> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenGetFieldExpr(this);
        }
    }
}
