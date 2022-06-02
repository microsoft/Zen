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
    internal class SymbolicInputGenerator : ITypeVisitor<object, ZenGenerationConfiguration>
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

        public object VisitBool(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<bool>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitByte(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<byte>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitChar(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<char>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitInt(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<int>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitBigInteger(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<BigInteger>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitReal(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<Real>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitList(Type listType, Type elementType, ZenGenerationConfiguration parameter)
        {
            if (!parameter.ExhaustiveDepth)
            {
                return ApplyToList(elementType, parameter, parameter.Depth);
            }

            var length = Arbitrary<byte>(parameter.Name + "_length");

            // start with empty list
            var emptyMethod = emptyListMethod.MakeGenericMethod(elementType);
            var ifMethod = ifConditionMethod.MakeGenericMethod(listType);

            var list = emptyMethod.Invoke(null, CommonUtilities.EmptyArray);

            for (int i = parameter.Depth; i > 0; i--)
            {
                var guard = length == Constant((byte)i);
                var trueBranch = ApplyToList(elementType, parameter, i);
                list = ifMethod.Invoke(null, new object[] { guard, trueBranch, list });
            }

            return list;
        }

        public object VisitMap(Type dictionaryType, Type keyType, Type valueType, ZenGenerationConfiguration parameter)
        {
            var method = arbitraryDictMethod.MakeGenericMethod(keyType, valueType);
            var e = method.Invoke(null, new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitSeq(Type sequenceType, Type innerType, ZenGenerationConfiguration parameter)
        {
            var method = arbitrarySeqMethod.MakeGenericMethod(innerType);
            var e = method.Invoke(null, new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object ApplyToList(Type innerType, ZenGenerationConfiguration parameter, int size)
        {
            var method = listMethod.MakeGenericMethod(innerType);

            var args = new object[size];
            for (int i = 0; i < size; i++)
            {
                var arg = ReflectionUtilities.ApplyTypeVisitor(this, innerType, new ZenGenerationConfiguration
                {
                    Depth = parameter.Depth,
                    ExhaustiveDepth = parameter.ExhaustiveDepth,
                    Name = parameter.Name + $"_elt_{size}_{i}",
                });
                args[i] = arg;
            }

            var zenType = typeof(Zen<>).MakeGenericType(innerType);
            var finalArgs = Array.CreateInstance(zenType, args.Length);
            Array.Copy(args, finalArgs, args.Length);
            return method.Invoke(null, new object[] { finalArgs });
        }

        public object VisitLong(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<long>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitObject(Type objectType, SortedDictionary<string, Type> fields, ZenGenerationConfiguration parameter)
        {
            var asList = fields.ToArray();

            var method = createMethod.MakeGenericMethod(objectType);

            var args = new (string, object)[asList.Length];
            for (int i = 0; i < asList.Length; i++)
            {
                var fieldName = asList[i].Key;
                var fieldType = asList[i].Value;
                var newParameter = UpdateDepthConfiguration(parameter, GetSizeAttribute(objectType, fieldName));
                newParameter.Name = parameter.Name + "_" + fieldName;
                args[i] = (fieldName, ReflectionUtilities.ApplyTypeVisitor(this, fieldType, newParameter));
            }

            return method.Invoke(null, new object[] { args });
        }

        private ZenGenerationConfiguration UpdateDepthConfiguration(ZenGenerationConfiguration config, ZenSizeAttribute attribute)
        {
            if (attribute == null)
            {
                return new ZenGenerationConfiguration { Depth = config.Depth, ExhaustiveDepth = config.ExhaustiveDepth, Name = config.Name };
            }

            var depth = attribute.Depth > 0 ? attribute.Depth : config.Depth;
            var exhaustive = attribute.EnumerationType == EnumerationType.User ?
                config.ExhaustiveDepth :
                attribute.EnumerationType == EnumerationType.Exhaustive;

            return new ZenGenerationConfiguration { Depth = depth, ExhaustiveDepth = exhaustive, Name = config.Name };
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

        public object VisitShort(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<short>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUint(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<uint>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUlong(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<ulong>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitUshort(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<ushort>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitFixedInteger(Type intType, ZenGenerationConfiguration parameter)
        {
            var c = typeof(ZenArbitraryExpr<>).MakeGenericType(intType).GetConstructor(new Type[] { typeof(string) });
            var e = c.Invoke(new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        public object VisitString(ZenGenerationConfiguration parameter)
        {
            var v = (Zen<Seq<char>>)ReflectionUtilities.ApplyTypeVisitor(this, typeof(Seq<char>), parameter);
            return ZenCastExpr<Seq<char>, string>.Create(v);
        }
    }
}
