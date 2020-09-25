// <copyright file="StateSet.cs" company="Microsoft">
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
        /// Converts between a the decision diagram variables used to represent the set.
        /// </summary>
        /// <param name="newVariableSet">The new decision diagram variables.</param>
        /// <param name="newZenExpression">The new Zen expression for the Zen variables.</param>
        /// <returns>A new state set with the underlying variables replaced.</returns>
        internal StateSet<T> ConvertSetVariables(VariableSet<BDDNode> newVariableSet, Zen<T> newZenExpression)
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
            return new StateSet<T>(this.Solver, x, this.ArbitraryMapping, newZenExpression, newVariableSet);
        }

        /// <summary>
        /// Intersect two state sets.
        /// </summary>
        /// <param name="other">The other state set.</param>
        /// <returns>The intersected state set.</returns>
        public StateSet<T> Intersect(StateSet<T> other)
        {
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
        public Option<T> Element()
        {
            var model = this.Solver.Satisfiable(this.Set);
            if (!model.HasValue)
            {
                return Option.None<T>();
            }

            var assignment = new Dictionary<object, object>();
            foreach (var kv in this.ArbitraryMapping)
            {
                var value = this.Solver.Get(model.Value, kv.Value);
                assignment[kv.Key] = value;
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result = CommonUtilities.ConvertSymbolicResultToCSharp<T>(this.ZenExpression.Accept(new ExpressionEvaluator(), interpreterEnv));
            return Option.Some(result);
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
    }
}
