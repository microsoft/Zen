﻿// <copyright file="SymbolicEvaluator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DecisionDiagrams;
    using ZenLib.Generation;
    using ZenLib.Interpretation;
    using ZenLib.Solver;

    /// <summary>
    /// Helper class to perform symbolic reasoning.
    /// </summary>
    internal static class SymbolicEvaluator
    {
        /// <summary>
        /// Manager object for building transformers.
        /// </summary>
        private static DDManager<BDDNode> transformerManager =
            new DDManager<BDDNode>(new BDDNodeFactory());

        /// <summary>
        /// Canonical variables used for a given type.
        /// </summary>
        private static Dictionary<Type, (object, VariableSet<BDDNode>)> canonicalValues =
            new Dictionary<Type, (object, VariableSet<BDDNode>)>();

        /// <summary>
        /// Mapping from arbitrary expression to their assigned variables.
        /// </summary>
        private static Dictionary<object, Variable<BDDNode>> arbitraryMapping =
            new Dictionary<object, Variable<BDDNode>>();

        /// <summary>
        /// Keep track of, for each type, if there is an output
        /// of that type that is dependency free.
        /// </summary>
        private static Dictionary<Type, VariableSet<BDDNode>> dependencyFreeOutput =
            new Dictionary<Type, VariableSet<BDDNode>>();

        /// <summary>
        /// Determine if an expression has a satisfying assignment.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>True or false.</returns>
        public static bool Find(Zen<bool> expression, Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);
            return assignment != null;
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="input">The Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<T> Find<T>(
            Zen<bool> expression,
            Zen<T> input,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);
            if (assignment == null)
            {
                return Option.None<T>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result = input.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some((T)result);
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="input1">The first Zen expression for the input to the function.</param>
        /// <param name="input2">The second Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<(T1, T2)> Find<T1, T2>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);

            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<(T1, T2)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some(((T1)result1, (T2)result2));
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="input1">The first Zen expression for the input to the function.</param>
        /// <param name="input2">The second Zen expression for the input to the function.</param>
        /// <param name="input3">The third Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<(T1, T2, T3)> Find<T1, T2, T3>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<(T1, T2, T3)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result3 = input3.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some(((T1)result1, (T2)result2, (T3)result3));
        }

        /// <summary>
        /// Find an input to a function satisfying some condition.
        /// </summary>
        /// <param name="expression">The Zen expression for the function.</param>
        /// <param name="input1">The first Zen expression for the input to the function.</param>
        /// <param name="input2">The second Zen expression for the input to the function.</param>
        /// <param name="input3">The third Zen expression for the input to the function.</param>
        /// <param name="input4">The fourth Zen expression for the input to the function.</param>
        /// <param name="backend">The backend to use.</param>
        /// <returns>An optional input value.</returns>
        public static Option<(T1, T2, T3, T4)> Find<T1, T2, T3, T4>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Backend backend)
        {
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);

            if (assignment == null)
            {
                return Option.None<(T1, T2, T3, T4)>();
            }

            var interpreterEnv = new ExpressionEvaluatorEnvironment(assignment);
            var result1 = input1.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result2 = input2.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result3 = input3.Accept(new ExpressionEvaluator(), interpreterEnv);
            var result4 = input4.Accept(new ExpressionEvaluator(), interpreterEnv);
            return Option.Some(((T1)result1, (T2)result2, (T3)result3, (T4)result4));
        }

        /// <summary>
        /// Create a state set transformer from a zen function.
        /// </summary>
        /// <param name="function">The Zen function to make into a transformer.</param>
        /// <returns>A state set transformer between input and output types.</returns>
        public static StateSetTransformer<T1, T2> StateTransformer<T1, T2>(Func<Zen<T1>, Zen<T2>> function)
        {
            // create an arbitrary input and invoke the function
            var generator = new SymbolicInputGenerator(0, false);
            var input = Language.Arbitrary<T1>(generator);
            var arbitrariesForInput = generator.ArbitraryExpressions;

            var expression = function(input);

            // create an arbitrary output value
            generator = new SymbolicInputGenerator(0, false);
            var output = Language.Arbitrary<T2>(generator);
            var arbitrariesForOutput = generator.ArbitraryExpressions;

            // create an expression relating input and output.
            Zen<bool> newExpression = (expression == output);

            // initialize the decision diagram solver
            var heuristic = new InterleavingHeuristic();
            var mustInterleave = heuristic.Compute(newExpression);

            var solver = new SolverDD<BDDNode>(transformerManager, mustInterleave);

            // optimization: if there are no variable ordering dependencies,
            // then we can reuse the input variables from the canonical variable case.
            var maxDependenciesPerType =
                mustInterleave.Values.Select(v => v.GroupBy(e => e.GetType()).Select(o => o.Count()).Max()).Max();

            bool isDependencyFree = maxDependenciesPerType <= 1;
            if (isDependencyFree)
            {
                if (canonicalValues.TryGetValue(typeof(T1), out var canonicalValue))
                {
                    var variablesIn = canonicalValue.Item2.Variables;
                    for (int i = 0; i < arbitrariesForInput.Count; i++)
                    {
                        solver.SetVariable(arbitrariesForInput[i], variablesIn[i]);
                    }
                }

                if (dependencyFreeOutput.TryGetValue(typeof(T2), out var variableSet))
                {
                    var variablesOut = variableSet.Variables;
                    for (int i = 0; i < arbitrariesForOutput.Count; i++)
                    {
                        solver.SetVariable(arbitrariesForOutput[i], variablesOut[i]);
                    }
                }
            }

            solver.Init();

            // hack: forces all arbitrary expressions to get evaluated even if not used in the invariant.
            var arbitraryToVariable = new Dictionary<object, Variable<BDDNode>>();
            foreach (var arbitrary in arbitrariesForInput)
            {
                ForceVariableAssignment(solver, arbitraryToVariable, arbitrary);
            }

            // get the decision diagram representing the equality.
            var symbolicEvaluator =
                new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit>(solver);
            var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit>();
            var symbolicValue = newExpression.Accept(symbolicEvaluator, env);
            var symbolicResult = (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>, Unit, Unit>)symbolicValue;

            DD result = (DD)(object)symbolicResult.Value;

            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
            {
                arbitraryToVariable[kv.Key] = kv.Value;
            }

            // collect information about input and output manager variables
            var inputVariables = new List<Variable<BDDNode>>();
            var outputVariables = new List<Variable<BDDNode>>();

            foreach (var arbitrary in arbitrariesForInput)
            {
                var variable = arbitraryToVariable[arbitrary];
                arbitraryMapping[arbitrary] = variable;
                inputVariables.Add(variable);
            }

            foreach (var arbitrary in arbitrariesForOutput)
            {
                var variable = arbitraryToVariable[arbitrary];
                arbitraryMapping[arbitrary] = variable;
                outputVariables.Add(variable);
            }

            // create variable sets for easy quantification
            var inputVariableSet = solver.Manager.CreateVariableSet(inputVariables.ToArray());
            var outputVariableSet = solver.Manager.CreateVariableSet(outputVariables.ToArray());

            if (isDependencyFree && !dependencyFreeOutput.ContainsKey(typeof(T2)))
            {
                dependencyFreeOutput[typeof(T2)] = outputVariableSet;
            }

            if (!canonicalValues.ContainsKey(typeof(T1)))
            {
                canonicalValues[typeof(T1)] = (input, inputVariableSet);
            }

            if (!canonicalValues.ContainsKey(typeof(T2)))
            {
                canonicalValues[typeof(T2)] = (output, outputVariableSet);
            }

            return new StateSetTransformer<T1, T2>(
                solver,
                result,
                (input, inputVariableSet),
                (output, outputVariableSet),
                arbitraryMapping,
                canonicalValues);
        }

        /// <summary>
        /// Forces an assignment to variables that may not be referenced
        /// in the user's Zen expression.
        /// </summary>
        /// <param name="solver">The solver to force the assignment with.</param>
        /// <param name="zenExprToVariable">Mapping from zen expression to variables.</param>
        /// <param name="zenExpr">The zen expression to force an assignement to.</param>
        private static void ForceVariableAssignment(
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

            throw new ZenException($"Unsupported type: {zenExpr.GetType()} in transformer.");
        }
    }
}
