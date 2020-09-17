﻿// <copyright file="ITypeVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Visitor class for building objects from a type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface ITypeVisitor<T>
    {
        /// <summary>
        /// Visit the boolean type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitBool();

        /// <summary>
        /// Visit the byte type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitByte();

        /// <summary>
        /// Visit the short type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitShort();

        /// <summary>
        /// Visit the ushort type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitUshort();

        /// <summary>
        /// Visit the int type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitInt();

        /// <summary>
        /// Visit the uint type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitUint();

        /// <summary>
        /// Visit the long type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitLong();

        /// <summary>
        /// Visit the ulong type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitUlong();

        /// <summary>
        /// Visit the string type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitString();

        /// <summary>
        /// Visit the option type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitOption(Func<Type, T> recurse, Type optionType, Type innerType);

        /// <summary>
        /// Visit the tuple type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitTuple(Func<Type, T> recurse, Type tupleType, Type innerTypeLeft, Type innerTypeRight);

        /// <summary>
        /// Visit the value tuple type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitValueTuple(Func<Type, T> recurse, Type tupleType, Type innerTypeLeft, Type innerTypeRight);

        /// <summary>
        /// Visit the list type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitList(Func<Type, T> recurse, Type listType, Type innerType);

        /// <summary>
        /// Visit the dictionary type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitDictionary(Func<Type, T> recurse, Type dictType, Type keyType, Type valueType);

        /// <summary>
        /// Visit a class/struct type.
        /// </summary>
        /// <returns>A value.</returns>
        T VisitObject(Func<Type, T> recurse, Type objectType, SortedDictionary<string, Type> fields);
    }
}
