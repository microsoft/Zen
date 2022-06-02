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
        public SolverZ3(ModelCheckerContext context)
        {
            this.nextIndex = 0;
            this.ModelCheckerContext = context;
            this.Params = Context.MkParams();
            this.Params.Add("compact", false);
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
            this.ExprToObjectConverter = new Z3ExprToObjectConverter();
            this.OptionSorts = new Dictionary<Sort, DatatypeSort>();
        }

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

        private Symbol FreshSymbol()
        {
            return Context.MkSymbol(nextIndex++);
        }

        public BoolExpr And(BoolExpr x, BoolExpr y)
        {
            return Context.MkAnd(x, y);
        }

        public BitVecExpr BitwiseAnd(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVAND(x, y);
        }

        public BitVecExpr BitwiseNot(BitVecExpr x)
        {
            return Context.MkBVNot(x);
        }

        public BitVecExpr BitwiseOr(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVOR(x, y);
        }

        public BitVecExpr BitwiseXor(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVXOR(x, y);
        }

        public (Expr, BoolExpr) CreateBoolVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.BoolSort);
            return (v, (BoolExpr)v);
        }

        public BitVecExpr CreateByteConst(byte b)
        {
            return Context.MkBV(b, 8);
        }

        public (Expr, BitVecExpr) CreateByteVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.ByteSort);
            return (v, (BitVecExpr)v);
        }

        public (Expr, Expr) CreateCharVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.CharSort);
            return (v, v);
        }

        public Expr CreateCharConst(char c)
        {
            return Context.CharFromBV(this.CreateShortConst((short)c));
        }

        public SeqExpr CreateStringConst(string s)
        {
            return Context.MkString(s);
        }

        public BitVecExpr CreateShortConst(short s)
        {
            return Context.MkBV(s, 16);
        }

        public (Expr, BitVecExpr) CreateShortVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.ShortSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateIntConst(int i)
        {
            return Context.MkBV(i, 32);
        }

        public (Expr, BitVecExpr) CreateIntVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.IntSort);
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateLongConst(long l)
        {
            return Context.MkBV(l, 64);
        }

        public (Expr, BitVecExpr) CreateLongVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.LongSort);
            return (v, (BitVecExpr)v);
        }

        public (Expr, BitVecExpr) CreateBitvecVar(object e, uint size)
        {
            var v = Context.MkConst(FreshSymbol(), Context.MkBitVecSort(size));
            return (v, (BitVecExpr)v);
        }

        public BitVecExpr CreateBitvecConst(bool[] bits)
        {
            var littleEndian = new bool[bits.Length];

            for (int i = 0; i < bits.Length; i++)
            {
                littleEndian[i] = bits[bits.Length - 1 - i];
            }

            return Context.MkBV(littleEndian);
        }

        public IntExpr CreateBigIntegerConst(BigInteger bi)
        {
            return Context.MkInt(bi.ToString());
        }

        public (Expr, IntExpr) CreateBigIntegerVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.BigIntSort);
            return (v, (IntExpr)v);
        }

        public RealExpr CreateRealConst(Real r)
        {
            return Context.MkReal(r.ToString());
        }

        public (Expr, RealExpr) CreateRealVar(object e)
        {
            var v = Context.MkConst(FreshSymbol(), this.RealSort);
            return (v, (RealExpr)v);
        }

        public (Expr, SeqExpr) CreateSeqVar(object e)
        {
            var seqType = e.GetType().GetGenericArgumentsCached()[0];
            var innerType = seqType.GetGenericArgumentsCached()[0];
            var innerSort = this.TypeToSortConverter.GetSortForType(innerType);
            var seqSort = Context.MkSeqSort(innerSort);
            var v = Context.MkConst(FreshSymbol(), seqSort);
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

        public BoolExpr Eq(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkEq(x, y);
        }

        public BoolExpr Eq(IntExpr x, IntExpr y)
        {
            return Context.MkEq(x, y);
        }

        public BoolExpr Eq(RealExpr x, RealExpr y)
        {
            return Context.MkEq(x, y);
        }

        public BoolExpr Eq(SeqExpr x, SeqExpr y)
        {
            return Context.MkEq(x, y);
        }

        public BoolExpr Eq(ArrayExpr x, ArrayExpr y)
        {
            return Context.MkEq(x, y);
        }

        public BoolExpr Eq(Expr x, Expr y)
        {
            return Context.MkEq(x, y);
        }

        public BoolExpr False()
        {
            return Context.MkFalse();
        }

        public BoolExpr True()
        {
            return Context.MkTrue();
        }

        public BoolExpr Iff(BoolExpr x, BoolExpr y)
        {
            return Context.MkIff(x, y);
        }

        public BoolExpr Ite(BoolExpr g, BoolExpr t, BoolExpr f)
        {
            return (BoolExpr)Context.MkITE(g, t, f);
        }

        public BitVecExpr Ite(BoolExpr g, BitVecExpr t, BitVecExpr f)
        {
            return (BitVecExpr)Context.MkITE(g, t, f);
        }

        public IntExpr Ite(BoolExpr g, IntExpr t, IntExpr f)
        {
            return (IntExpr)Context.MkITE(g, t, f);
        }

        public RealExpr Ite(BoolExpr g, RealExpr t, RealExpr f)
        {
            return (RealExpr)Context.MkITE(g, t, f);
        }

        public SeqExpr Ite(BoolExpr g, SeqExpr t, SeqExpr f)
        {
            return (SeqExpr)Context.MkITE(g, t, f);
        }

        public ArrayExpr Ite(BoolExpr g, ArrayExpr t, ArrayExpr f)
        {
            return (ArrayExpr)Context.MkITE(g, t, f);
        }

        public Expr Ite(BoolExpr g, Expr t, Expr f)
        {
            return Context.MkITE(g, t, f);
        }

        public BoolExpr LessThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVULE(x, y);
        }

        public BoolExpr LessThanOrEqual(IntExpr x, IntExpr y)
        {
            return Context.MkLe(x, y);
        }

        public BoolExpr LessThanOrEqual(RealExpr x, RealExpr y)
        {
            return Context.MkLe(x, y);
        }

        public BoolExpr LessThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSLE(x, y);
        }

        public BoolExpr LessThan(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVULT(x, y);
        }

        public BoolExpr LessThan(IntExpr x, IntExpr y)
        {
            return Context.MkLt(x, y);
        }

        public BoolExpr LessThan(RealExpr x, RealExpr y)
        {
            return Context.MkLt(x, y);
        }

        public BoolExpr LessThanSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSLT(x, y);
        }

        public BoolExpr GreaterThanOrEqual(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVUGE(x, y);
        }

        public BoolExpr GreaterThanOrEqual(IntExpr x, IntExpr y)
        {
            return Context.MkGe(x, y);
        }

        public BoolExpr GreaterThanOrEqual(RealExpr x, RealExpr y)
        {
            return Context.MkGe(x, y);
        }

        public BoolExpr GreaterThanOrEqualSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSGE(x, y);
        }

        public BoolExpr GreaterThan(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVUGT(x, y);
        }

        public BoolExpr GreaterThan(IntExpr x, IntExpr y)
        {
            return Context.MkGt(x, y);
        }

        public BoolExpr GreaterThan(RealExpr x, RealExpr y)
        {
            return Context.MkGt(x, y);
        }

        public BoolExpr GreaterThanSigned(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSGT(x, y);
        }

        public BoolExpr Not(BoolExpr x)
        {
            return Context.MkNot(x);
        }

        public BoolExpr Or(BoolExpr x, BoolExpr y)
        {
            return Context.MkOr(x, y);
        }

        public BitVecExpr Add(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVAdd(x, y);
        }

        public IntExpr Add(IntExpr x, IntExpr y)
        {
            return (IntExpr)Context.MkAdd(x, y);
        }

        public RealExpr Add(RealExpr x, RealExpr y)
        {
            return (RealExpr)Context.MkAdd(x, y);
        }

        public BitVecExpr Subtract(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVSub(x, y);
        }

        public IntExpr Subtract(IntExpr x, IntExpr y)
        {
            return (IntExpr)Context.MkSub(x, y);
        }

        public RealExpr Subtract(RealExpr x, RealExpr y)
        {
            return (RealExpr)Context.MkSub(x, y);
        }

        public BitVecExpr Multiply(BitVecExpr x, BitVecExpr y)
        {
            return Context.MkBVMul(x, y);
        }

        public IntExpr Multiply(IntExpr x, IntExpr y)
        {
            return (IntExpr)Context.MkMul(x, y);
        }

        public RealExpr Multiply(RealExpr x, RealExpr y)
        {
            return (RealExpr)Context.MkMul(x, y);
        }

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

        public SeqExpr SeqConcat(SeqExpr x, SeqExpr y)
        {
            return Context.MkConcat(x, y);
        }

        public BoolExpr SeqPrefixOf(SeqExpr x, SeqExpr y)
        {
            return Context.MkPrefixOf(y, x);
        }

        public BoolExpr SeqSuffixOf(SeqExpr x, SeqExpr y)
        {
            return Context.MkSuffixOf(y, x);
        }

        public BoolExpr SeqContains(SeqExpr x, SeqExpr y)
        {
            return Context.MkContains(x, y);
        }

        public SeqExpr SeqReplaceFirst(SeqExpr x, SeqExpr y, SeqExpr z)
        {
            return Context.MkReplace(x, y, z);
        }

        public SeqExpr SeqSlice(SeqExpr x, IntExpr y, IntExpr z)
        {
            return Context.MkExtract(x, y, z);
        }

        public SeqExpr SeqAt(SeqExpr x, IntExpr y)
        {
            return Context.MkAt(x, y);
        }

        public IntExpr SeqLength(SeqExpr x)
        {
            return Context.MkLength(x);
        }

        public IntExpr SeqIndexOf(SeqExpr x, SeqExpr y, IntExpr z)
        {
            return Context.MkIndexOf(x, y, z);
        }

        public BoolExpr SeqRegex<T>(SeqExpr x, Regex<T> y)
        {
            var regexConverter = new Z3RegexConverter<T>(this);
            var seqSort = this.TypeToSortConverter.GetSortForType(typeof(T));
            var regexExpr = y.Accept(regexConverter, seqSort);
            return Context.MkInRe(x, regexExpr);
        }

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

        public SeqExpr SeqUnit(
            SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> valueExpr,
            Type type)
        {
            var value = this.SymbolicValueToExprConverter.ConvertSymbolicValue(valueExpr, type);
            return Context.MkUnit(value);
        }

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

        public ArrayExpr DictUnion(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return Context.MkSetUnion(arrayExpr1, arrayExpr2);
        }

        public ArrayExpr DictIntersect(ArrayExpr arrayExpr1, ArrayExpr arrayExpr2)
        {
            return Context.MkSetIntersection(arrayExpr1, arrayExpr2);
        }

        public SeqExpr SeqEmpty(Type type)
        {
            var sort = this.TypeToSortConverter.GetSortForType(type);
            var seqSort = Context.MkSeqSort(sort);
            return Context.MkEmptySeq(seqSort);
        }

        public object Get(Model m, Expr v, Type type)
        {
            var e = m.Evaluate(v, true);
            return this.ExprToObjectConverter.Convert(e, type);
        }

        public SymbolicValue<Model, Expr, BoolExpr, BitVecExpr, IntExpr, SeqExpr, ArrayExpr, Expr, RealExpr> ConvertExprToSymbolicValue(object e, Type type)
        {
            return this.ExprToSymbolicValueConverter.ConvertExpr((Expr)e, type);
        }

        public Model Solve(BoolExpr x)
        {
            this.Assert(x);
            var status = this.Solver.Check();
            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            ThrowIfUnknown(status);
            return this.Solver.Model;
        }

        public Model Maximize(BitVecExpr objective, BoolExpr subjectTo)
        {
            return Maximize((Expr)objective, subjectTo);
        }

        public Model Maximize(IntExpr objective, BoolExpr subjectTo)
        {
            return Maximize((Expr)objective, subjectTo);
        }

        public Model Maximize(RealExpr objective, BoolExpr subjectTo)
        {
            return Maximize((Expr)objective, subjectTo);
        }

        public Model Maximize(Expr objective, BoolExpr subjectTo)
        {
            this.Assert(subjectTo);
            this.Optimize.MkMaximize(objective);
            var status = this.Optimize.Check();
            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            ThrowIfUnknown(status);
            return this.Optimize.Model;
        }

        public Model Minimize(BitVecExpr objective, BoolExpr subjectTo)
        {
            return Minimize((Expr)objective, subjectTo);
        }

        public Model Minimize(IntExpr objective, BoolExpr subjectTo)
        {
            return Minimize((Expr)objective, subjectTo);
        }

        public Model Minimize(RealExpr objective, BoolExpr subjectTo)
        {
            return Minimize((Expr)objective, subjectTo);
        }

        public Model Minimize(Expr objective, BoolExpr subjectTo)
        {
            this.Assert(subjectTo);
            this.Optimize.MkMinimize(objective);
            var status = this.Optimize.Check();
            if (status == Status.UNSATISFIABLE)
            {
                return null;
            }

            ThrowIfUnknown(status);
            return this.Optimize.Model;
        }

        [ExcludeFromCodeCoverage]
        private void ThrowIfUnknown(Status status)
        {
            if (status == Status.UNKNOWN)
            {
                throw new ZenException($"Unknown result: {this.Solver.ReasonUnknown}");
            }
        }

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
    }
}
