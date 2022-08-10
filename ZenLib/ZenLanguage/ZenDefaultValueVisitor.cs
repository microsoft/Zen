// <copyright file="ZenDefaultValueVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ZenLanguage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Visitor class for building a default objects from a type.
    /// </summary>
    internal class ZenDefaultValueVisitor : TypeVisitor<object, Unit>
    {
        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitBigInteger(Unit parameter)
        {
            return new BigInteger(0);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitBool(Unit parameter)
        {
            return false;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitByte(Unit parameter)
        {
            return (byte)0;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitChar(Unit parameter)
        {
            return '0';
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitConstMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            return mapType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitFixedInteger(Type intType, Unit parameter)
        {
            return intType.GetConstructor(new Type[] { typeof(long) }).Invoke(new object[] { 0L });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitInt(Unit parameter)
        {
            return 0;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitList(Type listType, Type innerType, Unit parameter)
        {
            return listType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitLong(Unit parameter)
        {
            return 0L;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            return mapType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitObject(Type objectType, SortedDictionary<string, Type> fields, Unit parameter)
        {
            var objFields = new SortedDictionary<string, object>();

            foreach (var field in ReflectionUtilities.GetAllFields(objectType))
            {
                objFields[field.Name] = Visit(field.FieldType, parameter);
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(objectType))
            {
                objFields[property.Name] = Visit(property.PropertyType, parameter);
            }

            return ReflectionUtilities.CreateInstance(objectType, objFields.Keys.ToArray(), objFields.Values.ToArray());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitReal(Unit parameter)
        {
            return new Real(0, 1);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitSeq(Type sequenceType, Type innerType, Unit parameter)
        {
            return sequenceType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitShort(Unit parameter)
        {
            return (short)0;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitString(Unit parameter)
        {
            return string.Empty;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitUint(Unit parameter)
        {
            return 0U;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitUlong(Unit parameter)
        {
            return 0UL;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The default value.</returns>
        public override object VisitUshort(Unit parameter)
        {
            return (ushort)0;
        }
    }
}
