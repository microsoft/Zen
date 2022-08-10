// <copyright file="ZenFieldExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Field expression.
    /// </summary>
    internal sealed class ZenGetFieldExpr<T1, T2> : Zen<T2>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(Zen<T1>, string), Zen<T2>> createFunc = (v) => Simplify(v.Item1, v.Item2);

        /// <summary>
        /// Hash cons table for ZenGetFieldExpr.
        /// </summary>
        private static HashConsTable<(long, string), Zen<T2>> hashConsTable = new HashConsTable<(long, string), Zen<T2>>();

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Zen<T1> Expr { get; }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Simplify and create a new ZenGetFieldExpr.
        /// </summary>
        /// <param name="expr">The object expr.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns></returns>
        private static Zen<T2> Simplify(Zen<T1> expr, string fieldName)
        {
            // get(with(o, name, f), name) == f
            // get(with(o, name', f), name) == get(o, name)
            var type = expr.GetType();
            if (type.GetGenericTypeDefinition() == typeof(ZenWithFieldExpr<,>))
            {
                var fieldNameProperty = type.GetProperty("FieldName");
                var fieldValueProperty = type.GetProperty("FieldExpr");
                var exprProperty = type.GetProperty("Expr");

                return ((string)fieldNameProperty.GetValue(expr) == fieldName) ?
                        (Zen<T2>)fieldValueProperty.GetValue(expr) :
                        Create((Zen<T1>)exprProperty.GetValue(expr), fieldName); // recurse
            }

            // get(createobject(p1, ..., pn), namei) == pi
            if (expr is ZenCreateObjectExpr<T1> coe)
            {
                return (Zen<T2>)coe.Fields[fieldName];
            }

            return new ZenGetFieldExpr<T1, T2>(expr, fieldName);
        }

        /// <summary>
        /// Create a new ZenGetFieldExpr.
        /// </summary>
        /// <param name="expr">The object expr.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns></returns>
        public static Zen<T2> Create(Zen<T1> expr, string fieldName)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(fieldName);
            Contract.AssertFieldOrProperty(typeof(T1), typeof(T2), fieldName);

            var key = (expr.Id, fieldName);
            hashConsTable.GetOrAdd(key, (expr, fieldName), createFunc, out var value);
            return value;
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
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Expr}.{this.FieldName})";
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
            return visitor.VisitGetField(this, parameter);
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
