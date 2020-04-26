// <copyright file="ReflectionUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A collection of helper functions for manipulating Zen
    /// objects through C#'s reflection runtime.
    /// </summary>
    public static class ReflectionUtilities
    {
        /// <summary>
        /// The type of string values.
        /// </summary>
        public readonly static Type StringType = typeof(string);

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
            var type = typeof(T);
            var obj = Activator.CreateInstance(type);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];
                var value = values[i];
                SetFieldOrProperty(obj, fieldName, value);
            }

            return (T)obj;
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

            /* if (type == typeof(string))
            {
                return string.Copy((string)obj);
            } */

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
        public static T ApplyTypeVisitor<T>(ITypeVisitor<T> visitor, Type type)
        {
            if (type == BoolType)
            {
                return visitor.VisitBool();
            }

            if (type == ByteType)
            {
                return visitor.VisitByte();
            }

            if (type == ShortType)
            {
                return visitor.VisitShort();
            }

            if (type == UshortType)
            {
                return visitor.VisitUshort();
            }

            if (type == IntType)
            {
                return visitor.VisitInt();
            }

            if (type == UintType)
            {
                return visitor.VisitUint();
            }

            if (type == LongType)
            {
                return visitor.VisitLong();
            }

            if (type == UlongType)
            {
                return visitor.VisitUlong();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>))
            {
                var t = type.GetGenericArguments()[0];
                return visitor.VisitOption(ty => ApplyTypeVisitor(visitor, ty), t);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                var tleft = type.GetGenericArguments()[0];
                var tright = type.GetGenericArguments()[1];
                return visitor.VisitTuple(ty => ApplyTypeVisitor(visitor, ty), tleft, tright);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                var tleft = type.GetGenericArguments()[0];
                var tright = type.GetGenericArguments()[1];
                return visitor.VisitValueTuple(ty => ApplyTypeVisitor(visitor, ty), tleft, tright);
            }

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var t = type.GetGenericArguments()[0];
                return visitor.VisitList(ty => ApplyTypeVisitor(visitor, ty), type, t);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var args = type.GetGenericArguments();
                var tkey = args[0];
                var tvalue = args[1];
                return visitor.VisitDictionary(ty => ApplyTypeVisitor(visitor, ty), type, tkey, tvalue);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                throw new InvalidOperationException($"Unsupported object field type: {type}, use IList");
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                throw new InvalidOperationException($"Unsupported object field type: {type}, use IDictionary");
            }

            // some class or struct
            var dict = new Dictionary<string, Type>();

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
    }
}
