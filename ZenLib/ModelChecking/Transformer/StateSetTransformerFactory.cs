﻿// <copyright file="StateSetTransformerFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DecisionDiagrams;
    using ZenLib.Generation;
    using ZenLib.Solver;

    /// <summary>
    /// Class to help create state set transformers.
    /// </summary>
    internal static class StateSetTransformerFactory
    {
        /// <summary>
        /// Default manager object will allocate all objects.
        /// </summary>
        internal static StateSetTransformerManager DefaultManager = new StateSetTransformerManager();

        /// <summary>
        /// Get the manager or the default if null.
        /// </summary>
        /// <param name="manager">The provided manager object.</param>
        /// <returns>A non-null manager to use.</returns>
        public static StateSetTransformerManager GetOrDefaultManager(StateSetTransformerManager manager)
        {
            return manager == null ? DefaultManager : manager;
        }

        /// <summary>
        /// Create a state set transformer from a zen function.
        /// </summary>
        /// <param name="function">The Zen function to make into a transformer.</param>
        /// <param name="manager">The transformation manager object to use.</param>
        /// <returns>A state set transformer between input and output types.</returns>
        public static StateSetTransformer<T1, T2> CreateTransformer<T1, T2>(Func<Zen<T1>, Zen<T2>> function, StateSetTransformerManager manager)
        {
            // create an arbitrary input and invoke the function
            var generator = new SymbolicInputVisitor();
            var input = Zen.Arbitrary<T1>(generator, depth: 0);
            var arbitrariesForInput = generator.ArbitraryExpressions;

            var expression = function(input);

            // create an arbitrary output value
            generator = new SymbolicInputVisitor();
            var output = Zen.Arbitrary<T2>(generator, depth: 0);
            var arbitrariesForOutput = generator.ArbitraryExpressions;

            // create an expression relating input and output.
            Zen<bool> newExpression = (expression == output);

            // initialize the decision diagram solver
            var heuristic = new InterleavingHeuristicVisitor();
            var mustInterleave = heuristic.GetInterleavedVariables(newExpression, ImmutableDictionary<long, object>.Empty);
            var solver = new SolverDD<BDDNode>(manager.DecisionDiagramManager, mustInterleave);

            // optimization: if there are no variable ordering dependencies,
            // then we can reuse the input variables from the canonical variable case.
            var maxDependenciesPerType = mustInterleave
                .Select(v => v.GroupBy(e => e.GetType()).Select(o => o.Count()).MaxOrDefault())
                .MaxOrDefault();

            bool isDependencyFree = maxDependenciesPerType <= 1;

            if (isDependencyFree)
            {
                if (manager.CanonicalValues.TryGetValue(typeof(T1), out var canonicalInput))
                {
                    for (int i = 0; i < arbitrariesForInput.Count; i++)
                    {
                        solver.SetVariable(arbitrariesForInput[i], canonicalInput.VariableSet.Variables[i]);
                    }
                }

                if (manager.DependencyFreeOutput.TryGetValue(typeof(T2), out var canonicalOutput))
                {
                    for (int i = 0; i < arbitrariesForOutput.Count; i++)
                    {
                        solver.SetVariable(arbitrariesForOutput[i], canonicalOutput.Variables[i]);
                    }
                }
            }

            solver.Init();

            // get the decision diagram representing the equality.
            var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit, Unit, Unit, Unit>(solver);
            var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit, Unit, Unit, Unit>(new Dictionary<long, object>());
            var symbolicValue = symbolicEvaluator.Compute(newExpression, env);
            var symbolicResult = (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit, Unit, Unit, Unit>)symbolicValue;
            DD result = (DD)(object)symbolicResult.Value;

            // forces all arbitrary expressions to get evaluated even if not used in the invariant.
            var arbitraryToVariable = new Dictionary<object, Variable<BDDNode>>();
            foreach (var arbitrary in arbitrariesForInput)
            {
                forceVariableAssignment(solver, arbitraryToVariable, arbitrary);
            }

            // we get the actual assignment from arbitrary to bdd variable
            // note that this will override the temporary assignments set above.
            // this is done to ensure unused arguments are still allocated variables.
            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
            {
                arbitraryToVariable[kv.Key] = kv.Value;
            }

            // collect information about input and output manager variables
            var inputVariables = new List<Variable<BDDNode>>();
            var outputVariables = new List<Variable<BDDNode>>();
            var transientVariables = new List<Variable<BDDNode>>();
            var arbitraryMappingInput = new Dictionary<object, Variable<BDDNode>>();
            var arbitraryMappingOutput = new Dictionary<object, Variable<BDDNode>>();

            // there might be Zen.Arbitrary variables used that are not part of the input type
            // for example to set a field to be any value. In this case we want to include these
            // variables as inputs so they will get quantified away when applying transformers.
            // This means the variables are not respected across transformers.
            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
            {
                if (!arbitrariesForOutput.Contains(kv.Key) && !arbitrariesForInput.Contains(kv.Key))
                {
                    var variable = arbitraryToVariable[kv.Key];
                    arbitraryMappingInput[kv.Key] = variable;
                    transientVariables.Add(variable);
                }
            }

            foreach (var arbitrary in arbitrariesForInput)
            {
                var variable = arbitraryToVariable[arbitrary];
                arbitraryMappingInput[arbitrary] = variable;
                inputVariables.Add(variable);
            }

            foreach (var arbitrary in arbitrariesForOutput)
            {
                var variable = arbitraryToVariable[arbitrary];
                arbitraryMappingOutput[arbitrary] = variable;
                outputVariables.Add(variable);
            }

            // create variable sets for easy quantification
            var inputVariableSet = solver.Manager.CreateVariableSet(inputVariables.ToArray());
            var outputVariableSet = solver.Manager.CreateVariableSet(outputVariables.ToArray());
            var transientVariableSet = solver.Manager.CreateVariableSet(transientVariables.ToArray());

            if (isDependencyFree && !manager.DependencyFreeOutput.ContainsKey(typeof(T2)))
            {
                manager.DependencyFreeOutput[typeof(T2)] = outputVariableSet;
            }

            if (!manager.CanonicalValues.ContainsKey(typeof(T1)))
            {
                manager.CanonicalValues[typeof(T1)] = new StateSetMetadata
                {
                    ZenParameter = input,
                    VariableSet = inputVariableSet,
                    ZenArbitraryMapping = arbitraryMappingInput,
                };
            }

            if (!manager.CanonicalValues.ContainsKey(typeof(T2)))
            {
                manager.CanonicalValues[typeof(T2)] = new StateSetMetadata
                {
                    ZenParameter = output,
                    VariableSet = outputVariableSet,
                    ZenArbitraryMapping = arbitraryMappingOutput,
                };
            }

            return new StateSetTransformer<T1, T2>(
                solver,
                result,
                (input, inputVariableSet),
                (output, outputVariableSet),
                transientVariableSet,
                arbitraryMappingInput,
                arbitraryMappingOutput,
                manager);
        }

        /// <summary>
        /// Create a state set transformer from a zen function.
        /// </summary>
        /// <param name="function">The Zen function to make into a transformer.</param>
        /// <param name="manager">The transformation manager object to use.</param>
        /// <returns>A state set transformer between input and output types.</returns>
        public static StateSet<T> CreateStateSet<T>(Func<Zen<T>, Zen<bool>> function, StateSetTransformerManager manager)
        {
            // create an arbitrary input and invoke the function
            var generator = new SymbolicInputVisitor();
            var input = Zen.Arbitrary<T>(generator, depth: 0);
            var arbitrariesForInput = generator.ArbitraryExpressions;

            var expression = function(input);

            // initialize the decision diagram solver
            var heuristic = new InterleavingHeuristicVisitor();
            var mustInterleave = heuristic.GetInterleavedVariables(expression, ImmutableDictionary<long, object>.Empty);
            var solver = new SolverDD<BDDNode>(manager.DecisionDiagramManager, mustInterleave);

            // if we already have canonical values for this type, we should use those.
            // there is no point allocating new variables since we just convert to these.
            if (manager.CanonicalValues.TryGetValue(typeof(T), out var canonicalInput))
            {
                for (int i = 0; i < arbitrariesForInput.Count; i++)
                {
                    solver.SetVariable(arbitrariesForInput[i], canonicalInput.VariableSet.Variables[i]);
                }
            }

            solver.Init();

            // get the decision diagram representing the equality.
            var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit, Unit, Unit, Unit>(solver);
            var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit, Unit, Unit, Unit>(new Dictionary<long, object>());
            var symbolicValue = symbolicEvaluator.Compute(expression, env);
            var symbolicResult = (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit, Unit, Unit, Unit>)symbolicValue;
            DD result = (DD)(object)symbolicResult.Value;

            // forces all arbitrary expressions to get evaluated even if not used in the invariant.
            var arbitraryToVariable = new Dictionary<object, Variable<BDDNode>>();
            foreach (var arbitrary in arbitrariesForInput)
            {
                forceVariableAssignment(solver, arbitraryToVariable, arbitrary);
            }

            // we get the actual assignment from arbitrary to bdd variable
            // note that this will override the temporary assignments set above.
            // this is done to ensure unused arguments are still allocated variables.
            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
            {
                arbitraryToVariable[kv.Key] = kv.Value;
            }

            // collect information about input and output manager variables
            var inputVariables = new List<Variable<BDDNode>>();
            var arbitraryMappingInput = new Dictionary<object, Variable<BDDNode>>();

            foreach (var arbitrary in arbitrariesForInput)
            {
                var variable = arbitraryToVariable[arbitrary];
                arbitraryMappingInput[arbitrary] = variable;
                inputVariables.Add(variable);
            }

            // create variable sets for easy quantification
            var inputVariableSet = solver.Manager.CreateVariableSet(inputVariables.ToArray());

            if (!manager.CanonicalValues.ContainsKey(typeof(T)))
            {
                manager.CanonicalValues[typeof(T)] = new StateSetMetadata
                {
                    ZenParameter = input,
                    VariableSet = inputVariableSet,
                    ZenArbitraryMapping = arbitraryMappingInput,
                };
            }

            return new StateSet<T>(solver, result, arbitraryMappingInput, input, inputVariableSet);
        }

        /// <summary>
        /// Forces an assignment to variables that may not be referenced
        /// in the user's Zen expression.
        /// </summary>
        /// <param name="solver">The solver to force the assignment with.</param>
        /// <param name="zenExprToVariable">Mapping from zen expression to variables.</param>
        /// <param name="zenExpr">The zen expression to force an assignement to.</param>
        private static void forceVariableAssignment(
            SolverDD<BDDNode> solver,
            Dictionary<object, Variable<BDDNode>> zenExprToVariable,
            object zenExpr)
        {
            if (zenExpr is Zen<bool>)
            {
                var (v, _) = solver.CreateBoolVar(zenExpr);
                zenExprToVariable[zenExpr] = v;
                return;
            }

            if (zenExpr is Zen<byte>)
            {
                var (v, _) = solver.CreateByteVar(zenExpr);
                zenExprToVariable[zenExpr] = v;
                return;
            }

            if (zenExpr is Zen<short> || zenExpr is Zen<ushort>)
            {
                var (v, _) = solver.CreateShortVar(zenExpr);
                zenExprToVariable[zenExpr] = v;
                return;
            }

            if (zenExpr is Zen<int> || zenExpr is Zen<uint>)
            {
                var (v, _) = solver.CreateIntVar(zenExpr);
                zenExprToVariable[zenExpr] = v;
                return;
            }

            if (zenExpr is Zen<long> || zenExpr is Zen<ulong>)
            {
                var (v, _) = solver.CreateLongVar(zenExpr);
                zenExprToVariable[zenExpr] = v;
                return;
            }

            var type = zenExpr.GetType();
            var zenType = type.GetGenericArguments()[0];

            if (ReflectionUtilities.IsFixedIntegerType(zenType))
            {
                var size = ReflectionUtilities.IntegerSize(zenType);
                var (v, _) = solver.CreateBitvecVar(zenExpr, (uint)size);
                zenExprToVariable[zenExpr] = v;
                return;
            }

            throw new ZenException($"Unsupported type: {zenExpr.GetType()} in transformer.");
        }

        [ExcludeFromCodeCoverage]
        private static int MaxOrDefault(this IEnumerable<int> enumerable)
        {
            if (enumerable.Count() == 0)
            {
                return 0;
            }

            return enumerable.Max();
        }
    }
}
