// <copyright file="SymbolicInputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;

    /// <summary>
    /// Class to help generate a default symbolic value.
    /// </summary>
    internal class DefaultTypeGenerator : ITypeVisitor<object>
    {
        /// <summary>
        /// Method for the creating an empty Zen list.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Language).GetMethod("EmptyList");

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

        public object VisitList(Func<Type, object> recurse, Type listType, Type innerType)
        {
            var method = emptyListMethod.MakeGenericMethod(innerType);
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
            return Language.Constant(value);
        }

        public object VisitBigInteger()
        {
            return ZenConstantExpr<BigInteger>.Create(new BigInteger(0));
        }

        public object VisitObject(Func<Type, object> recurse, Type objectType, SortedDictionary<string, Type> fields)
        {
            return GeneratorHelper.ApplyToObject(recurse, objectType, fields);
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
