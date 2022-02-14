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
    internal class SolverDD<T> : ISolver<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit>
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
                        var size = CommonUtilities.IntegerSize(type.GetGenericArgumentsCached()[0]);

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

        public DD False()
        {
            return this.Manager.False();
        }

        public DD True()
        {
            return this.Manager.True();
        }

        public DD Iff(DD x, DD y)
        {
            return this.Manager.Iff(x, y);
        }

        public DD And(DD x, DD y)
        {
            return this.Manager.And(x, y);
        }

        public BitVector<T> BitwiseAnd(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.And(x, y);
        }

        public BitVector<T> BitwiseNot(BitVector<T> x)
        {
            return this.Manager.Not(x);
        }

        public BitVector<T> BitwiseOr(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Or(x, y);
        }

        public BitVector<T> BitwiseXor(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Xor(x, y);
        }

        public BitVector<T> Add(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Add(x, y);
        }

        public BitVector<T> Subtract(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Subtract(x, y);
        }

        public BitVector<T> Multiply(BitVector<T> x, BitVector<T> y)
        {
            throw new ZenException("Decision diagram backend does not support multiplication");
        }

        [ExcludeFromCodeCoverage]
        public Unit Concat(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public DD PrefixOf(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public DD SuffixOf(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public DD Contains(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit ReplaceFirst(Unit x, Unit y, Unit z)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit Substring(Unit x, Unit y, Unit z)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit At(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit Length(Unit x)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit IndexOf(Unit x, Unit y, Unit z)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        public DD Eq(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Eq(x, y);
        }

        [ExcludeFromCodeCoverage]
        public DD Eq(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support this operation. Use Z3 backend.");
        }

        public DD LessThanOrEqual(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.LessOrEqual(x, y);
        }

        public DD GreaterThanOrEqual(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.GreaterOrEqual(x, y);
        }

        public DD LessThanOrEqualSigned(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.LessOrEqualSigned(x, y);
        }

        public DD GreaterThanOrEqualSigned(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.GreaterOrEqualSigned(x, y);
        }

        public (Variable<T>, DD) CreateBoolVar(object e)
        {
            if (this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                var varbool = (VarBool<T>)variable;
                return (variable, varbool.Id());
            }
            else
            {
                var varbool = this.Manager.CreateBool();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = varbool;
                return (varbool, varbool.Id());
            }
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateByteVar(object e)
        {
            if (this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                return (variable, this.Manager.CreateBitvector(variable));
            }
            else
            {
                variable = this.Manager.CreateInt8();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
                return (variable, this.Manager.CreateBitvector(variable));
            }
        }

        public BitVector<T> CreateByteConst(byte b)
        {
            return this.Manager.CreateBitvector(b);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateShortVar(object e)
        {
            if (this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                return (variable, this.Manager.CreateBitvector(variable));
            }
            else
            {
                variable = this.Manager.CreateInt16();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
                return (variable, this.Manager.CreateBitvector(variable));
            }
        }

        public BitVector<T> CreateShortConst(short s)
        {
            return this.Manager.CreateBitvector(s);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateIntVar(object e)
        {
            if (this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                return (variable, this.Manager.CreateBitvector(variable));
            }
            else
            {
                variable = this.Manager.CreateInt32();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
                return (variable, this.Manager.CreateBitvector(variable));
            }
        }

        public BitVector<T> CreateIntConst(int i)
        {
            return this.Manager.CreateBitvector(i);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateLongVar(object e)
        {
            if (this.ExistingAssignment.TryGetValue(e, out var variable))
            {
                return (variable, this.Manager.CreateBitvector(variable));
            }
            else
            {
                variable = this.Manager.CreateInt64();
                this.Variables.Add(variable);
                this.ExistingAssignment[e] = variable;
                return (variable, this.Manager.CreateBitvector(variable));
            }
        }

        public BitVector<T> CreateLongConst(long l)
        {
            return this.Manager.CreateBitvector(l);
        }

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

        public BitVector<T> CreateBitvecConst(bool[] bits)
        {
            var dds = new DD[bits.Length];

            for (int i = 0; i < dds.Length; i++)
            {
                dds[i] = bits[i] ? this.Manager.True() : this.Manager.False();
            }

            return this.Manager.CreateBitvector(dds);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateStringVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit CreateStringConst(string s)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateDictVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support dictionary operations. Use Z3 backend.");
        }

        public object Get(Assignment<T> m, Variable<T> v, Type type)
        {
            if (v is VarBool<T> v1)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(bool), "Internal type mismatch");
                return m.Get(v1);
            }
            else if (v is VarInt8<T> v8)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(byte), "Internal type mismatch");
                return m.Get(v8);
            }
            else if (v is VarInt16<T> v16)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(short) || type == typeof(ushort), "Internal type mismatch");
                return type == typeof(short) ? m.Get(v16) : (object)(ushort)m.Get(v16);
            }
            else if (v is VarInt32<T> v32)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(int) || type == typeof(uint), "Internal type mismatch");
                return type == typeof(int) ? m.Get(v32) : (object)(uint)m.Get(v32);
            }
            else if (v is VarInt64<T> v64)
            {
                CommonUtilities.ValidateIsTrue(type == typeof(long) || type == typeof(ulong), "Internal type mismatch");
                return type == typeof(long) ? m.Get(v64) : (object)(ulong)m.Get(v64);
            }
            else
            {
                // fixed width integer. bits are stored in little endian, need to reverse
                CommonUtilities.ValidateIsTrue(ReflectionUtilities.IsFixedIntegerType(type), "Internal type mismatch");
                var bytes = m.Get((VarInt<T>)v);
                var remainder = v.NumBits % 8;

                if (remainder != 0)
                {
                    bytes[bytes.Length - 1] >>= (8 - remainder);
                }

                return type.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { bytes });
            }
        }

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

        public BitVector<T> Ite(DD g, BitVector<T> t, BitVector<T> f)
        {
            return this.Manager.Ite(g, t, f);
        }

        [ExcludeFromCodeCoverage]
        public Unit Ite(DD g, Unit t, Unit f)
        {
            throw new ZenException("Decision diagram backend does not support this operation. Use Z3 backend.");
        }

        public DD Not(DD x)
        {
            return this.Manager.Not(x);
        }

        public DD Or(DD x, DD y)
        {
            return this.Manager.Or(x, y);
        }

        public Assignment<T> Satisfiable(DD x)
        {
            return this.Manager.Sat(x);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, Unit) CreateBigIntegerVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit CreateBigIntegerConst(BigInteger b)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit Add(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit Subtract(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit Multiply(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public DD LessThanOrEqual(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public DD GreaterThanOrEqual(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support BigInteger operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit DictEmpty(Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit DictSet(Unit arrayExpr, SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit> keyExpr, SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit> valueExpr, Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit DictDelete(Unit arrayExpr, SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit> keyExpr, Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public (DD, object) DictGet(Unit arrayExpr, SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit> keyExpr, Type keyType, Type valueType)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public SymbolicValue<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit, Unit, Unit> ConvertExprToSymbolicValue(object e, Type type)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit DictUnion(Unit arrayExpr1, Unit arrayExpr2)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit DictIntersect(Unit arrayExpr1, Unit arrayExpr2)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }

        [ExcludeFromCodeCoverage]
        public Unit DictDifference(Unit arrayExpr1, Unit arrayExpr2)
        {
            throw new ZenException("Decision diagram backend does not support IDictionary operations. Use Z3 backend.");
        }
    }
}
