﻿// <copyright file="ZenCreateObjectExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject> : Zen<TObject>
    {
        /// <summary>
        /// The fields of the object.
        /// </summary>
        public SortedDictionary<string, object> Fields { get; }

        /// <summary>
        /// Simplify and create a new ZenCreateObjectExpr.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>The new expr.</returns>
        private static Zen<TObject> Simplify((string, object)[] fields) => new ZenCreateObjectExpr<TObject>(fields);

        /// <summary>
        /// Creates a new ZenCreateObjectExpr.
        /// </summary>
        /// <param name="fields">The fields and their values.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<TObject> Create(params (string, object)[] fields)
        {
            Contract.AssertNotNull(fields);

            foreach (var field in fields)
            {
                var fieldType = field.Item2.GetType();
                Contract.Assert(ReflectionUtilities.IsZenType(fieldType));
                Contract.AssertFieldOrProperty(
                    typeof(TObject),
                    fieldType.BaseType.GetGenericArgumentsCached()[0],
                    field.Item1);
            }

            System.Array.Sort(fields, (x, y) => x.Item1.CompareTo(y.Item1));

            var fieldIds = new object[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var id = (long)f.Item2.GetType().GetFieldCached("Id").GetValue(f.Item2);
                fieldIds[i] = (f.Item1, id);
            }

            var flyweight = ZenAstCache<ZenCreateObjectExpr<TObject>, Zen<TObject>>.FlyweightArray;
            flyweight.GetOrAdd(fieldIds, fields, Simplify, out var value);
            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, object)[] fields)
        {
            this.Fields = new SortedDictionary<string, object>();
            foreach (var (field, value) in fields)
            {
                this.Fields[field] = value;
            }
        }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"new {typeof(TObject).Name}(");
            var fieldCount = this.Fields.Count;
            int i = 0;
            foreach (var fieldValuePair in this.Fields)
            {
                sb.Append($"{fieldValuePair.Key}={fieldValuePair.Value}");
                if (++i < fieldCount)
                    sb.Append(", ");
            }

            sb.Append(")");
            return sb.ToString();
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
            return visitor.VisitCreateObject(this, parameter);
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
