// <copyright file="SymbolicInputVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using ZenLib;
    using static ZenLib.Zen;

    /// <summary>
    /// Class to help generate a symbolic input.
    /// </summary>
    internal class SymbolicInputVisitor : TypeVisitor<object, ZenGenerationConfiguration>
    {
        /// <summary>
        /// The method for creating the empty list at runtime.
        /// </summary>
        private static MethodInfo emptyListMethod = typeof(Zen).GetMethod("EmptyList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The method for creating the empty map at runtime.
        /// </summary>
        private static MethodInfo arbitraryMapMethod = typeof(Zen).GetMethod("ArbitraryMap", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The method for creating the empty map at runtime.
        /// </summary>
        private static MethodInfo arbitraryConstMapMethod = typeof(Zen).GetMethod("ArbitraryConstMap", BindingFlags.Static | BindingFlags.NonPublic);

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

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitBool(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<bool>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitByte(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<byte>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitChar(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<char>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitInt(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<int>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitBigInteger(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<BigInteger>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitReal(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<Real>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="elementType">The element type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitList(Type listType, Type elementType, ZenGenerationConfiguration parameter)
        {
            var method = listMethod.MakeGenericMethod(elementType);

            var args = new object[parameter.Depth];
            for (int i = 0; i < parameter.Depth; i++)
            {
                var optionType = typeof(Option<>).MakeGenericType(elementType);
                var arg = this.Visit(optionType, new ZenGenerationConfiguration
                {
                    Depth = parameter.Depth,
                    ExhaustiveDepth = parameter.ExhaustiveDepth,
                    Name = parameter.Name + $"_elt_{i + 1}",
                });
                args[i] = arg;
            }

            var zenType = typeof(Zen<>).MakeGenericType(typeof(Option<>).MakeGenericType(elementType));
            var finalArgs = Array.CreateInstance(zenType, args.Length);
            Array.Copy(args, finalArgs, args.Length);
            return method.Invoke(null, new object[] { finalArgs });
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitMap(Type mapType, Type keyType, Type valueType, ZenGenerationConfiguration parameter)
        {
            var method = arbitraryMapMethod.MakeGenericMethod(keyType, valueType);
            var e = method.Invoke(null, new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitConstMap(Type mapType, Type keyType, Type valueType, ZenGenerationConfiguration parameter)
        {
            var method = arbitraryConstMapMethod.MakeGenericMethod(keyType, valueType);
            var e = method.Invoke(null, new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitSeq(Type sequenceType, Type innerType, ZenGenerationConfiguration parameter)
        {
            var method = arbitrarySeqMethod.MakeGenericMethod(innerType);
            var e = method.Invoke(null, new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitLong(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<long>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitObject(Type objectType, SortedDictionary<string, Type> fields, ZenGenerationConfiguration parameter)
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
                args[i] = (fieldName, this.Visit(fieldType, newParameter));
            }

            return method.Invoke(null, new object[] { args });
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitShort(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<short>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitUint(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<uint>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitUlong(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<ulong>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitUshort(ZenGenerationConfiguration parameter)
        {
            var e = new ZenArbitraryExpr<ushort>(parameter.Name);
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitFixedInteger(Type intType, ZenGenerationConfiguration parameter)
        {
            var c = typeof(ZenArbitraryExpr<>).MakeGenericType(intType).GetConstructor(new Type[] { typeof(string) });
            var e = c.Invoke(new object[] { parameter.Name });
            this.ArbitraryExpressions.Add(e);
            return e;
        }

        /// <summary>
        /// Visit the type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The input.</returns>
        public override object VisitString(ZenGenerationConfiguration parameter)
        {
            var v = (Zen<Seq<char>>)this.Visit(typeof(Seq<char>), parameter);
            return ZenCastExpr<Seq<char>, string>.Create(v);
        }

        /// <summary>
        /// Update a depth configuration given an attribute.
        /// </summary>
        /// <param name="config">The depth configuration.</param>
        /// <param name="attribute">The Zen size attribute.</param>
        /// <returns>A new configuration.</returns>
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

        /// <summary>
        /// Get the Zen size attribute for a field.
        /// </summary>
        /// <param name="type">The object type.</param>
        /// <param name="fieldName">The field or property name.</param>
        /// <returns>The Zen attribute.</returns>
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
    }
}
