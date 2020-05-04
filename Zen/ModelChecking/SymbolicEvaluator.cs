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

        private static Dictionary<Type, object> transformerExprInputs = new Dictionary<Type, object>();

        private static Dictionary<Type, object> transformerExprOutputs = new Dictionary<Type, object>();

        private static Dictionary<Type, List<object>> transformerInputArbitraries = new Dictionary<Type, List<object>>();

        private static Dictionary<Type, List<object>> transformerOutputArbitraries = new Dictionary<Type, List<object>>();

        private static Dictionary<object, Variable<BDDNode>> transformerArbitraryToVariableMap = new Dictionary<object, Variable<BDDNode>>();

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
            Zen<T1> input;
            if (transformerExprInputs.TryGetValue(typeof(T1), out var cachedInput))
            {
                input = (Zen<T1>)cachedInput;
                arbitrariesForInput = transformerInputArbitraries[typeof(T1)];
            }
            else
            {
                var generator = new SymbolicInputGenerator(0, false);
                input = Language.Arbitrary<T1>(generator);
                arbitrariesForInput = generator.ArbitraryExpressions;

                transformerExprInputs[typeof(T1)] = input;
                transformerInputArbitraries[typeof(T1)] = arbitrariesForInput;
            }

            var expression = function(input);

            // Console.WriteLine($"input: {input}");
            // Console.WriteLine($"expression: {expression}");

            // create an arbitrary output value
            Zen<T2> output;
            if (transformerExprOutputs.TryGetValue(typeof(T2), out var cachedOutput))
            {
                output = (Zen<T2>)cachedOutput;
                arbitrariesForOutput = transformerOutputArbitraries[typeof(T2)];
            }
            else
            {
                var generator = new SymbolicInputGenerator(1, false);
                output = Language.Arbitrary<T2>(generator);
                arbitrariesForOutput = generator.ArbitraryExpressions;

                transformerExprOutputs[typeof(T2)] = output;
                transformerOutputArbitraries[typeof(T2)] = arbitrariesForOutput;
            }

            // create an expression relating input and output.
            Zen<bool> newExpression = (expression == output);

            // Console.WriteLine($"input: {input}");
            // Console.WriteLine($"output: {output}");
            // Console.WriteLine($"newExpression: {newExpression}");

            // initialize the decision diagram solver
            var heuristic = new InterleavingHeuristic();
            var mustInterleave = heuristic.Compute(newExpression);
            var solverDD = new SolverDD<BDDNode>(transformerManager, mustInterleave, transformerArbitraryToVariableMap);

            // get the decision diagram representing the equality.
            var symbolicEvaluator = new SymbolicEvaluationVisitor<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>(solverDD);
            var env = new SymbolicEvaluationEnvironment<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>();
            var symbolicResult =
                (SymbolicBool<Assignment<BDDNode>, Variable<BDDNode>, DD, BitVector<BDDNode>>)newExpression.Accept(symbolicEvaluator, env);

            // foreach (var kv in transformerArbitraryToVariableMap)
            // {
            //    Console.WriteLine($"  {kv.Key} --> {kv.Value}");
            // }

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

                if (outputArbitraryExprs.Contains(arbitraryExpr))
                {
                    // Console.WriteLine($"  output var: {arbitraryExpr}, {variable}");
                    outputVariables.Add(variable);
                }

                if (inputArbitraryExprs.Contains(arbitraryExpr))
                {
                    // Console.WriteLine($"  input var: {arbitraryExpr}, {variable}");
                    inputVariables.Add(variable);
                }
            }

            // create variable sets for easy quantification
            var inputVariableSet = solverDD.Manager.CreateVariableSet(inputVariables.ToArray());
            var outputVariableSet = solverDD.Manager.CreateVariableSet(outputVariables.ToArray());

            return new StateSetTransformer<T1, T2>(
                solverDD, result, inputVariableSet, outputVariableSet, symbolicEvaluator.ArbitraryVariables, input, output);
        }
    }
}
