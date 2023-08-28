// <copyright file="SolverZ3.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Threading;
    using System.Timers;
    using Microsoft.Z3;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Zen solver based on the Z3 SMT solver.
    /// </summary>
    internal class SolverZ3 : ISolver<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr>
    {
        /// <summary>
        /// Keep one Z3 context per thread. This is necessary because Z3 contexts are not thread safe.
        /// Moreover, we cannot create a new context each time because Z3 leaks memory when creating many contexts
        /// and this eventually crashes the process.
        /// </summary>
        internal static ThreadLocal<Context> _context;

        /// <summary>
        /// Whether we are are model checking or doing optimization.
        /// </summary>
        internal ModelCheckerContext ModelCheckerContext;

        /// <summary>
        /// An optional debugging callback with the Z3 query.
        /// </summary>
        internal Action<SolverDebugInfo> Debug;

        /// <summary>
        /// The Z3 parameters used.
        /// </summary>
        internal Params Params;

        /// <summary>
        /// The solver used to collect constraints.
        /// </summary>
        internal Solver Solver;

        /// <summary>
        /// The optimization solver when in the optimization setting.
        /// </summary>
        internal Optimize Optimize;

        /// <summary>
        /// An index used to allocate fresh ids.
        /// </summary>
        private int nextIndex;

        /// <summary>
        /// The sort for boolean values.
        /// </summary>
        internal Sort BoolSort;

        /// <summary>
        /// The sort for byte values.
        /// </summary>
        internal Sort ByteSort;

        /// <summary>
        /// The sort for unicode char values.
        /// </summary>
        internal Sort CharSort;

        /// <summary>
        /// The sort for short values.
        /// </summary>
        internal Sort ShortSort;

        /// <summary>
        /// The sort for int values.
        /// </summary>
        internal Sort IntSort;

        /// <summary>
        /// The sort for long values.
        /// </summary>
        internal Sort LongSort;

        /// <summary>
        /// The sort for BigInteger values.
        /// </summary>
        internal Sort BigIntSort;

        /// <summary>
        /// The sort for Real values.
        /// </summary>
        internal Sort RealSort;

        /// <summary>
        /// The sort for string values.
        /// </summary>
        internal Sort StringSort;

        /// <summary>
        /// A cache/mapping from sorts to their lifted option data type sort.
        /// These are used for sets and maps.
        /// </summary>
        internal Dictionary<Sort, DatatypeSort> OptionSorts;

        /// <summary>
        /// A converter from a C# type to a Z3 sort.
        /// </summary>
        internal Z3TypeToSortConverter TypeToSortConverter;

        /// <summary>
        /// A converter from a Z3 expr to a symbolic value.
        /// </summary>
        internal Z3ExprToSymbolicValueConverter ExprToSymbolicValueConverter;

        /// <summary>
        /// A converter from a symbolic value to a Z3 expr.
        /// </summary>
        internal Z3SymbolicValueToExprConverter SymbolicValueToExprConverter;

        /// <summary>
        /// A converter from a Z3 expr to a C# object.
        /// </summary>
        internal Z3ExprToObjectConverter ExprToObjectConverter;

        /// <summary>
        /// Static constructor for the solver.
        /// </summary>
        static SolverZ3()
        {
            Native.Z3_global_param_set("encoding", "bmp");
            _context = new ThreadLocal<Context>(() => new Context());
        }

        /// <summary>
        /// The context for the current thread.
        /// </summary>
        public static Context Context { get => _context.Value; }

        /// <summary>
        /// Ininitializes a new instance of the <see cref="SolverZ3"/> class.
        /// </summary>
        /// <param name="context">The model checking context.</param>
        /// <param name="timeout">The timeout for the solver.</param>
        /// <param name="debug">An optional debugging callback.</param>
        public SolverZ3(ModelCheckerContext context, TimeSpan? timeout, Action<SolverDebugInfo> debug = null)
        {
            this.nextIndex = 0;
            this.ModelCheckerContext = context;
            this.Debug = debug;
            this.Params = Context.MkParams();
            this.Params.Add("compact", false);
            if (timeout.HasValue)
            {
                this.Params.Add("timeout", (uint)timeout.Value.TotalMilliseconds);
            }
            var t1 = Context.MkTactic("simplify");
            var t2 = Context.MkTactic("solve-eqs");
            var t3 = Context.MkTactic("smt");
            var tactic = Context.AndThen(t1, t2, t3);
            this.Solver = Context.MkSolver(tactic);
            this.Solver.Parameters = this.Params;
            this.Optimize = Context.MkOptimize();
            this.BoolSort = Context.MkBoolSort();
            this.ByteSort = Context.MkBitVecSort(8);
            this.CharSort = Context.CharSort;
            this.ShortSort = Context.MkBitVecSort(16);
            this.IntSort = Context.MkBitVecSort(32);
            this.LongSort = Context.MkBitVecSort(64);
            this.BigIntSort = Context.MkIntSort();
            this.RealSort = Context.MkRealSort();
            this.StringSort = Context.StringSort;
            this.TypeToSortConverter = new Z3TypeToSortConverter(this);
            this.ExprToSymbolicValueConverter = new Z3ExprToSymbolicValueConverter(this);
            this.SymbolicValueToExprConverter = new Z3SymbolicValueToExprConverter(this);
            this.ExprToObjectConverter = new Z3ExprToObjectConverter(this);
            this.OptionSorts = new Dictionary<Sort, DatatypeSort>();
        }

        /// <summary>
        /// The false expression.
        /// </summary>
        /// <returns>The expression.</returns>
        public BoolExpr False()
        {
            return Context.MkFalse();
        }

        /// <summary>
        /// The true expression.
        /// </summary>
        /// <returns>The expression.</returns>
        public BoolExpr True()
        {
            return Context.MkTrue();
        }

        /// <summary>
        /// The 'And' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr And(BoolExpr x, BoolExpr y)
        {
            return Context.MkAnd(x, y);
        }

        /// <summary>
        /// The 'BitwiseAnd' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVecExpr BitwiseAnd(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVAND(x, y);
        }

        /// <summary>
        /// The 'BitwiseNot' of an expression.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <returns></returns>
        public BitVecExpr BitwiseNot(BitVecExpr x)
        {
            return Context.MkBVNot(x);
        }

        /// <summary>
        /// The 'BitwiseOr' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVecExpr BitwiseOr(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVOR(x, y);
        }

        /// <summary>
        /// The 'BitwiseXor' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVecExpr BitwiseXor(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVXOR(x, y);
        }

        /// <summary>
        /// Create a new boolean expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, BoolExpr) CreateBoolVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.BoolSort);
            return (v, (BoolExpr)v);
        }

        /// <summary>
        /// Create a new byte expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, BitVecExpr) CreateByteVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.ByteSort);
            return (v, (BitVecExpr)v);
        }

        /// <summary>
        /// Create a byte constant.
        /// </summary>
        /// <returns></returns>
        public BitVecExpr CreateByteConst(byte b)
        {
            return Context.MkBV(b, 8);
        }

        /// <summary>
        /// Create a new char expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, Expr) CreateCharVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.CharSort);
            return (v, v);
        }

        /// <summary>
        /// Create a char constant.
        /// </summary>
        /// <returns></returns>
        public Expr CreateCharConst(char c)
        {
            return Context.CharFromBV(this.CreateShortConst((short)c));
        }

        /// <summary>
        /// Create a char constant.
        /// </summary>
        /// <returns></returns>
        public SeqExpr CreateStringConst(string s)
        {
            return Context.MkString(s);
        }

        /// <summary>
        /// Create a short constant.
        /// </summary>
        /// <returns></returns>
        public BitVecExpr CreateShortConst(short s)
        {
            return Context.MkBV(s, 16);
        }

        /// <summary>
        /// Create a new short expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, BitVecExpr) CreateShortVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.ShortSort);
            return (v, (BitVecExpr)v);
        }

        /// <summary>
        /// Create an integer constant.
        /// </summary>
        /// <returns></returns>
        public BitVecExpr CreateIntConst(int i)
        {
            return Context.MkBV(i, 32);
        }

        /// <summary>
        /// Create a new int expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, BitVecExpr) CreateIntVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.IntSort);
            return (v, (BitVecExpr)v);
        }

        /// <summary>
        /// Create a long constant.
        /// </summary>
        /// <returns></returns>
        public BitVecExpr CreateLongConst(long l)
        {
            return Context.MkBV(l, 64);
        }

        /// <summary>
        /// Create a new long expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, BitVecExpr) CreateLongVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.LongSort);
            return (v, (BitVecExpr)v);
        }

        /// <summary>
        /// Create a bitvector variable.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <param name="size">The size of the bitvector.</param>
        /// <returns>The expression.</returns>
        public (Expr, BitVecExpr) CreateBitvecVar(object e, uint size)
        {
            var v = Context.MkConst(FreshSymbol(), Context.MkBitVecSort(size));
            return (v, (BitVecExpr)v);
        }

        /// <summary>
        /// Create a bitvector constant.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <returns>A bitvector expression.</returns>
        public BitVecExpr CreateBitvecConst(bool[] bits)
        {
            var littleEndian = new bool[bits.Length];

            for (int i = 0; i < bits.Length; i++)
            {
                littleEndian[i] = bits[bits.Length - 1 - i];
            }

            return Context.MkBV(littleEndian);
        }

        /// <summary>
        /// Create a big integer constant.
        /// </summary>
        /// <returns></returns>
        public IntExpr CreateBigIntegerConst(BigInteger bi)
        {
            return Context.MkInt(bi.ToString());
        }

        /// <summary>
        /// Create a new big integer expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, IntExpr) CreateBigIntegerVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.BigIntSort);
            return (v, (IntExpr)v);
        }

        /// <summary>
        /// Create a real constant.
        /// </summary>
        /// <returns></returns>
        public RealExpr CreateRealConst(Real r)
        {
            return Context.MkReal(r.ToString());
        }

        /// <summary>
        /// Create a new real expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, RealExpr) CreateRealVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.RealSort);
            return (v, (RealExpr)v);
        }

        /// <summary>
        /// Create a new sequence expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Expr, SeqExpr) CreateSeqVar(object e)
        {
            var seqType = e.GetType().GetGenericArgumentsCached()[0];
            var innerType = seqType.GetGenericArgumentsCached()[0];
            var innerSort = this.TypeToSortConverter.GetSortForType(innerType);
            var seqSort = Context.MkSeqSort(innerSort);
            var v = Context.MkConst(FreshSymbol(), seqSort);
            return (v, (SeqExpr)v);
        }

        /// <summary>
        /// Create a new dictionary expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
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
                var v = Context.MkArrayConst(FreshSymbol(), keySort, this.BoolSort);
                this.Assert(Context.MkEq(Context.MkTermArray(v), Context.MkFalse()));
                return (v, v);
            }
            else
            {
                var optionSort = this.GetOrCreateOptionSort(valueSort);
                var none = Context.MkApp(optionSort.Constructors[0]);
                var v = Context.MkArrayConst(FreshSymbol(), keySort, optionSort);
                this.Assert(Context.MkEq(Context.MkTermArray(v), none));
                return (v, v);
            }
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Eq(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkEq(x, y);
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Eq(IntExpr x, IntExpr y)
        {
            return Context.MkEq(x, y);
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Eq(RealExpr x, RealExpr y)
        {
            return Context.MkEq(x, y);
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Eq(SeqExpr x, SeqExpr y)
        {
            return Context.MkEq(x, y);
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Eq(ArrayExpr x, ArrayExpr y)
        {
            return Context.MkEq(x, y);
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Eq(Expr x, Expr y)
        {
            return Context.MkEq(x, y);
        }

        /// <summary>
        /// The 'Iff' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Iff(BoolExpr x, BoolExpr y)
        {
            return Context.MkIff(x, y);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public BoolExpr Ite(BoolExpr g, BoolExpr t, BoolExpr f)
        {
            return (BoolExpr)Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public BitVecExpr Ite(BoolExpr g, BitVecExpr t, BitVecExpr f)
        {
            return (BitVecExpr)Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public IntExpr Ite(BoolExpr g, IntExpr t, IntExpr f)
        {
            return (IntExpr)Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public RealExpr Ite(BoolExpr g, RealExpr t, RealExpr f)
        {
            return (RealExpr)Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public SeqExpr Ite(BoolExpr g, SeqExpr t, SeqExpr f)
        {
            return (SeqExpr)Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public ArrayExpr Ite(BoolExpr g, ArrayExpr t, ArrayExpr f)
        {
            return (ArrayExpr)Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two expressions.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public Expr Ite(BoolExpr g, Expr t, Expr f)
        {
            return Context.MkITE(g, t, f);
        }

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVULE(x, y);
        }

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThanOrEqual(IntExpr x, IntExpr y)
        {
            return Context.MkLe(x, y);
        }

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThanOrEqual(RealExpr x, RealExpr y)
        {
            return Context.MkLe(x, y);
        }

        /// <summary>
        /// The 'LessThanOrEqualSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSLE(x, y);
        }

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThan(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVULT(x, y);
        }

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThan(IntExpr x, IntExpr y)
        {
            return Context.MkLt(x, y);
        }

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThan(RealExpr x, RealExpr y)
        {
            return Context.MkLt(x, y);
        }

        /// <summary>
        /// The 'LessThanSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr LessThanSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSLT(x, y);
        }

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVUGE(x, y);
        }

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThanOrEqual(IntExpr x, IntExpr y)
        {
            return Context.MkGe(x, y);
        }

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThanOrEqual(RealExpr x, RealExpr y)
        {
            return Context.MkGe(x, y);
        }

        /// <summary>
        /// The 'GreaterThanOrEqualSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSGE(x, y);
        }

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThan(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVUGT(x, y);
        }

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThan(IntExpr x, IntExpr y)
        {
            return Context.MkGt(x, y);
        }

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThan(RealExpr x, RealExpr y)
        {
            return Context.MkGt(x, y);
        }

        /// <summary>
        /// The 'GreaterThanSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr GreaterThanSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSGT(x, y);
        }

        /// <summary>
        /// The 'Not' of an expression.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <returns></returns>
        public BoolExpr Not(BoolExpr x)
        {
            return Context.MkNot(x);
        }

        /// <summary>
        /// The 'Or' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BoolExpr Or(BoolExpr x, BoolExpr y)
        {
            return Context.MkOr(x, y);
        }

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVecExpr Add(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVAdd(x, y);
        }

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public IntExpr Add(IntExpr x, IntExpr y)
        {
            return (IntExpr)Context.MkAdd(x, y);
        }

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public RealExpr Add(RealExpr x, RealExpr y)
        {
            return (RealExpr)Context.MkAdd(x, y);
        }

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVecExpr Subtract(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSub(x, y);
        }

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public IntExpr Subtract(IntExpr x, IntExpr y)
        {
            return (IntExpr)Context.MkSub(x, y);
        }

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public RealExpr Subtract(RealExpr x, RealExpr y)
        {
            return (RealExpr)Context.MkSub(x, y);
        }

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVecExpr Multiply(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVMul(x, y);
        }

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public IntExpr Multiply(IntExpr x, IntExpr y)
        {
            return (IntExpr)Context.MkMul(x, y);
        }

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public RealExpr Multiply(RealExpr x, RealExpr y)
        {
            return (RealExpr)Context.MkMul(x, y);
        }

        /// <summary>
        /// The 'Resize' of a bitvec expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="sourceSize">The source bitwidth.</param>
        /// <param name="targetSize">The target bitwidth.</param>
        /// <returns></returns>
        public BitVecExpr Resize(BitVecExpr x, int sourceSize, int targetSize)
        {
            if (sourceSize == targetSize)
            {
                return x;
            }
            else if (sourceSize < targetSize)
            {
                return Context.MkZeroExt((uint)(targetSize - sourceSize), x);
            }
            else
            {
                return Context.MkExtract((uint)(targetSize - 1), 0U, x);
            }
        }

        /// <summary>
        /// The 'EmptySeq' for a given type.
        /// </summary>
        /// <param name="type">The type of the sequence.</param>
        /// <returns></returns>
        public SeqExpr SeqEmpty(Type type)
        {
            var sort = this.TypeToSortConverter.GetSortForType(type);
            var seqSort = Context.MkSeqSort(sort);
            return Context.MkEmptySeq(seqSort);
        }

        /// <summary>
        /// The 'SeqUnit' for a given type.
        /// </summary>
        /// <param name="valueExpr">The value expression.</param>
        /// <param name="type">The type of the sequence.</param>
        /// <returns></returns>
        public SeqExpr SeqUnit(
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> valueExpr,
            Type type)
        {
            var value = this.SymbolicValueToExprConverter.ConvertSymbolicValue(valueExpr, type);
            return Context.MkUnit(value);
        }

        /// <summary>
        /// The 'Concat' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public SeqExpr SeqConcat(SeqExpr x, SeqExpr y)
        {
            return Context.MkConcat(x, y);
        }

        /// <summary>
        /// The 'PrefixOf' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        public BoolExpr SeqPrefixOf(SeqExpr x, SeqExpr y)
        {
            return Context.MkPrefixOf(y, x);
        }

        /// <summary>
        /// The 'SuffixOf' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        public BoolExpr SeqSuffixOf(SeqExpr x, SeqExpr y)
        {
            return Context.MkSuffixOf(y, x);
        }

        /// <summary>
        /// The 'Contains' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        public BoolExpr SeqContains(SeqExpr x, SeqExpr y)
        {
            return Context.MkContains(x, y);
        }

        /// <summary>
        /// The seq 'Replace' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <param name="z">The replacement expression.</param>
        /// <returns></returns>
        public SeqExpr SeqReplaceFirst(SeqExpr x, SeqExpr y, SeqExpr z)
        {
            return Context.MkReplace(x, y, z);
        }

        /// <summary>
        /// The seq 'Slice' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The offset expression.</param>
        /// <param name="z">The length expression.</param>
        /// <returns></returns>
        public SeqExpr SeqSlice(SeqExpr x, IntExpr y, IntExpr z)
        {
            return Context.MkExtract(x, y, z);
        }

        /// <summary>
        /// The seq 'At' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The index expression.</param>
        /// <returns></returns>
        public SeqExpr SeqAt(SeqExpr x, IntExpr y)
        {
            return Context.MkAt(x, y);
        }

        /// <summary>
        /// The seq 'Nth' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The index expression.</param>
        /// <returns></returns>
        public object SeqNth(SeqExpr x, IntExpr y)
        {
            return Context.MkNth(x, y);
        }

        /// <summary>
        /// The seq 'Length' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <returns></returns>
        public IntExpr SeqLength(SeqExpr x)
        {
            return Context.MkLength(x);
        }

        /// <summary>
        /// The seq 'IndexOf' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <param name="z">The offset expression.</param>
        /// <returns></returns>
        public IntExpr SeqIndexOf(SeqExpr x, SeqExpr y, IntExpr z)
        {
            return Context.MkIndexOf(x, y, z);
        }

        /// <summary>
        /// The seq 'Regex' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The regex expression.</param>
        /// <returns></returns>
        public BoolExpr SeqRegex<T>(SeqExpr x, Regex<T> y)
        {
            var regexConverter = new Z3RegexConverter<T>(this);
            var seqSort = this.TypeToSortConverter.GetSortForType(typeof(T));
            var regexExpr = y.Accept(regexConverter, seqSort);
            return Context.MkInRe(x, regexExpr);
        }

        /// <summary>
        /// The empty dictionary.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        public ArrayExpr DictEmpty(Type keyType, Type valueType)
        {
            var keySort = this.TypeToSortConverter.GetSortForType(keyType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                return Context.MkConstArray(keySort, Context.MkFalse());
            }
            else
            {
                var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
                var optionSort = GetOrCreateOptionSort(valueSort);
                var none = Context.MkApp(optionSort.Constructors[0]);
                return Context.MkConstArray(keySort, none);
            }
        }

        /// <summary>
        /// The result of setting a key to a value for an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="valueExpr">The value expression.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        public ArrayExpr DictSet(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> keyExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> valueExpr,
            Type keyType,
            Type valueType)
        {
            var key = this.SymbolicValueToExprConverter.ConvertSymbolicValue(keyExpr, keyType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                return Context.MkStore(arrayExpr, key, Context.MkTrue());
            }
            else
            {
                var value = this.SymbolicValueToExprConverter.ConvertSymbolicValue(valueExpr, valueType);
                var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
                var optionSort = GetOrCreateOptionSort(valueSort);
                var some = Context.MkApp(optionSort.Constructors[1], value);
                return Context.MkStore(arrayExpr, key, some);
            }
        }

        /// <summary>
        /// The result of deleting a key from an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        public ArrayExpr DictDelete(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> keyExpr,
            Type keyType,
            Type valueType)
        {
            var key = this.SymbolicValueToExprConverter.ConvertSymbolicValue(keyExpr, keyType);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                return Context.MkStore(arrayExpr, key, Context.MkFalse());
            }
            else
            {
                var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
                var optionSort = GetOrCreateOptionSort(valueSort);
                var none = Context.MkApp(optionSort.Constructors[0]);
                return Context.MkStore(arrayExpr, key, none);
            }
        }

        /// <summary>
        /// The result of getting a value for a key from an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        public (BoolExpr, object) DictGet(
            ArrayExpr arrayExpr,
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> keyExpr,
            Type keyType,
            Type valueType)
        {
            var key = this.SymbolicValueToExprConverter.ConvertSymbolicValue(keyExpr, keyType);
            var valueSort = this.TypeToSortConverter.GetSortForType(valueType);
            var optionSort = GetOrCreateOptionSort(valueSort);

            if (valueType == ReflectionUtilities.SetUnitType)
            {
                var hasValue = (BoolExpr)Context.MkSelect(arrayExpr, key);
                return (hasValue, Context.MkApp(optionSort.Constructors[0]));
            }
            else
            {
                var optionResult = Context.MkSelect(arrayExpr, key);
                var none = Context.MkApp(optionSort.Constructors[0]);
                var someAccessor = optionSort.Accessors[1][0];
                var hasValue = Context.MkNot(Context.MkEq(optionResult, none));
                return (hasValue, Context.MkApp(someAccessor, optionResult));
            }
        }

        /// <summary>
        /// The result of unioning two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        public ArrayExpr DictUnion(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return Context.MkSetUnion(arrayExpr1, arrayExpr2);
        }

        /// <summary>
        /// The result of intersecting two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        public ArrayExpr DictIntersect(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return Context.MkSetIntersection(arrayExpr1, arrayExpr2);
        }

        /// <summary>
        /// The result of differencing two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        public ArrayExpr DictDifference(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return Context.MkSetDifference(arrayExpr1, arrayExpr2);
        }

        /// <summary>
        /// Get the value for a variable in a model.
        /// </summary>
        /// <param name="m">The model.</param>
        /// <param name="v">The variable.</param>
        /// <param name="type">The C# type to coerce the result to.</param>
        /// <returns>The value.</returns>
        public object Get(Model m, Expr v, Type type)
        {
            var e = m.Evaluate(v, true);
            return this.ExprToObjectConverter.Convert(e, type);
        }

        /// <summary>
        /// Convert a value back into a symbolic value.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> ConvertExprToSymbolicValue(object e, Type type)
        {
            return this.ExprToSymbolicValueConverter.Visit(type, (Expr)e);
        }

        /// <summary>
        /// Check whether a boolean expression is satisfiable.
        /// </summary>
        /// <param name="x">The expression.</param>
        /// <returns>A model, if satisfiable.</returns>
        public Model Solve(BoolExpr x)
        {
            this.Assert((BoolExpr)x.Simplify());
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var status = this.Solver.Check();
            this.Debug?.Invoke(new SolverDebugInfo
            {
                SolverQuery = this.Solver.ToString(),
                SolverTime = timer.Elapsed,
            });

            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            ThrowIfUnknown(status);
            return this.Solver.Model;
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Maximize(BitVecExpr objective, BoolExpr subjectTo)
        {
            return Maximize((Expr)objective, subjectTo);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Maximize(IntExpr objective, BoolExpr subjectTo)
        {
            return Maximize((Expr)objective, subjectTo);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Maximize(RealExpr objective, BoolExpr subjectTo)
        {
            return Maximize((Expr)objective, subjectTo);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Maximize(Expr objective, BoolExpr subjectTo)
        {
            this.Assert(subjectTo);
            this.Optimize.MkMaximize(objective);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var status = this.Optimize.Check();
            this.Debug?.Invoke(new SolverDebugInfo
            {
                SolverQuery = this.Optimize.ToString(),
                SolverTime = timer.Elapsed,
            });

            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            ThrowIfUnknown(status);
            return this.Optimize.Model;
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Minimize(BitVecExpr objective, BoolExpr subjectTo)
        {
            return Minimize((Expr)objective, subjectTo);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Minimize(IntExpr objective, BoolExpr subjectTo)
        {
            return Minimize((Expr)objective, subjectTo);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Minimize(RealExpr objective, BoolExpr subjectTo)
        {
            return Minimize((Expr)objective, subjectTo);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        public Model Minimize(Expr objective, BoolExpr subjectTo)
        {
            this.Assert(subjectTo);
            this.Optimize.MkMinimize(objective);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var status = this.Optimize.Check();
            this.Debug?.Invoke(new SolverDebugInfo
            {
                SolverQuery = this.Optimize.ToString(),
                SolverTime = timer.Elapsed,
            });

            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            ThrowIfUnknown(status);
            return this.Optimize.Model;
        }

        /// <summary>
        /// Throw an exception if the status result is 'UNKNOWN'.
        /// </summary>
        /// <param name="status">The status.</param>
        [ExcludeFromCodeCoverage]
        private void ThrowIfUnknown(Status status)
        {
            if (status == Status.UNKNOWN)
            {
                if (this.Solver.ReasonUnknown == "timeout")
                {
                    throw new ZenSolverTimeoutException();
                }
                else
                {
                    throw new ZenException($"Unknown result: {this.Solver.ReasonUnknown}");
                }
            }
        }

        /// <summary>
        /// Createe an option sort for a given sort. Uses caching.
        /// </summary>
        /// <param name="valueSort">The value short.</param>
        /// <returns>The option sort.</returns>
        [ExcludeFromCodeCoverage] // some weird coverage bug in MkDatatypeSort.
        public DatatypeSort GetOrCreateOptionSort(Sort valueSort)
        {
            if (this.OptionSorts.TryGetValue(valueSort, out var optionSort))
            {
                return optionSort;
            }

            var c1 = Context.MkConstructor("None", "none", new string[] { }, new Sort[] { }, new uint[] { });
            var c2 = Context.MkConstructor("Some", "some", new string[] { "some" }, new Sort[] { valueSort });
            var optSort = Context.MkDatatypeSort("Option_" + valueSort, new Constructor[] { c1, c2 });
            this.OptionSorts[valueSort] = optSort;
            return optSort;
        }

        /// <summary>
        /// Assert a boolean expression and add the assertion to the right solver.
        /// </summary>
        /// <param name="e">The boolean expression.</param>
        private void Assert(BoolExpr e)
        {
            switch (this.ModelCheckerContext)
            {
                case ModelCheckerContext.Solving:
                    this.Solver.Assert(e);
                    return;
                default:
                    Contract.Assert(this.ModelCheckerContext == ModelCheckerContext.Optimization);
                    this.Optimize.Assert(e);
                    return;
            }
        }

        /// <summary>
        /// Create a fresh symbol/name.
        /// </summary>
        /// <returns>The new symbol.</returns>
        private Symbol FreshSymbol()
        {
            return Context.MkSymbol(Interlocked.Increment(ref nextIndex));
        }
    }
}
