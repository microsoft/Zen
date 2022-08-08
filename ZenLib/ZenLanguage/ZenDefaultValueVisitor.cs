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
        public override object VisitBigInteger(Unit parameter)
        {
            return new BigInteger(0);
        }

        public override object VisitBool(Unit parameter)
        {
            return false;
        }

        public override object VisitByte(Unit parameter)
        {
            return (byte)0;
        }

        public override object VisitChar(Unit parameter)
        {
            return char.MinValue;
        }

        public override object VisitConstMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            return mapType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        public override object VisitFixedInteger(Type intType, Unit parameter)
        {
            return intType.GetConstructor(new Type[] { typeof(long) }).Invoke(new object[] { 0L });
        }

        public override object VisitInt(Unit parameter)
        {
            return 0;
        }

        public override object VisitList(Type listType, Type innerType, Unit parameter)
        {
            return listType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        public override object VisitLong(Unit parameter)
        {
            return 0L;
        }

        public override object VisitMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            return mapType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

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

        public override object VisitReal(Unit parameter)
        {
            return new Real(0, 1);
        }

        public override object VisitSeq(Type sequenceType, Type innerType, Unit parameter)
        {
            return sequenceType.GetConstructor(new Type[] { }).Invoke(CommonUtilities.EmptyArray);
        }

        public override object VisitShort(Unit parameter)
        {
            return (short)0;
        }

        public override object VisitString(Unit parameter)
        {
            return string.Empty;
        }

        public override object VisitUint(Unit parameter)
        {
            return 0U;
        }

        public override object VisitUlong(Unit parameter)
        {
            return 0UL;
        }

        public override object VisitUshort(Unit parameter)
        {
            return (ushort)0;
        }
    }
}
