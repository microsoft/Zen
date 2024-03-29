﻿// <copyright file="StateSet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DecisionDiagrams;
    using ZenLib.Interpretation;
    using ZenLib.Solver;

    /// <summary>
    /// An set of values of a given type.
    /// </summary>
    public class StateSet<T>
    {
        /// <summary>
        /// The underlying decision diagram solver.
        /// </summary>
        internal SolverDD<BDDNode> Solver { get; }

        /// <summary>
        /// The set of values as a decision diagram.
        /// </summary>
        internal DD Set { get; }

        /// <summary>
        /// The cache from zen expression to decision diagram variables.
        /// </summary>
        internal Dictionary<object, Variable<BDDNode>> ArbitraryMapping { get; }

        /// <summary>
        /// The zen expression for the variables in the type of the set.
        /// </summary>
        internal Zen<T> ZenExpression { get; }

        /// <summary>
        /// The set of decision diagram variables.
        /// </summary>
        internal VariableSet<BDDNode> VariableSet { get; }

        /// <summary>
        /// Create a new instance of a <see cref="StateSet{T}"/>.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="stateSet">The state set as a decision diagram.</param>
        /// <param name="arbitraryMapping">The variable cache.</param>
        /// <param name="zenExpression">The Zen expression for variables.</param>
        /// <param name="variableSet">The decision diagram variable set.</param>
        internal StateSet(
            SolverDD<BDDNode> solver,
            DD stateSet,
            Dictionary<object, Variable<BDDNode>> arbitraryMapping,
            Zen<T> zenExpression,
            VariableSet<BDDNode> variableSet)
        {
            this.Solver = solver;
            this.Set = stateSet;
            this.ArbitraryMapping = arbitraryMapping;
            this.ZenExpression = zenExpression;
            this.VariableSet = variableSet;
        }

        /// <summary>
        /// Covert a state set to contain the desired decision diagram variables.
        /// </summary>
        /// <param name="conversionData">The conversion variables.</param>
        /// <returns>A converted state set.</returns>
        internal StateSet<T> ConvertTo(StateSetMetadata conversionData)
        {
            if (this.VariableSet.Id.Equals(conversionData.VariableSet.Id))
            {
                return this;
            }

            return this.ConvertSetVariables(conversionData.VariableSet, (Zen<T>)conversionData.ZenParameter, conversionData.ZenArbitraryMapping);
        }

        /// <summary>
        /// Converts between a the decision diagram variables used to represent the set.
        /// </summary>
        /// <param name="newVariableSet">The new decision diagram variables.</param>
        /// <param name="newZenExpression">The new Zen expression for the Zen variables.</param>
        /// <param name="arbitraryMapping">The mapping from arbitrary to bdd variable.</param>
        /// <returns>A new state set with the underlying variables replaced.</returns>
        internal StateSet<T> ConvertSetVariables(VariableSet<BDDNode> newVariableSet, Zen<T> newZenExpression, Dictionary<object, Variable<BDDNode>> arbitraryMapping)
        {
            var a1 = this.VariableSet.Variables;
            var a2 = newVariableSet.Variables;
            var map = new Dictionary<Variable<BDDNode>, Variable<BDDNode>>();
            for (int i = 0; i < a1.Length; i++)
            {
                map[a1[i]] = a2[i];
            }

            var mapping = this.Solver.Manager.CreateVariableMap(map);
            var x = this.Solver.Manager.Replace(this.Set, mapping);
            return new StateSet<T>(this.Solver, x, arbitraryMapping, newZenExpression, newVariableSet);
        }

        /// <summary>
        /// Intersect two state sets.
        /// </summary>
        /// <param name="other">The other state set.</param>
        /// <returns>The intersected state set.</returns>
        public StateSet<T> Intersect(StateSet<T> other)
        {
            CheckValidOperation(other);
            var dd = this.Solver.Manager.And(this.Set, other.Set);
            return new StateSet<T>(this.Solver, dd, this.ArbitraryMapping, this.ZenExpression, this.VariableSet);
        }

        /// <summary>
        /// Union two state sets.
        /// </summary>
        /// <param name="other">The other state set.</param>
        /// <returns>The intersected state set.</returns>
        public StateSet<T> Union(StateSet<T> other)
        {
            CheckValidOperation(other);
            var dd = this.Solver.Manager.Or(this.Set, other.Set);
            return new StateSet<T>(this.Solver, dd, this.ArbitraryMapping, this.ZenExpression, this.VariableSet);
        }

        /// <summary>
        /// Take the complement of a state set.
        /// </summary>
        /// <returns>The complemented state set.</returns>
        public StateSet<T> Complement()
        {
            var dd = this.Solver.Manager.Not(this.Set);
            return new StateSet<T>(this.Solver, dd, this.ArbitraryMapping, this.ZenExpression, this.VariableSet);
        }

        /// <summary>
        /// Get an element from the set.
        /// </summary>
        /// <returns>An element if non-empty.</returns>
        public T Element()
        {
            var variables = new List<Variable<BDDNode>>(this.VariableSet.Variables);
            var model = this.Solver.Manager.Sat(this.Set, variables);
            if (model == null)
            {
                throw new ZenException("No element exists in state set.");
            }

            var assignment = new Dictionary<object, object>();
            foreach (var kv in this.ArbitraryMapping)
            {
                var type = kv.Key.GetType().GetGenericArgumentsCached()[0];
                var value = this.Solver.Get(model, kv.Value, type);
                assignment[kv.Key] = value;
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(assignment) };
            var interpreter = new ExpressionEvaluatorVisitor(false);
            return (T)interpreter.Visit(this.ZenExpression, interpreterEnv);
        }

        /// <summary>
        /// Whether the set is empty.
        /// </summary>
        /// <returns>True or false.</returns>
        public bool IsEmpty()
        {
            return this.Set.IsFalse();
        }

        /// <summary>
        /// Whether the set is full.
        /// </summary>
        /// <returns>True or false.</returns>
        public bool IsFull()
        {
            return this.Set.IsTrue();
        }

        /// <summary>
        /// Equality between two state sets.
        /// </summary>
        /// <param name="other">The other state set.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object other)
        {
            return (other is StateSet<T> o) && this.Set.Equals(o.Set);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return this.Set.GetHashCode();
        }

        /// <summary>
        /// Checks if an operation with a state set object is valid.
        /// </summary>
        /// <param name="argument">The state set argument.</param>
        private void CheckValidOperation<T2>(StateSet<T2> argument)
        {
            if (!this.Solver.Manager.Equals(argument.Solver.Manager))
            {
                throw new ZenException($"Attempting to combine transformations and state sets with different manager objects");
            }
        }
    }
}
