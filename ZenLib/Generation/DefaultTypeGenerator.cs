// <copyright file="SymbolicInputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
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
            return ZenConstantBoolExpr.False;
        }

        public object VisitByte()
        {
            return ZenConstantByteExpr.Create(0);
        }

        public object VisitInt()
        {
            return ZenConstantIntExpr.Create(0);
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
            return ZenConstantLongExpr.Create(0);
        }

        public object VisitObject(Func<Type, object> recurse, Type objectType, Dictionary<string, Type> fields)
        {
            return GeneratorHelper.ApplyToObject(recurse, objectType, fields);
        }

        public object VisitOption(Func<Type, object> recurse, Type innerType)
        {
            var method = nullMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, new object[] { });
        }

        public object VisitShort()
        {
            return ZenConstantShortExpr.Create(0);
        }

        public object VisitTuple(Func<Type, object> recurse, Type innerTypeLeft, Type innerTypeRight)
        {
            return GeneratorHelper.ApplyToTuple(recurse, innerTypeLeft, innerTypeRight);
        }

        public object VisitValueTuple(Func<Type, object> recurse, Type innerTypeLeft, Type innerTypeRight)
        {
            return GeneratorHelper.ApplyToValueTuple(recurse, innerTypeLeft, innerTypeRight);
        }

        public object VisitUint()
        {
            return ZenConstantUintExpr.Create(0);
        }

        public object VisitUlong()
        {
            return ZenConstantUlongExpr.Create(0);
        }

        public object VisitUshort()
        {
            return ZenConstantUshortExpr.Create(0);
        }

        // FIXME: default value for a c# string is null, not empty. How to represent null strings?
        public object VisitString()
        {
            return ZenConstantStringExpr.Create("");
        }
    }
}
