// <copyright file="SymbolicEqualityVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using ZenLib.Solver;

    /// <summary>
    /// A class to derive an equality expression for two
    /// symbolic values of the same type.
    /// </summary>
    internal class SymbolicEqualityVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> :
        TypeVisitor<TBool,
            (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>,
             SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)>
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
        /// The solver instance.
        /// </summary>
        private ISolver<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> solver;

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolicEqualityVisitor{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="evaluationVisitor">The evaluation visitor.</param>
        /// <param name="evaluationEnv">The evaluation environment.</param>
        public SymbolicEqualityVisitor(
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
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitBigInteger((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitBool((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Iff(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitByte((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitChar((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitConstMap(Type mapType, Type keyType, Type valueType, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;

            var keys1 = new HashSet<object>(v1.Value.Keys);
            var keys2 = new HashSet<object>(v2.Value.Keys);
            keys1.UnionWith(keys2);
            object deflt = null;

            var result = this.solver.True();
            foreach (var key in keys1)
            {
                if (!v1.Value.TryGetValue(key, out var val1))
                {
                    deflt = deflt ?? ReflectionUtilities.CreateZenConstant(ReflectionUtilities.GetDefaultValue(valueType));
                    val1 = this.evaluationVisitor.Visit((dynamic)deflt, this.evaluationEnv);
                }

                if (!v2.Value.TryGetValue(key, out var val2))
                {
                    deflt = deflt ?? ReflectionUtilities.CreateZenConstant(ReflectionUtilities.GetDefaultValue(valueType));
                    val2 = this.evaluationVisitor.Visit((dynamic)deflt, this.evaluationEnv);
                }

                var valuesEq = this.Visit(valueType, (val1, val2));
                result = this.solver.And(result,  valuesEq);
            }

            return result;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitFixedInteger(Type intType, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitInt((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitList(Type listType, Type innerType, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;

            object deflt = null;
            var result = this.solver.True();

            for (int i = 0; i < Math.Max(v1.Value.Count, v2.Value.Count); i++)
            {
                (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) elt1;
                (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) elt2;

                if (i < v1.Value.Count)
                {
                    elt1 = v1.Value[i];
                }
                else
                {
                    deflt = deflt ?? ReflectionUtilities.CreateZenConstant(ReflectionUtilities.GetDefaultValue(innerType));
                    elt1 = (this.solver.False(), this.evaluationVisitor.Visit((dynamic)deflt, this.evaluationEnv));
                }

                if (i < v2.Value.Count)
                {
                    elt2 = v2.Value[i];
                }
                else
                {
                    deflt = deflt ?? ReflectionUtilities.CreateZenConstant(ReflectionUtilities.GetDefaultValue(innerType));
                    elt2 = (this.solver.False(), this.evaluationVisitor.Visit((dynamic)deflt, this.evaluationEnv));
                }

                var eqEnabled = this.solver.Iff(elt1.Item1, elt2.Item1);
                var eqValues = this.Visit(innerType, (elt1.Item2, elt2.Item2));
                var eqElements = this.solver.And(eqEnabled, this.solver.Or(this.solver.Not(elt1.Item1), eqValues));
                result = this.solver.And(result, eqElements);
            }

            return result;
        }

        /// <summary>
        /// Equality for finite sets.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        private TBool EqualFiniteSets(Type listType, Type innerType, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicFSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;

            return this.solver.And(
                EqualFiniteSetsHelper(innerType, v1.Value, v2.Value),
                EqualFiniteSetsHelper(innerType, v2.Value, v1.Value));
        }

        /// <summary>
        /// Helper function for finite set equality.
        /// </summary>
        /// <param name="innerType">The inner type.</param>
        /// <param name="values1">The symbolic values for the first set.</param>
        /// <param name="values2">The symbolic values for the second set.</param>
        /// <returns></returns>
        private TBool EqualFiniteSetsHelper(
            Type innerType,
            ImmutableList<(TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)> values1,
            ImmutableList<(TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)> values2)
        {
            var result = this.solver.True();

            foreach (var elt1 in values1)
            {
                var isContained = this.solver.False();
                foreach (var elt2 in values2)
                {
                    var eqValues = this.Visit(innerType, (elt1.Item2, elt2.Item2));
                    isContained = this.solver.Or(isContained, this.solver.And(elt2.Item1, eqValues));
                }

                var elementActiveImpliesContained = this.solver.Or(this.solver.Not(elt1.Item1), isContained);
                result = this.solver.And(result, elementActiveImpliesContained);
            }

            return result;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitLong((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitMap(Type mapType, Type keyType, Type valueType, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The field types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitObject(Type objectType, SortedDictionary<string, Type> fields, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;

            // special case for equality with the FSet type.
            if (ReflectionUtilities.IsFSetType(objectType))
            {
                var l1 = v1.Fields["Values"];
                var l2 = v2.Fields["Values"];
                return EqualFiniteSets(fields["Values"], fields["Values"].GetGenericArgumentsCached()[0], (l1, l2));
            }

            var result = this.solver.True();
            foreach (var kv in fields)
            {
                var fieldName = kv.Key;
                var fieldType = kv.Value;

                var fv1 = v1.Fields[fieldName];
                var fv2 = v2.Fields[fieldName];

                var fieldsEq = this.Visit(fieldType, (fv1, fv2));
                result = this.solver.And(result, fieldsEq);
            }

            return result;
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitReal((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitSeq(Type sequenceType, Type innerType, (SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitShort((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitString((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitUint((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitUlong((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override TBool VisitUshort((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecEquality(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        private TBool BitvecEquality((SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item1;
            var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            return this.solver.Eq(v1.Value, v2.Value);
        }
    }
}
