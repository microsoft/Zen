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
    [ExcludeFromCodeCoverage] // Z3 changes its internal representation frequently.
    internal class Z3ExprToObjectConverter : TypeVisitor<object, Expr>
    {
        /// <summary>
        /// The solver.
        /// </summary>
        private SolverZ3 solver;

        /// <summary>
        /// Creates a new instance of the <see cref="Z3ExprToObjectConverter"/> class.
        /// </summary>
        /// <param name="solver">The Z3 solver.</param>
        public Z3ExprToObjectConverter(SolverZ3 solver)
        {
            this.solver = solver;
        }

        /// <summary>
        /// Convert an expression to a C# object of the given type.
        /// </summary>
        /// <param name="e">The Z3 expression.</param>
        /// <param name="type">The C# type.</param>
        /// <returns>A C# object.</returns>
        public object Convert(Expr e, Type type)
        {
            if (e.IsApp && e.FuncDecl.Name.ToString() == "Some")
            {
                return Convert(e.Args[0], type);
            }

            return this.Visit(type, e);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitBigInteger(Expr parameter)
        {
            return BigInteger.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitReal(Expr parameter)
        {
            Contract.Assert(parameter.IsRatNum);
            var e = (RatNum)parameter;
            var numerator = BigInteger.Parse(e.Numerator.ToString());
            var denominator = BigInteger.Parse(e.Denominator.ToString());
            return new Real(numerator, denominator);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitBool(Expr parameter)
        {
            if (parameter.IsEq)
            {
                var left = (char)Convert(parameter.Args[0], typeof(char));
                var right = (char)Convert(parameter.Args[1], typeof(char));
                return left.Equals(right);
            }
            else
            {
                return bool.Parse(parameter.ToString());
            }
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitByte(Expr parameter)
        {
            return byte.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitChar(Expr parameter)
        {
            Contract.Assert(parameter.IsApp && parameter.FuncDecl.Name.ToString() == "Char");
            return (char)parameter.FuncDecl.Parameters[0].Int;

            /* Contract.Assert(parameter.IsApp);
            Contract.Assert(parameter.FuncDecl.Name.ToString() == "char.from_bv");
            return new ZenLib.Char(int.Parse(parameter.Args[0].ToString())); */
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitMap(Type mapType, Type keyType, Type valueType, Expr parameter)
        {
            if (parameter.IsConstantArray)
            {
                if (parameter.Args[0].IsTrue)
                {
                    var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    var c = typeof(Map<,>)
                        .MakeGenericType(keyType, valueType)
                        .GetConstructor(flags, null, new Type[] { typeof(bool) }, null);
                    return c.Invoke(new object[] { true });
                }
                else
                {
                    var c = typeof(Map<,>).MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                    return c.Invoke(CommonUtilities.EmptyArray);
                }
            }
            else if (parameter.IsStore)
            {
                var arrayExpr = parameter.Args[0];
                var keyExpr = parameter.Args[1];
                var valueExpr = parameter.Args[2];
                var dict = Convert(arrayExpr, mapType);
                var key = Convert(keyExpr, keyType);
                var value = Convert(valueExpr, valueType);
                return AddKeyValuePair(dict, key, value, keyType, valueType);
            }
            else if (parameter.IsApp && parameter.FuncDecl.Name.ToString() == "map")
            {
                var lambda = parameter.FuncDecl.Parameters[0].FuncDecl.Name.ToString();
                var e1 = Convert(parameter.Args[0], mapType);
                var e2 = Convert(parameter.Args[1], mapType);
                var methodName = (lambda == "and") ? "DictionaryIntersect" : "DictionaryUnion";
                var m = typeof(CommonUtilities).GetMethodCached(methodName).MakeGenericMethod(keyType);
                return m.Invoke(null, new object[] { e1, e2 });
            }
            else
            {
                Contract.Assert(parameter.IsApp);
                Contract.Assert(parameter.FuncDecl.Name.ToString() == "as-array");

                var lambda = parameter.FuncDecl.Parameters[0].FuncDecl;
                var interpretation = this.solver.Solver.Model.FuncInterp(lambda);
                var elseCase = interpretation.Else.ToString();
                Contract.Assert(elseCase == "false" || elseCase == "None", "Internal error.");

                var dictConstructor = typeof(Map<,>).MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                var dict = dictConstructor.Invoke(new object[] { });

                // var dict = CreateEmptyDictionary(keyType, valueType);
                for (int i = 0; i < interpretation.NumEntries; i++)
                {
                    var keyExpr = interpretation.Entries[i].Args[0];
                    var valueExpr = interpretation.Entries[i].Value;

                    if (valueExpr.IsApp && valueExpr.FuncDecl.Name.ToString() == "None")
                    {
                        continue;
                    }

                    var key = Convert(keyExpr, keyType);
                    var value = Convert(valueExpr, valueType);
                    dict = AddKeyValuePair(dict, key, value, keyType, valueType);
                }

                return dict;
            }
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        [ExcludeFromCodeCoverage]
        public override object VisitConstMap(Type mapType, Type keyType, Type valueType, Expr parameter)
        {
            throw new ZenUnreachableException();
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitFixedInteger(Type intType, Expr parameter)
        {
            var bytes = BigInteger.Parse(parameter.ToString()).ToByteArray();
            int lastIndex = Array.FindLastIndex(bytes, b => b != 0);
            Array.Resize(ref bytes, lastIndex + 1);
            Array.Reverse(bytes);
            return intType.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { bytes });
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitInt(Expr parameter)
        {
            return (int)uint.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        [ExcludeFromCodeCoverage]
        public override object VisitList(Type listType, Type innerType, Expr parameter)
        {
            throw new ZenException("Invalid use of list in map or set type.");
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitLong(Expr parameter)
        {
            return (long)ulong.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitObject(Type objectType, SortedDictionary<string, Type> fields, Expr parameter)
        {
            var fieldsAndTypes = fields.ToArray();
            var fieldNames = new string[fieldsAndTypes.Length];
            var fieldValues = new object[fieldsAndTypes.Length];
            for (int i = 0; i < fieldsAndTypes.Length; i++)
            {
                fieldNames[i] = fieldsAndTypes[i].Key;
                fieldValues[i] = Convert(parameter.Args[i], fieldsAndTypes[i].Value);
            }

            return ReflectionUtilities.CreateInstance(objectType, fieldNames, fieldValues);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitShort(Expr parameter)
        {
            return (short)ushort.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitString(Expr parameter)
        {
            var result = (Seq<char>)Convert(parameter, ReflectionUtilities.UnicodeSequenceType);
            return Seq.AsString(result);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitUint(Expr parameter)
        {
            return uint.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitUlong(Expr parameter)
        {
            return ulong.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitUshort(Expr parameter)
        {
            return ushort.Parse(parameter.ToString());
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The Z3 expression.</param>
        /// <returns>A C# object.</returns>
        public override object VisitSeq(Type sequenceType, Type innerType, Expr parameter)
        {
            if (parameter.IsApp && parameter.FuncDecl.Name.ToString() == "seq.empty")
            {
                var c = sequenceType.GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }
            else if (parameter.IsApp && parameter.FuncDecl.Name.ToString() == "seq.unit")
            {
                var value = Convert(parameter.Args[0], innerType);
                var c = sequenceType.GetConstructor(new Type[] { innerType });
                return c.Invoke(new object[] { value });
            }
            else if (parameter.IsString)
            {
                return Seq.FromString(CommonUtilities.ConvertZ3StringToCSharp(parameter.ToString()));
            }
            else if (parameter.IsApp && parameter.FuncDecl.Name.ToString() == "str.++")
            {
                var s1 = (Seq<char>)Convert(parameter.Args[0], sequenceType);
                var s2 = (Seq<char>)Convert(parameter.Args[1], sequenceType);
                return s1.Concat(s2);
            }
            else
            {
                Contract.Assert(parameter.IsApp);
                Contract.Assert(parameter.FuncDecl.Name.ToString() == "seq.++");

                var seq1 = Convert(parameter.Args[0], sequenceType);
                var seq2 = Convert(parameter.Args[1], sequenceType);
                var m = sequenceType.GetMethod("Concat");
                return m.Invoke(seq1, new object[] { seq2 });
            }
        }

        /// <summary>
        /// Add a key value pair to a map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        private object AddKeyValuePair(object map, object key, object value, Type keyType, Type valueType)
        {
            // Contract.Assert(!valueExpr.IsFalse);
            // for sets, don't add the key when the value is false.
            /* if (valueType == typeof(SetUnit) && valueExpr.IsFalse)
            {
                return dict;
            }
            else
            {
                var m = typeof(ImmutableDictionary<,>).MakeGenericType(keyType, valueType).GetMethod("SetItem", new Type[] { keyType, valueType });
                return m.Invoke(dict, new object[] { key, value });
            } */

            var m = typeof(Map<,>).MakeGenericType(keyType, valueType).GetMethod("Set", new Type[] { keyType, valueType });
            return m.Invoke(map, new object[] { key, value });
        }
    }
}
