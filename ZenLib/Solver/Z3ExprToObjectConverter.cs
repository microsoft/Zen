// <copyright file="Z3ExprToObjectConverter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
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

        public object VisitDictionary(Func<Type, Expr, object> recurse, Type dictionaryType, Type keyType, Type valueType, Expr parameter)
        {
            if (parameter.IsConstantArray)
            {
                return CreateEmptyDictionary(keyType, valueType);
            }
            else if (parameter.IsStore)
            {
                var arrayExpr = parameter.Args[0];
                var keyExpr = parameter.Args[1];
                var valueExpr = parameter.Args[2];
                var dict = this.solver.ConvertExprToObject(arrayExpr, dictionaryType);
                var key = this.solver.ConvertExprToObject(keyExpr, keyType);
                var value = this.solver.ConvertExprToObject(valueExpr, valueType);
                return AddKeyValuePair(dict, key, value, keyType, valueType, valueExpr);
            }
            else if (parameter.IsApp && parameter.FuncDecl.Name.ToString() == "map")
            {
                var lambda = parameter.FuncDecl.Parameters[0].FuncDecl.Name.ToString();
                var e1 = this.solver.ConvertExprToObject(parameter.Args[0], dictionaryType);
                var e2 = this.solver.ConvertExprToObject(parameter.Args[1], dictionaryType);
                var methodName = (lambda == "and") ? "DictionaryIntersect" : "DictionaryUnion";
                var m = typeof(CommonUtilities).GetMethodCached(methodName).MakeGenericMethod(keyType);
                return m.Invoke(null, new object[] { e1, e2 });
            }
            else if (parameter.IsApp && parameter.FuncDecl.Name.ToString() == "as-array")
            {
                var lambda = parameter.FuncDecl.Parameters[0].FuncDecl;
                var interpretation = this.solver.Solver.Model.FuncInterp(lambda);
                var elseCase = interpretation.Else.ToString();
                CommonUtilities.ValidateIsTrue(elseCase == "false" || elseCase == "None", "Internal error.");

                var dict = CreateEmptyDictionary(keyType, valueType);
                for (int i = 0; i < interpretation.NumEntries; i++)
                {
                    var keyExpr = interpretation.Entries[i].Args[0];
                    var valueExpr = interpretation.Entries[i].Value;
                    var key = this.solver.ConvertExprToObject(keyExpr, keyType);
                    var value = this.solver.ConvertExprToObject(valueExpr, valueType);
                    dict = AddKeyValuePair(dict, key, value, keyType, valueType, valueExpr);
                }

                return dict;
            }
            else
            {
                throw new ZenException("Internal error. Unexpected Z3 AST type.");
            }
        }

        private object CreateEmptyDictionary(Type keyType, Type valueType)
        {
            var m = typeof(ImmutableDictionary).GetMethod("Create", new Type[] { }).MakeGenericMethod(keyType, valueType);
            return m.Invoke(null, CommonUtilities.EmptyArray);
        }

        private object AddKeyValuePair(object dict, object key, object value, Type keyType, Type valueType, Expr valueExpr)
        {
            // for sets, don't add the key when the value is false.
            if (valueType == typeof(SetUnit) && valueExpr.IsFalse)
            {
                return dict;
            }
            else
            {
                var m = typeof(ImmutableDictionary<,>).MakeGenericType(keyType, valueType).GetMethod("SetItem", new Type[] { keyType, valueType });
                return m.Invoke(dict, new object[] { key, value });
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
