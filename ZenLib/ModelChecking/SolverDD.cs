// <copyright file="SolverDD.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using DecisionDiagrams;

    /// <summary>
    /// Implementation of a solver using decision diagrams.
    /// </summary>
    /// <typeparam name="T">The diagram node type.</typeparam>
    internal class SolverDD<T> : ISolver<Assignment<T>, Variable<T>, DD, BitVector<T>, Unit>
        where T : IDDNode
    {
        /// <summary>
        /// Gets the manger object.
        /// </summary>
        internal DDManager<T> Manager { get; }

        /// <summary>
        /// All allocated variables.
        /// </summary>
        internal List<Variable<T>> Variables { get; } = new List<Variable<T>>();

        /// <summary>
        /// The variable "must interleave" dependencies.
        /// </summary>
        private Dictionary<object, ImmutableHashSet<object>> interleavingDependencies { get; }

        /// <summary>
        /// The existing assignment provided.
        /// </summary>
        private Dictionary<object, Variable<T>> ExistingAssignment { get; }

        /// <summary>
        /// Creates a new instanceof the SolverDD class.
        /// </summary>
        /// <param name="manager">The manager object.</param>
        /// <param name="interleavingDependencies">Variable interleaving data.</param>
        public SolverDD(DDManager<T> manager, Dictionary<object, ImmutableHashSet<object>> interleavingDependencies)
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
            foreach (var set in this.interleavingDependencies.Values)
            {
                var objsByte = new List<object>();
                var objsShort = new List<object>();
                var objsUshort = new List<object>();
                var objsInt = new List<object>();
                var objsUint = new List<object>();
                var objsLong = new List<object>();
                var objsUlong = new List<object>();

                foreach (var elt in set)
                {
                    var type = elt.GetType();

                    if (type == typeof(ZenArbitraryExpr<byte>))
                    {
                        objsByte.Add(elt);
                    }

                    if (type == typeof(ZenArbitraryExpr<short>))
                    {
                        objsShort.Add(elt);
                    }

                    if (type == typeof(ZenArbitraryExpr<ushort>))
                    {
                        objsUshort.Add(elt);
                    }

                    if (type == typeof(ZenArbitraryExpr<int>))
                    {
                        objsInt.Add(elt);
                    }

                    if (type == typeof(ZenArbitraryExpr<uint>))
                    {
                        objsUint.Add(elt);
                    }

                    if (type == typeof(ZenArbitraryExpr<long>))
                    {
                        objsLong.Add(elt);
                    }

                    if (type == typeof(ZenArbitraryExpr<ulong>))
                    {
                        objsUlong.Add(elt);
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

        public Unit Concat(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        public DD Eq(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Eq(x, y);
        }

        public DD Eq(Unit x, Unit y)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
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

        public (Variable<T>, Unit) CreateStringVar(object e)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        public Unit CreateStringConst(string s)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        public object Get(Assignment<T> m, Variable<T> v)
        {
            if (v is VarBool<T> v1)
            {
                return m.Get(v1);
            }

            if (v is VarInt8<T> v8)
            {
                return m.Get(v8);
            }

            if (v is VarInt16<T> v16)
            {
                return m.Get(v16);
            }

            if (v is VarInt32<T> v32)
            {
                return m.Get(v32);
            }

            return m.Get((VarInt64<T>)v);
        }

        public DD Ite(DD g, DD t, DD f)
        {
            return this.Manager.Ite(g, t, f);
        }

        public BitVector<T> Ite(DD g, BitVector<T> t, BitVector<T> f)
        {
            return this.Manager.Ite(g, t, f);
        }

        public Unit Ite(DD g, Unit t, Unit f)
        {
            throw new ZenException("Decision diagram backend does not support string operations. Use Z3 backend.");
        }

        public DD Not(DD x)
        {
            return this.Manager.Not(x);
        }

        public DD Or(DD x, DD y)
        {
            return this.Manager.Or(x, y);
        }

        public Option<Assignment<T>> Satisfiable(DD x)
        {
            var m = this.Manager.Sat(x);
            if (m == null)
            {
                return Option.None<Assignment<T>>();
            }

            return Option.Some(m);
        }
    }
}
