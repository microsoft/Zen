﻿// <copyright file="SolverZ3.cs" company="Microsoft">
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
    using Microsoft.Z3;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Zen solver based on the Z3 SMT solver.
    /// </summary>
    internal class SolverZ3 : ISolver<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>
    {
        private Context context;

        private Solver solver;

        private int nextIndex;

        private Sort BoolSort;

        private Sort ByteSort;

        private Sort ShortSort;

        private Sort IntSort;

        private Sort LongSort;

        private Sort BigIntSort;

        private Sort StringSort;

        private Dictionary<Sort, DatatypeSort> optionSorts;

        private TypeToSortConverter typeToSortConverter;

        public SolverZ3()
        {
            this.nextIndex = 0;
            this.context = new Context();
            var t1 = this.context.MkTactic("simplify");
            var t2 = this.context.MkTactic("solve-eqs");
            var t3 = this.context.MkTactic("smt");
            var tactic = this.context.AndThen(t1, t2, t3);
            this.solver = this.context.MkSolver(tactic);
            this.BoolSort = this.context.MkBoolSort();
            this.ByteSort = this.context.MkBitVecSort(8);
            this.ShortSort = this.context.MkBitVecSort(16);
            this.IntSort = this.context.MkBitVecSort(32);
            this.LongSort = this.context.MkBitVecSort(64);
            this.BigIntSort = this.context.MkIntSort();
            this.StringSort = this.context.StringSort;
            this.typeToSortConverter = new TypeToSortConverter(this);
            this.optionSorts = new Dictionary<Sort, DatatypeSort>();
        }

        private Symbol FreshSymbol()
        {
            return this.context.MkSymbol(nextIndex++);
        }

        public BoolExpr And(BoolExpr x, BoolExpr y)
        {
            return this.context.MkAnd(x, y);
        }

        public BitVecExpr BitwiseAnd(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVAND(x, y);
        }

        public BitVecExpr BitwiseNot(BitVecExpr x)
        {
            return this.context.MkBVNot(x);
        }

        public BitVecExpr BitwiseOr(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVOR(x, y);
        }

        public BitVecExpr BitwiseXor(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVXOR(x, y);
        }

        public (Expr, BoolExpr) CreateBoolVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.BoolSort);
            return (v, (BoolExpr)v);
        }

        public BitVecExpr CreateByteConst(byte b)
        {
            return this.context.MkBV(b, 8);
        }

        public (Expr, BitVecExpr) CreateByteVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.ByteSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateShortConst(short s)
        {
            return this.context.MkBV(s, 16);
        }

        public (Expr, BitVecExpr) CreateShortVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.ShortSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateIntConst(int i)
        {
            return this.context.MkBV(i, 32);
        }

        public (Expr, BitVecExpr) CreateIntVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.IntSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateLongConst(long l)
        {
            return this.context.MkBV(l, 64);
        }

        public (Expr, BitVecExpr) CreateLongVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.LongSort);
            return (v, (BitVecExpr)v);
        }

        public (Expr, BitVecExpr) CreateBitvecVar(object e, uint size)
        {
            var v = this.context.MkConst(FreshSymbol(), this.context.MkBitVecSort(size));
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateBitvecConst(bool[] bits)
        {
            var littleEndian = new bool[bits.Length];

            for (int i = 0; i < bits.Length; i++)
            {
                littleEndian[i] = bits[bits.Length - 1 - i];
            }

            return this.context.MkBV(littleEndian);
        }

        public IntExpr CreateBigIntegerConst(BigInteger bi)
        {
            return this.context.MkInt(bi.ToString());
        }

        public (Expr, IntExpr) CreateBigIntegerVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.BigIntSort);
            return (v, (IntExpr)v);
        }

        public SeqExpr CreateStringConst(string s)
        {
            return this.context.MkString(s);
        }

        public (Expr, SeqExpr) CreateStringVar(object e)
        {
            var v = this.context.MkConst(FreshSymbol(), this.StringSort);
            return (v, (SeqExpr)v);
        }

        public (Expr, ArrayExpr) CreateDictVar(object e)
        {
            var dictType = e.GetType().GetGenericArgumentsCached()[0];
            var typeArguments = dictType.GetGenericArgumentsCached();
            var keyType = typeArguments[0];
            var valueType = typeArguments[1];

            var keySort = GetSortForType(keyType);
            var valueSort = GetSortForType(valueType);
            var optionSort = this.GetOrCreateOptionSort(valueSort);
            var none = this.context.MkApp(optionSort.Constructors[0]);

            // create the new array variable
            var v = this.context.MkArrayConst(FreshSymbol(), keySort, optionSort);

            // need to ensure we constrain the array default to be none
            this.solver.Assert(this.context.MkEq(this.context.MkTermArray(v), none));

            return (v, v);
        }

        public BoolExpr Eq(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkEq(x, y);
        }

        public BoolExpr Eq(IntExpr x, IntExpr y)
        {
            return this.context.MkEq(x, y);
        }

        public BoolExpr Eq(SeqExpr x, SeqExpr y)
        {
            return this.context.MkEq(x, y);
        }

        public BoolExpr Eq(ArrayExpr x, ArrayExpr y)
        {
            return this.context.MkEq(x, y);
        }

        public BoolExpr False()
        {
            return this.context.MkFalse();
        }

        public BoolExpr True()
        {
            return this.context.MkTrue();
        }

        public BoolExpr Iff(BoolExpr x, BoolExpr y)
        {
            return this.context.MkIff(x, y);
        }

        public BoolExpr Ite(BoolExpr g, BoolExpr t, BoolExpr f)
        {
            return (BoolExpr)this.context.MkITE(g, t, f);
        }

        public BitVecExpr Ite(BoolExpr g, BitVecExpr t, BitVecExpr f)
        {
            return (BitVecExpr)this.context.MkITE(g, t, f);
        }

        public IntExpr Ite(BoolExpr g, IntExpr t, IntExpr f)
        {
            return (IntExpr)this.context.MkITE(g, t, f);
        }

        public SeqExpr Ite(BoolExpr g, SeqExpr t, SeqExpr f)
        {
            return (SeqExpr)this.context.MkITE(g, t, f);
        }

        public ArrayExpr Ite(BoolExpr g, ArrayExpr t, ArrayExpr f)
        {
            return (ArrayExpr)this.context.MkITE(g, t, f);
        }

        public BoolExpr LessThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVULE(x, y);
        }

        public BoolExpr LessThanOrEqual(IntExpr x, IntExpr y)
        {
            return this.context.MkLe(x, y);
        }

        public BoolExpr LessThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVSLE(x, y);
        }

        public BoolExpr GreaterThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVUGE(x, y);
        }

        public BoolExpr GreaterThanOrEqual(IntExpr x, IntExpr y)
        {
            return this.context.MkGe(x, y);
        }

        public BoolExpr GreaterThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVSGE(x, y);
        }

        public BoolExpr Not(BoolExpr x)
        {
            return this.context.MkNot(x);
        }

        public BoolExpr Or(BoolExpr x, BoolExpr y)
        {
            return this.context.MkOr(x, y);
        }

        public BitVecExpr Add(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVAdd(x, y);
        }

        public IntExpr Add(IntExpr x, IntExpr y)
        {
            return (IntExpr)this.context.MkAdd(x, y);
        }

        public BitVecExpr Subtract(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVSub(x, y);
        }

        public IntExpr Subtract(IntExpr x, IntExpr y)
        {
            return (IntExpr)this.context.MkSub(x, y);
        }

        public BitVecExpr Multiply(BitVecExpr x, BitVecExpr y)
        {
            return this.context.MkBVMul(x, y);
        }

        public IntExpr Multiply(IntExpr x, IntExpr y)
        {
            return (IntExpr)this.context.MkMul(x, y);
        }

        public SeqExpr Concat(SeqExpr x, SeqExpr y)
        {
            return this.context.MkConcat(x, y);
        }

        public BoolExpr PrefixOf(SeqExpr x, SeqExpr y)
        {
            return this.context.MkPrefixOf(y, x);
        }

        public BoolExpr SuffixOf(SeqExpr x, SeqExpr y)
        {
            return this.context.MkSuffixOf(y, x);
        }

        public BoolExpr Contains(SeqExpr x, SeqExpr y)
        {
            return this.context.MkContains(x, y);
        }

        public SeqExpr ReplaceFirst(SeqExpr x, SeqExpr y, SeqExpr z)
        {
            return this.context.MkReplace(x, y, z);
        }

        public SeqExpr Substring(SeqExpr x, IntExpr y, IntExpr z)
        {
            return this.context.MkExtract(x, y, z);
        }

        public SeqExpr At(SeqExpr x, IntExpr y)
        {
            return this.context.MkAt(x, y);
        }

        public IntExpr Length(SeqExpr x)
        {
            return this.context.MkLength(x);
        }

        public IntExpr IndexOf(SeqExpr x, SeqExpr y, IntExpr z)
        {
            return this.context.MkIndexOf(x, y, z);
        }

        public ArrayExpr DictEmpty(Type keyType, Type valueType)
        {
            var keySort = GetSortForType(keyType);
            var valueSort = GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);
            var none = this.context.MkApp(optionSort.Constructors[0]);
            return this.context.MkConstArray(keySort, none);
        }

        public ArrayExpr DictSet(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> keyExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> valueExpr,
            Type keyType,
            Type valueType)
        {
            var key = ConvertSymbolicValueToExpr(keyExpr, keyType);
            var value = ConvertSymbolicValueToExpr(valueExpr, valueType);
            var valueSort = GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);
            var some = this.context.MkApp(optionSort.Constructors[1], value);
            return this.context.MkStore(arrayExpr, key, some);
        }

        public ArrayExpr DictDelete(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> keyExpr,
            Type keyType,
            Type valueType)
        {
            var key = ConvertSymbolicValueToExpr(keyExpr, keyType);
            var valueSort = GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);
            var none = this.context.MkApp(optionSort.Constructors[0]);
            return this.context.MkStore(arrayExpr, key, none);
        }

        public (BoolExpr, object) DictGet(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> keyExpr,
            Type keyType,
            Type valueType)
        {
            var key = ConvertSymbolicValueToExpr(keyExpr, keyType);
            var valueSort = GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);
            var optionResult = this.context.MkSelect(arrayExpr, key);
            var none = this.context.MkApp(optionSort.Constructors[0]);
            var someAccessor = optionSort.Accessors[1][0];
            var hasValue = this.context.MkNot(this.context.MkEq(optionResult, none));
            return (hasValue, this.context.MkApp(someAccessor, optionResult));
        }

        private DatatypeSort GetOrCreateOptionSort(Sort valueSort)
        {
            if (this.optionSorts.TryGetValue(valueSort, out var optionSort))
            {
                return optionSort;
            }

            var c1 = this.context.MkConstructor("None", "none");
            var c2 = this.context.MkConstructor("Some", "some", new string[] { "some" }, new Sort[] { valueSort });
            var optSort = this.context.MkDatatypeSort("Option_" + valueSort, new Constructor[] { c1, c2 });
            this.optionSorts[valueSort] = optSort;
            return optSort;
        }

        public object Get(Model m, Expr v, Type type)
        {
            var e = m.Evaluate(v, true);
            // Console.WriteLine(e);
            return ConvertExprToObject(e, type);
        }

        private Expr ConvertSymbolicValueToExpr(SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> value, Type type)
        {
            if (value is SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> sb)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(bool), "Internal type mismatch");
                return sb.Value;
            }
            else if (value is SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> sbv)
            {
                CommonUtilities.ValidateIsTrue(
                    ReflectionUtilities.IsUnsignedIntegerType(type) ||
                    ReflectionUtilities.IsSignedIntegerType(type) ||
                    ReflectionUtilities.IsFixedIntegerType(type), "Internal type mismatch");

                return sbv.Value;
            }
            else if (value is SymbolicInteger<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> sbi)
            {
                CommonUtilities.ValidateIsTrue(ReflectionUtilities.IsBigIntegerType(type), "Internal type mismatch");
                return sbi.Value;
            }
            else if (value is SymbolicString<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> sbs)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(string), "Internal type mismatch");
                return sbs.Value;
            }
            else if (value is SymbolicClass<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> sbc)
            {
                Console.WriteLine($"It is a symbolic class.");
                var fieldTypes = ReflectionUtilities.GetAllFieldsAndProperties(sbc.ObjectType);
                var fields = sbc.Fields.ToArray();
                var fieldNames = new string[fields.Length];
                var fieldExprs = new Expr[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    fieldNames[i] = fields[i].Key;
                    fieldExprs[i] = ConvertSymbolicValueToExpr(fields[i].Value, fieldTypes[fieldNames[i]]);
                    Console.WriteLine($"{fieldNames[i]} -> {fieldExprs[i]}");
                }

                var dataTypeSort = (DatatypeSort)GetSortForType(type);
                var objectConstructor = dataTypeSort.Constructors[0];
                Console.WriteLine("am done");
                return this.context.MkApp(objectConstructor, fieldExprs);
            }
            else
            {
                throw new ZenException($"Type {type} used in an unsupported context.");
            }
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> ConvertExprToSymbolicValue(object e, Type type)
        {
            if (type == ReflectionUtilities.BoolType)
            {
                return new SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this, (BoolExpr)e);
            }
            else if (ReflectionUtilities.IsUnsignedIntegerType(type) ||
                ReflectionUtilities.IsSignedIntegerType(type) ||
                ReflectionUtilities.IsFixedIntegerType(type))
            {
                return new SymbolicBitvec<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this, (BitVecExpr)e);
            }
            else if (type == ReflectionUtilities.BigIntType)
            {
                return new SymbolicInteger<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this, (IntExpr)e);
            }
            else if (type == ReflectionUtilities.StringType)
            {
                CommonUtilities.ValidateIsTrue(type == ReflectionUtilities.StringType, "unexpected type");
                return new SymbolicString<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this, (SeqExpr)e);
            }
            else
            {
                var dataTypeSort = (DatatypeSort)GetSortForType(type);
                var fields = ReflectionUtilities.GetAllFieldsAndProperties(type).ToArray();
                var result = ImmutableSortedDictionary<string, SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>>.Empty;
                for (int i = 0; i < fields.Length; i++)
                {
                    var fieldName = fields[i].Key;
                    var fieldAccessor = dataTypeSort.Accessors[0][i];
                    var fieldSymbolicValue = ConvertExprToSymbolicValue(this.context.MkApp(fieldAccessor, (Expr)e), fields[i].Value);
                    result = result.Add(fieldName, fieldSymbolicValue);
                }

                return new SymbolicClass<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(type, this, result);
            }
        }

        private object ConvertExprToObject(Expr e, Type type)
        {
            if (e.Sort == this.BoolSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(bool), "Internal type mismatch");
                return bool.Parse(e.ToString());
            }
            else if (e.Sort == this.ByteSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(byte), "Internal type mismatch");
                return byte.Parse(e.ToString());
            }
            else if (e.Sort == this.ShortSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(short) || type == typeof(ushort), "Internal type mismatch");
                var result = ushort.Parse(e.ToString());
                return type == typeof(ushort) ? result : (object)(short)result;
            }
            else if (e.Sort == this.IntSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(int) || type == typeof(uint), "Internal type mismatch");
                var result = uint.Parse(e.ToString());
                return type == typeof(uint) ? result : (object)(int)result;
            }
            else if (e.Sort == this.BigIntSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(BigInteger), "Internal type mismatch");
                return BigInteger.Parse(e.ToString());
            }
            else if (e.Sort == this.StringSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(string), "Internal type mismatch");
                return CommonUtilities.ConvertZ3StringToCSharp(e.ToString());
            }
            else if (e.Sort == this.LongSort)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(long) || type == typeof(ulong), "Internal type mismatch");
                var result = ulong.Parse(e.ToString());
                return type == typeof(ulong) ? result : (object)(long)result;
            }
            else if (e.IsConstantArray)
            {
                CommonUtilities.ValidateIsTrue(ReflectionUtilities.IsIDictType(type), "Internal type mismatch");
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                var c = typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetConstructor(new Type[] { });
                return c.Invoke(CommonUtilities.EmptyArray);
            }
            else if (e.IsStore)
            {
                var typeParameters = type.GetGenericArgumentsCached();
                var keyType = typeParameters[0];
                var valueType = typeParameters[1];
                var arrayExpr = e.Args[0];
                var keyExpr = e.Args[1];
                var valueExpr = e.Args[2];
                var dict = ConvertExprToObject(arrayExpr, type);
                var key = ConvertExprToObject(keyExpr, keyType);
                var value = ConvertExprToObject(valueExpr, valueType);
                var m = typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetMethod("Add", new Type[] { keyType, valueType });
                m.Invoke(dict, new object[] { key, value });
                return dict;
            }
            else if (e.IsApp)
            {
                if (this.typeToSortConverter.ObjectAppNames.Contains(e.FuncDecl.Name.ToString()))
                {
                    var fields = ReflectionUtilities.GetAllFieldsAndProperties(type).ToArray();
                    var fieldNames = new string[fields.Length];
                    var fieldValues = new object[fields.Length];
                    for (int i = 0; i < fields.Length; i++)
                    {
                        fieldNames[i] = fields[i].Key;
                        fieldValues[i] = ConvertExprToObject(e.Args[i], fields[i].Value);
                    }

                    return ReflectionUtilities.CreateInstance(type, fieldNames, fieldValues);
                }
                else if (e.Args.Length == 0)
                {
                    return typeof(Option).GetMethod("None").MakeGenericMethod(type).Invoke(null, CommonUtilities.EmptyArray);
                }
                else
                {
                    return ConvertExprToObject(e.Args[0], type);
                }
            }
            else
            {
                // must be a fixed width integer
                var bytes = BigInteger.Parse(e.ToString()).ToByteArray();
                RemoveTrailingZeroes(ref bytes);
                Array.Reverse(bytes);
                return type.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { bytes });
            }
        }

        public Model Satisfiable(BoolExpr x)
        {
            this.solver.Assert(x);
            var status = this.solver.Check();
            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            return this.solver.Model;
        }

        private static void RemoveTrailingZeroes(ref byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
        }

        private Sort GetSortForType(Type type)
        {
            return ReflectionUtilities.ApplyTypeVisitor(this.typeToSortConverter, type, new Unit());
        }

        /// <summary>
        /// Convert a C# type into a Z3 sort.
        /// </summary>
        internal class TypeToSortConverter : ITypeVisitor<Sort, Unit>
        {
            private SolverZ3 solver;

            private Dictionary<Type, Sort> typeToSort;

            public ISet<string> ObjectAppNames;

            public TypeToSortConverter(SolverZ3 solver)
            {
                this.solver = solver;
                this.ObjectAppNames = new HashSet<string>();
                this.typeToSort = new Dictionary<Type, Sort>();
                this.typeToSort[typeof(bool)] = this.solver.BoolSort;
                this.typeToSort[typeof(byte)] = this.solver.ByteSort;
                this.typeToSort[typeof(short)] = this.solver.ShortSort;
                this.typeToSort[typeof(ushort)] = this.solver.ShortSort;
                this.typeToSort[typeof(int)] = this.solver.IntSort;
                this.typeToSort[typeof(uint)] = this.solver.IntSort;
                this.typeToSort[typeof(long)] = this.solver.LongSort;
                this.typeToSort[typeof(ulong)] = this.solver.LongSort;
                this.typeToSort[typeof(BigInteger)] = this.solver.BigIntSort;
                this.typeToSort[typeof(string)] = this.solver.StringSort;
            }

            public Sort VisitBigInteger()
            {
                return this.solver.BigIntSort;
            }

            public Sort VisitBool()
            {
                return this.solver.BoolSort;
            }

            public Sort VisitByte()
            {
                return this.solver.ByteSort;
            }

            [ExcludeFromCodeCoverage]
            public Sort VisitDictionary(Type dictionaryType, Type keyType, Type valueType)
            {
                throw new ZenException("Can not use map type in another map.");
            }

            public Sort VisitFixedInteger(Type intType)
            {
                if (this.typeToSort.TryGetValue(intType, out var sort))
                {
                    return sort;
                }

                int size = ((dynamic)Activator.CreateInstance(intType, 0L)).Size;
                var bitvecSort = this.solver.context.MkBitVecSort((uint)size);
                this.typeToSort[intType] = bitvecSort;
                return bitvecSort;
            }

            public Sort VisitInt()
            {
                return this.solver.IntSort;
            }

            [ExcludeFromCodeCoverage]
            public Sort VisitList(Func<Type, Unit, Sort> recurse, Type listType, Type innerType, Unit parameter)
            {
                throw new ZenException("Can not use finite sequence type in another map.");
            }

            public Sort VisitLong()
            {
                return this.solver.LongSort;
            }

            public Sort VisitObject(Func<Type, Unit, Sort> recurse, Type objectType, SortedDictionary<string, Type> objectFields, Unit parameter)
            {
                if (this.typeToSort.TryGetValue(objectType, out var sort))
                {
                    return sort;
                }

                var fields = objectFields.ToArray();
                var fieldNames = new string[fields.Length];
                var fieldSorts = new Sort[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    fieldNames[i] = fields[i].Key;
                    fieldSorts[i] = recurse((Type)fields[i].Value, new Unit());
                }

                var objectConstructor = this.solver.context.MkConstructor(objectType.ToString(), "value", fieldNames, fieldSorts);
                var objectSort = this.solver.context.MkDatatypeSort(objectType.ToString(), new Constructor[] { objectConstructor });
                this.ObjectAppNames.Add(objectType.ToString());
                this.typeToSort[objectType] = objectSort;
                return objectSort;
            }

            public Sort VisitShort()
            {
                return this.solver.ShortSort;
            }

            public Sort VisitString()
            {
                return this.solver.StringSort;
            }

            public Sort VisitUint()
            {
                return this.solver.IntSort;
            }

            public Sort VisitUlong()
            {
                return this.solver.LongSort;
            }

            public Sort VisitUshort()
            {
                return this.solver.ShortSort;
            }
        }
    }
}
