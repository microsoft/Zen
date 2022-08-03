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
    using static ZenLib.Zen;

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
        /// The type of byte sequences values.
        /// </summary>
        public readonly static Type UnicodeSequenceType = typeof(Seq<char>);

        /// <summary>
        /// The type of finite string values.
        /// </summary>
        public readonly static Type FiniteStringType = typeof(FString);

        /// <summary>
        /// The type of set unit values.
        /// </summary>
        public readonly static Type SetUnitType = typeof(SetUnit);

        /// <summary>
        /// The type of bool values.
        /// </summary>
        public readonly static Type BoolType = typeof(bool);

        /// <summary>
        /// The type of byte values.
        /// </summary>
        public readonly static Type ByteType = typeof(byte);

        /// <summary>
        /// The type of char values.
        /// </summary>
        public readonly static Type CharType = typeof(char);

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
        /// The type of real values.
        /// </summary>
        public readonly static Type RealType = typeof(Real);

        /// <summary>
        /// The type of seq values.
        /// </summary>
        public readonly static Type SeqType = typeof(Seq<>);

        /// <summary>
        /// The type of map values.
        /// </summary>
        public readonly static Type MapType = typeof(Map<,>);

        /// <summary>
        /// The type of map values.
        /// </summary>
        public readonly static Type ConstMapType = typeof(ConstMap<,>);

        /// <summary>
        /// The type of fseq values.
        /// </summary>
        public readonly static Type FSeqType = typeof(FSeq<>);

        /// <summary>
        /// Type of a fixed size integer.
        /// </summary>
        public readonly static Type IntNType = typeof(IntN<,>);

        /// <summary>
        /// The object creation method.
        /// </summary>
        public static MethodInfo CreateMethod = typeof(Zen).GetMethod("Create");

        /// <summary>
        /// The zen constant list creation method.
        /// </summary>
        public static MethodInfo CreateZenSeqConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenSeqConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// The zen constant list creation method.
        /// </summary>
        public static MethodInfo CreateZenListConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenListConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// The zen constant map creation method.
        /// </summary>
        public static MethodInfo CreateZenMapConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenMapConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// The zen constant map creation method.
        /// </summary>
        public static MethodInfo CreateZenConstMapConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenConstMapConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// The zen constant option creation method.
        /// </summary>
        public static MethodInfo CreateZenOptionConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenOptionConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// The zen constant tuple creation method.
        /// </summary>
        public static MethodInfo CreateZenTupleConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenTupleConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// The zen constant value tuple creation method.
        /// </summary>
        public static MethodInfo CreateZenValueTupleConstantMethod =
            typeof(ReflectionUtilities).GetMethod("CreateZenValueTupleConstant", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Cache of generic arguments.
        /// </summary>
        private static Dictionary<Type, Type[]> genericArgumentsCache = new Dictionary<Type, Type[]>();

        /// <summary>
        /// Generic arguments cache lock.
        /// </summary>
        private static object genericArgumentsCacheLock = new object();

        /// <summary>
        /// Cache for property infos.
        /// </summary>
        private static Dictionary<(Type, string), PropertyInfo> propertyCache = new Dictionary<(Type, string), PropertyInfo>();

        /// <summary>
        /// Property cache lock.
        /// </summary>
        private static object propertyCacheLock = new object();

        /// <summary>
        /// Cache for field infos.
        /// </summary>
        private static Dictionary<(Type, string), FieldInfo> fieldCache = new Dictionary<(Type, string), FieldInfo>();

        /// <summary>
        /// Field cache lock.
        /// </summary>
        private static object fieldCacheLock = new object();

        /// <summary>
        /// Cache for GetGenericTypeDefinition().
        /// </summary>
        private static Dictionary<Type, Type> genericDefinitionCache = new Dictionary<Type, Type>();

        /// <summary>
        /// Generic definition cache lock.
        /// </summary>
        private static object genericDefinitionCacheLock = new object();

        /// <summary>
        /// Cache for method infos.
        /// </summary>
        public static Dictionary<(Type, string), MethodInfo> methodCache = new Dictionary<(Type, string), MethodInfo>();

        /// <summary>
        /// Method cache lock.
        /// </summary>
        private static object methodCacheLock = new object();

        /// <summary>
        /// Get the generic arguments for a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The generic argument types.</returns>
        public static Type[] GetGenericArgumentsCached(this Type type)
        {
            lock (genericArgumentsCacheLock)
            {
                if (genericArgumentsCache.TryGetValue(type, out var args))
                {
                    return args;
                }

                var ret = type.GetGenericArguments();
                genericArgumentsCache[type] = ret;
                return ret;
            }
        }

        /// <summary>
        /// Get a property from its name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The PropertyInfo object.</returns>
        public static PropertyInfo GetPropertyCached(this Type type, string propertyName)
        {
            lock (propertyCacheLock)
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
        }

        /// <summary>
        /// Get a field from its name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The FieldInfo object.</returns>
        public static FieldInfo GetFieldCached(this Type type, string fieldName)
        {
            lock (fieldCacheLock)
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
        }

        /// <summary>
        /// Gets the generic type definition.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The generic type.</returns>
        public static Type GetGenericTypeDefinitionCached(this Type type)
        {
            lock (genericDefinitionCacheLock)
            {
                if (genericDefinitionCache.TryGetValue(type, out var genericType))
                {
                    return genericType;
                }

                var ret = type.GetGenericTypeDefinition();
                genericDefinitionCache[type] = ret;
                return ret;
            }
        }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">The method name.</param>
        /// <returns>The generic type.</returns>
        public static MethodInfo GetMethodCached(this Type type, string methodName)
        {
            lock (methodCacheLock)
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
        }

        /// <summary>
        /// Check if a type is a kind of integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsArithmeticType(Type type)
        {
            return IsFiniteIntegerType(type) || IsBigIntegerType(type) || IsRealType(type);
        }

        /// <summary>
        /// Check if a type is a kind of finite integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsFiniteIntegerType(Type type)
        {
            return IsUnsignedIntegerType(type) || IsSignedIntegerType(type) || IsFixedIntegerType(type);
        }

        /// <summary>
        /// Check if a type is a kind of unsigned integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsUnsignedIntegerType(Type type)
        {
            if (type == ByteType || type == UshortType || type == UintType || type == UlongType)
            {
                return true;
            }
            else if (IsFixedIntegerType(type))
            {
                var signed = type.BaseType.GetGenericArgumentsCached()[1];
                return signed == typeof(Unsigned);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a type is a kind of signed integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsSignedIntegerType(Type type)
        {
            if (type == ShortType || type == IntType || type == LongType)
            {
                return true;
            }
            else if (IsFixedIntegerType(type))
            {
                var signed = type.BaseType.GetGenericTypeDefinitionCached().GetGenericArgumentsCached()[1];
                return signed == typeof(Signed);
            }
            else
            {
                return false;
            }
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
        /// Check if a type is a real.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsRealType(Type type)
        {
            return type == RealType;
        }

        /// <summary>
        /// Check if a type is a Seq type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsSeqType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == SeqType;
        }

        /// <summary>
        /// Check if a type is a Map type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsMapType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == MapType;
        }

        /// <summary>
        /// Check if a type is a ConstMap type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsConstMapType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == ConstMapType;
        }

        /// <summary>
        /// Check if a type is a FSeq type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsFSeqType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinitionCached() == FSeqType;
        }

        /// <summary>
        /// Check if a type is a Zen type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsZenType(Type type)
        {
            return type.IsGenericType && !typeof(Zen<>).IsAssignableFrom(type.GetGenericTypeDefinitionCached());
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
            if (!IsZenType(type))
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
            if (propertyInfo == null)
            {
                throw new ZenException($"Unable to set field or property {fieldName} for an object of type {type}.");
            }

            try
            {
                propertyInfo.SetValue(obj, fieldValue);
            }
            catch (ArgumentException)
            {
                throw new ZenException($"Unable to set field or property {propertyInfo.Name} for an object of type {type}");
            }
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
            // first try to find if there is a constructor with parameter names matching the fields.
            // this helps in some cases and for F# where constructors are generated for records.
            foreach (var c in type.GetConstructors())
            {
                var parameters = FindMatchingParameters(c, fieldNames, values);
                if (parameters != null)
                {
                    return Activator.CreateInstance(type, parameters);
                }
            }

            // otherwise, use a default constructor then set the fields.
            var obj = Activator.CreateInstance(type);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];
                var value = values[i];
                SetFieldOrProperty(obj, fieldName, value);
            }

            return obj;
        }

        private static object[] FindMatchingParameters(ConstructorInfo c, string[] fieldNames, object[] values)
        {
            var parameters = c.GetParameters();
            if (parameters.Length != values.Length)
            {
                return null;
            }

            var result = new object[values.Length];
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var fieldName = fieldNames[i];
                var value = values[i];

                var parameterIndex = FindMatchingParamter(parameters, fieldName, value.GetType());
                if (parameterIndex < 0)
                {
                    return null;
                }

                // matched the same parameter twice.
                if (result[parameterIndex] != null)
                {
                    return null;
                }

                result[parameterIndex] = values[i];
            }

            return result;
        }

        private static int FindMatchingParamter(ParameterInfo[] parameters, string fieldName, Type fieldType)
        {
            for (int j = 0; j < parameters.Length; j++)
            {
                var parameterNameMatchesField = string.Compare(parameters[j].Name, fieldName, true) == 0;

                if (parameterNameMatchesField && parameters[j].ParameterType == fieldType)
                {
                    return j;
                }
            }

            return -1;
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
            if (type == CharType)
                return char.MinValue;
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
            if (type == RealType)
                return new Real(0, 1);
            if (type == StringType)
                return string.Empty;
            if (IsFixedIntegerType(type))
                return type.GetConstructor(new Type[] { typeof(long) }).Invoke(new object[] { 0L });

            if (IsSeqType(type))
            {
                var innerType = type.GetGenericArgumentsCached()[0];
                var c = SeqType.MakeGenericType(innerType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }

            if (IsMapType(type))
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                var c = MapType.MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }

            if (IsConstMapType(type))
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                var c = ConstMapType.MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }

            if (IsFSeqType(type))
            {
                var c = type.GetConstructor(new Type[] { });
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
        /// <param name="parameter">The parameter.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <typeparam name="TParam">The parameter type.</typeparam>
        /// <returns>A value.</returns>
        internal static T ApplyTypeVisitor<T, TParam>(ITypeVisitor<T, TParam> visitor, Type type, TParam parameter)
        {
            if (type == BoolType)
                return visitor.VisitBool(parameter);
            if (type == ByteType)
                return visitor.VisitByte(parameter);
            if (type == CharType)
                return visitor.VisitChar(parameter);
            if (type == ShortType)
                return visitor.VisitShort(parameter);
            if (type == UshortType)
                return visitor.VisitUshort(parameter);
            if (type == IntType)
                return visitor.VisitInt(parameter);
            if (type == UintType)
                return visitor.VisitUint(parameter);
            if (type == LongType)
                return visitor.VisitLong(parameter);
            if (type == UlongType)
                return visitor.VisitUlong(parameter);
            if (type == BigIntType)
                return visitor.VisitBigInteger(parameter);
            if (type == RealType)
                return visitor.VisitReal(parameter);
            if (type == StringType)
                return visitor.VisitString(parameter);
            if (IsFixedIntegerType(type))
                return visitor.VisitFixedInteger(type, parameter);

            if (IsSeqType(type))
            {
                var t = type.GetGenericArgumentsCached()[0];
                return visitor.VisitSeq(type, t, parameter);
            }

            if (IsMapType(type))
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                return visitor.VisitMap(type, keyType, valueType, parameter);
            }

            if (IsConstMapType(type))
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                return visitor.VisitConstMap(type, keyType, valueType, parameter);
            }

            if (IsFSeqType(type))
            {
                var t = type.GetGenericArgumentsCached()[0];
                return visitor.VisitList(type, t, parameter);
            }

            // some class or struct
            var dict = GetAllFieldAndPropertyTypes(type);
            return visitor.VisitObject(type, dict, parameter);
        }

        /// <summary>
        /// Gets all fields and their types for a given class/struct type.
        /// </summary>
        /// <param name="type">The class or struct type.</param>
        /// <returns>A sorted dictionary of field names to types.</returns>
        public static SortedDictionary<string, Type> GetAllFieldAndPropertyTypes(Type type)
        {
            var fieldTypes = new SortedDictionary<string, Type>();

            foreach (var field in GetAllFields(type))
            {
                fieldTypes[field.Name] = field.FieldType;
            }

            foreach (var property in GetAllProperties(type))
            {
                fieldTypes[property.Name] = property.PropertyType;
            }

            return fieldTypes;
        }

        /// <summary>
        /// Create a constant Zen Seq value.
        /// </summary>
        /// <param name="value">The seq value.</param>
        /// <returns>The Zen value representing the seq.</returns>
        internal static Zen<Seq<T>> CreateZenSeqConstant<T>(Seq<T> value)
        {
            if (typeof(T) == typeof(char))
            {
                return ZenConstantExpr<Seq<T>>.Create(value);
            }

            Zen<Seq<T>> seq = ZenSeqEmptyExpr<T>.Instance;
            foreach (var elt in value.Values)
            {
                seq = ZenSeqConcatExpr<T>.Create(seq, ZenSeqUnitExpr<T>.Create(elt));
            }

            return seq;
        }

        /// <summary>
        /// Create a constant Zen list value.
        /// </summary>
        /// <param name="value">The list value.</param>
        /// <returns>The Zen value representing the list.</returns>
        internal static Zen<FSeq<T>> CreateZenListConstant<T>(FSeq<T> value)
        {
            Zen<FSeq<T>> list = ZenListEmptyExpr<T>.Instance;
            foreach (var elt in value.Values.Reverse())
            {
                ReportIfNullConversionError(elt, "element", typeof(FSeq<T>));
                list = ZenListAddFrontExpr<T>.Create(list, elt);
            }

            return list;
        }

        /// <summary>
        /// Create a constant Zen list value.
        /// </summary>
        /// <param name="value">The list value.</param>
        /// <returns>The Zen value representing the list.</returns>
        internal static Zen<Map<TKey, TValue>> CreateZenMapConstant<TKey, TValue>(Map<TKey, TValue> value)
        {
            Zen<Map<TKey, TValue>> map = ZenMapEmptyExpr<TKey, TValue>.Instance;
            foreach (var elt in value.Values)
            {
                ReportIfNullConversionError(elt, "element", typeof(Map<TKey, TValue>));
                map = ZenMapSetExpr<TKey, TValue>.Create(map, elt.Key, elt.Value);
            }

            return map;
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
                ReportIfNullConversionError(value.Value, "Value", typeof(T));
                return Option.Create((Zen<T>)CreateZenConstant<T>(value.Value));
            }

            return Option.Null<T>();
        }

        /// <summary>
        /// Create a constant Zen value.
        /// </summary>
        /// <param name="value">The type.</param>
        /// <returns>The Zen value representing the constant.</returns>
        internal static object CreateZenConstant<T>(T value)
        {
            try
            {
                var type = typeof(T);

                if (value is bool || value is byte || value is char || value is short || value is ushort ||
                    value is int  || value is uint || value is long || value is ulong || value is BigInteger ||
                    value is Real || IsFixedIntegerType(type) || IsConstMapType(type))
                {
                    return ZenConstantExpr<T>.Create(value);
                }

                var typeArgs = type.GetGenericArgumentsCached();

                if (type == StringType)
                {
                    var asSeq = (Zen<Seq<char>>)CreateZenConstant(Seq.FromString((string)(object)value));
                    return ZenCastExpr<Seq<char>, string>.Create(asSeq);
                }

                if (IsSeqType(type))
                {
                    var innerType = typeArgs[0];
                    return CreateZenSeqConstantMethod.MakeGenericMethod(innerType).Invoke(null, new object[] { value });
                }

                if (IsFSeqType(type))
                {
                    var innerType = typeArgs[0];
                    return CreateZenListConstantMethod.MakeGenericMethod(innerType).Invoke(null, new object[] { value });
                }

                if (IsMapType(type))
                {
                    var keyType = typeArgs[0];
                    var valueType = typeArgs[1];
                    return CreateZenMapConstantMethod.MakeGenericMethod(keyType, valueType).Invoke(null, new object[] { value });
                }

                // option type, we need this separate from classes/structs
                // because options may create null values for the None case
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>))
                {
                    var innerType = typeArgs[0];
                    return CreateZenOptionConstantMethod.MakeGenericMethod(innerType).Invoke(null, new object[] { value });
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
                    var fieldValue = asList[i].Value;
                    ReportIfNullConversionError(fieldValue, "field", type);
                    args[i] = (asList[i].Key, CreateZenConstant(fieldValue));
                }

                return createMethod.Invoke(null, new object[] { args });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        ///     Reports a null error in a conversion from a constant to a Zen value.
        /// </summary>
        /// <param name="obj">The object that may be null.</param>
        /// <param name="where">Description of where the error occurs.</param>
        /// <param name="type">The containing type.</param>
        private static void ReportIfNullConversionError(object obj, string where, Type type)
        {
            if (obj is null)
            {
                throw new ZenException($"Null constant in {where} of type {type} can not be converted to a Zen value.");
            }
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
            Contract.Assert(type == UlongType);
            return (Zen<T>)(object)ZenConstantExpr<ulong>.Create((ulong)value);
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
            Contract.Assert(type == UlongType);
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
            Contract.Assert(type == UlongType);
            return (long)(ulong)value;
        }

        /// <summary>
        /// Gets the size of a finite integer type.
        /// </summary>
        /// <typeparam name="T">The integer type.</typeparam>
        /// <returns>The bitwidth size of the integer.</returns>
        public static int GetFiniteIntegerSize<T>()
        {
            var type = typeof(T);

            if (type == ByteType)
                return 8;
            if (type == ShortType || type == UshortType)
                return 16;
            if (type == IntType || type == UintType)
                return 32;
            if (type == LongType || type == UlongType)
                return 64;

            Contract.Assert(IsFixedIntegerType(type));
            var c = type.GetConstructor(new Type[] { typeof(long) });
            dynamic obj = (T)c.Invoke(new object[] { 0L });
            return obj.Size;
        }

        /// <summary>
        /// Cast one finite integer to another finite integer type.
        /// </summary>
        /// <param name="x">The source finite integer.</param>
        /// <returns>The resulting finite integer.</returns>
        public static TTarget CastFiniteInteger<TSource, TTarget>(TSource x)
        {
            var b1 = IsFixedIntegerType(typeof(TSource));
            var b2 = IsFixedIntegerType(typeof(TTarget));

            if (!b1 && !b2)
            {
                return (TTarget)(dynamic)x;
            }
            else if (b1 && !b2)
            {
                var result = 0L;
                byte[] bytes = ((dynamic)x).Bytes;
                for (int i = 0; i < Math.Min(bytes.Length, 4); i++)
                {
                    result <<= 8;
                    result |= bytes[bytes.Length - 1 - i];
                }

                return (TTarget)(dynamic)result;
            }
            else
            {
                byte[] bytes;
                if (b1)
                {
                    bytes = ((dynamic)x).Bytes;
                }
                else
                {
                    bytes = BitConverter.GetBytes((long)(dynamic)x);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                }

                var c = typeof(TTarget).GetConstructor(new Type[] { typeof(byte[]) });
                return (TTarget)c.Invoke(new object[] { bytes });
            }
        }

        /// <summary>
        /// Gets the minimum value for a type.
        /// This is a workaround for lack of integer interfaces.
        /// </summary>
        /// <typeparam name="T">The integer type.</typeparam>
        /// <returns></returns>
        public static T MinValue<T>()
        {
            var type = typeof(T);

            if (type == ByteType)
                return (T)(object)byte.MinValue;
            if (type == typeof(char))
                return (T)(object)char.MinValue;
            if (type == ShortType)
                return (T)(object)short.MinValue;
            if (type == UshortType)
                return (T)(object)ushort.MinValue;
            if (type == IntType)
                return (T)(object)int.MinValue;
            if (type == UintType)
                return (T)(object)uint.MinValue;
            if (type == LongType)
                return (T)(object)long.MinValue;
            if (type == UlongType)
                return (T)(object)ulong.MinValue;

            Contract.Assert(IsFixedIntegerType(type));
            var c = type.GetConstructor(new Type[] { typeof(long) });
            dynamic obj = (T)c.Invoke(new object[] { 0L });
            obj.SetBit(0, obj.Signed);
            return obj;
        }

        /// <summary>
        /// Gets the maximum value for a type.
        /// This is a workaround for lack of integer interfaces.
        /// </summary>
        /// <typeparam name="T">The integer type.</typeparam>
        /// <returns></returns>
        public static T MaxValue<T>()
        {
            var type = typeof(T);

            if (type == ByteType)
                return (T)(object)byte.MaxValue;
            if (type == typeof(char))
                return (T)(object)char.MaxValue;
            if (type == ShortType)
                return (T)(object)short.MaxValue;
            if (type == UshortType)
                return (T)(object)ushort.MaxValue;
            if (type == IntType)
                return (T)(object)int.MaxValue;
            if (type == UintType)
                return (T)(object)uint.MaxValue;
            if (type == LongType)
                return (T)(object)long.MaxValue;
            if (type == UlongType)
                return (T)(object)ulong.MaxValue;

            Contract.Assert(IsFixedIntegerType(type));
            var c = type.GetConstructor(new Type[] { typeof(long) });
            dynamic obj = (T)c.Invoke(new object[] { 0L });
            obj.SetBit(0, !obj.Signed);
            for (int i = 1; i < obj.Size; i++)
            {
                obj.SetBit(i, true);
            }

            return obj;
        }

        public static byte Add(byte x, int i)
        {
            return (byte)(x + i);
        }

        public static char Add(char x, int i)
        {
            return (char)(x + i);
        }

        public static short Add(short x, int i)
        {
            return (short)(x + i);
        }

        public static ushort Add(ushort x, int i)
        {
            return (ushort)(x + i);
        }

        public static int Add(int x, int i)
        {
            return (int)(x + i);
        }

        public static uint Add(uint x, int i)
        {
            return (uint)(x + i);
        }

        public static long Add(long x, int i)
        {
            return (long)(x + i);
        }

        public static ulong Add(ulong x, int i)
        {
            return (ulong)(x + (ulong)i);
        }

        public static T Add<T, TSign>(IntN<T, TSign> x, int i)
        {
            var c = typeof(T).GetConstructor(new Type[] { typeof(long) });
            var instance = (IntN<T, TSign>)c.Invoke(new object[] { (long)i });
            return x.Add(instance);
        }

        public static byte Subtract(byte x, int i)
        {
            return (byte)(x - i);
        }

        public static char Subtract(char x, int i)
        {
            return (char)(x - i);
        }

        public static short Subtract(short x, int i)
        {
            return (short)(x - i);
        }

        public static ushort Subtract(ushort x, int i)
        {
            return (ushort)(x - i);
        }

        public static int Subtract(int x, int i)
        {
            return (int)(x - i);
        }

        public static uint Subtract(uint x, int i)
        {
            return (uint)(x - i);
        }

        public static long Subtract(long x, int i)
        {
            return (long)(x - i);
        }

        public static ulong Subtract(ulong x, int i)
        {
            return (ulong)(x - (ulong)i);
        }

        public static T Subtract<T, TSign>(IntN<T, TSign> x, int i)
        {
            var c = typeof(T).GetConstructor(new Type[] { typeof(long) });
            var instance = (IntN<T, TSign>)c.Invoke(new object[] { (long)i });
            return x.Subtract(instance);
        }
    }
}
