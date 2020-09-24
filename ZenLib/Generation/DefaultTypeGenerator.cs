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

        /// <summary>
        /// Method for creating an empty Zen dictionary.
        /// </summary>
        private static MethodInfo emptyDictMethod = typeof(Language).GetMethod("EmptyDict");

        /// <summary>
        /// Method for creating a null option.
        /// </summary>
        private static MethodInfo nullMethod = typeof(Language).GetMethod("Null");

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
            return method.Invoke(null, new object[] { });
        }

        public object VisitDictionary(Func<Type, object> recurse, Type dictType, Type keyType, Type valueType)
        {
            var method = emptyDictMethod.MakeGenericMethod(keyType, valueType);
            return method.Invoke(null, new object[] { });
        }

        public object VisitLong()
        {
            return ZenConstantExpr<long>.Create(0);
        }

        public object VisitBigInteger()
        {
            return ZenConstantExpr<BigInteger>.Create(new BigInteger(0));
        }

        public object VisitObject(Func<Type, object> recurse, Type objectType, SortedDictionary<string, Type> fields)
        {
            return GeneratorHelper.ApplyToObject(recurse, objectType, fields);
        }

        public object VisitOption(Func<Type, object> recurse, Type optionType, Type innerType)
        {
            var method = nullMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, new object[] { });
        }

        public object VisitShort()
        {
            return ZenConstantExpr<short>.Create(0);
        }

        public object VisitTuple(Func<Type, object> recurse, Type tupleType, Type innerTypeLeft, Type innerTypeRight)
        {
            return GeneratorHelper.ApplyToTuple(recurse, innerTypeLeft, innerTypeRight);
        }

        public object VisitValueTuple(Func<Type, object> recurse, Type tupleType, Type innerTypeLeft, Type innerTypeRight)
        {
            return GeneratorHelper.ApplyToValueTuple(recurse, innerTypeLeft, innerTypeRight);
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
