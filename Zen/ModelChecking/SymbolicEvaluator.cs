// <copyright file="SymbolicEvaluator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using DecisionDiagrams;
    using Microsoft.Research.Zen.Generation;
    using Microsoft.Research.Zen.Interpretation;

    /// <summary>
    /// Helper class to perform symbolic reasoning.
    /// </summary>
    internal static class SymbolicEvaluator
    {
        private static DDManager<BDDNode> transformerManager = new DDManager<BDDNode>(new BDDNodeFactory());

        private static Dictionary<Type, (object, VariableSet<BDDNode>)> canonicalValues = new Dictionary<Type, (object, VariableSet<BDDNode>)>();

        private static Dictionary<object, Variable<BDDNode>> arbitraryMapping = new Dictionary<object, Variable<BDDNode>>();

        public static bool Find(Zen<bool> expression, Backend backend, bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var modelChecker = ModelCheckerFactory.CreateModelChecker(backend, expression);
            var assignment = modelChecker.ModelCheck(expression);
            return assignment != null;
        }

        public static Option<T> Find<T>(
            Zen<bool> expression,
            Zen<T> input,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
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

        public static Option<(T1, T2)> Find<T1, T2>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
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

        public static Option<(T1, T2, T3)> Find<T1, T2, T3>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
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

        public static Option<(T1, T2, T3, T4)> Find<T1, T2, T3, T4>(
            Zen<bool> expression,
            Zen<T1> input1,
            Zen<T2> input2,
            Zen<T3> input3,
            Zen<T4> input4,
            Backend backend,
            bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
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

        public static StateSetTransformer<T1, T2> StateTransformer<T1, T2>(Func<Zen<T1>, Zen<T2>> function)
        {
            List<object> arbitrariesForOutput;
            List<object> arbitrariesForInput;

            // create an arbitrary input and invoke the function
            var generator = new SymbolicInputGenerator(0, false);
            var input = Language.Arbitrary<T1>(generator);
            arbitrariesForInput = generator.ArbitraryExpressions;

            var expression = function(input);

            // create an arbitrary output value
            generator = new SymbolicInputGenerator(1, false);
            var output = Language.Arbitrary<T2>(generator);
            arbitrariesForOutput = generator.ArbitraryExpressions;

            // create an expression relating input and output.
            Zen<bool> newExpression = (expression == output);

            // initialize the decision diagram solver
            var heuristic = new InterleavingHeuristic();
            var mustInterleave = heuristic.Compute(newExpression);
            var solverDD = new SolverDD<BDDNode>(transformerManager, mustInterleave);

            // get the decision diagram representing the equality.
            var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>(solverDD);
            var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>();
            var symbolicResult =
                (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>)newExpression.Accept(symbolicEvaluator, env);

            DD result = (DD)(object)symbolicResult.Value;

            // collect information about input and output manager variables
            var inputArbitraryExprs = new HashSet<object>(arbitrariesForInput);
            var outputArbitraryExprs = new HashSet<object>(arbitrariesForOutput);

            var inputVariables = new List<Variable<BDDNode>>();
            var outputVariables = new List<Variable<BDDNode>>();

            foreach (var kv in symbolicEvaluator.ArbitraryVariables)
            {
                var arbitraryExpr = kv.Key;
                var variable = kv.Value;

                arbitraryMapping[arbitraryExpr] = variable;

                if (outputArbitraryExprs.Contains(arbitraryExpr))
                {
                    outputVariables.Add(variable);
                }

                if (inputArbitraryExprs.Contains(arbitraryExpr))
                {
                    inputVariables.Add(variable);
                }
            }

            // create variable sets for easy quantification
            var inputVariableSet = solverDD.Manager.CreateVariableSet(inputVariables.ToArray());
            var outputVariableSet = solverDD.Manager.CreateVariableSet(outputVariables.ToArray());

            if (!canonicalValues.ContainsKey(typeof(T1)))
            {
                canonicalValues[typeof(T1)] = (input, inputVariableSet);
            }

            if (!canonicalValues.ContainsKey(typeof(T2)))
            {
                canonicalValues[typeof(T2)] = (output, outputVariableSet);
            }

            return new StateSetTransformer<T1, T2>(
                solverDD,
                result,
                (input, inputVariableSet),
                (output, outputVariableSet),
                arbitraryMapping,
                canonicalValues);
        }
    }
}
