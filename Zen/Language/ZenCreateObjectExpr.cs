// <copyright file="ZenCreateObjectExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
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
        private static Dictionary<(string, object)[], ZenCreateObjectExpr<TObject>> hashConsTable =
            new Dictionary<(string, object)[], ZenCreateObjectExpr<TObject>>(new ArrayComparer());

        public static ZenCreateObjectExpr<TObject> Create(params (string, object)[] fields)
        {
            CommonUtilities.Validate(fields);
            foreach (var field in fields)
            {
                ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field.Item1);
                ReflectionUtilities.ValidateFieldIsZenObject(typeof(TObject), field.Item2, field.Item1);
            }

            Array.Sort(fields, (x, y) => x.Item1.CompareTo(y.Item1));

            if (hashConsTable.TryGetValue(fields, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject>(fields);
            hashConsTable[fields] = ret;
            return ret;
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
            sb.Append("CreateObject(");
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
        /// Implementing the transformer interface.
        /// </summary>
        /// <param name="visitor">The visitor object.</param>
        /// <returns>A return value.</returns>
        internal override Zen<TObject> Accept(IZenExprTransformer visitor)
        {
            return visitor.VisitZenCreateObjectExpr(this);
        }

        /// <summary>
        /// Custom array comparer for ensuring hash consing uniqueness.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class ArrayComparer : IEqualityComparer<(string, object)[]>
        {
            public bool Equals((string, object)[] a1, (string, object)[] a2)
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

            public int GetHashCode((string, object)[] array)
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
