// <copyright file="StateSetTransformer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using DecisionDiagrams;

    /// <summary>
    /// An input-output set transformer.
    /// </summary>
    public class StateSetTransformer<T1, T2>
    {
        private SolverDD<BDDNode> solver;

        private DD setTransformer;

        private VariableSet<BDDNode> inputVariables;

        private VariableSet<BDDNode> outputVariables;

        private Dictionary<object, Variable<BDDNode>> arbitraryMapping;

        private Zen<T1> zenInput;

        private Zen<T2> zenOutput;

        internal StateSetTransformer(
            SolverDD<BDDNode> solver,
            DD setTransformer,
            VariableSet<BDDNode> inputVariables,
            VariableSet<BDDNode> outputVariables,
            Dictionary<object, Variable<BDDNode>> arbitraryMapping,
            Zen<T1> zenInput,
            Zen<T2> zenOutput)
        {
            this.solver = solver;
            this.setTransformer = setTransformer;
            this.inputVariables = inputVariables;
            this.outputVariables = outputVariables;
            this.arbitraryMapping = arbitraryMapping;
            this.zenInput = zenInput;
            this.zenOutput = zenOutput;
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        public StateSet<T1> InputSet(Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant = null)
        {
            DD set = setTransformer;

            if (invariant != null)
            {
                var expr = invariant(this.zenInput, this.zenOutput);

                var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>(this.solver);
                var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>();
                var symbolicResult =
                    (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>)expr.Accept(symbolicEvaluator, env);
                var ddOutput = symbolicResult.Value;
                set = this.solver.And(set, ddOutput);
            }

            var dd = solver.Manager.Exists(set, this.outputVariables);
            return new StateSet<T1>(this.solver, dd, this.arbitraryMapping, this.zenInput, this.inputVariables, true);
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        public StateSet<T2> OutputSet(Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant = null)
        {
            DD set = setTransformer;
            if (invariant != null)
            {
                var expr = invariant(this.zenInput, this.zenOutput);

                var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>(this.solver);
                var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>();
                var symbolicResult =
                    (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>)expr.Accept(symbolicEvaluator, env);
                var ddInput = symbolicResult.Value;
                set = this.solver.And(set, ddInput);
            }

            var dd = solver.Manager.Exists(set, inputVariables);
            return new StateSet<T2>(this.solver, dd, this.arbitraryMapping, this.zenOutput, this.outputVariables, false);
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        public StateSet<T2> TransformForward(StateSet<T1> input)
        {
            input = input.IsInput ? input : input.AlignSetVariables(this.inputVariables, this.zenInput, true);
            DD set = input.Set;
            DD result = this.solver.Manager.And(set, this.setTransformer);
            result = this.solver.Manager.Exists(result, this.inputVariables);
            return new StateSet<T2>(this.solver, result, this.arbitraryMapping, this.zenOutput, this.outputVariables, false);
        }

        /// <summary>
        /// Compute the input set for the transformer.
        /// </summary>
        public StateSet<T1> TransformBackwards(StateSet<T2> output)
        {
            output = output.IsInput ? output.AlignSetVariables(this.outputVariables, this.zenOutput, false) : output;
            DD set = output.Set;
            DD result = this.solver.Manager.And(set, this.setTransformer);
            result = this.solver.Manager.Exists(result, this.outputVariables);
            return new StateSet<T1>(this.solver, result, this.arbitraryMapping, this.zenInput, this.inputVariables, true);
        }
    }
}
