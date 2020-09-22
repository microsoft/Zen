// <copyright file="ReflectionUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static ZenLib.Language;

    /// <summary>
    /// A collection of helper functions for manipulating Zen
    /// objects through C#'s reflection runtime.
    /// </summary>
    internal static class ReflectionUtilities
    {
        /// <summary>
        /// The type of string values.
        /// </summary>
        public readonly static Type StringType = typeof(string);

        /// <summary>
        /// The type of finite string values.
        /// </summary>
        public readonly static Type FiniteStringType = typeof(FiniteString);

        /// <summary>
        /// The type of bool values.
        /// </summary>
        public readonly static Type BoolType = typeof(bool);

        /// <summary>
        /// The type of byte values.
        /// </summary>
        public readonly static Type ByteType = typeof(byte);

        /// <summary>
        /// The type of short values.
        /// </summary>
        public readonly static Type ShortType = typeof(short);

        /// <summary>
        /// The type of ushort values.
        /// </summary>
        public readonly static Type UshortType = typeof(ushort);

        /// <summary>
        /// The type of int values.
        /// </summary>
        public readonly static Type IntType = typeof(int);

        /// <summary>
        /// The type of uint values.
        /// </summary>
        public readonly static Type UintType = typeof(uint);

        /// <summary>
        /// The type of long values.
        /// </summary>
        public readonly static Type LongType = typeof(long);

        /// <summary>
        /// The type of ulong values.
        /// </summary>
        public readonly static Type UlongType = typeof(ulong);

        /// <summary>
        /// The type of a tuple.
        /// </summary>
        public readonly static Type TupleType = typeof(Tuple<,>);

        /// <summary>
        /// The type of a value tuple.
        /// </summary>
        public readonly static Type ValueTupleType = typeof(ValueTuple<,>);

        /// <summary>
        /// The type of an option.
        /// </summary>
        public readonly static Type OptionType = typeof(Option<>);

        /// <summary>
        /// Type of an IList.
        /// </summary>
        public readonly static Type IListType = typeof(IList<>);

        /// <summary>
        /// Type of an IDictionary.
        /// </summary>
        public readonly static Type IDictType = typeof(IDictionary<,>);

        /// <summary>
        /// Type of an List.
        /// </summary>
        public readonly static Type ListType = typeof(List<>);

        /// <summary>
        /// Type of an Dictionary.
        /// </summary>
        public readonly static Type DictType = typeof(Dictionary<,>);

        /// <summary>
        /// The object creation method.
        /// </summary>
        public static MethodInfo CreateMethod = typeof(Language).GetMethod("Create");

        /// <summary>
        /// Check if a type is a kind of integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsIntegerType(Type type)
        {
            return IsUnsignedIntegerType(type) || IsSignedIntegerType(type);
        }

        /// <summary>
        /// Check if a type is a kind of unsigned integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsUnsignedIntegerType(Type type)
        {
            return (type == ByteType || type == UshortType || type == UintType || type == UlongType);
        }

        /// <summary>
        /// Check if a type is a kind of signed integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsSignedIntegerType(Type type)
        {
            return (type == ShortType || type == IntType || type == LongType);
        }

        /// <summary>
        /// Check if a type is a tuple type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTupleType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == TupleType;
        }

        /// <summary>
        /// Check if a type is some kind of tuple type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueTupleType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == ValueTupleType;
        }

        /// <summary>
        /// Check if a type is some kind of tuple type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSomeTupleType(Type type)
        {
            return IsTupleType(type) || IsValueTupleType(type);
        }

        /// <summary>
        /// Check if a type is some kind of tuple type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsOptionType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == OptionType;
        }

        /// <summary>
        /// Check if a type is an IList type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIListType(Type type)
        {
            return type.IsInterface &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition() == IListType;
        }

        /// <summary>
        /// Check if a type is an IDictionary type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIDictionaryType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == IDictType;
        }

        /// <summary>
        /// Check if a type is a List type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == ListType;
        }

        /// <summary>
        /// Check if a type is a Dictionary type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == DictType;
        }

        /// <summary>
        /// Get the value of a field or property using reflection.
        /// </summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <param name="obj">The runtime object.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The field value.</returns>
        public static TField GetFieldOrProperty<TObject, TField>(TObject obj, string fieldName)
        {
            var type = typeof(TObject);
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo != null)
            {
                return (TField)fieldInfo.GetValue(obj);
            }

            var propertyInfo = type.GetProperty(fieldName);
            return (TField)propertyInfo.GetValue(obj);
        }

        /// <summary>
        /// Validates whether the field or property exists.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldOrPropertyName">The field or property name.</param>
        public static void ValidateFieldOrProperty(Type type, string fieldOrPropertyName)
        {
            if (type.GetProperty(fieldOrPropertyName) == null && type.GetField(fieldOrPropertyName) == null)
            {
                throw new ZenException($"Invalid field or property {fieldOrPropertyName} or object with type {type}");
            }
        }

        /// <summary>
        /// Validates that an object is a Zen object of some type.
        /// </summary>
        /// <param name="type">The object type.</param>
        /// <param name="value">The field value to validate.</param>
        /// <param name="fieldOrPropertyName">The object field name.</param>
        public static void ValidateFieldIsZenObject(Type type, object value, string fieldOrPropertyName)
        {
            var fieldInfo = type.GetField(fieldOrPropertyName);
            var expectedType = fieldInfo != null ? fieldInfo.FieldType : type.GetProperty(fieldOrPropertyName).PropertyType;
            var expectedZenType = typeof(Zen<>).MakeGenericType(expectedType);

            var valueType = value.GetType();
            if (!expectedZenType.IsAssignableFrom(valueType))
            {
                throw new ZenException($"Attempting to create an object of type {type} using field {fieldOrPropertyName} with type {valueType}, when field type {expectedZenType} is expected");
            }
        }

        /// <summary>
        /// Set the value of a field or property using reflection.
        /// </summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <param name="obj">The runtime object.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldValue">The new field value.</param>
        /// <returns>The field value.</returns>
        public static void SetFieldOrProperty<TObject, TField>(TObject obj, string fieldName, TField fieldValue)
        {
            var type = obj.GetType();
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, fieldValue);
                return;
            }

            var propertyInfo = type.GetProperty(fieldName);
            propertyInfo.SetValue(obj, fieldValue);
        }

        /// <summary>
        /// Create an instance of an object using reflection.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="fieldNames">The field names.</param>
        /// <param name="values">The field values.</param>
        /// <returns>An instance of the object.</returns>
        public static T CreateInstance<T>(string[] fieldNames, object[] values)
        {
            return (T)CreateInstance(typeof(T), fieldNames, values);
        }

        /// <summary>
        /// Create an instance of an object using reflection.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <param name="values">The field values.</param>
        /// <returns>An instance of the object.</returns>
        public static object CreateInstance(Type type, string[] fieldNames, object[] values)
        {
            var obj = Activator.CreateInstance(type);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];
                var value = values[i];
                SetFieldOrProperty(obj, fieldName, value);
            }

            return obj;
        }

        /// <summary>
        /// Get all the relevant fields for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The fields.</returns>
        public static FieldInfo[] GetAllFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Get all the relevant properties for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The properties.</returns>
        public static PropertyInfo[] GetAllProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Create a new object with a modified field.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="fieldName">The field to modify.</param>
        /// <param name="value">The new field value.</param>
        /// <returns>A copy of the object with the field modified.</returns>
        public static T WithField<T>(object obj, string fieldName, object value)
        {
            var copy = CopyObject(obj);
            SetFieldOrProperty(copy, fieldName, value);
            return (T)copy;
        }

        /// <summary>
        /// Perform a deep copy of an object.
        /// </summary>
        /// <returns></returns>
        public static object CopyObject(object obj)
        {
            var type = obj.GetType();
            if (type.IsValueType)
            {
                return obj;
            }

            var result = Activator.CreateInstance(type);
            foreach (var fieldInfo in GetAllFields(type))
            {
                fieldInfo.SetValue(result, fieldInfo.GetValue(obj));
            }

            foreach (var propertyInfo in GetAllProperties(type))
            {
                propertyInfo.SetValue(result, propertyInfo.GetValue(obj));
            }

            return result;
        }

        /// <summary>
        /// Walk over a type to create a value.
        /// </summary>
        /// <param name="visitor">The type visitor object.</param>
        /// <param name="type">The type.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>A value.</returns>
        internal static T ApplyTypeVisitor<T>(ITypeVisitor<T> visitor, Type type)
        {
            if (type == BoolType)
                return visitor.VisitBool();
            if (type == ByteType)
                return visitor.VisitByte();
            if (type == ShortType)
                return visitor.VisitShort();
            if (type == UshortType)
                return visitor.VisitUshort();
            if (type == IntType)
                return visitor.VisitInt();
            if (type == UintType)
                return visitor.VisitUint();
            if (type == LongType)
                return visitor.VisitLong();
            if (type == UlongType)
                return visitor.VisitUlong();
            if (type == StringType)
                return visitor.VisitString();

            if (IsOptionType(type))
            {
                var t = type.GetGenericArguments()[0];
                return visitor.VisitOption(ty => ApplyTypeVisitor(visitor, ty), type, t);
            }

            if (IsTupleType(type))
            {
                var tleft = type.GetGenericArguments()[0];
                var tright = type.GetGenericArguments()[1];
                return visitor.VisitTuple(ty => ApplyTypeVisitor(visitor, ty), type, tleft, tright);
            }

            if (IsValueTupleType(type))
            {
                var tleft = type.GetGenericArguments()[0];
                var tright = type.GetGenericArguments()[1];
                return visitor.VisitValueTuple(ty => ApplyTypeVisitor(visitor, ty), type, tleft, tright);
            }

            if (IsIListType(type))
            {
                var t = type.GetGenericArguments()[0];
                return visitor.VisitList(ty => ApplyTypeVisitor(visitor, ty), type, t);
            }

            if (IsIDictionaryType(type))
            {
                var args = type.GetGenericArguments();
                var tkey = args[0];
                var tvalue = args[1];
                return visitor.VisitDictionary(ty => ApplyTypeVisitor(visitor, ty), type, tkey, tvalue);
            }

            if (IsListType(type) || IsDictionaryType(type))
            {
                throw new InvalidOperationException($"Unsupported object field type: {type}");
            }

            // some class or struct
            var dict = new SortedDictionary<string, Type>();

            foreach (var field in GetAllFields(type))
            {
                dict[field.Name] = field.FieldType;
            }

            foreach (var property in GetAllProperties(type))
            {
                dict[property.Name] = property.PropertyType;
            }

            return visitor.VisitObject(t => ApplyTypeVisitor(visitor, t), type, dict);
        }

        /// <summary>
        /// Create a constant Zen list value.
        /// </summary>
        /// <param name="value">The list value.</param>
        /// <returns>The Zen value representing the list.</returns>
        internal static Zen<IList<T>> CreateZenListConstant<T>(IList<T> value)
        {
            var list = EmptyList<T>();
            foreach (var elt in value.Reverse())
            {
                list = list.AddFront(elt);
            }

            return list;
        }

        /// <summary>
        /// Create a constant Zen option value.
        /// </summary>
        /// <param name="value">The option value.</param>
        /// <returns>The Zen value representing the option.</returns>
        internal static Zen<Option<T>> CreateZenOptionConstant<T>(Option<T> value)
        {
            if (value.HasValue)
            {
                Language.Some<T>(value.Value);
            }

            return Language.Null<T>();
        }

        /// <summary>
        /// Create a constant Zen dict value.
        /// </summary>
        /// <param name="value">The dictionary.</param>
        /// <returns>The Zen value representing the constant dictinary.</returns>
        internal static Zen<IDictionary<TKey, TValue>> CreateZenDictConstant<TKey, TValue>(IDictionary<TKey, TValue> value)
        {
            var dict = EmptyDict<TKey, TValue>();
            foreach (var kv in value)
            {
                dict = dict.Add(kv.Key, kv.Value);
            }

            return dict;
        }

        /// <summary>
        /// Create a constant Zen value.
        /// </summary>
        /// <param name="value">The type.</param>
        /// <returns>The Zen value representing the constant.</returns>
        internal static object CreateZenConstant<T>(T value)
        {
            var type = typeof(T);
            dynamic v = value;

            if (value is bool)
                return Bool(v);
            if (value is byte)
                return Byte(v);
            if (value is short)
                return Short(v);
            if (value is ushort)
                return UShort(v);
            if (value is int)
                return Int(v);
            if (value is uint)
                return UInt(v);
            if (value is long)
                return Long(v);
            if (value is ulong)
                return ULong(v);
            if (value is string)
                return String(v);
            if (IsOptionType(type))
                return CreateZenOptionConstant(v);
            if (IsTupleType(type))
                return Tuple(CreateZenConstant(v.Item1), CreateZenConstant(v.Item2));
            if (IsValueTupleType(type))
                return ValueTuple(CreateZenConstant(v.Item1), CreateZenConstant(v.Item2));
            if (type.IsGenericType && IListType.MakeGenericType(type.GetGenericArguments()[0]).IsAssignableFrom(type))
                return CreateZenListConstant(v);
            if (type.IsGenericType && IDictType.MakeGenericType(type.GetGenericArguments()[0], type.GetGenericArguments()[1]).IsAssignableFrom(type))
                return CreateZenDictConstant(v);

            // some class or struct
            var fields = new SortedDictionary<string, dynamic>();
            foreach (var field in GetAllFields(type))
            {
                fields[field.Name] = field.GetValue(value);
            }

            foreach (var property in GetAllProperties(type))
            {
                fields[property.Name] = property.GetValue(value);
            }

            var asList = fields.ToArray();
            var createMethod = CreateMethod.MakeGenericMethod(type);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                args[i] = (asList[i].Key, CreateZenConstant(asList[i].Value));
            }

            return createMethod.Invoke(null, new object[] { args });
        }

        /// <summary>
        /// Get an integer value as a long.
        /// </summary>
        /// <typeparam name="T">The integer gype.</typeparam>
        /// <param name="value">The Zen integer value.</param>
        /// <returns>A long.</returns>
        public static long? GetConstantIntegerValue<T>(Zen<T> value)
        {
            if (value is ZenConstantByteExpr xb)
                return xb.Value;
            if (value is ZenConstantShortExpr xs)
                return xs.Value;
            if (value is ZenConstantUshortExpr xus)
                return xus.Value;
            if (value is ZenConstantIntExpr xi)
                return xi.Value;
            if (value is ZenConstantUintExpr xui)
                return xui.Value;
            if (value is ZenConstantLongExpr xl)
                return xl.Value;
            if (value is ZenConstantUlongExpr xul)
                return (long?)xul.Value;
            return null;
        }

        /// <summary>
        /// Get a constant string value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The Zen string value.</param>
        /// <returns>A string.</returns>
        public static string GetConstantString<T>(Zen<T> value)
        {
            if (value is ZenConstantStringExpr xs)
            {
                return xs.UnescapedValue;
            }

            return null;
        }

        /// <summary>
        /// Create a constant Zen integer value.
        /// </summary>
        /// <typeparam name="T">The integer gype.</typeparam>
        /// <param name="value">The Zen integer value.</param>
        /// <returns>A long.</returns>
        public static Zen<T> CreateConstantIntegerValue<T>(long value)
        {
            var type = typeof(T);

            if (type == BoolType)
                return (Zen<T>)(object)(value == 0L ? False() : True());
            if (type == ByteType)
                return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)value);
            if (type == ShortType)
                return (Zen<T>)(object)ZenConstantShortExpr.Create((short)value);
            if (type == UshortType)
                return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)value);
            if (type == IntType)
                return (Zen<T>)(object)ZenConstantIntExpr.Create((int)value);
            if (type == UintType)
                return (Zen<T>)(object)ZenConstantUintExpr.Create((uint)value);
            if (type == LongType)
                return (Zen<T>)(object)ZenConstantLongExpr.Create(value);

            return (Zen<T>)(object)ZenConstantUlongExpr.Create((ulong)value);
        }

        /// <summary>
        /// Create a constant Zen string value.
        /// </summary>
        /// <param name="value">The Zen string value.</param>
        /// <returns>A string.</returns>
        public static Zen<string> CreateConstantString(string value)
        {
            return (Zen<string>)(object)ZenConstantStringExpr.Create(value);
        }

        /// <summary>
        /// Specialize a long to a particular type.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static object Specialize<T>(long value)
        {
            var type = typeof(T);

            if (type == ByteType)
                return (byte)value;
            if (type == ShortType)
                return (short)value;
            if (type == UshortType)
                return (ushort)value;
            if (type == IntType)
                return (int)value;
            if (type == UintType)
                return (uint)value;
            if (type == LongType)
                return (long)value;
            return (ulong)value;
        }

        /// <summary>
        /// Specialize a long to a particular type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static long ToLong(object value)
        {
            var type = value.GetType();

            if (type == ByteType)
                return (byte)value;
            if (type == ShortType)
                return (short)value;
            if (type == UshortType)
                return (ushort)value;
            if (type == IntType)
                return (int)value;
            if (type == UintType)
                return (uint)value;
            if (type == LongType)
                return (long)value;
            return (long)(ulong)value;
        }
    }
}
