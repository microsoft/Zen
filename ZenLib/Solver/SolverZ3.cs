// <copyright file="SolverZ3.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using Microsoft.Z3;

    /// <summary>
    /// Zen solver based on the Z3 SMT solver.
    /// </summary>
    internal class SolverZ3 : ISolver<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr>
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

        private Dictionary<Type, Sort> typeToSort;

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

            this.typeToSort = new Dictionary<Type, Sort>();
            this.typeToSort[typeof(bool)] = this.BoolSort;
            this.typeToSort[typeof(byte)] = this.ByteSort;
            this.typeToSort[typeof(short)] = this.ShortSort;
            this.typeToSort[typeof(ushort)] = this.ShortSort;
            this.typeToSort[typeof(int)] = this.IntSort;
            this.typeToSort[typeof(uint)] = this.IntSort;
            this.typeToSort[typeof(long)] = this.LongSort;
            this.typeToSort[typeof(ulong)] = this.LongSort;
            this.typeToSort[typeof(BigInteger)] = this.BigIntSort;
            this.typeToSort[typeof(string)] = this.StringSort;
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

        public object Get(Model m, Expr v, Type type)
        {
            var e = m.Evaluate(v, true);

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

        public static void RemoveTrailingZeroes(ref byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
        }
    }
}
