// <copyright file="ZenWithFieldExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Field expression.
    /// </summary>
    internal sealed class ZenWithFieldExpr<T1, T2> : Zen<T1>
    {
        /// <summary>
        /// Hash cons table for ZenWithFieldExpr.
        /// </summary>
        private static HashConsTable<(long, string, long), ZenWithFieldExpr<T1, T2>> hashConsTable = new HashConsTable<(long, string, long), ZenWithFieldExpr<T1, T2>>();

        /// <summary>
        /// Unroll the ZenWithFieldExpr.
        /// </summary>
        /// <returns>The unrolled expr.</returns>
        public override Zen<T1> Unroll()
        {
            return Create(this.Expr.Unroll(), this.FieldName, this.FieldValue.Unroll());
        }

        /// <summary>
        /// Create a new ZenWithFieldExpr.
        /// </summary>
        /// <param name="expr">The object expr.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static ZenWithFieldExpr<T1, T2> Create(Zen<T1> expr, string fieldName, Zen<T2> fieldValue)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(fieldName);
            CommonUtilities.ValidateNotNull(fieldValue);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(T1), typeof(T2), fieldName);

            var key = (expr.Id, fieldName, fieldValue.Id);
            hashConsTable.GetOrAdd(key, () => new ZenWithFieldExpr<T1, T2>(expr, fieldName, fieldValue), out var value);
            return value;
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
            return $"({this.Expr} with {this.FieldName}={this.FieldValue})";
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
    }
}
