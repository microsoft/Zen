// <copyright file="ZenLiftingVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ZenLanguage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using ZenLib;

    /// <summary>
    /// Visitor class for creating a Zen expression for a constant.
    /// </summary>
    internal class ZenLiftingVisitor : TypeVisitor<object, object>
    {
        /// <summary>
        /// The object creation method.
        /// </summary>
        public static MethodInfo CreateMethod = typeof(Zen).GetMethod("Create");

        /// <summary>
        /// The zen constant list creation method.
        /// </summary>
        public static MethodInfo CreateZenSeqConstantMethod =
            typeof(ZenLiftingVisitor).GetMethod("CreateZenSeqConstant", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The zen constant list creation method.
        /// </summary>
        public static MethodInfo CreateZenListConstantMethod =
            typeof(ZenLiftingVisitor).GetMethod("CreateZenListConstant", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The zen constant map creation method.
        /// </summary>
        public static MethodInfo CreateZenMapConstantMethod =
            typeof(ZenLiftingVisitor).GetMethod("CreateZenMapConstant", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The zen constant map creation method.
        /// </summary>
        public static MethodInfo CreateZenConstMapConstantMethod =
            typeof(ZenLiftingVisitor).GetMethod("CreateZenConstMapConstant", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The zen constant option creation method.
        /// </summary>
        public static MethodInfo CreateZenOptionConstantMethod =
            typeof(ZenLiftingVisitor).GetMethod("CreateZenOptionConstant", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitBigInteger(object parameter)
        {
            return ZenConstantExpr<BigInteger>.Create((BigInteger)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitBool(object parameter)
        {
            return ZenConstantExpr<bool>.Create((bool)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitByte(object parameter)
        {
            return ZenConstantExpr<byte>.Create((byte)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitChar(object parameter)
        {
            return ZenConstantExpr<char>.Create((char)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitConstMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            return typeof(ZenConstantExpr<>).MakeGenericType(mapType).GetMethod("Create").Invoke(null, new object[] { parameter });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitFixedInteger(Type intType, object parameter)
        {
            return typeof(ZenConstantExpr<>).MakeGenericType(intType).GetMethod("Create").Invoke(null, new object[] { parameter });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitInt(object parameter)
        {
            return ZenConstantExpr<int>.Create((int)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitList(Type listType, Type innerType, object parameter)
        {
            return CreateZenListConstantMethod.MakeGenericMethod(innerType).Invoke(this, new object[] { parameter });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitLong(object parameter)
        {
            return ZenConstantExpr<long>.Create((long)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            return CreateZenMapConstantMethod.MakeGenericMethod(keyType, valueType).Invoke(this, new object[] { parameter });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitObject(Type objectType, SortedDictionary<string, Type> fields, object parameter)
        {
            // option type, we need this separate from classes/structs
            // because options may create null values for the None case
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>))
            {
                var innerType = objectType.GetGenericArgumentsCached()[0];
                return CreateZenOptionConstantMethod.MakeGenericMethod(innerType).Invoke(this, new object[] { parameter });
            }

            // some class or struct
            var objectFields = new SortedDictionary<string, dynamic>();
            foreach (var field in ReflectionUtilities.GetAllFields(objectType))
            {
                objectFields[field.Name] = field.GetValue(parameter);
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(objectType))
            {
                objectFields[property.Name] = property.GetValue(parameter);
            }

            var asList = objectFields.ToArray();
            var createMethod = CreateMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                var fieldValue = asList[i].Value;
                Contract.AssertNullConversion(fieldValue, "field", objectType);
                args[i] = (asList[i].Key, ReflectionUtilities.CreateZenConstant(fieldValue));
            }

            return createMethod.Invoke(null, new object[] { args });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitReal(object parameter)
        {
            return ZenConstantExpr<Real>.Create((Real)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitSeq(Type sequenceType, Type innerType, object parameter)
        {
            return CreateZenSeqConstantMethod.MakeGenericMethod(innerType).Invoke(this, new object[] { parameter });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitShort(object parameter)
        {
            return ZenConstantExpr<short>.Create((short)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitString(object parameter)
        {
            var asSeq = (Zen<Seq<char>>)Visit(typeof(Seq<char>), Seq.FromString((string)parameter));
            return ZenCastExpr<Seq<char>, string>.Create(asSeq);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitUint(object parameter)
        {
            return ZenConstantExpr<uint>.Create((uint)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitUlong(object parameter)
        {
            return ZenConstantExpr<ulong>.Create((ulong)parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">A C# constant.</param>
        /// <returns>A Zen constant.</returns>
        public override object VisitUshort(object parameter)
        {
            return ZenConstantExpr<ushort>.Create((ushort)parameter);
        }

        /// <summary>
        /// Create a constant Zen Seq value.
        /// </summary>
        /// <param name="value">The seq value.</param>
        /// <returns>The Zen value representing the seq.</returns>
        internal Zen<Seq<T>> CreateZenSeqConstant<T>(Seq<T> value)
        {
            return ZenConstantExpr<Seq<T>>.Create(value);
        }

        /// <summary>
        /// Create a constant Zen list value.
        /// </summary>
        /// <param name="value">The list value.</param>
        /// <returns>The Zen value representing the list.</returns>
        internal Zen<FSeq<T>> CreateZenListConstant<T>(FSeq<T> value)
        {
            Zen<FSeq<T>> list = ZenFSeqEmptyExpr<T>.Instance;
            foreach (var elt in value.ToList().Reverse())
            {
                Contract.AssertNullConversion(elt, "element", typeof(FSeq<T>));
                list = ZenFSeqAddFrontExpr<T>.Create(list, Option.Create<T>(elt));
            }

            return list;
        }

        /// <summary>
        /// Create a constant Zen list value.
        /// </summary>
        /// <param name="value">The list value.</param>
        /// <returns>The Zen value representing the list.</returns>
        internal Zen<Map<TKey, TValue>> CreateZenMapConstant<TKey, TValue>(Map<TKey, TValue> value)
        {
            Zen<Map<TKey, TValue>> map = ZenMapEmptyExpr<TKey, TValue>.Instance;
            foreach (var elt in value.Values)
            {
                Contract.AssertNullConversion(elt.Key, "element", typeof(Map<TKey, TValue>));
                map = ZenMapSetExpr<TKey, TValue>.Create(map, elt.Key, elt.Value);
            }

            return map;
        }

        /// <summary>
        /// Create a constant Zen option value.
        /// </summary>
        /// <param name="value">The option value.</param>
        /// <returns>The Zen value representing the option.</returns>
        internal Zen<Option<T>> CreateZenOptionConstant<T>(Option<T> value)
        {
            if (value.HasValue)
            {
                Contract.AssertNullConversion(value.Value, "Value", typeof(T));
                return Option.Create((Zen<T>)Visit(typeof(T), value.Value));
            }

            return Option.Null<T>();
        }
    }
}
