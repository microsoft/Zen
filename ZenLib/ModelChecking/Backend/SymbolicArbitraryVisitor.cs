// <copyright file="SymbolicArbitraryVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Solver;

    /// <summary>
    /// Class to convert an arbitrary expression into a symbolc value.
    /// </summary>
    internal class SymbolicArbitraryVisitor<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> :
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
        /// Creates a new instance of the <see cref="SymbolicArbitraryVisitor{TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal}"/> class.
        /// </summary>
        /// <param name="evaluationVisitor">The evaluation visitor.</param>
        /// <param name="evaluationEnv">The evaluation environment.</param>
        public SymbolicArbitraryVisitor(
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
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBigInteger(object parameter)
        {
            var (variable, expr) = this.solver.CreateBigIntegerVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicInteger<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitBool(object parameter)
        {
            var (variable, expr) = this.solver.CreateBoolVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBool<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitByte(object parameter)
        {
            var (variable, expr) = this.solver.CreateByteVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitChar(object parameter)
        {
            var (variable, expr) = this.solver.CreateCharVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicChar<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitConstMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            var constants = this.evaluationVisitor.MapConstants[mapType];
            var arbitraryMethod = typeof(Zen).GetMethod("Symbolic").MakeGenericMethod(valueType);

            var assignment = new Dictionary<object, object>();
            var result = ImmutableDictionary<object, SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>>.Empty;
            foreach (var constant in constants)
            {
                var newArbitrary = arbitraryMethod.Invoke(null, new object[] { "k!", 5 });
                var symbolicValue = this.evaluationVisitor.Visit((dynamic)newArbitrary, this.evaluationEnv);
                result = result.Add(constant, symbolicValue);
                assignment[constant] = newArbitrary;
            }

            this.evaluationVisitor.ConstMapAssignment[parameter] = assignment;
            return new SymbolicConstMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, result);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitFixedInteger(Type intType, object parameter)
        {
            var size = ReflectionUtilities.IntegerSize(intType);
            var (v, e) = this.solver.CreateBitvecVar(parameter, (uint)size);
            this.evaluationVisitor.Variables.Add(v);
            this.evaluationVisitor.ArbitraryVariables[parameter] = v;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, e);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitInt(object parameter)
        {
            var (variable, expr) = this.solver.CreateIntVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        [ExcludeFromCodeCoverage]
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitList(Type listType, Type innerType, object parameter)
        {
            throw new ZenUnreachableException();
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitLong(object parameter)
        {
            var (variable, expr) = this.solver.CreateLongVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="mapType">The map type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitMap(Type mapType, Type keyType, Type valueType, object parameter)
        {
            var (variable, expr) = this.solver.CreateDictVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicMap<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        [ExcludeFromCodeCoverage]
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitObject(Type objectType, SortedDictionary<string, Type> fields, object parameter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitReal(object parameter)
        {
            var (variable, expr) = this.solver.CreateRealVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicReal<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The inner type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitSeq(Type sequenceType, Type innerType, object parameter)
        {
            var (v, e) = this.solver.CreateSeqVar(parameter);
            this.evaluationVisitor.Variables.Add(v);
            this.evaluationVisitor.ArbitraryVariables[parameter] = v;
            return new SymbolicSeq<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, e);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitShort(object parameter)
        {
            var (variable, expr) = this.solver.CreateShortVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        [ExcludeFromCodeCoverage]
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitString(object parameter)
        {
            throw new ZenUnreachableException();
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUint(object parameter)
        {
            var (variable, expr) = this.solver.CreateIntVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUlong(object parameter)
        {
            var (variable, expr) = this.solver.CreateLongVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }

        /// <summary>
        /// Visit a type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A symbolic value.</returns>
        public override SymbolicValue<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal> VisitUshort(object parameter)
        {
            var (variable, expr) = this.solver.CreateShortVar(parameter);
            this.evaluationVisitor.Variables.Add(variable);
            this.evaluationVisitor.ArbitraryVariables[parameter] = variable;
            return new SymbolicBitvec<TModel, TVar, TBool, TBitvec, TInt, TSeq, TArray, TChar, TReal>(this.solver, expr);
        }
    }
}
