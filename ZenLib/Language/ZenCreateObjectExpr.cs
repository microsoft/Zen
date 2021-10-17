// <copyright file="ZenCreateObjectExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject> : Zen<TObject>
    {
        /// <summary>
        /// Static creation function for hash consing.
        /// </summary>
        private static Func<(string, object)[], Zen<TObject>> createFunc = (v) => new ZenCreateObjectExpr<TObject>(v);

        /// <summary>
        /// Hash cons table for ZenCreateObjectExpr.
        /// </summary>
        private static HashConsTable<(string, long)[], Zen<TObject>> hashConsTable = new HashConsTable<(string, long)[], Zen<TObject>>(new ArrayComparer());

        /// <summary>
        /// Unroll a ZenCreateObjectExpr.
        /// </summary>
        /// <returns>The unrolled expression.</returns>
        public override Zen<TObject> Unroll()
        {
            var newFields = new (string, object)[this.Fields.Count];

            int i = 0;
            foreach (var kv in this.Fields)
            {
                var value = kv.Value.GetType()
                    .GetMethodCached("Unroll")
                    .Invoke(kv.Value, CommonUtilities.EmptyArray);
                newFields[i++] = (kv.Key, value);
            }

            return CreateFast(newFields);
        }

        /// <summary>
        /// Creates a new ZenCreateObjectExpr.
        /// </summary>
        /// <param name="fields">The fields and their values.</param>
        /// <returns>The new Zen expr.</returns>
        public static Zen<TObject> Create(params (string, object)[] fields)
        {
            CommonUtilities.ValidateNotNull(fields);

            foreach (var field in fields)
            {
                var fieldType = field.Item2.GetType();
                ReflectionUtilities.ValidateIsZenType(fieldType);
                ReflectionUtilities.ValidateFieldOrProperty(
                    typeof(TObject),
                    fieldType.BaseType.GetGenericArgumentsCached()[0],
                    field.Item1);
            }

            Array.Sort(fields, (x, y) => x.Item1.CompareTo(y.Item1));

            (string, long)[] fieldIds = new (string, long)[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                Console.WriteLine(f.Item2.GetType());
                var id = (long)f.Item2.GetType().GetFieldCached("Id").GetValue(f.Item2);
                fieldIds[i] = (f.Item1, id);
            }

            hashConsTable.GetOrAdd(fieldIds, fields, createFunc, out var value);
            return value;
        }

        /// <summary>
        /// Creates a ZenCreateObjectExpr without the sorting and checks.
        /// </summary>
        /// <param name="fields">The already sorted fields.</param>
        /// <returns>The new ZenExpr.</returns>
        private static Zen<TObject> CreateFast(params (string, object)[] fields)
        {
            (string, long)[] fieldIds = new (string, long)[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var id = (long)f.Item2.GetType().GetFieldCached("Id").GetValue(f.Item2);
                fieldIds[i] = (f.Item1, id);
            }

            hashConsTable.GetOrAdd(fieldIds, fields, (v) => new ZenCreateObjectExpr<TObject>(v), out var value);
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

        public SortedDictionary<string, object> Fields { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"new {typeof(TObject).Name}(");
            foreach (var fieldValuePair in this.Fields)
            {
                sb.Append($"{fieldValuePair.Key}={fieldValuePair.Value}, ");
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
        internal override TReturn Accept<TParam, TReturn>(IZenExprVisitor<TParam, TReturn> visitor, TParam parameter)
        {
            return visitor.VisitZenCreateObjectExpr(this, parameter);
        }

        /// <summary>
        /// Custom array comparer for ensuring hash consing uniqueness.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class ArrayComparer : IEqualityComparer<(string, long)[]>
        {
            public bool Equals((string, long)[] a1, (string, long)[] a2)
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }

                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode((string, long)[] array)
            {
                int result = 31;
                for (int i = 0; i < array.Length; i++)
                {
                    var (s, o) = array[i];
                    result = result * 7 + s.GetHashCode() + o.GetHashCode();
                }

                return result;
            }
        }
    }
}
