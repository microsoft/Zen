// <copyright file="SolverDD.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.ModelChecking
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using DecisionDiagrams;

    /// <summary>
    /// Implementation of a solver using decision diagrams.
    /// </summary>
    /// <typeparam name="T">The diagram node type.</typeparam>
    internal class SolverDD<T> : ISolver<Assignment<T>, Variable<T>, DD, BitVector<T>>
        where T : IDDNode
    {
        /// <summary>
        /// Gets the manger object.
        /// </summary>
        private DDManager<T> Manager { get; }

        /// <summary>
        /// Allocated manager variables for byte variables.
        /// </summary>
        private Dictionary<object, VarInt8<T>> Allocated8 { get; }

        /// <summary>
        /// Allocated manager variables for short variables.
        /// </summary>
        private Dictionary<object, VarInt16<T>> Allocated16 { get; }

        /// <summary>
        /// Allocated manager variables for int variables.
        /// </summary>
        private Dictionary<object, VarInt32<T>> Allocated32 { get; }

        /// <summary>
        /// Allocated manager variables for long variables.
        /// </summary>
        private Dictionary<object, VarInt64<T>> Allocated64 { get; }

        /// <summary>
        /// Creates a new instanceof the SolverDD class.
        /// </summary>
        /// <param name="manager">The manager object.</param>
        /// <param name="mustInterleave">Variable interleaving data.</param>
        public SolverDD(DDManager<T> manager, Dictionary<object, ImmutableHashSet<object>> mustInterleave)
        {
            this.Manager = manager;
            this.Allocated8 = new Dictionary<object, VarInt8<T>>();
            this.Allocated16 = new Dictionary<object, VarInt16<T>>();
            this.Allocated32 = new Dictionary<object, VarInt32<T>>();
            this.Allocated64 = new Dictionary<object, VarInt64<T>>();

            foreach (var set in mustInterleave.Values)
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
                    if (elt.GetType() == typeof(ZenArbitraryExpr<byte>))
                    {
                        objsByte.Add(elt);
                    }

                    if (elt.GetType() == typeof(ZenArbitraryExpr<short>))
                    {
                        objsShort.Add(elt);
                    }

                    if (elt.GetType() == typeof(ZenArbitraryExpr<ushort>))
                    {
                        objsUshort.Add(elt);
                    }

                    if (elt.GetType() == typeof(ZenArbitraryExpr<int>))
                    {
                        objsInt.Add(elt);
                    }

                    if (elt.GetType() == typeof(ZenArbitraryExpr<uint>))
                    {
                        objsUint.Add(elt);
                    }

                    if (elt.GetType() == typeof(ZenArbitraryExpr<long>))
                    {
                        objsLong.Add(elt);
                    }

                    if (elt.GetType() == typeof(ZenArbitraryExpr<ulong>))
                    {
                        objsUlong.Add(elt);
                    }
                }

                var bytevars = manager.CreateInterleavedInt8(objsByte.Count);
                for (int i = 0; i < bytevars.Length; i++)
                {
                    this.Allocated8[objsByte[i]] = bytevars[i];
                }

                var shortvars = manager.CreateInterleavedInt16(objsShort.Count);
                for (int i = 0; i < shortvars.Length; i++)
                {
                    this.Allocated16[objsShort[i]] = shortvars[i];
                }

                var ushortvars = manager.CreateInterleavedInt16(objsUshort.Count);
                for (int i = 0; i < ushortvars.Length; i++)
                {
                    this.Allocated16[objsUshort[i]] = ushortvars[i];
                }

                var intvars = manager.CreateInterleavedInt32(objsInt.Count);
                for (int i = 0; i < intvars.Length; i++)
                {
                    this.Allocated32[objsInt[i]] = intvars[i];
                }

                var uintvars = manager.CreateInterleavedInt32(objsUint.Count);
                for (int i = 0; i < uintvars.Length; i++)
                {
                    this.Allocated32[objsUint[i]] = uintvars[i];
                }

                var longvars = manager.CreateInterleavedInt64(objsLong.Count);
                for (int i = 0; i < longvars.Length; i++)
                {
                    this.Allocated64[objsLong[i]] = longvars[i];
                }

                var ulongvars = manager.CreateInterleavedInt64(objsUlong.Count);
                for (int i = 0; i < ulongvars.Length; i++)
                {
                    this.Allocated64[objsUlong[i]] = ulongvars[i];
                }
            }
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

        public DD Eq(BitVector<T> x, BitVector<T> y)
        {
            return this.Manager.Eq(x, y);
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
            var variable = this.Manager.CreateBool();
            return (variable, variable.Id());
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateByteVar(object e)
        {
            var variable = this.Allocated8.ContainsKey(e) ? this.Allocated8[e] : this.Manager.CreateInt8();
            return (variable, this.Manager.CreateBitvector(variable));
        }

        public BitVector<T> CreateByteConst(byte b)
        {
            return this.Manager.CreateBitvector(b);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateShortVar(object e)
        {
            var variable = this.Allocated16.ContainsKey(e) ? this.Allocated16[e] : this.Manager.CreateInt16();
            return (variable, this.Manager.CreateBitvector(variable));
        }

        public BitVector<T> CreateShortConst(short s)
        {
            return this.Manager.CreateBitvector(s);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateIntVar(object e)
        {
            var variable = this.Allocated32.ContainsKey(e) ? this.Allocated32[e] : this.Manager.CreateInt32();
            return (variable, this.Manager.CreateBitvector(variable));
        }

        public BitVector<T> CreateIntConst(int i)
        {
            return this.Manager.CreateBitvector(i);
        }

        [ExcludeFromCodeCoverage]
        public (Variable<T>, BitVector<T>) CreateLongVar(object e)
        {
            var variable = this.Allocated64.ContainsKey(e) ? this.Allocated64[e] : this.Manager.CreateInt64();
            return (variable, this.Manager.CreateBitvector(variable));
        }

        public BitVector<T> CreateLongConst(long l)
        {
            return this.Manager.CreateBitvector(l);
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

        public BitVector<T> Max(BitVector<T> x, BitVector<T> y, bool signed)
        {
            var geq = signed ?
                this.Manager.GreaterOrEqualSigned(x, y) :
                this.Manager.GreaterOrEqual(x, y);

            return this.Manager.Ite(geq, x, y);
        }

        public BitVector<T> Min(BitVector<T> x, BitVector<T> y, bool signed)
        {
            var leq = signed ?
                this.Manager.LessOrEqualSigned(x, y) :
                this.Manager.LessOrEqual(x, y);

            return this.Manager.Ite(leq, x, y);
        }
    }
}