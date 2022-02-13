// <copyright file="Z3ExprToObjectConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using Microsoft.Z3;

    /// <summary>
    /// Convert a Z3 Expr to a C# object.
    /// </summary>
    internal class Z3ExprToObjectConverter : ITypeVisitor<object, Expr>
    {
        private SolverZ3 solver;

        public Z3ExprToObjectConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        public object VisitBigInteger(Expr parameter)
        {
            return BigInteger.Parse(parameter.ToString());
        }

        public object VisitBool(Expr parameter)
        {
            return bool.Parse(parameter.ToString());
        }

        public object VisitByte(Expr parameter)
        {
            return byte.Parse(parameter.ToString());
        }

        public object VisitDictionary(Type dictionaryType, Type keyType, Type valueType, Expr parameter)
        {
            if (parameter.IsConstantArray)
            {
                var c = typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }
            else if (parameter.IsStore)
            {
                var arrayExpr = parameter.Args[0];
                var keyExpr = parameter.Args[1];
                var valueExpr = parameter.Args[2];
                var dict = this.solver.ConvertExprToObject(arrayExpr, dictionaryType);
                var key = this.solver.ConvertExprToObject(keyExpr, keyType);
                var value = this.solver.ConvertExprToObject(valueExpr, valueType);
                var m = typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetMethod("Add", new Type[] { keyType, valueType });
                m.Invoke(dict, new object[] { key, value });
                return dict;
            }
            else
            {
                throw new ZenException("Internal error. Unexpected Z3 AST type.");
            }
        }

        public object VisitFixedInteger(Type intType, Expr parameter)
        {
            var bytes = BigInteger.Parse(parameter.ToString()).ToByteArray();
            int lastIndex = Array.FindLastIndex(bytes, b => b != 0);
            Array.Resize(ref bytes, lastIndex + 1);
            Array.Reverse(bytes);
            return intType.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { bytes });
        }

        public object VisitInt(Expr parameter)
        {
            return (int)uint.Parse(parameter.ToString());
        }

        [ExcludeFromCodeCoverage]
        public object VisitList(Func<Type, Expr, object> recurse, Type listType, Type innerType, Expr parameter)
        {
            throw new ZenException("Invalid use of list in map or set type.");
        }

        public object VisitLong(Expr parameter)
        {
            return (long)ulong.Parse(parameter.ToString());
        }

        public object VisitObject(Func<Type, Expr, object> recurse, Type objectType, SortedDictionary<string, Type> fields, Expr parameter)
        {
            var fieldsAndTypes = fields.ToArray();
            var fieldNames = new string[fieldsAndTypes.Length];
            var fieldValues = new object[fieldsAndTypes.Length];
            for (int i = 0; i < fieldsAndTypes.Length; i++)
            {
                fieldNames[i] = fieldsAndTypes[i].Key;
                fieldValues[i] = this.solver.ConvertExprToObject(parameter.Args[i], fieldsAndTypes[i].Value);
            }

            return ReflectionUtilities.CreateInstance(objectType, fieldNames, fieldValues);
        }

        public object VisitShort(Expr parameter)
        {
            return (short)ushort.Parse(parameter.ToString());
        }

        public object VisitString(Expr parameter)
        {
            return CommonUtilities.ConvertZ3StringToCSharp(parameter.ToString());
        }

        public object VisitUint(Expr parameter)
        {
            return uint.Parse(parameter.ToString());
        }

        public object VisitUlong(Expr parameter)
        {
            return ulong.Parse(parameter.ToString());
        }

        public object VisitUshort(Expr parameter)
        {
            return ushort.Parse(parameter.ToString());
        }
    }
}
