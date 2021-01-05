// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Interpretation
{
    using System;
    using System.Collections.Immutable;
    using ZenLib.SymbolicExecution;
    using static ZenLib.Language;

    /// <summary>
    /// Helper functions to interpret a Zen function.
    /// </summary>
    internal static class Interpreter
    {
        public static (T, PathConstraint) Run<T>(Func<Zen<T>> function, bool trackBranches = false)
        {
            var args = ImmutableDictionary<string, object>.Empty;
            var expression = function();
            return Interpret(expression, args, trackBranches);
        }

        public static (T2, PathConstraint) Run<T1, T2>(Func<Zen<T1>, Zen<T2>> function, T1 value1, bool trackBranches = false)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.ArgumentId, value1);
            var expression = function(arg1);
            return Interpret(expression, args, trackBranches);
        }

        public static (T3, PathConstraint) Run<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> function, T1 value1, T2 value2, bool trackBranches = false)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.ArgumentId, value1);
            args = args.Add(arg2.ArgumentId, value2);
            var expression = function(arg1, arg2);
            return Interpret(expression, args, trackBranches);
        }

        public static (T4, PathConstraint) Run<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function,
            T1 value1,
            T2 value2,
            T3 value3,
            bool trackBranches = false)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            var arg3 = new ZenArgumentExpr<T3>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.ArgumentId, value1);
            args = args.Add(arg2.ArgumentId, value2);
            args = args.Add(arg3.ArgumentId, value3);
            var expression = function(arg1, arg2, arg3);
            return Interpret(expression, args, trackBranches);
        }

        public static (T5, PathConstraint) Run<T1, T2, T3, T4, T5>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function,
            T1 value1,
            T2 value2,
            T3 value3,
            T4 value4,
            bool trackBranches = false)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            var arg3 = new ZenArgumentExpr<T3>();
            var arg4 = new ZenArgumentExpr<T4>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.ArgumentId, value1);
            args = args.Add(arg2.ArgumentId, value2);
            args = args.Add(arg3.ArgumentId, value3);
            args = args.Add(arg4.ArgumentId, value4);
            var expression = function(arg1, arg2, arg3, arg4);
            return Interpret(expression, args, trackBranches);
        }

        private static (T, PathConstraint) Interpret<T>(Zen<T> expression, ImmutableDictionary<string, object> arguments, bool trackBranches)
        {
            var environment = new ExpressionEvaluatorEnvironment(arguments);
            var interpreter = new ExpressionEvaluator(trackBranches);
            var result = (T)expression.Accept(interpreter, environment);

            return (result, interpreter.PathConstraint);
        }

        public static T3 CompileRunHelper<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> function,
            T1 value1,
            T2 value2,
            ImmutableDictionary<string, object> args)
        {
            var expression = function(Constant(value1), Constant(value2));
            return Interpret(expression, args, false).Item1;
        }
    }
}
