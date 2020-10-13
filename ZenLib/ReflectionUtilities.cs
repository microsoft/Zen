// <copyright file="ReflectionUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
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
        /// The type of big integer values.
        /// </summary>
        public readonly static Type BigIntType = typeof(BigInteger);

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
        /// Type of a fixed size integer.
        /// </summary>
        public readonly static Type IntNType = typeof(IntN<,>);

        /// <summary>
        /// The object creation method.
        /// </summary>
        public static MethodInfo CreateMethod = typeof(Language).GetMethod("Create");

        /// <summary>
        /// The zen constant list creation method.
        /// </summary>
        public static MethodInfo CreateZenListConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenListConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Cache of generic arguments.
        /// </summary>
        private static Dictionary<Type, Type[]> genericArgumentsCache = new Dictionary<Type, Type[]>();

        /// <summary>
        /// Cache for property infos.
        /// </summary>
        private static Dictionary<(Type, string), PropertyInfo> propertyCache = new Dictionary<(Type, string), PropertyInfo>();

        /// <summary>
        /// Cache for field infos.
        /// </summary>
        private static Dictionary<(Type, string), FieldInfo> fieldCache = new Dictionary<(Type, string), FieldInfo>();

        /// <summary>
        /// Cache for GetGenericTypeDefinition().
        /// </summary>
        private static Dictionary<Type, Type> genericDefinitionCache = new Dictionary<Type, Type>();

        /// <summary>
        /// Cache for method infos.
        /// </summary>
        public static Dictionary<(Type, string), MethodInfo> methodCache = new Dictionary<(Type, string), MethodInfo>();

        /// <summary>
        /// Get the generic arguments for a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The generic argument types.</returns>
        public static Type[] GetGenericArgumentsCached(this Type type)
        {
            if (genericArgumentsCache.TryGetValue(type, out var args))
            {
                return args;
            }

            var ret = type.GetGenericArguments();
            genericArgumentsCache[type] = ret;
            return ret;
        }

        /// <summary>
        /// Get a property from its name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The PropertyInfo object.</returns>
        public static PropertyInfo GetPropertyCached(this Type type, string propertyName)
        {
            var key = (type, propertyName);
            if (propertyCache.TryGetValue(key, out var propertyInfo))
            {
                return propertyInfo;
            }

            var ret = type.GetProperty(propertyName);
            propertyCache[key] = ret;
            return ret;
        }

        /// <summary>
        /// Get a field from its name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The FieldInfo object.</returns>
        public static FieldInfo GetFieldCached(this Type type, string fieldName)
        {
            var key = (type, fieldName);
            if (fieldCache.TryGetValue(key, out var fieldInfo))
            {
                return fieldInfo;
            }

            var ret = type.GetField(fieldName);
            fieldCache[key] = ret;
            return ret;
        }

        /// <summary>
        /// Gets the generic type definition.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The generic type.</returns>
        public static Type GetGenericTypeDefinitionCached(this Type type)
        {
            if (genericDefinitionCache.TryGetValue(type, out var genericType))
            {
                return genericType;
            }

            var ret = type.GetGenericTypeDefinition();
            genericDefinitionCache[type] = ret;
            return ret;
        }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">The method name.</param>
        /// <returns>The generic type.</returns>
        public static MethodInfo GetMethodCached(this Type type, string methodName)
        {
            var key = (type, methodName);
            if (methodCache.TryGetValue(key, out var methodInfo))
            {
                return methodInfo;
            }

            var ret = type.GetMethod(methodName);
            methodCache[key] = ret;
            return ret;
        }

        /// <summary>
        /// Check if a type is a kind of integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsIntegerType(Type type)
        {
            return IsUnsignedIntegerType(type) || IsSignedIntegerType(type) || IsBigIntegerType(type) || IsFixedIntegerType(type);
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
        /// Check if a type is a big integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsBigIntegerType(Type type)
        {
            return type == BigIntType;
        }

        /// <summary>
        /// Check if a type is a tuple type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsTupleType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == TupleType;
        }

        /// <summary>
        /// Check if a type is some kind of tuple type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsValueTupleType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == ValueTupleType;
        }

        /// <summary>
        /// Check if a type is some kind of tuple type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsSomeTupleType(Type type)
        {
            return IsTupleType(type) || IsValueTupleType(type);
        }

        /// <summary>
        /// Check if a type is some kind of tuple type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsOptionType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == OptionType;
        }

        /// <summary>
        /// Check if a type is an IList type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsIListType(Type type)
        {
            return type.IsInterface &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinitionCached() == IListType;
        }

        /// <summary>
        /// Check if a type is an IDictionary type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsIDictionaryType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinitionCached() == IDictType;
        }

        /// <summary>
        /// Check if a type is a List type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == ListType;
        }

        /// <summary>
        /// Check if a type is a Dictionary type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == DictType;
        }

        /// <summary>
        /// Check if a type is fixed width integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsFixedIntegerType(Type type)
        {
            return type.BaseType != null &&
                   type.BaseType.IsGenericType &&
                   type.BaseType.GetGenericTypeDefinitionCached() == IntNType;
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

            var propertyInfo = type.GetPropertyCached(fieldName);
            return (TField)propertyInfo.GetValue(obj);
        }

        /// <summary>
        /// Validates whether the field or property exists.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fieldType">The field type.</param>
        /// <param name="fieldOrPropertyName">The field or property name.</param>
        public static void ValidateFieldOrProperty(Type objectType, Type fieldType, string fieldOrPropertyName)
        {
            var p = objectType.GetPropertyCached(fieldOrPropertyName);

            if (p != null && p.PropertyType != fieldType)
            {
                throw new ZenException($"Field or property {fieldOrPropertyName} type mismatch with {fieldType} for object with type {objectType}.");
            }

            var f = objectType.GetFieldCached(fieldOrPropertyName);

            if (f != null && f.FieldType != fieldType)
            {
                throw new ZenException($"Field or property {fieldOrPropertyName} type mismatch with {fieldType} for object with type {objectType}.");
            }

            if (p == null && f == null)
            {
                throw new ZenException($"Invalid field or property {fieldOrPropertyName} for object with type {objectType}");
            }
        }

        /// <summary>
        /// Validates that a type is a Zen type.
        /// </summary>
        /// <param name="type">The object type.</param>
        public static void ValidateIsZenType(Type type)
        {
            if (!type.IsGenericType || typeof(Zen<>).IsAssignableFrom(type.GetGenericTypeDefinitionCached()))
            {
                throw new ZenException($"Attempting to use value of non-Zen type: {type}");
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
            var fieldInfo = type.GetFieldCached(fieldName);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, fieldValue);
                return;
            }

            var propertyInfo = type.GetPropertyCached(fieldName);
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
        /// Get a default value of a given type.
        /// </summary>
        /// <returns>Default value of that type.</returns>
        internal static T GetDefaultValue<T>()
        {
            return (T)GetDefaultValue(typeof(T));
        }

        private static object GetDefaultValue(Type type)
        {
            if (type == BoolType)
                return false;
            if (type == ByteType)
                return (byte)0;
            if (type == ShortType)
                return (short)0;
            if (type == UshortType)
                return (ushort)0;
            if (type == IntType)
                return 0;
            if (type == UintType)
                return 0U;
            if (type == LongType)
                return 0L;
            if (type == UlongType)
                return 0UL;
            if (type == BigIntType)
                return new BigInteger(0);
            if (type == StringType)
                return string.Empty;
            if (IsFixedIntegerType(type))
                return type.GetConstructor(new Type[] { typeof(long) }).Invoke(new object[] { 0L });

            if (IsOptionType(type))
            {
                var innerType = type.GetGenericArgumentsCached()[0];
                return typeof(Option)
                    .GetMethodCached("None")
                    .MakeGenericMethod(innerType).Invoke(null, CommonUtilities.EmptyArray);
            }

            if (IsTupleType(type) || IsValueTupleType(type))
            {
                var args = type.GetGenericArgumentsCached();
                var innerType1 = args[0];
                var innerType2 = args[1];
                var c = type.GetConstructor(new Type[] { innerType1, innerType2 });
                return c.Invoke(new object[] { GetDefaultValue(innerType1), GetDefaultValue(innerType2) });
            }

            if (IsIListType(type))
            {
                var innerType = type.GetGenericArgumentsCached()[0];
                var c = ListType.MakeGenericType(innerType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }

            if (IsIDictionaryType(type))
            {
                var args = type.GetGenericArgumentsCached();
                var keyType = args[0];
                var valueType = args[1];
                var c = DictType.MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }

            // some class or struct

            var fields = new SortedDictionary<string, object>();

            foreach (var field in GetAllFields(type))
            {
                fields[field.Name] = GetDefaultValue(field.FieldType);
            }

            foreach (var property in GetAllProperties(type))
            {
                fields[property.Name] = GetDefaultValue(property.PropertyType);
            }

            return CreateInstance(type, fields.Keys.ToArray(), fields.Values.ToArray());
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
            if (type == BigIntType)
                return visitor.VisitBigInteger();
            if (type == StringType)
                return visitor.VisitString();
            if (IsFixedIntegerType(type))
                return visitor.VisitFixedInteger(type);

            if (IsOptionType(type))
            {
                var t = type.GetGenericArgumentsCached()[0];
                return visitor.VisitOption(ty => ApplyTypeVisitor(visitor, ty), type, t);
            }

            if (IsTupleType(type))
            {
                var args = type.GetGenericArgumentsCached();
                var tleft = args[0];
                var tright = args[1];
                return visitor.VisitTuple(ty => ApplyTypeVisitor(visitor, ty), type, tleft, tright);
            }

            if (IsValueTupleType(type))
            {
                var args = type.GetGenericArgumentsCached();
                var tleft = args[0];
                var tright = args[1];
                return visitor.VisitValueTuple(ty => ApplyTypeVisitor(visitor, ty), type, tleft, tright);
            }

            if (IsIListType(type))
            {
                var t = type.GetGenericArgumentsCached()[0];
                return visitor.VisitList(ty => ApplyTypeVisitor(visitor, ty), type, t);
            }

            if (IsIDictionaryType(type))
            {
                var args = type.GetGenericArgumentsCached();
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
                return Language.Some<T>(value.Value);
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

            if (value is bool || value is byte || value is short || value is ushort || value is int ||
                value is uint || value is long || value is ulong || value is string || value is BigInteger ||
                IsFixedIntegerType(type))
            {
                return Constant(v);
            }

            if (IsOptionType(type))
                return CreateZenOptionConstant(v);
            if (IsTupleType(type))
                return Tuple(CreateZenConstant(v.Item1), CreateZenConstant(v.Item2));
            if (IsValueTupleType(type))
                return ValueTuple(CreateZenConstant(v.Item1), CreateZenConstant(v.Item2));

            var typeArgs = type.GetGenericArgumentsCached();

            if (type.IsGenericType && IListType.MakeGenericType(typeArgs[0]).IsAssignableFrom(type))
            {
                var innerType = typeArgs[0];
                return CreateZenListConstantMethod.MakeGenericMethod(innerType).Invoke(null, new object[] { value });
            }

            if (type.IsGenericType && IDictType.MakeGenericType(typeArgs[0], typeArgs[1]).IsAssignableFrom(type))
            {
                return CreateZenDictConstant(v);
            }

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
            if (value is ZenConstantExpr<byte> xb)
                return xb.Value;
            if (value is ZenConstantExpr<short> xs)
                return xs.Value;
            if (value is ZenConstantExpr<ushort> xus)
                return xus.Value;
            if (value is ZenConstantExpr<int> xi)
                return xi.Value;
            if (value is ZenConstantExpr<uint> xui)
                return xui.Value;
            if (value is ZenConstantExpr<long> xl)
                return xl.Value;
            if (value is ZenConstantExpr<ulong> xul)
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
            if (value is ZenConstantExpr<string> xs)
            {
                return xs.Value;
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
                return (Zen<T>)(object)ZenConstantExpr<byte>.Create((byte)value);
            if (type == ShortType)
                return (Zen<T>)(object)ZenConstantExpr<short>.Create((short)value);
            if (type == UshortType)
                return (Zen<T>)(object)ZenConstantExpr<ushort>.Create((ushort)value);
            if (type == IntType)
                return (Zen<T>)(object)ZenConstantExpr<int>.Create((int)value);
            if (type == UintType)
                return (Zen<T>)(object)ZenConstantExpr<uint>.Create((uint)value);
            if (type == LongType)
                return (Zen<T>)(object)ZenConstantExpr<long>.Create(value);

            return (Zen<T>)(object)ZenConstantExpr<ulong>.Create((ulong)value);
        }

        /// <summary>
        /// Create a constant Zen string value.
        /// </summary>
        /// <param name="value">The Zen string value.</param>
        /// <returns>A string.</returns>
        public static Zen<string> CreateConstantString(string value)
        {
            return (Zen<string>)(object)ZenConstantExpr<string>.Create(value);
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
