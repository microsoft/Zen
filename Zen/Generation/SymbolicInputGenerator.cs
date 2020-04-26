// <copyright file="SymbolicInputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using static Zen.Language;

    /// <summary>
    /// Class to help generate a symbolic input.
    /// </summary>
    internal class SymbolicInputGenerator : ITypeVisitor<object>
    {
        /// <summary>
        /// The method for creating the empty list at runtime.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Language).GetMethod("EmptyList");

        /// <summary>
        /// The method for converting a list to a dictionary.
        /// </summary>
        private static MethodInfo listToDictMethod = typeof(Language).GetMethod("ListToDictionary");

        /// <summary>
        /// The method for creating and if expression at runtime.
        /// </summary>
        private static MethodInfo ifConditionMethod = typeof(Language).GetMethod("If");

        /// <summary>
        /// The method for creating an option from a tuple at runtime.
        /// </summary>
        private static MethodInfo tupleToOptionMethod = typeof(Language).GetMethod("TupleToOption");

        /// <summary>
        /// Maximum length of a list.
        /// </summary>
        private int maxSize;

        /// <summary>
        /// Whether to exhaustively test list sizes.
        /// </summary>
        private bool exhaustiveLists;

        public SymbolicInputGenerator(int maxSize, bool exhaustiveLists = true)
        {
            this.maxSize = maxSize;
            this.exhaustiveLists = exhaustiveLists;
        }

        public object VisitBool()
        {
            return new ZenArbitraryExpr<bool>();
        }

        public object VisitByte()
        {
            return new ZenArbitraryExpr<byte>();
        }

        public object VisitInt()
        {
            return new ZenArbitraryExpr<int>();
        }

        public object VisitList(Func<Type, object> recurse, Type listType, Type elementType)
        {
            if (!exhaustiveLists)
            {
                return GeneratorHelper.ApplyToList(recurse, elementType, maxSize);
            }

            var length = Arbitrary<byte>();

            // start with empty list
            var emptyMethod = emptyListMethod.MakeGenericMethod(elementType);
            var ifMethod = ifConditionMethod.MakeGenericMethod(listType);

            var list = emptyMethod.Invoke(null, new object[] { });

            for (int i = maxSize; i > 0; i--)
            {
                var guard = length == Byte((byte)i);
                var trueBranch = GeneratorHelper.ApplyToList(recurse, elementType, i);
                list = ifMethod.Invoke(null, new object[] { guard, trueBranch, list });
            }

            return list;
        }

        public object VisitDictionary(Func<Type, object> recurse, Type dictType, Type keyType, Type valueType)
        {
            var tupleType = typeof(Tuple<,>).MakeGenericType(keyType, valueType);
            var listType = typeof(IList<>).MakeGenericType(tupleType);
            var list = VisitList(recurse, listType, tupleType);
            var method = listToDictMethod.MakeGenericMethod(keyType, valueType);
            return method.Invoke(null, new object[] { list });
        }

        public object VisitLong()
        {
            return new ZenArbitraryExpr<long>();
        }

        public object VisitObject(Func<Type, object> recurse, Type objectType, Dictionary<string, Type> fields)
        {
            return GeneratorHelper.ApplyToObject(recurse, objectType, fields);
        }

        public object VisitShort()
        {
            return new ZenArbitraryExpr<short>();
        }

        public object VisitUint()
        {
            return new ZenArbitraryExpr<uint>();
        }

        public object VisitUlong()
        {
            return new ZenArbitraryExpr<ulong>();
        }

        public object VisitUshort()
        {
            return new ZenArbitraryExpr<ushort>();
        }

        public object VisitOption(Func<Type, object> recurse, Type innerType)
        {
            var flag = recurse(ReflectionUtilities.BoolType);
            var value = recurse(innerType);
            var method = tupleToOptionMethod.MakeGenericMethod(innerType);
            return method.Invoke(null, new object[] { flag, value });
        }

        public object VisitTuple(Func<Type, object> recurse, Type innerTypeLeft, Type innerTypeRight)
        {
            return GeneratorHelper.ApplyToTuple(recurse, innerTypeLeft, innerTypeRight);
        }

        public object VisitValueTuple(Func<Type, object> recurse, Type innerTypeLeft, Type innerTypeRight)
        {
            return GeneratorHelper.ApplyToValueTuple(recurse, innerTypeLeft, innerTypeRight);
        }
    }
}
