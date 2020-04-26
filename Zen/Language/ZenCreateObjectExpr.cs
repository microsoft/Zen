// <copyright file="ZenCreateObjectExpr.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1>>();

        public static ZenCreateObjectExpr<TObject, T1> Create((string, Zen<T1>) field1)
        {
            CommonUtilities.Validate(field1.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            var key = field1;
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1>(field1);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject({this.FieldName1}={this.FieldValue1})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2>>();

        public static ZenCreateObjectExpr<TObject, T1, T2> Create((string, Zen<T1>) field1, (string, Zen<T2>) field2)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            var key = (field1, field2);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2>(field1, field2);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2}"/> class.
        /// </summary>
        private ZenCreateObjectExpr(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2, T3> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3>>();

        public static ZenCreateObjectExpr<TObject, T1, T2, T3> Create(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2,
            (string, Zen<T3>) field3)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            CommonUtilities.Validate(field3.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field3.Item1);
            var key = (field1, field2, field3);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2, T3>(field1, field2, field3);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2, T3}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1, (string, Zen<T2>) field2, (string, Zen<T3>) field3)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
            this.FieldName3 = field3.Item1;
            this.FieldValue3 = field3.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        public string FieldName3 { get; }

        public Zen<T3> FieldValue3 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2}," +
                $"{this.FieldName3}={this.FieldValue2})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2, T3, T4> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4>>();

        public static ZenCreateObjectExpr<TObject, T1, T2, T3, T4> Create(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2,
            (string, Zen<T3>) field3,
            (string, Zen<T4>) field4)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            CommonUtilities.Validate(field3.Item2);
            CommonUtilities.Validate(field4.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field3.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field4.Item1);

            var key = (field1, field2, field3, field4);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2, T3, T4>(field1, field2, field3, field4);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2, T3, T4}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1, (string, Zen<T2>) field2, (string, Zen<T3>) field3, (string, Zen<T4>) field4)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
            this.FieldName3 = field3.Item1;
            this.FieldValue3 = field3.Item2;
            this.FieldName4 = field4.Item1;
            this.FieldValue4 = field4.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        public string FieldName3 { get; }

        public Zen<T3> FieldValue3 { get; }

        public string FieldName4 { get; }

        public Zen<T4> FieldValue4 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2}," +
                $"{this.FieldName3}={this.FieldValue2}," +
                $"{this.FieldName4}={this.FieldValue4})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5>>();

        public static ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5> Create(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2,
            (string, Zen<T3>) field3,
            (string, Zen<T4>) field4,
            (string, Zen<T5>) field5)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            CommonUtilities.Validate(field3.Item2);
            CommonUtilities.Validate(field4.Item2);
            CommonUtilities.Validate(field5.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field3.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field4.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field5.Item1);

            var key = (field1, field2, field3, field4, field5);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5>(field1, field2, field3, field4, field5);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1, (string, Zen<T2>) field2, (string, Zen<T3>) field3, (string, Zen<T4>) field4, (string, Zen<T5>) field5)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
            this.FieldName3 = field3.Item1;
            this.FieldValue3 = field3.Item2;
            this.FieldName4 = field4.Item1;
            this.FieldValue4 = field4.Item2;
            this.FieldName5 = field5.Item1;
            this.FieldValue5 = field5.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        public string FieldName3 { get; }

        public Zen<T3> FieldValue3 { get; }

        public string FieldName4 { get; }

        public Zen<T4> FieldValue4 { get; }

        public string FieldName5 { get; }

        public Zen<T5> FieldValue5 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2}," +
                $"{this.FieldName3}={this.FieldValue2}," +
                $"{this.FieldName4}={this.FieldValue4}," +
                $"{this.FieldName5}={this.FieldValue5})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6>>();

        public static ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6> Create(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2,
            (string, Zen<T3>) field3,
            (string, Zen<T4>) field4,
            (string, Zen<T5>) field5,
            (string, Zen<T6>) field6)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            CommonUtilities.Validate(field3.Item2);
            CommonUtilities.Validate(field4.Item2);
            CommonUtilities.Validate(field5.Item2);
            CommonUtilities.Validate(field6.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field3.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field4.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field5.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field6.Item1);

            var key = (field1, field2, field3, field4, field5, field6);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6>(field1, field2, field3, field4, field5, field6);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2, T3, T4, T5, T6}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1, (string, Zen<T2>) field2, (string, Zen<T3>) field3, (string, Zen<T4>) field4, (string, Zen<T5>) field5, (string, Zen<T6>) field6)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
            this.FieldName3 = field3.Item1;
            this.FieldValue3 = field3.Item2;
            this.FieldName4 = field4.Item1;
            this.FieldValue4 = field4.Item2;
            this.FieldName5 = field5.Item1;
            this.FieldValue5 = field5.Item2;
            this.FieldName6 = field6.Item1;
            this.FieldValue6 = field6.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        public string FieldName3 { get; }

        public Zen<T3> FieldValue3 { get; }

        public string FieldName4 { get; }

        public Zen<T4> FieldValue4 { get; }

        public string FieldName5 { get; }

        public Zen<T5> FieldValue5 { get; }

        public string FieldName6 { get; }

        public Zen<T6> FieldValue6 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2}," +
                $"{this.FieldName3}={this.FieldValue2}," +
                $"{this.FieldName4}={this.FieldValue4}," +
                $"{this.FieldName5}={this.FieldValue5}," +
                $"{this.FieldName6}{this.FieldValue6})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7>>();

        public static ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7> Create(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2,
            (string, Zen<T3>) field3,
            (string, Zen<T4>) field4,
            (string, Zen<T5>) field5,
            (string, Zen<T6>) field6,
            (string, Zen<T7>) field7)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            CommonUtilities.Validate(field3.Item2);
            CommonUtilities.Validate(field4.Item2);
            CommonUtilities.Validate(field5.Item2);
            CommonUtilities.Validate(field6.Item2);
            CommonUtilities.Validate(field7.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field3.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field4.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field5.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field6.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field7.Item1);

            var key = (field1, field2, field3, field4, field5, field6, field7);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7>(field1, field2, field3, field4, field5, field6, field7);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2, T3, T4, T5, T6, T7}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1, (string, Zen<T2>) field2, (string, Zen<T3>) field3, (string, Zen<T4>) field4, (string, Zen<T5>) field5, (string, Zen<T6>) field6, (string, Zen<T7>) field7)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
            this.FieldName3 = field3.Item1;
            this.FieldValue3 = field3.Item2;
            this.FieldName4 = field4.Item1;
            this.FieldValue4 = field4.Item2;
            this.FieldName5 = field5.Item1;
            this.FieldValue5 = field5.Item2;
            this.FieldName6 = field6.Item1;
            this.FieldValue6 = field6.Item2;
            this.FieldName7 = field7.Item1;
            this.FieldValue7 = field7.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        public string FieldName3 { get; }

        public Zen<T3> FieldValue3 { get; }

        public string FieldName4 { get; }

        public Zen<T4> FieldValue4 { get; }

        public string FieldName5 { get; }

        public Zen<T5> FieldValue5 { get; }

        public string FieldName6 { get; }

        public Zen<T6> FieldValue6 { get; }

        public string FieldName7 { get; }

        public Zen<T7> FieldValue7 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2}," +
                $"{this.FieldName3}={this.FieldValue2}," +
                $"{this.FieldName4}={this.FieldValue4}," +
                $"{this.FieldName5}={this.FieldValue5}," +
                $"{this.FieldName6}{this.FieldValue6}," +
                $"{this.FieldName7}{this.FieldValue7})";
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
    }

    /// <summary>
    /// Class representing an object expression.
    /// </summary>
    internal sealed class ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8> : Zen<TObject>
    {
        private static Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8>> hashConsTable =
            new Dictionary<object, ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8>>();

        public static ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8> Create(
            (string, Zen<T1>) field1,
            (string, Zen<T2>) field2,
            (string, Zen<T3>) field3,
            (string, Zen<T4>) field4,
            (string, Zen<T5>) field5,
            (string, Zen<T6>) field6,
            (string, Zen<T7>) field7,
            (string, Zen<T8>) field8)
        {
            CommonUtilities.Validate(field1.Item2);
            CommonUtilities.Validate(field2.Item2);
            CommonUtilities.Validate(field3.Item2);
            CommonUtilities.Validate(field4.Item2);
            CommonUtilities.Validate(field5.Item2);
            CommonUtilities.Validate(field6.Item2);
            CommonUtilities.Validate(field7.Item2);
            CommonUtilities.Validate(field8.Item2);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field1.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field2.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field3.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field4.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field5.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field6.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field7.Item1);
            ReflectionUtilities.ValidateFieldOrProperty(typeof(TObject), field8.Item1);

            var key = (field1, field2, field3, field4, field5, field6, field7, field8);
            if (hashConsTable.TryGetValue(key, out var value))
            {
                return value;
            }

            var ret = new ZenCreateObjectExpr<TObject, T1, T2, T3, T4, T5, T6, T7, T8>(field1, field2, field3, field4, field5, field6, field7, field8);
            hashConsTable[key] = ret;
            return ret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenCreateObjectExpr{TObject, T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
        /// </summary>
        private ZenCreateObjectExpr((string, Zen<T1>) field1, (string, Zen<T2>) field2, (string, Zen<T3>) field3, (string, Zen<T4>) field4, (string, Zen<T5>) field5, (string, Zen<T6>) field6, (string, Zen<T7>) field7, (string, Zen<T8>) field8)
        {
            this.FieldName1 = field1.Item1;
            this.FieldValue1 = field1.Item2;
            this.FieldName2 = field2.Item1;
            this.FieldValue2 = field2.Item2;
            this.FieldName3 = field3.Item1;
            this.FieldValue3 = field3.Item2;
            this.FieldName4 = field4.Item1;
            this.FieldValue4 = field4.Item2;
            this.FieldName5 = field5.Item1;
            this.FieldValue5 = field5.Item2;
            this.FieldName6 = field6.Item1;
            this.FieldValue6 = field6.Item2;
            this.FieldName7 = field7.Item1;
            this.FieldValue7 = field7.Item2;
            this.FieldName8 = field8.Item1;
            this.FieldValue8 = field8.Item2;
        }

        public string FieldName1 { get; }

        public Zen<T1> FieldValue1 { get; }

        public string FieldName2 { get; }

        public Zen<T2> FieldValue2 { get; }

        public string FieldName3 { get; }

        public Zen<T3> FieldValue3 { get; }

        public string FieldName4 { get; }

        public Zen<T4> FieldValue4 { get; }

        public string FieldName5 { get; }

        public Zen<T5> FieldValue5 { get; }

        public string FieldName6 { get; }

        public Zen<T6> FieldValue6 { get; }

        public string FieldName7 { get; }

        public Zen<T7> FieldValue7 { get; }

        public string FieldName8 { get; }

        public Zen<T8> FieldValue8 { get; }

        /// <summary>
        /// Convert the expression to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"CreateObject(" +
                $"{this.FieldName1}={this.FieldValue1}," +
                $"{this.FieldName2}={this.FieldValue2}," +
                $"{this.FieldName3}={this.FieldValue2}," +
                $"{this.FieldName4}={this.FieldValue4}," +
                $"{this.FieldName5}={this.FieldValue5}," +
                $"{this.FieldName6}{this.FieldValue6}," +
                $"{this.FieldName7}{this.FieldValue7}," +
                $"{this.FieldName8}{this.FieldValue8})";
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
    }
}
