// <copyright file="SolverZ3.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.Z3;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Zen solver based on the Z3 SMT solver.
    /// </summary>
    internal class SolverZ3 : ISolver<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>
    {
        internal Context Context;

        internal Params Params;

        internal Solver Solver;

        private int nextIndex;

        internal Sort BoolSort;

        internal Sort ByteSort;

        internal Sort ShortSort;

        internal Sort IntSort;

        internal Sort LongSort;

        internal Sort BigIntSort;

        internal Sort StringSort;

        internal Dictionary<Sort, DatatypeSort> OptionSorts;

        internal Z3TypeToSortConverter TypeToSortConverter;

        internal Z3ExprToSymbolicValueConverter ExprToSymbolicValueConverter;

        internal Z3SymbolicValueToExprConverter SymbolicValueToExprConverter;

        internal Z3ExprToObjectConverter ExprToObjectConverter;

        public SolverZ3()
        {
            this.nextIndex = 0;
            this.Context = new Context();
            this.Params = this.Context.MkParams();
            this.Params.Add("compact", false);
            var t1 = this.Context.MkTactic("simplify");
            var t2 = this.Context.MkTactic("solve-eqs");
            var t3 = this.Context.MkTactic("smt");
            var tactic = this.Context.AndThen(t1, t2, t3);
            this.Solver = this.Context.MkSolver(tactic);
            this.Solver.Parameters = this.Params;
            this.BoolSort = this.Context.MkBoolSort();
            this.ByteSort = this.Context.MkBitVecSort(8);
            this.ShortSort = this.Context.MkBitVecSort(16);
            this.IntSort = this.Context.MkBitVecSort(32);
            this.LongSort = this.Context.MkBitVecSort(64);
            this.BigIntSort = this.Context.MkIntSort();
            this.StringSort = this.Context.StringSort;
            this.TypeToSortConverter = new Z3TypeToSortConverter(this);
            this.ExprToSymbolicValueConverter = new Z3ExprToSymbolicValueConverter(this);
            this.SymbolicValueToExprConverter = new Z3SymbolicValueToExprConverter(this);
            this.ExprToObjectConverter = new Z3ExprToObjectConverter(this);
            this.OptionSorts = new Dictionary<Sort, DatatypeSort>();
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

        public (Expr, SeqExpr) CreateSeqVar(object e)
        {
            var seqType = e.GetType().GetGenericArgumentsCached()[0];
            var innerType = seqType.GetGenericArgumentsCached()[0];
            var innerSort = this.TypeToSortConverter.GetSortForType(innerType);
            var seqSort = this.Context.MkSeqSort(innerSort);
            var v = this.Context.MkConst(FreshSymbol(), seqSort);
            return (v, (SeqExpr)v);
        }

        public (Expr, ArrayExpr) CreateDictVar(object e)
        {
            var dictType = e.GetType().GetGenericArgumentsCached()[0];
            var typeArguments = dictType.GetGenericArgumentsCached();
            var keyType = typeArguments[0];
            var valueType = typeArguments[1];

            var keySort = this.TypeToSortConverter.GetSortForType(keyType);
            var valueSort = this.TypeToSortConverter.GetSortForType(valueType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                var v = this.Context.MkArrayConst(FreshSymbol(), keySort, this.BoolSort);
                this.Solver.Assert(this.Context.MkEq(this.Context.MkTermArray(v), this.Context.MkFalse()));
                return (v, v);
            }
            else
            {
                var optionSort = this.GetOrCreateOptionSort(valueSort);
                var none = this.Context.MkApp(optionSort.Constructors[0]);
                var v = this.Context.MkArrayConst(FreshSymbol(), keySort, optionSort);
                this.Solver.Assert(this.Context.MkEq(this.Context.MkTermArray(v), none));
                return (v, v);
            }
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

        public SeqExpr SeqConcat(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkConcat(x, y);
        }

        public BoolExpr SeqPrefixOf(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkPrefixOf(y, x);
        }

        public BoolExpr SeqSuffixOf(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkSuffixOf(y, x);
        }

        public BoolExpr SeqContains(SeqExpr x, SeqExpr y)
        {
            return this.Context.MkContains(x, y);
        }

        public SeqExpr SeqReplaceFirst(SeqExpr x, SeqExpr y, SeqExpr z)
        {
            return this.Context.MkReplace(x, y, z);
        }

        public SeqExpr SeqSlice(SeqExpr x, IntExpr y, IntExpr z)
        {
            return this.Context.MkExtract(x, y, z);
        }

        [ExcludeFromCodeCoverage] // not used yet
        public SeqExpr SeqAt(SeqExpr x, IntExpr y)
        {
            return this.Context.MkAt(x, y);
        }

        public IntExpr SeqLength(SeqExpr x)
        {
            return this.Context.MkLength(x);
        }

        public IntExpr SeqIndexOf(SeqExpr x, SeqExpr y, IntExpr z)
        {
            return this.Context.MkIndexOf(x, y, z);
        }

        public ArrayExpr DictEmpty(Type keyType, Type valueType)
        {
            var keySort = this.TypeToSortConverter.GetSortForType(keyType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                return this.Context.MkConstArray(keySort, this.Context.MkFalse());
            }
            else
            {
                var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
                var optionSort = GetOrCreateOptionSort(valueSort);
                var none = this.Context.MkApp(optionSort.Constructors[0]);
                return this.Context.MkConstArray(keySort, none);
            }
        }

        public ArrayExpr DictSet(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> keyExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> valueExpr,
            Type keyType,
            Type valueType)
        {
            var key = this.SymbolicValueToExprConverter.ConvertSymbolicValue(keyExpr, keyType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                return this.Context.MkStore(arrayExpr, key, this.Context.MkTrue());
            }
            else
            {
                var value = this.SymbolicValueToExprConverter.ConvertSymbolicValue(valueExpr, valueType);
                var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
                var optionSort = GetOrCreateOptionSort(valueSort);
                var some = this.Context.MkApp(optionSort.Constructors[1], value);
                return this.Context.MkStore(arrayExpr, key, some);
            }
        }

        public SeqExpr SeqUnit(
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> valueExpr,
            Type type)
        {
            var value = this.SymbolicValueToExprConverter.ConvertSymbolicValue(valueExpr, type);
            return this.Context.MkUnit(value);
        }

        public ArrayExpr DictDelete(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> keyExpr,
            Type keyType,
            Type valueType)
        {
            var key = this.SymbolicValueToExprConverter.ConvertSymbolicValue(keyExpr, keyType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                return this.Context.MkStore(arrayExpr, key, this.Context.MkFalse());
            }
            else
            {
                var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
                var optionSort = GetOrCreateOptionSort(valueSort);
                var none = this.Context.MkApp(optionSort.Constructors[0]);
                return this.Context.MkStore(arrayExpr, key, none);
            }
        }

        public (BoolExpr, object) DictGet(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> keyExpr,
            Type keyType,
            Type valueType)
        {
            var key = this.SymbolicValueToExprConverter.ConvertSymbolicValue(keyExpr, keyType);
            var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                var hasValue = (BoolExpr)this.Context.MkSelect(arrayExpr, key);
                return (hasValue, this.Context.MkApp(optionSort.Constructors[0]));
            }
            else
            {
                var optionResult = this.Context.MkSelect(arrayExpr, key);
                var none = this.Context.MkApp(optionSort.Constructors[0]);
                var someAccessor = optionSort.Accessors[1][0];
                var hasValue = this.Context.MkNot(this.Context.MkEq(optionResult, none));
                return (hasValue, this.Context.MkApp(someAccessor, optionResult));
            }
        }

        public ArrayExpr DictUnion(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return this.Context.MkSetUnion(arrayExpr1, arrayExpr2);
        }

        public ArrayExpr DictIntersect(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return this.Context.MkSetIntersection(arrayExpr1, arrayExpr2);
        }

        public SeqExpr SeqEmpty(Type type)
        {
            var sort = this.TypeToSortConverter.GetSortForType(type);
            var seqSort = this.Context.MkSeqSort(sort);
            return this.Context.MkEmptySeq(seqSort);
        }

        public SymbolicObject<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> SeqGet(SeqExpr x, Type seqInnerType, IntExpr y)
        {
            var sort = this.TypeToSortConverter.GetSortForType(seqInnerType);
            var result = this.Context.MkAt(x, y);
            var resultIsEmpty = this.Context.MkEq(result, this.SeqEmpty(seqInnerType));

            // create fresh variables to represent that flag and value for an option result
            var freshVariable = this.Context.MkConst(FreshSymbol(), sort);
            var freshSequence = this.Context.MkUnit(freshVariable);
            var freshFlag = (BoolExpr)this.Context.MkConst(FreshSymbol(), this.BoolSort);

            // ensure the values for the fresh flag and value are constrained appropriately
            this.Solver.Assert(this.Context.MkEq(resultIsEmpty, this.Context.MkNot(freshFlag)));
            this.Solver.Assert(this.Context.MkImplies(this.Context.MkNot(resultIsEmpty), this.Context.MkEq(freshSequence, result)));

            // build the resulting symbolic object
            var fields = ImmutableSortedDictionary<string, SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>>.Empty;
            fields = fields.Add("HasValue", new SymbolicBool<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(this, freshFlag));
            fields = fields.Add("Value", this.ExprToSymbolicValueConverter.ConvertExpr(freshVariable, seqInnerType));

            var objectType = typeof(Option<>).MakeGenericType(seqInnerType);
            return new SymbolicObject<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr>(objectType, this, fields);
        }

        public object Get(Model m, Expr v, Type type)
        {
            var e = m.Evaluate(v, true);
            return this.ExprToObjectConverter.Convert(e, type);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr> ConvertExprToSymbolicValue(object e, Type type)
        {
            return this.ExprToSymbolicValueConverter.ConvertExpr((Expr)e, type);
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

        public DatatypeSort GetOrCreateOptionSort(Sort valueSort)
        {
            if (this.OptionSorts.TryGetValue(valueSort, out var optionSort))
            {
                return optionSort;
            }

            var c1 = this.Context.MkConstructor("None", "none");
            var c2 = this.Context.MkConstructor("Some", "some", new string[] { "some" }, new Sort[] { valueSort });
            var optSort = this.Context.MkDatatypeSort("Option_" + valueSort, new Constructor[] { c1, c2 });
            this.OptionSorts[valueSort] = optSort;
            return optSort;
        }
    }
}
