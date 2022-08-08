// <copyright file="TypeVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Visitor class for building objects from a type.
    /// </summary>
    internal abstract class TypeVisitor<T, TParam>
    {
        /// <summary>
        /// Visit the boolean type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitBool(TParam parameter);

        /// <summary>
        /// Visit the byte type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitByte(TParam parameter);

        /// <summary>
        /// Visit the char type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitChar(TParam parameter);

        /// <summary>
        /// Visit the short type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitShort(TParam parameter);

        /// <summary>
        /// Visit the ushort type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitUshort(TParam parameter);

        /// <summary>
        /// Visit the int type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitInt(TParam parameter);

        /// <summary>
        /// Visit the uint type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitUint(TParam parameter);

        /// <summary>
        /// Visit the long type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitLong(TParam parameter);

        /// <summary>
        /// Visit the ulong type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitUlong(TParam parameter);

        /// <summary>
        /// Visit the BigInteger type.
        /// </summary>
        /// <returns></returns>
        public abstract T VisitBigInteger(TParam parameter);

        /// <summary>
        /// Visit a fixed width integer type.
        /// </summary>
        /// <returns></returns>
        public abstract T VisitFixedInteger(Type intType, TParam parameter);

        /// <summary>
        /// Visit the Real type.
        /// </summary>
        /// <returns></returns>
        public abstract T VisitReal(TParam parameter);

        /// <summary>
        /// Visit the string type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitString(TParam parameter);

        /// <summary>
        /// Visit the map type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitMap(Type mapType, Type keyType, Type valueType, TParam parameter);

        /// <summary>
        /// Visit the const map type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitConstMap(Type mapType, Type keyType, Type valueType, TParam parameter);

        /// <summary>
        /// Visit the sequence type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitSeq(Type sequenceType, Type innerType, TParam parameter);

        /// <summary>
        /// Visit the list type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitList(Type listType, Type innerType, TParam parameter);

        /// <summary>
        /// Visit a class/struct type.
        /// </summary>
        /// <returns>A value.</returns>
        public abstract T VisitObject(Type objectType, SortedDictionary<string, Type> fields, TParam parameter);

        /// <summary>
        /// Visit a type with a given parameter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A value from walking over the type.</returns>
        public T Visit(Type type, TParam parameter)
        {
            if (type == ReflectionUtilities.BoolType)
                return this.VisitBool(parameter);
            if (type == ReflectionUtilities.ByteType)
                return this.VisitByte(parameter);
            if (type == ReflectionUtilities.CharType)
                return this.VisitChar(parameter);
            if (type == ReflectionUtilities.ShortType)
                return this.VisitShort(parameter);
            if (type == ReflectionUtilities.UshortType)
                return this.VisitUshort(parameter);
            if (type == ReflectionUtilities.IntType)
                return this.VisitInt(parameter);
            if (type == ReflectionUtilities.UintType)
                return this.VisitUint(parameter);
            if (type == ReflectionUtilities.LongType)
                return this.VisitLong(parameter);
            if (type == ReflectionUtilities.UlongType)
                return this.VisitUlong(parameter);
            if (type == ReflectionUtilities.BigIntType)
                return this.VisitBigInteger(parameter);
            if (type == ReflectionUtilities.RealType)
                return this.VisitReal(parameter);
            if (type == ReflectionUtilities.StringType)
                return this.VisitString(parameter);
            if (ReflectionUtilities.IsFixedIntegerType(type))
                return this.VisitFixedInteger(type, parameter);

            if (ReflectionUtilities.IsSeqType(type))
            {
                var t = type.GetGenericArgumentsCached()[0];
                return this.VisitSeq(type, t, parameter);
            }

            if (ReflectionUtilities.IsMapType(type))
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                return this.VisitMap(type, keyType, valueType, parameter);
            }

            if (ReflectionUtilities.IsConstMapType(type))
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                return this.VisitConstMap(type, keyType, valueType, parameter);
            }

            if (ReflectionUtilities.IsFSeqType(type))
            {
                var t = type.GetGenericArgumentsCached()[0];
                return this.VisitList(type, t, parameter);
            }

            // some class or struct
            var dict = ReflectionUtilities.GetAllFieldAndPropertyTypes(type);
            return this.VisitObject(type, dict, parameter);
        }
    }
}
