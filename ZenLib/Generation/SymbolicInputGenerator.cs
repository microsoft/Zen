// <copyright file="SymbolicInputGenerator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;
    using static ZenLib.Basic;

    /// <summary>
    /// Class to help generate a symbolic input.
    /// </summary>
    internal class SymbolicInputGenerator : ITypeVisitor<object>
    {
        /// <summary>
        /// The method for creating the empty list at runtime.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Basic).GetMethod("EmptyList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The method for creating and if expression at runtime.
        /// </summary>
        private static MethodInfo ifConditionMethod = typeof(Basic).GetMethod("If");

        /// <summary>
        /// The arbitrary expressions generated.
        /// </summary>
        internal List<object> ArbitraryExpressions { get; } = new List<object>();

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
            var e = new ZenArbitraryExpr<bool>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitByte()
        {
            var e = new ZenArbitraryExpr<byte>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitInt()
        {
            var e = new ZenArbitraryExpr<int>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitBigInteger()
        {
            var e = new ZenArbitraryExpr<BigInteger>();
            this.ArbitraryExpressions.Add(e);
            return e;
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

            var list = emptyMethod.Invoke(null, CommonUtilities.EmptyArray);

            for (int i = maxSize; i > 0; i--)
            {
                var guard = length == Constant((byte)i);
                var trueBranch = GeneratorHelper.ApplyToList(recurse, elementType, i);
                list = ifMethod.Invoke(null, new object[] { guard, trueBranch, list });
            }

            return list;
        }

        public object VisitLong()
        {
            var e = new ZenArbitraryExpr<long>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitObject(Func<Type, object> recurse, Type objectType, SortedDictionary<string, Type> fields)
        {
            return GeneratorHelper.ApplyToObject(recurse, objectType, fields);
        }

        public object VisitShort()
        {
            var e = new ZenArbitraryExpr<short>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUint()
        {
            var e = new ZenArbitraryExpr<uint>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUlong()
        {
            var e = new ZenArbitraryExpr<ulong>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUshort()
        {
            var e = new ZenArbitraryExpr<ushort>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitFixedInteger(Type intType)
        {
            var c = typeof(ZenArbitraryExpr<>).MakeGenericType(intType).GetConstructor(new Type[] { });
            var e = c.Invoke(CommonUtilities.EmptyArray);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitString()
        {
            var e = new ZenArbitraryExpr<string>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }
    }
}
