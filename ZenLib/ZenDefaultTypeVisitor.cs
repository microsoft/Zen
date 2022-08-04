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
    internal class ZenDefaultTypeVisitor : ITypeVisitor<object, Unit>
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

        public object VisitBool(Unit parameter)
        {
            return ZenConstantExpr<bool>.Create(false);
        }

        public object VisitByte(Unit parameter)
        {
            return ZenConstantExpr<byte>.Create(0);
        }

        public object VisitChar(Unit parameter)
        {
            return ZenConstantExpr<char>.Create('0');
        }

        public object VisitInt(Unit parameter)
        {
            return ZenConstantExpr<int>.Create(0);
        }

        public object VisitList(Type listType, Type innerType, Unit u)
        {
            var method = emptyListMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        public object VisitSeq(Type sequenceType, Type innerType, Unit parameter)
        {
            var method = emptySeqMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        public object VisitMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            var method = emptyMapMethod.MakeGenericMethod(keyType, valueType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        public object VisitConstMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            var emptyMap = typeof(ConstMap<,>)
                .MakeGenericType(keyType, valueType)
                .GetConstructor(new Type[] { })
                .Invoke(CommonUtilities.EmptyArray);
            return constantMethod.MakeGenericMethod(mapType).Invoke(null, new object[] { emptyMap });
        }

        public object VisitLong(Unit parameter)
        {
            return ZenConstantExpr<long>.Create(0);
        }

        public object VisitFixedInteger(Type intType, Unit parameter)
        {
            var c = intType.GetConstructor(new Type[] { typeof(long) });
            dynamic value = c.Invoke(new object[] { 0L });
            return Zen.Constant(value);
        }

        public object VisitBigInteger(Unit parameter)
        {
            return ZenConstantExpr<BigInteger>.Create(new BigInteger(0));
        }

        public object VisitReal(Unit parameter)
        {
            return ZenConstantExpr<Real>.Create(new Real(0));
        }

        public object VisitObject(Type objectType, SortedDictionary<string, Type> fields, Unit u)
        {
            var asList = fields.ToArray();

            var method = createMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                args[i] = (asList[i].Key, ReflectionUtilities.ApplyTypeVisitor(this, asList[i].Value, new Unit()));
            }

            return method.Invoke(null, new object[] { args });
        }

        public object VisitShort(Unit parameter)
        {
            return ZenConstantExpr<short>.Create(0);
        }

        public object VisitUint(Unit parameter)
        {
            return ZenConstantExpr<uint>.Create(0);
        }

        public object VisitUlong(Unit parameter)
        {
            return ZenConstantExpr<ulong>.Create(0);
        }

        public object VisitUshort(Unit parameter)
        {
            return ZenConstantExpr<ushort>.Create(0);
        }

        public object VisitString(Unit parameter)
        {
            return Zen.Cast<Seq<char>, string>(Seq.Empty<char>());
        }
    }
}
