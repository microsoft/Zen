// <copyright file="SymbolicConstantVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using ZenLib.Solver;

    /// <summary>
    /// Class to convert a C# constant into a symbolc value.
    /// </summary>
    internal class SymbolicConstantVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> :
        TypeVisitor<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, object>
    {
        /// <summary>
        /// The evaluation visitor.
        /// </summary>
        private SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> evaluationVisitor;

        /// <summary>
        /// The evaluation environment.
        /// </summary>
        private SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> evaluationEnv;

        /// <summary>
        /// The symbolic solver.
        /// </summary>
        private ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver;

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolicConstantVisitor{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="evaluationVisitor">The evaluation visitor.</param>
        /// <param name="evaluationEnv">The evaluation environment.</param>
        public SymbolicConstantVisitor(
            SymbolicEvaluationVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> evaluationVisitor,
            SymbolicEvaluationEnvironment<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> evaluationEnv)
        {
            this.evaluationVisitor = evaluationVisitor;
            this.evaluationEnv = evaluationEnv;
            this.solver = evaluationVisitor.Solver;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBigInteger(object parameter)
        {
            var bi = this.solver.CreateBigIntegerConst((BigInteger)parameter);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bi);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBool(object parameter)
        {
            var b = (bool)parameter ? this.solver.True() : this.solver.False();
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, b);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitByte(object parameter)
        {
            var bv = this.solver.CreateByteConst((byte)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitChar(object parameter)
        {
            var c = this.solver.CreateCharConst((char)parameter);
            return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, c);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitConstMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            var result = ImmutableDictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            dynamic constant = parameter;
            foreach (var kv in constant.Values)
            {
                result = result.SetItem(kv.Key, this.evaluationVisitor.Visit(Zen.Constant(kv.Value), this.evaluationEnv));
            }

            return new SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitFixedInteger(Type intType, object parameter)
        {
            var bv = this.solver.CreateBitvecConst(((dynamic)parameter).GetBits());
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitInt(object parameter)
        {
            var bv = this.solver.CreateIntConst((int)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The element type.</param>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitList(Type listType, Type innerType, object parameter)
        {
            var list = ImmutableList<(TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)>.Empty;
            dynamic constant = parameter;
            foreach (var element in constant.ToList())
            {
                list = list.Add((this.solver.True(), this.Visit(innerType, element)));
            }

            return new SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, list);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitLong(object parameter)
        {
            var bv = this.solver.CreateLongConst((long)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        [ExcludeFromCodeCoverage]
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            var result = this.solver.DictEmpty(keyType, valueType);
            dynamic map = parameter;
            foreach (var kv in map.Values)
            {
                var keyExpr = this.Visit(keyType, kv.Key);
                var valueExpr = this.Visit(valueType, kv.Value);
                result = this.solver.DictSet(result, keyExpr, valueExpr, keyType, valueType);
            }
            return new SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitObject(Type objectType, SortedDictionary<string, Type> fields, object parameter)
        {
            var result = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            foreach (var field in fields)
            {
                var fieldName = field.Key;
                var fieldType = field.Value;
                var fieldValue = ReflectionUtilities.GetFieldOrProperty(parameter, fieldName);
                result = result.Add(fieldName, this.Visit(fieldType, fieldValue));
            }

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(objectType, this.solver, result);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitReal(object parameter)
        {
            var bi = this.solver.CreateRealConst((Real)parameter);
            return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bi);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeq(Type sequenceType, Type innerType, object parameter)
        {
            if (sequenceType == typeof(Seq<char>))
            {
                var escapedString = CommonUtilities.ConvertCShaprStringToZ3((Seq<char>)parameter);
                var s = this.solver.CreateStringConst(escapedString);
                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, s);
            }
            else
            {
                var result = this.solver.SeqEmpty(innerType);
                dynamic seq = parameter;
                foreach (var value in seq.Values)
                {
                    var element = this.Visit(innerType, value);
                    result = this.solver.SeqConcat(result, this.solver.SeqUnit(element, innerType));
                }

                return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result);
            }
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitShort(object parameter)
        {
            var bv = this.solver.CreateShortConst((short)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        [ExcludeFromCodeCoverage]
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitString(object parameter)
        {
            var seq = Seq.FromString((string)parameter);
            var escapedString = CommonUtilities.ConvertCShaprStringToZ3(seq);
            var s = this.solver.CreateStringConst(escapedString);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, s);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUint(object parameter)
        {
            var bv = this.solver.CreateIntConst((int)(uint)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUlong(object parameter)
        {
            var bv = this.solver.CreateLongConst((long)(ulong)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The C# constant.</param>
        /// <returns>A symbolic value for the constant.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUshort(object parameter)
        {
            var bv = this.solver.CreateShortConst((short)(ushort)parameter);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, bv);
        }
    }
}
