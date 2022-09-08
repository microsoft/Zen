// <copyright file="SolverDD.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using DecisionDiagrams;
    using ZenLib.ModelChecking;

    /// <summary>
    /// Implementation of a solver using decision diagrams.
    /// </summary>
    /// <typeparam name="T">The diagram node type.</typeparam>
    internal class SolverDD<T> : ISolver<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit>
        where T : IDDNode
    {
        /// <summary>
        /// Gets the manger object.
        /// </summary>A
        internal DDManager<T> Manager { get; }

        /// <summary>
        /// All allocated variables.
        /// </summary>
        internal List<Variable<T>> Variables { get; } = new List<Variable<T>>();

        /// <summary>
        /// The variable "must interleave" dependencies.
        /// </summary>
        private List<List<object>> interleavingDependencies { get; }

        /// <summary>
        /// The existing assignment provided.
        /// </summary>
        private Dictionary<object, Variable<T>> ExistingAssignment { get; }

        /// <summary>
        /// Creates a new instanceof the SolverDD class.
        /// </summary>
        /// <param name="manager">The manager object.</param>
        /// <param name="interleavingDependencies">Variable interleaving data.</param>
        public SolverDD(DDManager<T> manager, List<List<object>> interleavingDependencies)
        {
            this.Manager = manager;
            this.ExistingAssignment = new Dictionary<object, Variable<T>>();
            this.interleavingDependencies = interleavingDependencies;
        }

        /// <summary>
        /// Initialize the solver.
        /// </summary>
        public void Init()
        {
            foreach (var dependentVariableSet in this.interleavingDependencies)
            {
                var objsByte = new List<object>();
                var objsShort = new List<object>();
                var objsUshort = new List<object>();
                var objsInt = new List<object>();
                var objsUint = new List<object>();
                var objsLong = new List<object>();
                var objsUlong = new List<object>();
                var objsFixedInt = new Dictionary<int, List<object>>();

                foreach (var arbitraryVariable in dependentVariableSet)
                {
                    if (this.ExistingAssignment.ContainsKey(arbitraryVariable))
                    {
                        continue;
                    }

                    var type = arbitraryVariable.GetType();

                    if (type == typeof(ZenArbitraryExpr<byte>))
                    {
                        objsByte.Add(arbitraryVariable);
                    }

                    if (type == typeof(ZenArbitraryExpr<short>))
                    {
                        objsShort.Add(arbitraryVariable);
                    }

                    if (type == typeof(ZenArbitraryExpr<ushort>))
                    {
                        objsUshort.Add(arbitraryVariable);
                    }

                    if (type == typeof(ZenArbitraryExpr<int>))
                    {
                        objsInt.Add(arbitraryVariable);
                    }

                    if (type == typeof(ZenArbitraryExpr<uint>))
                    {
                        objsUint.Add(arbitraryVariable);
                    }

                    if (type == typeof(ZenArbitraryExpr<long>))
                    {
                        objsLong.Add(arbitraryVariable);
                    }

                    if (type == typeof(ZenArbitraryExpr<ulong>))
                    {
                        objsUlong.Add(arbitraryVariable);
                    }

                    if (type.IsGenericType &&
                        type.GetGenericTypeDefinitionCached() == typeof(ZenArbitraryExpr<>) &&
                        ReflectionUtilities.IsFixedIntegerType(type.GetGenericArgumentsCached()[0]))
                    {
                        var size = ReflectionUtilities.IntegerSize(type.GetGenericArgumentsCached()[0]);

                        if (!objsFixedInt.TryGetValue(size, out List<object> list))
                        {
                            list = new List<object>();
                            objsFixedInt[size] = list;
                        }

                        list.Add(arbitraryVariable);
                    }
                }

                var bytevars = this.Manager.CreateInterleavedInt8(objsByte.Count);
                for (int i = 0; i < bytevars.Length; i++)
                {
                    this.ExistingAssignment[objsByte[i]] = bytevars[i];
                    this.Variables.Add(bytevars[i]);
                }

                var shortvars = this.Manager.CreateInterleavedInt16(objsShort.Count);
                for (int i = 0; i < shortvars.Length; i++)
                {
                    this.ExistingAssignment[objsShort[i]] = shortvars[i];
                    this.Variables.Add(shortvars[i]);
                }

                var ushortvars = this.Manager.CreateInterleavedInt16(objsUshort.Count);
                for (int i = 0; i < ushortvars.Length; i++)
                {
                    this.ExistingAssignment[objsUshort[i]] = ushortvars[i];
                    this.Variables.Add(ushortvars[i]);
                }

                var intvars = this.Manager.CreateInterleavedInt32(objsInt.Count);
                for (int i = 0; i < intvars.Length; i++)
                {
                    this.ExistingAssignment[objsInt[i]] = intvars[i];
                    this.Variables.Add(intvars[i]);
                }

                var uintvars = this.Manager.CreateInterleavedInt32(objsUint.Count);
                for (int i = 0; i < uintvars.Length; i++)
                {
                    this.ExistingAssignment[objsUint[i]] = uintvars[i];
                    this.Variables.Add(uintvars[i]);
                }

                var longvars = this.Manager.CreateInterleavedInt64(objsLong.Count);
                for (int i = 0; i < longvars.Length; i++)
                {
                    this.ExistingAssignment[objsLong[i]] = longvars[i];
                    this.Variables.Add(longvars[i]);
                }

                var ulongvars = this.Manager.CreateInterleavedInt64(objsUlong.Count);
                for (int i = 0; i < ulongvars.Length; i++)
                {
                    this.ExistingAssignment[objsUlong[i]] = ulongvars[i];
                    this.Variables.Add(ulongvars[i]);
                }

                foreach (var kv in objsFixedInt)
                {
                    var size = kv.Key;
                    var fixedvars = this.Manager.CreateInterleavedInt(kv.Value.Count, size);

                    for (int i = 0; i < fixedvars.Length; i++)
                    {
                        this.ExistingAssignment[kv.Value[i]] = fixedvars[i];
                        this.Variables.Add(fixedvars[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the variable to use for an arbitrary expression.
        /// </summary>
        /// <param name="zenArbitraryExpr">The expression.</param>
        /// <param name="variable">The decision diagram variable.</param>
        public void SetVariable(object zenArbitraryExpr, Variable<T> variable)
        {
            this.ExistingAssignment[zenArbitraryExpr] = variable;
        }

        /// <summary>
        /// The false expression.
        /// </summary>
        /// <returns>The expression.</returns>
        public DD False()
        {
            return this.Manager.False();
        }

        /// <summary>
        /// The true expression.
        /// </summary>
        /// <returns>The expression.</returns>
        public DD True()
        {
            return this.Manager.True();
        }

        /// <summary>
        /// The 'Iff' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD Iff(DD x, DD y)
        {
            return this.Manager.Iff(x, y);
        }

        /// <summary>
        /// The 'And' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD And(DD x, DD y)
        {
            return this.Manager.And(x, y);
        }

        /// <summary>
        /// The 'BitwiseAnd' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVector<T> BitwiseAnd(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.And(x, y);
        }

        /// <summary>
        /// The 'BitwiseNot' of an expression.
        /// </summary>
        /// <param name="x">The expression.</param>
        /// <returns></returns>
        public BitVector<T> BitwiseNot(BitVector<T> x)
        {
            return this.Manager.Not(x);
        }

        /// <summary>
        /// The 'BitwiseOr' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVector<T> BitwiseOr(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Or(x, y);
        }

        /// <summary>
        /// The 'BitwiseXor' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVector<T> BitwiseXor(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Xor(x, y);
        }

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVector<T> Add(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Add(x, y);
        }

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVector<T> Subtract(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Subtract(x, y);
        }

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD LessThanOrEqual(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.LessOrEqual(x, y);
        }

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD LessThan(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Less(x, y);
        }

        /// <summary>
        /// The 'LessThanOrEqualSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD LessThanOrEqualSigned(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.LessOrEqualSigned(x, y);
        }

        /// <summary>
        /// The 'LessThanSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD LessThanSigned(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.And(this.Manager.Not(this.Manager.Eq(x, y)), this.Manager.LessOrEqualSigned(x, y));
        }

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD GreaterThanOrEqual(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.GreaterOrEqual(x, y);
        }

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD GreaterThan(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Greater(x, y);
        }

        /// <summary>
        /// The 'GreaterThanSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD GreaterThanSigned(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.And(this.Manager.Not(this.Manager.Eq(x, y)), this.Manager.GreaterOrEqualSigned(x, y));
        }

        /// <summary>
        /// The 'GreaterThanOrEqualSigned' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD GreaterThanOrEqualSigned(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.GreaterOrEqualSigned(x, y);
        }

        /// <summary>
        /// The 'Resize' of a bitvec expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="sourceSize">The source bitwidth.</param>
        /// <param name="targetSize">The target bitwidth.</param>
        /// <returns></returns>
        public BitVector<T> Resize(BitVector<T> x, int sourceSize, int targetSize)
        {
            var newBits = CommonUtilities.CopyBigEndian(x.GetBits(), Manager.False(), targetSize);
            return this.Manager.CreateBitvector(newBits);
        }

        /// <summary>
        /// Create a new boolean expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        public (Variable<T>, DD) CreateBoolVar(object e)
        {
            if (!this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                variable = this.Manager.CreateBool();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
            }

            var varbool = (VarBool<T>)variable;
            return (variable, varbool.Id());
        }

        /// <summary>
        /// Create a new byte expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateByteVar(object e)
        {
            if (!this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                variable = this.Manager.CreateInt8();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
            }

            return (variable, this.Manager.CreateBitvector(variable));
        }

        /// <summary>
        /// Create a byte constant.
        /// </summary>
        /// <returns></returns>
        public BitVector<T> CreateByteConst(byte b)
        {
            return this.Manager.CreateBitvector(b);
        }

        /// <summary>
        /// Create a new short expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateShortVar(object e)
        {
            if (!this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                variable = this.Manager.CreateInt16();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
            }

            return (variable, this.Manager.CreateBitvector(variable));
        }

        /// <summary>
        /// Create a short constant.
        /// </summary>
        /// <returns></returns>
        public BitVector<T> CreateShortConst(short s)
        {
            return this.Manager.CreateBitvector(s);
        }

        /// <summary>
        /// Create a new int expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateIntVar(object e)
        {
            if (!this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                variable = this.Manager.CreateInt32();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
            }

            return (variable, this.Manager.CreateBitvector(variable));
        }

        /// <summary>
        /// Create an integer constant.
        /// </summary>
        /// <returns></returns>
        public BitVector<T> CreateIntConst(int i)
        {
            return this.Manager.CreateBitvector(i);
        }

        /// <summary>
        /// Create a new long expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateLongVar(object e)
        {
            if (!this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                variable = this.Manager.CreateInt64();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
            }

            return (variable, this.Manager.CreateBitvector(variable));
        }

        /// <summary>
        /// Create a long constant.
        /// </summary>
        /// <returns></returns>
        public BitVector<T> CreateLongConst(long l)
        {
            return this.Manager.CreateBitvector(l);
        }

        /// <summary>
        /// Create a bitvector variable.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <param name="size">The size of the bitvector.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateBitvecVar(object e, uint size)
        {
            if (this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                return (variable, this.Manager.CreateBitvector(variable));
            }
            else
            {
                variable = this.Manager.CreateInt((int)size);
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
                return (variable, this.Manager.CreateBitvector(variable));
            }
        }

        /// <summary>
        /// Create a bitvector constant.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <returns>A bitvector expression.</returns>
        public BitVector<T> CreateBitvecConst(bool[] bits)
        {
            var dds = new DD[bits.Length];

            for (int i = 0; i < dds.Length; i++)
            {
                dds[i] = bits[i] ? this.Manager.True() : this.Manager.False();
            }

            return this.Manager.CreateBitvector(dds);
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD Eq(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Eq(x, y);
        }

        /// <summary>
        /// Get the value for a variable in a model.
        /// </summary>
        /// <param name="m">The model.</param>
        /// <param name="v">The variable.</param>
        /// <param name="type">The C# type to coerce the result to.</param>
        /// <returns>The value.</returns>
        public object Get(Assignment<T> m, Variable<T> v, Type type)
        {
            if (v is VarBool<T> v1)
            {
                Contract.Assert(type == typeof(bool), "Internal type mismatch");
                return m.Get(v1);
            }
            else if (v is VarInt8<T> v8)
            {
                Contract.Assert(type == typeof(byte), "Internal type mismatch");
                return m.Get(v8);
            }
            else if (v is VarInt16<T> v16)
            {
                Contract.Assert(type == typeof(short) || type == typeof(ushort), "Internal type mismatch");
                var value = m.Get(v16);
                if (type == typeof(short))
                    return value;
                return (ushort)value;
            }
            else if (v is VarInt32<T> v32)
            {
                Contract.Assert(type == typeof(int) || type == typeof(uint), "Internal type mismatch");
                return type == typeof(int) ? m.Get(v32) : (object)(uint)m.Get(v32);
            }
            else if (v is VarInt64<T> v64)
            {
                Contract.Assert(type == typeof(long) || type == typeof(ulong), "Internal type mismatch");
                return type == typeof(long) ? m.Get(v64) : (object)(ulong)m.Get(v64);
            }
            else
            {
                // fixed width integer. bits are stored in little endian, need to reverse
                Contract.Assert(ReflectionUtilities.IsFixedIntegerType(type), "Internal type mismatch");
                var bytes = m.Get((VarInt<T>)v);
                var remainder = v.NumBits % 8;

                if (remainder != 0)
                {
                    bytes[bytes.Length - 1] >>= (8 - remainder);
                }

                return type.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { bytes });
            }
        }

        /// <summary>
        /// The 'Ite' of a guard and two integers.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public DD Ite(DD g, DD t, DD f)
        {
            if (t.Equals(this.Manager.True()))
            {
                return this.Or(g, f);
            }

            if (t.Equals(this.Manager.False()))
            {
                return this.And(this.Not(g), f);
            }

            if (f.Equals(this.Manager.False()))
            {
                return this.And(g, t);
            }

            if (f.Equals(this.Manager.True()))
            {
                this.Or(this.Not(g), f);
            }

            return this.Manager.Ite(g, t, f);
        }

        /// <summary>
        /// The 'Ite' of a guard and two integers.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        public BitVector<T> Ite(DD g, BitVector<T> t, BitVector<T> f)
        {
            return this.Manager.Ite(g, t, f);
        }

        /// <summary>
        /// The 'Not' of an expression.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <returns></returns>
        public DD Not(DD x)
        {
            return this.Manager.Not(x);
        }

        /// <summary>
        /// The 'Or' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public DD Or(DD x, DD y)
        {
            return this.Manager.Or(x, y);
        }

        /// <summary>
        /// Check whether a boolean expression is satisfiable.
        /// </summary>
        /// <param name="x">The expression.</param>
        /// <returns>A model, if satisfiable.</returns>
        public Assignment<T> Solve(DD x)
        {
            return this.Manager.Sat(x);
        }

        /// <summary>
        /// The 'Ite' of a guard and two integers.
        /// </summary>
        /// <param name="g">The guard expression.</param>
        /// <param name="t">The true expression.</param>
        /// <param name="f">The false expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit Ite(DD g, Unit t, Unit f)
        {
            throw new ZenException("Decision diagram backend does not support this operation. Use Z3 backend.");
        }

        /// <summary>
        /// Create a new dictionary expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateDictVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support dictionary operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a new sequence expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateSeqVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support Seq operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'EmptySeq' for a given type.
        /// </summary>
        /// <param name="type">The type of the sequence.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqEmpty(Type type)
        {
            throw new ZenException("Decision diagram backend does not support Seq operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'SeqUnit' for a given type.
        /// </summary>
        /// <param name="valueExpr">The value expression.</param>
        /// <param name="type">The type of the sequence.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqUnit(SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit> valueExpr, Type type)
        {
            throw new ZenException("Decision diagram backend does not support Seq operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'Concat' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqConcat(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'PrefixOf' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD SeqPrefixOf(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'SuffixOf' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD SeqSuffixOf(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'Contains' of two expressions.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD SeqContains(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'Replace' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <param name="z">The replacement expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqReplaceFirst(Unit x, Unit y, Unit z)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'Slice' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The offset expression.</param>
        /// <param name="z">The length expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqSlice(Unit x, Unit y, Unit z)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'At' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The index expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqAt(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'Nth' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The index expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public object SeqNth(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'Length' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqLength(Unit x)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'IndexOf' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The subseq expression.</param>
        /// <param name="z">The offset expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit SeqIndexOf(Unit x, Unit y, Unit z)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'Equal' of two integers.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD Eq(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support this operation. Use Z3 backend.");
        }

        /// <summary>
        /// Create a new big integer expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateBigIntegerVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a big integer constant.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit CreateBigIntegerConst(BigInteger b)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'Add' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit Add(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'Subtract' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit Subtract(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        public BitVector<T> Multiply(BitVector<T> x, BitVector<T> y)
        {
            throw new ZenException("Decision diagram backend does not support multiplication");
        }

        /// <summary>
        /// The 'Multiply' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit Multiply(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'LessThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD LessThan(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'LessThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD LessThanOrEqual(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'GreaterThan' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD GreaterThan(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The 'GreaterThanOrEqual' of two expressions.
        /// </summary>
        /// <param name="x">The first expression.</param>
        /// <param name="y">The second expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD GreaterThanOrEqual(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        /// <summary>
        /// The empty dictionary.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit DictEmpty(Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
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
        [ExcludeFromCodeCoverage]
        public Unit DictSet(
            Unit arrayExpr,
            SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit> keyExpr,
            SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit> valueExpr, Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }

        /// <summary>
        /// The result of deleting a key from an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit DictDelete(Unit arrayExpr, SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit> keyExpr, Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }

        /// <summary>
        /// The result of getting a value for a key from an array.
        /// </summary>
        /// <param name="arrayExpr">The array expression.</param>
        /// <param name="keyExpr">The key value.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public (DD, object) DictGet(Unit arrayExpr, SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit> keyExpr, Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }

        /// <summary>
        /// The result of unioning two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit DictUnion(Unit arrayExpr1, Unit arrayExpr2)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }

        /// <summary>
        /// The result of intersecting two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit DictIntersect(Unit arrayExpr1, Unit arrayExpr2)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }

        /// <summary>
        /// The result of differencing two arrays.
        /// </summary>
        /// <param name="arrayExpr1">The array expression.</param>
        /// <param name="arrayExpr2">The array value.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit DictDifference(Unit arrayExpr1, Unit arrayExpr2)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }

        /// <summary>
        /// The seq 'Regex' operation.
        /// </summary>
        /// <param name="x">The seq expression.</param>
        /// <param name="y">The regex expression.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public DD SeqRegex<T1>(Unit x, Regex<T1> y)
        {
            throw new ZenException("Decision diagram backend does not support Seq operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a new char expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateCharVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support Char operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a char constant.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit CreateCharConst(char c)
        {
            throw new ZenException("Decision diagram backend does not support Char operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a char constant.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit CreateStringConst(string s)
        {
            throw new ZenException("Decision diagram backend does not support String operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a new real expression.
        /// </summary>
        /// <param name="e">Zen arbitrary expr.</param>
        /// <returns>The expression.</returns>
        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateRealVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support Real operations. Use Z3 backend.");
        }

        /// <summary>
        /// Create a real constant.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Unit CreateRealConst(Real r)
        {
            throw new ZenException("Decision diagram backend does not support Real operations. Use Z3 backend.");
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        [ExcludeFromCodeCoverage]
        public Assignment<T> Maximize(BitVector<T> objective, DD subjectTo)
        {
            throw new ZenException("Decision diagram backend does not support Optimization. Use Z3 backend.");
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The maximize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        [ExcludeFromCodeCoverage]
        public Assignment<T> Maximize(Unit objective, DD subjectTo)
        {
            throw new ZenException("Decision diagram backend does not support Optimization. Use Z3 backend.");
        }

        /// <summary>
        /// Minimize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The minimize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        [ExcludeFromCodeCoverage]
        public Assignment<T> Minimize(BitVector<T> objective, DD subjectTo)
        {
            throw new ZenException("Decision diagram backend does not support Optimization. Use Z3 backend.");
        }

        /// <summary>
        /// Minimize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The minimize objective.</param>
        /// <param name="subjectTo">The constraints expression.</param>
        /// <returns>An optimal model, if satisfiable.</returns>
        [ExcludeFromCodeCoverage]
        public Assignment<T> Minimize(Unit objective, DD subjectTo)
        {
            throw new ZenException("Decision diagram backend does not support Optimization. Use Z3 backend.");
        }

        /// <summary>
        /// Convert a value back into a symbolic value.
        /// </summary>
        /// <param name="e">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit, Unit, Unit> ConvertExprToSymbolicValue(object e, Type type)
        {
            throw new ZenException("Decision diagram backend does not support Map operations. Use Z3 backend.");
        }
    }
}
