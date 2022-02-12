﻿// <copyright file="SolverZ3.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;
    using Microsoft.Z3;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Zen solver based on the Z3 SMT solver.
    /// </summary>
    internal class SolverZ3 : ISolver<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>
    {
        internal Context Context;

        internal Solver Solver;

        private int nextIndex;

        internal Sort BoolSort;

        internal Sort ByteSort;

        internal Sort ShortSort;

        internal Sort IntSort;

        internal Sort LongSort;

        internal Sort BigIntSort;

        internal Sort StringSort;

        private Dictionary<Sort, DatatypeSort> optionSorts;

        private Z3TypeToSortConverter typeToSortConverter;

        private Z3ExprToSymbolicValueConverter exprToSymbolicValueConverter;

        public SolverZ3()
        {
            this.nextIndex = 0;
            this.Context = new Context();
            var t1 = this.Context.MkTactic("simplify");
            var t2 = this.Context.MkTactic("solve-eqs");
            var t3 = this.Context.MkTactic("smt");
            var tactic = this.Context.AndThen(t1, t2, t3);
            this.Solver = this.Context.MkSolver(tactic);
            this.BoolSort = this.Context.MkBoolSort();
            this.ByteSort = this.Context.MkBitVecSort(8);
            this.ShortSort = this.Context.MkBitVecSort(16);
            this.IntSort = this.Context.MkBitVecSort(32);
            this.LongSort = this.Context.MkBitVecSort(64);
            this.BigIntSort = this.Context.MkIntSort();
            this.StringSort = this.Context.StringSort;
            this.typeToSortConverter = new Z3TypeToSortConverter(this);
            this.exprToSymbolicValueConverter = new Z3ExprToSymbolicValueConverter(this);
            this.optionSorts = new Dictionary<Sort, DatatypeSort>();
        }

        private Symbol FreshSymbol()
        {
            return this.Context.MkSymbol(nextIndex++);
        }

        public BoolExpr And(BoolExpr x, BoolExpr y)
        {
            return this.Context.MkAnd(x, y);
        }

        public BitVecExpr BitwiseAnd(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVAND(x, y);
        }

        public BitVecExpr BitwiseNot(BitVecExpr x)
        {
            return this.Context.MkBVNot(x);
        }

        public BitVecExpr BitwiseOr(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVOR(x, y);
        }

        public BitVecExpr BitwiseXor(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVXOR(x, y);
        }

        public (Expr, BoolExpr) CreateBoolVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.BoolSort);
            return (v, (BoolExpr)v);
        }

        public BitVecExpr CreateByteConst(byte b)
        {
            return this.Context.MkBV(b, 8);
        }

        public (Expr, BitVecExpr) CreateByteVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.ByteSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateShortConst(short s)
        {
            return this.Context.MkBV(s, 16);
        }

        public (Expr, BitVecExpr) CreateShortVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.ShortSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateIntConst(int i)
        {
            return this.Context.MkBV(i, 32);
        }

        public (Expr, BitVecExpr) CreateIntVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.IntSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateLongConst(long l)
        {
            return this.Context.MkBV(l, 64);
        }

        public (Expr, BitVecExpr) CreateLongVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.LongSort);
            return (v, (BitVecExpr)v);
        }

        public (Expr, BitVecExpr) CreateBitvecVar(object e, uint size)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.Context.MkBitVecSort(size));
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateBitvecConst(bool[] bits)
        {
            var littleEndian = new bool[bits.Length];

            for (int i = 0; i < bits.Length; i++)
            {
                littleEndian[i] = bits[bits.Length - 1 - i];
            }

            return this.Context.MkBV(littleEndian);
        }

        public IntExpr CreateBigIntegerConst(BigInteger bi)
        {
            return this.Context.MkInt(bi.ToString());
        }

        public (Expr, IntExpr) CreateBigIntegerVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.BigIntSort);
            return (v, (IntExpr)v);
        }

        public SeqExpr CreateStringConst(string s)
        {
            return this.Context.MkString(s);
        }

        public (Expr, SeqExpr) CreateStringVar(object e)
        {
            var v = this.Context.MkConst(FreshSymbol(), this.StringSort);
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
            var none = this.Context.MkApp(optionSort.Constructors[0]);

            // create the new array variable
            var v = this.Context.MkArrayConst(FreshSymbol(), keySort, optionSort);

            // need to ensure we constrain the array default to be none
            this.Solver.Assert(this.Context.MkEq(this.Context.MkTermArray(v), none));

            return (v, v);
        }

        public BoolExpr Eq(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkEq(x, y);
        }

        public BoolExpr Eq(IntExpr x, IntExpr y)
        {
            return this.Context.MkEq(x, y);
        }

        public BoolExpr Eq(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkEq(x, y);
        }

        public BoolExpr Eq(ArrayExpr x, ArrayExpr y)
        {
            return this.Context.MkEq(x, y);
        }

        public BoolExpr False()
        {
            return this.Context.MkFalse();
        }

        public BoolExpr True()
        {
            return this.Context.MkTrue();
        }

        public BoolExpr Iff(BoolExpr x, BoolExpr y)
        {
            return this.Context.MkIff(x, y);
        }

        public BoolExpr Ite(BoolExpr g, BoolExpr t, BoolExpr f)
        {
            return (BoolExpr)this.Context.MkITE(g, t, f);
        }

        public BitVecExpr Ite(BoolExpr g, BitVecExpr t, BitVecExpr f)
        {
            return (BitVecExpr)this.Context.MkITE(g, t, f);
        }

        public IntExpr Ite(BoolExpr g, IntExpr t, IntExpr f)
        {
            return (IntExpr)this.Context.MkITE(g, t, f);
        }

        public SeqExpr Ite(BoolExpr g, SeqExpr t, SeqExpr f)
        {
            return (SeqExpr)this.Context.MkITE(g, t, f);
        }

        public ArrayExpr Ite(BoolExpr g, ArrayExpr t, ArrayExpr f)
        {
            return (ArrayExpr)this.Context.MkITE(g, t, f);
        }

        public BoolExpr LessThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVULE(x, y);
        }

        public BoolExpr LessThanOrEqual(IntExpr x, IntExpr y)
        {
            return this.Context.MkLe(x, y);
        }

        public BoolExpr LessThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVSLE(x, y);
        }

        public BoolExpr GreaterThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVUGE(x, y);
        }

        public BoolExpr GreaterThanOrEqual(IntExpr x, IntExpr y)
        {
            return this.Context.MkGe(x, y);
        }

        public BoolExpr GreaterThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVSGE(x, y);
        }

        public BoolExpr Not(BoolExpr x)
        {
            return this.Context.MkNot(x);
        }

        public BoolExpr Or(BoolExpr x, BoolExpr y)
        {
            return this.Context.MkOr(x, y);
        }

        public BitVecExpr Add(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVAdd(x, y);
        }

        public IntExpr Add(IntExpr x, IntExpr y)
        {
            return (IntExpr)this.Context.MkAdd(x, y);
        }

        public BitVecExpr Subtract(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVSub(x, y);
        }

        public IntExpr Subtract(IntExpr x, IntExpr y)
        {
            return (IntExpr)this.Context.MkSub(x, y);
        }

        public BitVecExpr Multiply(BitVecExpr x, BitVecExpr y)
        {
            return this.Context.MkBVMul(x, y);
        }

        public IntExpr Multiply(IntExpr x, IntExpr y)
        {
            return (IntExpr)this.Context.MkMul(x, y);
        }

        public SeqExpr Concat(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkConcat(x, y);
        }

        public BoolExpr PrefixOf(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkPrefixOf(y, x);
        }

        public BoolExpr SuffixOf(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkSuffixOf(y, x);
        }

        public BoolExpr Contains(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkContains(x, y);
        }

        public SeqExpr ReplaceFirst(SeqExpr x, SeqExpr y, SeqExpr z)
        {
            return this.Context.MkReplace(x, y, z);
        }

        public SeqExpr Substring(SeqExpr x, IntExpr y, IntExpr z)
        {
            return this.Context.MkExtract(x, y, z);
        }

        public SeqExpr At(SeqExpr x, IntExpr y)
        {
            return this.Context.MkAt(x, y);
        }

        public IntExpr Length(SeqExpr x)
        {
            return this.Context.MkLength(x);
        }

        public IntExpr IndexOf(SeqExpr x, SeqExpr y, IntExpr z)
        {
            return this.Context.MkIndexOf(x, y, z);
        }

        public ArrayExpr DictEmpty(Type keyType, Type valueType)
        {
            var keySort = GetSortForType(keyType);
            var valueSort = GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);
            var none = this.Context.MkApp(optionSort.Constructors[0]);
            return this.Context.MkConstArray(keySort, none);
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
            var some = this.Context.MkApp(optionSort.Constructors[1], value);
            return this.Context.MkStore(arrayExpr, key, some);
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
            var none = this.Context.MkApp(optionSort.Constructors[0]);
            return this.Context.MkStore(arrayExpr, key, none);
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
            var optionResult = this.Context.MkSelect(arrayExpr, key);
            var none = this.Context.MkApp(optionSort.Constructors[0]);
            var someAccessor = optionSort.Accessors[1][0];
            var hasValue = this.Context.MkNot(this.Context.MkEq(optionResult, none));
            return (hasValue, this.Context.MkApp(someAccessor, optionResult));
        }

        public Sort GetSortForType(Type type)
        {
            return ReflectionUtilities.ApplyTypeVisitor(this.typeToSortConverter, type, new Unit());
        }

        private DatatypeSort GetOrCreateOptionSort(Sort valueSort)
        {
            if (this.optionSorts.TryGetValue(valueSort, out var optionSort))
            {
                return optionSort;
            }

            var c1 = this.Context.MkConstructor("None", "none");
            var c2 = this.Context.MkConstructor("Some", "some", new string[] { "some" }, new Sort[] { valueSort });
            var optSort = this.Context.MkDatatypeSort("Option_" + valueSort, new Constructor[] { c1, c2 });
            this.optionSorts[valueSort] = optSort;
            return optSort;
        }

        public object Get(Model m, Expr v, Type type)
        {
            var e = m.Evaluate(v, true);
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
                var fieldTypes = ReflectionUtilities.GetAllFieldAndPropertyTypes(sbc.ObjectType);
                var fields = sbc.Fields.ToArray();
                var fieldNames = new string[fields.Length];
                var fieldExprs = new Expr[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    fieldNames[i] = fields[i].Key;
                    fieldExprs[i] = ConvertSymbolicValueToExpr(fields[i].Value, fieldTypes[fieldNames[i]]);
                }

                var dataTypeSort = (DatatypeSort)GetSortForType(type);
                var objectConstructor = dataTypeSort.Constructors[0];
                return this.Context.MkApp(objectConstructor, fieldExprs);
            }
            else
            {
                throw new ZenException($"Type {type} used in an unsupported context.");
            }
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> ConvertExprToSymbolicValue(object e, Type type)
        {
            return ReflectionUtilities.ApplyTypeVisitor(this.exprToSymbolicValueConverter, type, (Expr)e);
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
                    var fields = ReflectionUtilities.GetAllFieldAndPropertyTypes(type).ToArray();
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
            this.Solver.Assert(x);
            var status = this.Solver.Check();
            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            return this.Solver.Model;
        }

        private static void RemoveTrailingZeroes(ref byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
        }
    }
}
