// <copyright file="ITypeVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Visitor class for building objects from a type.
    /// </summary>
    internal interface ITypeVisitor<T, TParam>
    {
        /// <summary>
        /// Visit the boolean type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitBool(TParam parameter);

        /// <summary>
        /// Visit the byte type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitByte(TParam parameter);

        /// <summary>
        /// Visit the char type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitChar(TParam parameter);

        /// <summary>
        /// Visit the short type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitShort(TParam parameter);

        /// <summary>
        /// Visit the ushort type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitUshort(TParam parameter);

        /// <summary>
        /// Visit the int type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitInt(TParam parameter);

        /// <summary>
        /// Visit the uint type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitUint(TParam parameter);

        /// <summary>
        /// Visit the long type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitLong(TParam parameter);

        /// <summary>
        /// Visit the ulong type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitUlong(TParam parameter);

        /// <summary>
        /// Visit the BigInteger type.
        /// </summary>
        /// <returns></returns>
        T VisitBigInteger(TParam parameter);

        /// <summary>
        /// Visit a fixed width integer type.
        /// </summary>
        /// <returns></returns>
        T VisitFixedInteger(Type intType, TParam parameter);

        /// <summary>
        /// Visit the string type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitString(TParam parameter);

        /// <summary>
        /// Visit the dictionary type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitMap(Type dictionaryType, Type keyType, Type valueType, TParam parameter);

        /// <summary>
        /// Visit the sequence type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitSeq(Type sequenceType, Type innerType, TParam parameter);

        /// <summary>
        /// Visit the list type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitList(Type listType, Type innerType, TParam parameter);

        /// <summary>
        /// Visit a class/struct type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitObject(Type objectType, SortedDictionary<string, Type> fields, TParam parameter);
    }
}
