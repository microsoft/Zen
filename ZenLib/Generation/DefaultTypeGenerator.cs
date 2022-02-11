// <copyright file="SymbolicInputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    /// <summary>
    /// Class to help generate a default symbolic value.
    /// </summary>
    internal class DefaultTypeGenerator : ITypeVisitor<object, Unit>
    {
        /// <summary>
        /// Method for the creating an empty Zen list.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Zen).GetMethod("EmptyList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Method for the creating an empty Zen list.
        /// </summary>
        private static MethodInfo emptyDictMethod = typeof(Zen).GetMethod("EmptyDict", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Name of the function used to create an object via reflection.
        /// </summary>
        private static MethodInfo createMethod = typeof(Zen).GetMethod("Create");

        public object VisitBool()
        {
            return ZenConstantExpr<bool>.Create(false);
        }

        public object VisitByte()
        {
            return ZenConstantExpr<byte>.Create(0);
        }

        public object VisitInt()
        {
            return ZenConstantExpr<int>.Create(0);
        }

        public object VisitList(Func<Type, Unit, object> recurse, Type listType, Type innerType, Unit u)
        {
            var method = emptyListMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        public object VisitDictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            var method = emptyDictMethod.MakeGenericMethod(keyType, valueType);
            return method.Invoke(null, CommonUtilities.EmptyArray);
        }

        public object VisitLong()
        {
            return ZenConstantExpr<long>.Create(0);
        }

        public object VisitFixedInteger(Type intType)
        {
            var c = intType.GetConstructor(new Type[] { typeof(long) });
            dynamic value = c.Invoke(new object[] { 0L });
            return Zen.Constant(value);
        }

        public object VisitBigInteger()
        {
            return ZenConstantExpr<BigInteger>.Create(new BigInteger(0));
        }

        public object VisitObject(Func<Type, Unit, object> recurse, Type objectType, SortedDictionary<string, Type> fields, Unit u)
        {
            var asList = fields.ToArray();

            var method = createMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                args[i] = (asList[i].Key, recurse(asList[i].Value, new Unit()));
            }

            return method.Invoke(null, new object[] { args });
        }

        public object VisitShort()
        {
            return ZenConstantExpr<short>.Create(0);
        }

        public object VisitUint()
        {
            return ZenConstantExpr<uint>.Create(0);
        }

        public object VisitUlong()
        {
            return ZenConstantExpr<ulong>.Create(0);
        }

        public object VisitUshort()
        {
            return ZenConstantExpr<ushort>.Create(0);
        }

        // FIXME: default value for a c# string is null, not empty. How to represent null strings?
        public object VisitString()
        {
            return ZenConstantExpr<string>.Create("");
        }
    }
}
