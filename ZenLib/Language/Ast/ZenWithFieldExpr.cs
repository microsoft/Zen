﻿// <copyright file="ZenWithFieldExpr.cs" company="Microsoft">
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
        public Zen<T2> FieldExpr { get; }

        /// <summary>
        /// Simplify and create a ZenWithFieldExpr.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<T1> Simplify((Zen<T1> objectExpr, string fieldName, Zen<T2> fieldExpr) args)
        {
            if (args.objectExpr is ZenCreateObjectExpr<T1> oe)
            {
                int i = 0;
                var newFields = new (string, object)[oe.Fields.Count];
                foreach (var fieldValue in oe.Fields)
                {
                    var newValue = fieldValue.Key == args.fieldName ? args.fieldExpr : fieldValue.Value;
                    newFields[i++] = (fieldValue.Key, newValue);
                }

                return ZenCreateObjectExpr<T1>.Create(newFields);
            }

            return new ZenWithFieldExpr<T1, T2>(args.objectExpr, args.fieldName, args.fieldExpr);
        }

        /// <summary>
        /// Create a new ZenWithFieldExpr.
        /// </summary>
        /// <param name="expr">The object expr.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static Zen<T1> Create(Zen<T1> expr, string fieldName, Zen<T2> fieldValue)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(fieldName);
            Contract.AssertNotNull(fieldValue);
            Contract.AssertFieldOrProperty(typeof(T1), typeof(T2), fieldName);

            var key = (expr.Id, fieldName, fieldValue.Id);
            var flyweight = ZenAstCache<ZenWithFieldExpr<T1, T2>, Zen<T1>>.Flyweight;
            flyweight.GetOrAdd(key, (expr, fieldName, fieldValue), Simplify, out var value);
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
            this.FieldExpr = fieldValue;
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Expr} with {this.FieldName}={this.FieldExpr})";
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <param name="parameter">The visitor parameter.</param>
        /// <typeparam name="TParam">The visitor parameter type.</typeparam>
        /// <typeparam name="TReturn">The visitor return type.</typeparam>
        /// <returns>A return value.</returns>
        internal override TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitWithField(this, parameter);
        }

        /// <summary>
        /// Implementing the visitor interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        internal override void Accept(ZenExprActionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
