// <copyright file="ZenFieldExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing a Field expression.
    /// </summary>
    internal sealed class ZenGetFieldExpr<T1, T2> : Zen<T2>
    {
        private static Dictionary<(object, object), Zen<T2>> hashConsTable = new Dictionary<(object, object), Zen<T2>>();

        private static Zen<T2> Simplify(Zen<T1> expr, string fieldName)
        {
            // get(with(o, name, f), name) == f
            // get(with(o, name', f), name) == get(o, name)
            if (expr is ZenWithFieldExpr<T1, T2> e1)
            {
                return (e1.FieldName == fieldName) ?
                        e1.FieldValue :
                        ZenGetFieldExpr<T1, T2>.Create(e1.Expr, fieldName); // recurse
            }

            var type = expr.GetType();

            if (type.GetGenericTypeDefinition() == typeof(ZenWithFieldExpr<,>))
            {
                var property = type.GetProperty("Expr");
                return ZenGetFieldExpr<T1, T2>.Create((Zen<T1>)property.GetValue(expr), fieldName);
            }

            // get(if e1 then e2 else e3, name) = if e1 then get(e2, name) else get(e3, name)
            if (Settings.SimplifyRecursive)
            {
                if (expr is ZenIfExpr<T1> e2)
                {
                    var trueBranch = ZenGetFieldExpr<T1, T2>.Create(e2.TrueExpr, fieldName);
                    var falseBranch = ZenGetFieldExpr<T1, T2>.Create(e2.FalseExpr, fieldName);
                    return ZenIfExpr<T2>.Create(e2.GuardExpr, trueBranch, falseBranch);
                }
            }

            // get(createobject(p1, ..., pn), namei) == pi
            if (expr is ZenCreateObjectExpr<T1> coe)
            {
                return (Zen<T2>)coe.Fields[fieldName];
            }

            return new ZenGetFieldExpr<T1, T2>(expr, fieldName);
        }

        public static Zen<T2> Create(Zen<T1> expr, string fieldName)
        {
            CommonUtilities.Validate(expr);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(T1), fieldName);

            var key = (expr, fieldName);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = Simplify(expr, fieldName);
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
        internal override TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitZenGetFieldExpr(this, parameter);
        }
    }
}
