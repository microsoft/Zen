// <copyright file="ReflectionUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.ZenLanguage;

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
        public readonly static Type ConstMapType = typeof(CMap<,>);

        /// <summary>
        /// The type of fseq values.
        /// </summary>
        public readonly static Type FSeqType = typeof(FSeq<>);

        /// <summary>
        /// Type of a fixed size integer.
        /// </summary>
        public readonly static Type IntNType = typeof(IntN<,>);

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
        /// Check if a type is a primitive integer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True or false.</returns>
        public static bool IsPrimitiveIntegerType(Type type)
        {
            return (type == ByteType || type == ShortType || type == UshortType || type == IntType ||
                    type == UintType || type == LongType || type == UlongType);
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
        /// Validate that a type is a regex char type.
        /// </summary>
        /// <param name="type">The type.</param>
        public static bool IsRegexCharType(Type type)
        {
            return (IsFiniteIntegerType(type) || type == typeof(char));
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

        /// <summary>
        /// Find matching parameters for a constructor.
        /// </summary>
        /// <param name="c">The constructor.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <param name="values">The field values.</param>
        /// <returns>The parameter match.</returns>
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

                var parameterIndex = FindMatchingParameter(parameters, fieldName, value.GetType());
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

        /// <summary>
        /// Find matching parameter.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldType">The field type.</param>
        /// <returns>The index or -1 if none.</returns>
        private static int FindMatchingParameter(ParameterInfo[] parameters, string fieldName, Type fieldType)
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
            return (T)new ZenDefaultValueVisitor().Visit(typeof(T), new Unit());
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
        /// Create a constant Zen value.
        /// </summary>
        /// <param name="value">The type.</param>
        /// <returns>The Zen value representing the constant.</returns>
        internal static object CreateZenConstant<T>(T value)
        {
            try
            {
                return new ZenLiftingVisitor().Visit(typeof(T), value);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
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
            if (IsPrimitiveIntegerType(typeof(T)) && value is ZenConstantExpr<T> x)
            {
                return (long)(dynamic)x.Value;
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
            return (Zen<T>)(object)ZenConstantExpr<T>.Create((T)FromLong<T>(value));
        }

        /// <summary>
        /// Specialize a long to a particular type.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static object FromLong<T>(long value)
        {
            var type = typeof(T);
            if (type == BoolType)
                return value == 0L ? false : true;
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
        /// Gets the minimum value for a type.
        /// This is a workaround for lack of integer interfaces.
        /// </summary>
        /// <typeparam name="T">The integer type.</typeparam>
        /// <returns>The minimum value.</returns>
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
        /// <returns>The maximum value.</returns>
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

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static byte Add(byte x, int i)
        {
            return (byte)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static char Add(char x, int i)
        {
            return (char)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static short Add(short x, int i)
        {
            return (short)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static ushort Add(ushort x, int i)
        {
            return (ushort)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static int Add(int x, int i)
        {
            return (int)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static uint Add(uint x, int i)
        {
            return (uint)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static long Add(long x, int i)
        {
            return (long)(x + i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static ulong Add(ulong x, int i)
        {
            return (ulong)(x + (ulong)i);
        }

        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static T Add<T, TSign>(IntN<T, TSign> x, int i)
        {
            var c = typeof(T).GetConstructor(new Type[] { typeof(long) });
            var instance = (IntN<T, TSign>)c.Invoke(new object[] { (long)i });
            return x.Add(instance);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static byte Subtract(byte x, int i)
        {
            return (byte)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static char Subtract(char x, int i)
        {
            return (char)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static short Subtract(short x, int i)
        {
            return (short)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static ushort Subtract(ushort x, int i)
        {
            return (ushort)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static int Subtract(int x, int i)
        {
            return (int)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static uint Subtract(uint x, int i)
        {
            return (uint)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static long Subtract(long x, int i)
        {
            return (long)(x - i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static ulong Subtract(ulong x, int i)
        {
            return (ulong)(x - (ulong)i);
        }

        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="i">The second value.</param>
        /// <returns>The result.</returns>
        public static T Subtract<T, TSign>(IntN<T, TSign> x, int i)
        {
            var c = typeof(T).GetConstructor(new Type[] { typeof(long) });
            var instance = (IntN<T, TSign>)c.Invoke(new object[] { (long)i });
            return x.Subtract(instance);
        }
    }
}
