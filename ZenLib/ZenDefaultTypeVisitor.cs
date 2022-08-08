// <copyright file="ZenDefaultTypeVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    /// <summary>
    /// Class to help generate a default symbolic value.
    /// </summary>
    internal class ZenDefaultTypeVisitor : TypeVisitor<object, Unit>
    {
        /// <summary>
        /// Method for the creating an empty Zen seq.
        /// </summary>
        private static MethodInfo emptySeqMethod = typeof(Seq).GetMethod("Empty");

        /// <summary>
        /// Method for the creating an empty Zen list.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Zen).GetMethod("EmptyList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Method for the creating an empty Zen map.
        /// </summary>
        private static MethodInfo emptyMapMethod = typeof(Zen).GetMethod("EmptyMap", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Method for the creating a constant.
        /// </summary>
        private static MethodInfo constantMethod = typeof(Zen).GetMethod("Constant");

        /// <summary>
        /// Name of the function used to create an object via reflection.
        /// </summary>
        private static MethodInfo createMethod = typeof(Zen).GetMethod("Create");

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitBool(Unit parameter)
        {
            return ZenConstantExpr<bool>.Create(false);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitByte(Unit parameter)
        {
            return ZenConstantExpr<byte>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitChar(Unit parameter)
        {
            return ZenConstantExpr<char>.Create('0');
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitInt(Unit parameter)
        {
            return ZenConstantExpr<int>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitList(Type listType, Type innerType, Unit parameter)
        {
            var method = emptyListMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitSeq(Type sequenceType, Type innerType, Unit parameter)
        {
            var method = emptySeqMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            var method = emptyMapMethod.MakeGenericMethod(keyType, valueType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitConstMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            var emptyMap = typeof(CMap<,>)
                .MakeGenericType(keyType, valueType)
                .GetConstructor(new Type[] { })
                .Invoke(CommonUtilities.EmptyArray);
            return constantMethod.MakeGenericMethod(mapType).Invoke(null, new object[] { emptyMap });
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitLong(Unit parameter)
        {
            return ZenConstantExpr<long>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitFixedInteger(Type intType, Unit parameter)
        {
            var c = intType.GetConstructor(new Type[] { typeof(long) });
            dynamic value = c.Invoke(new object[] { 0L });
            return Zen.Constant(value);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitBigInteger(Unit parameter)
        {
            return ZenConstantExpr<BigInteger>.Create(new BigInteger(0));
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitReal(Unit parameter)
        {
            return ZenConstantExpr<Real>.Create(new Real(0));
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their values.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitObject(Type objectType, SortedDictionary<string, Type> fields, Unit parameter)
        {
            var asList = fields.ToArray();

            var method = createMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                args[i] = (asList[i].Key, this.Visit(asList[i].Value, Unit.Instance));
            }

            return method.Invoke(null, new object[] { args });
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitShort(Unit parameter)
        {
            return ZenConstantExpr<short>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitUint(Unit parameter)
        {
            return ZenConstantExpr<uint>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitUlong(Unit parameter)
        {
            return ZenConstantExpr<ulong>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitUshort(Unit parameter)
        {
            return ZenConstantExpr<ushort>.Create(0);
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A default value.</returns>
        public override object VisitString(Unit parameter)
        {
            return Zen.Cast<Seq<char>, string>(Seq.Empty<char>());
        }
    }
}
