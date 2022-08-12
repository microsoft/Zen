// <copyright file="SymbolicMergeVisitor.cs" company="Microsoft">
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
    internal class SymbolicMergeVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> :
        TypeVisitor<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>,
            (TBool,
             SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>,
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
        /// Creates a new instance of the <see cref="SymbolicMergeVisitor{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="evaluationVisitor">The evaluation visitor.</param>
        /// <param name="evaluationEnv">The evaluation environment.</param>
        public SymbolicMergeVisitor(
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
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBigInteger((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBool((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitByte((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitChar((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitConstMap(Type mapType, Type keyType, Type valueType, (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;

            var keys1 = new HashSet<object>(v1.Value.Keys);
            var keys2 = new HashSet<object>(v2.Value.Keys);
            keys1.UnionWith(keys2);
            object deflt = null;

            var result = ImmutableDictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
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

                result = result.Add(key, this.Visit(valueType, (guard, val1, val2)));
            }

            return new SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result);

            /* var result = ImmutableDictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            foreach (var kv in v1.Value)
            {
                var x1 = kv.Value;
                var x2 = v2.Value[kv.Key];
                result = result.Add(kv.Key, this.Visit(valueType, (guard, x1, x2)));
            }

            return new SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result); */
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitFixedInteger(Type intType, (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitInt((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitList(Type listType, Type innerType, (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var result = Merge(innerType, guard, v1.GuardedListGroup, v2.GuardedListGroup);
            return new SymbolicList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result);
        }

        /// <summary>
        /// Merge two groups of guarded lists together.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        /// <param name="guard">The guard.</param>
        /// <param name="lists1">The first lists.</param>
        /// <param name="lists2">The second lists.</param>
        /// <returns>A new guarded group of lists.</returns>
        private GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            Type elementType,
            TBool guard,
            GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> lists1,
            GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> lists2)
        {
            var result = CommonUtilities.Merge(lists1.Mapping, lists2.Mapping, (len, list1, list2) =>
            {
                if (list1.HasValue && !list2.HasValue)
                {
                    var newList = MapGuard(guard, list1.Value);
                    return Option.Some(newList);
                }

                if (!list1.HasValue && list2.HasValue)
                {
                    var newList = MapGuard(this.solver.Not(guard), list2.Value);
                    return Option.Some(newList);
                }

                var merged = Merge(elementType, guard, list1.Value, list2.Value);

                /* if (merged.Guard.Equals(this.Solver.False()))
                {
                    return Option.None<GuardedList<TModel, TVar, TBool, TInt, TSeq, TArray, TChar, TReal>>();
                } */

                return Option.Some(merged);
            });

            return new GuardedListGroup<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(result);
        }

        /// <summary>
        /// Merge two guarded lists together.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        /// <param name="guard">The guard.</param>
        /// <param name="list1">The first list.</param>
        /// <param name="list2">The second list.</param>
        /// <returns></returns>
        private GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> Merge(
            Type elementType,
            TBool guard,
            GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> list1,
            GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> list2)
        {
            var newGuard = this.solver.Ite(guard, list1.Guard, list2.Guard);
            var newValues = ImmutableList<SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            for (int i = 0; i < list1.Values.Count; i++)
            {
                var v1 = list1.Values[i];
                var v2 = list2.Values[i];
                newValues = newValues.Add(this.Visit(elementType, (guard, v1, v2)));
            }

            return new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(newGuard, newValues);
        }

        /// <summary>
        /// Map the guard over a list.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> MapGuard(TBool guard, GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> list)
        {
            var newGuard = this.solver.And(guard, list.Guard);
            return new GuardedList<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(newGuard, list.Values);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitLong((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMap(Type mapType, Type keyType, Type valueType, (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The field types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitObject(Type objectType, SortedDictionary<string, Type> fields, (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;

            var newValue = ImmutableSortedDictionary<string, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            foreach (var kv in fields)
            {
                var value1 = v1.Fields[kv.Key];
                var value2 = v2.Fields[kv.Key];
                newValue = newValue.Add(kv.Key, this.Visit(kv.Value, (guard, value1, value2)));
            }

            return new SymbolicObject<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(objectType, this.solver, newValue);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitReal((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeq(Type sequenceType, Type innerType, (TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitShort((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitString((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicString<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUint((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUlong((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUshort((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            return BitvecMerge(parameter);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The result.</returns>
        private SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> BitvecMerge((TBool, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>) parameter)
        {
            var guard = parameter.Item1;
            var v1 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item2;
            var v2 = (SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>)parameter.Item3;
            var value = this.solver.Ite(guard, v1.Value, v2.Value);
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, value);
        }
    }
}
