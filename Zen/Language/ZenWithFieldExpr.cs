// <copyright file="ZenWithFieldExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Field expression.
    /// </summary>
    internal sealed class ZenWithFieldExpr<T1, T2> : Zen<T1>
    {
        private static Dictionary<(object, object, object), ZenWithFieldExpr<T1, T2>> hashConsTable =
            new Dictionary<(object, object, object), ZenWithFieldExpr<T1, T2>>();

        public static ZenWithFieldExpr<T1, T2> Create(Zen<T1> expr, string fieldName, Zen<T2> fieldValue)
        {
            CommonUtilities.Validate(expr);
            CommonUtilities.Validate(fieldValue);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(T1), fieldName);

            var key = (expr, fieldName, fieldValue);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenWithFieldExpr<T1, T2>(expr, fieldName, fieldValue);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenWithFieldExpr{T1, T2}"/> class.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldValue">The field value.</param>
        private ZenWithFieldExpr(Zen<T1> expr, string fieldName, Zen<T2> fieldValue)
        {
            this.Expr = expr;
            this.FieldName = fieldName;
            this.FieldValue = fieldValue;
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
        /// Gets the field value to set.
        /// </summary>
        public Zen<T2> FieldValue { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"WithField({this.Expr.ToString()}, {this.FieldName}, {this.FieldValue.ToString()})";
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
            return visitor.VisitZenWithFieldExpr(this, parameter);
        }

        /// <summary>
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<T1> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenWithFieldExpr(this);
        }
    }
}
