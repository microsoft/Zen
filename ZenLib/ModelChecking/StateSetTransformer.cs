// <copyright file="StateSetTransformer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using DecisionDiagrams;
    using ZenLib.Solver;

    /// <summary>
    /// An input-output set transformer.
    /// </summary>
    public class StateSetTransformer<T1, T2>
    {
        /// <summary>
        /// The underlying solver to use.
        /// </summary>
        private SolverDD<BDDNode> solver;

        /// <summary>
        /// The transformation as a single decision diagram.
        /// </summary>
        private DD setTransformer;

        /// <summary>
        /// A zen expression for the input variables.
        /// </summary>
        private Zen<T1> zenInput;

        /// <summary>
        /// A zen expression for the output variables.
        /// </summary>
        private Zen<T2> zenOutput;

        /// <summary>
        /// The decision diagram input variables.
        /// </summary>
        private VariableSet<BDDNode> inputVariables;

        /// <summary>
        /// The decision diagram output variables.
        /// </summary>
        private VariableSet<BDDNode> outputVariables;

        /// <summary>
        /// Cache of canonical variable values for different types.
        /// </summary>
        private Dictionary<Type, (object, VariableSet<BDDNode>)> canonicalValues;

        /// <summary>
        /// Cache from zen expression to variables for that expression.
        /// </summary>
        private Dictionary<object, Variable<BDDNode>> arbitraryMapping;

        internal StateSetTransformer(
            SolverDD<BDDNode> solver,
            DD setTransformer,
            (Zen<T1>, VariableSet<BDDNode>) inputAndVariables,
            (Zen<T2>, VariableSet<BDDNode>) outputAndVariables,
            Dictionary<object, Variable<BDDNode>> arbitraryMapping,
            Dictionary<Type, (object, VariableSet<BDDNode>)> canonicalValues)
        {
            this.solver = solver;
            this.setTransformer = setTransformer;
            this.zenInput = inputAndVariables.Item1;
            this.zenOutput = outputAndVariables.Item1;
            this.inputVariables = inputAndVariables.Item2;
            this.outputVariables = outputAndVariables.Item2;
            this.canonicalValues = canonicalValues;
            this.arbitraryMapping = arbitraryMapping;
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        /// <param name="invariant">The function constraining the inputs.</param>
        public StateSet<T1> InputSet(Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant = null)
        {
            DD set = setTransformer;

            if (invariant != null)
            {
                var expr = invariant(this.zenInput, this.zenOutput);
                var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit>(this.solver);
                var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit>();
                var symbolicResult =
                    (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit>)expr.Accept(symbolicEvaluator, env);
                var ddOutput = symbolicResult.Value;
                set = this.solver.And(set, ddOutput);
            }

            var dd = solver.Manager.Exists(set, this.outputVariables);
            var result = new StateSet<T1>(this.solver, dd, this.arbitraryMapping, this.zenInput, this.inputVariables);
            return ConvertTo(result, this.canonicalValues[typeof(T1)]);
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        /// <param name="invariant">The invariant constraining the outputs.</param>
        public StateSet<T2> OutputSet(Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant = null)
        {
            DD set = setTransformer;
            if (invariant != null)
            {
                var expr = invariant(this.zenInput, this.zenOutput);

                var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit>(this.solver);
                var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit>();
                var symbolicResult =
                    (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit>)expr.Accept(symbolicEvaluator, env);
                var ddInput = symbolicResult.Value;
                set = this.solver.And(set, ddInput);
            }

            var dd = solver.Manager.Exists(set, inputVariables);
            var result = new StateSet<T2>(this.solver, dd, this.arbitraryMapping, this.zenOutput, this.outputVariables);
            return ConvertTo(result, this.canonicalValues[typeof(T2)]);
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        /// <param name="input">The input set.</param>
        /// <returns>The post image output set of the input.</returns>
        public StateSet<T2> TransformForward(StateSet<T1> input)
        {
            input = ConvertTo(input, (this.zenInput, this.inputVariables));
            DD set = input.Set;
            DD dd = this.solver.Manager.And(set, this.setTransformer);
            dd = this.solver.Manager.Exists(dd, this.inputVariables);
            var result = new StateSet<T2>(this.solver, dd, this.arbitraryMapping, this.zenOutput, this.outputVariables);
            return ConvertTo(result, this.canonicalValues[typeof(T2)]);
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        /// <param name="output">The output set.</param>
        /// <returns>The pre image set of the output.</returns>
        public StateSet<T1> TransformBackwards(StateSet<T2> output)
        {
            output = ConvertTo(output, (this.zenOutput, this.outputVariables));
            DD set = output.Set;
            DD dd = this.solver.Manager.And(set, this.setTransformer);
            dd = this.solver.Manager.Exists(dd, this.outputVariables);
            var result = new StateSet<T1>(this.solver, dd, this.arbitraryMapping, this.zenInput, this.inputVariables);
            return ConvertTo(result, this.canonicalValues[typeof(T1)]);
        }

        /// <summary>
        /// Covert a state set to contain the desired decision diagram variables.
        /// </summary>
        /// <param name="sourceStateSet">The source state set.</param>
        /// <param name="conversionData">The conversion variables.</param>
        /// <returns>A converted state set.</returns>
        private StateSet<T> ConvertTo<T>(StateSet<T> sourceStateSet, (object, VariableSet<BDDNode>) conversionData)
        {
            var x = new HashSet<Variable<BDDNode>>(sourceStateSet.VariableSet.Variables);
            var y = new HashSet<Variable<BDDNode>>(conversionData.Item2.Variables);
            if (x.SetEquals(y))
            {
                return sourceStateSet;
            }

            return sourceStateSet.ConvertSetVariables(conversionData.Item2, (Zen<T>)conversionData.Item1);
        }
    }
}
