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
    using static ZenLib.Zen;

    /// <summary>
    /// Class to help generate a symbolic input.
    /// </summary>
    internal class SymbolicInputGenerator : ITypeVisitor<object, ZenDepthConfiguration>
    {
        /// <summary>
        /// The method for creating the empty list at runtime.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Zen).GetMethod("EmptyList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The method for creating the empty list at runtime.
        /// </summary>
        private static MethodInfo arbitraryDictMethod = typeof(Zen).GetMethod("ArbitraryDict", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The method for creating the empty seq at runtime.
        /// </summary>
        private static MethodInfo arbitrarySeqMethod = typeof(Zen).GetMethod("ArbitrarySeq", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The method for creating and if expression at runtime.
        /// </summary>
        private static MethodInfo ifConditionMethod = typeof(Zen).GetMethod("If");

        /// <summary>
        /// Name of the function used to create an object via reflection.
        /// </summary>
        private static MethodInfo createMethod = typeof(Zen).GetMethod("Create");

        /// <summary>
        /// List method from Zen.Language.
        /// </summary>
        private static MethodInfo listMethod = typeof(Zen).GetMethod("List", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The arbitrary expressions generated.
        /// </summary>
        internal List<object> ArbitraryExpressions { get; } = new List<object>();

        public object VisitBool(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<bool>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitByte(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<byte>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitChar(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<char>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitInt(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<int>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitBigInteger(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<BigInteger>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitList(Type listType, Type elementType, ZenDepthConfiguration config)
        {
            if (!config.ExhaustiveDepth)
            {
                return ApplyToList(elementType, config, config.Depth);
            }

            var length = Arbitrary<byte>();

            // start with empty list
            var emptyMethod = emptyListMethod.MakeGenericMethod(elementType);
            var ifMethod = ifConditionMethod.MakeGenericMethod(listType);

            var list = emptyMethod.Invoke(null, CommonUtilities.EmptyArray);

            for (int i = config.Depth; i > 0; i--)
            {
                var guard = length == Constant((byte)i);
                var trueBranch = ApplyToList(elementType, config, i);
                list = ifMethod.Invoke(null, new object[] { guard, trueBranch, list });
            }

            return list;
        }

        public object VisitMap(Type dictionaryType, Type keyType, Type valueType, ZenDepthConfiguration parameter)
        {
            var method = arbitraryDictMethod.MakeGenericMethod(keyType, valueType);
            var e = method.Invoke(null, CommonUtilities.EmptyArray);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitSeq(Type sequenceType, Type innerType, ZenDepthConfiguration parameter)
        {
            var method = arbitrarySeqMethod.MakeGenericMethod(innerType);
            var e = method.Invoke(null, CommonUtilities.EmptyArray);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object ApplyToList(Type innerType, ZenDepthConfiguration config, int size)
        {
            var method = listMethod.MakeGenericMethod(innerType);

            var args = new object[size];
            for (int i = 0; i < size; i++)
            {
                var arg = ReflectionUtilities.ApplyTypeVisitor(this, innerType, config);
                args[i] = arg;
            }

            var zenType = typeof(Zen<>).MakeGenericType(innerType);
            var finalArgs = Array.CreateInstance(zenType, args.Length);
            Array.Copy(args, finalArgs, args.Length);
            return method.Invoke(null, new object[] { finalArgs });
        }

        public object VisitLong(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<long>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitObject(Type objectType, SortedDictionary<string, Type> fields, ZenDepthConfiguration config)
        {
            var asList = fields.ToArray();

            var method = createMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                var fieldName = asList[i].Key;
                var fieldType = asList[i].Value;
                var newConfig = UpdateDepthConfiguration(config, GetSizeAttribute(objectType, fieldName));
                args[i] = (fieldName, ReflectionUtilities.ApplyTypeVisitor(this, fieldType, newConfig));
            }

            return method.Invoke(null, new object[] { args });
        }

        private ZenDepthConfiguration UpdateDepthConfiguration(ZenDepthConfiguration config, ZenSizeAttribute attribute)
        {
            if (attribute == null)
            {
                return config;
            }

            var depth = attribute.Depth > 0 ? attribute.Depth : config.Depth;
            var exhaustive = attribute.EnumerationType == EnumerationType.User ?
                config.ExhaustiveDepth :
                attribute.EnumerationType == EnumerationType.Exhaustive;

            return new ZenDepthConfiguration { Depth = depth, ExhaustiveDepth = exhaustive };
        }

        private ZenSizeAttribute GetSizeAttribute(Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo != null)
            {
                return (ZenSizeAttribute)fieldInfo.GetCustomAttribute(typeof(ZenSizeAttribute));
            }

            var propertyInfo = type.GetPropertyCached(fieldName);
            return (ZenSizeAttribute)propertyInfo.GetCustomAttribute(typeof(ZenSizeAttribute));
        }

        public object VisitShort(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<short>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUint(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<uint>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUlong(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<ulong>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUshort(ZenDepthConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<ushort>();
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitFixedInteger(Type intType, ZenDepthConfiguration parameter)
        {
            var c = typeof(ZenArbitraryExpr<>).MakeGenericType(intType).GetConstructor(new Type[] { });
            var e = c.Invoke(CommonUtilities.EmptyArray);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitString(ZenDepthConfiguration parameter)
        {
            var v = (Zen<Seq<char>>)ReflectionUtilities.ApplyTypeVisitor(this, typeof(Seq<char>), parameter);
            return ZenCastExpr<Seq<char>, string>.Create(v);
        }
    }
}
