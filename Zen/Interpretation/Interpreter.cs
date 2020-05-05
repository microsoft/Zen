// <copyright file="Interpreter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.Interpretation
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Helper functions to interpret a Zen function.
    /// </summary>
    internal static class Interpreter
    {
        public static T Run<T>(Func<Zen<T>> function, bool simplify)
        {
            var args = ImmutableDictionary<string, object>.Empty;
            var expression = function();
            return Interpret(expression, args, simplify);
        }

        public static T2 Run<T1, T2>(Func<Zen<T1>, Zen<T2>> function, T1 value1, bool simplify)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.Id, value1);
            var expression = function(arg1);
            return Interpret(expression, args, simplify);
        }

        public static T3 Run<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> function, T1 value1, T2 value2, bool simplify)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.Id, value1);
            args = args.Add(arg2.Id, value2);
            var expression = function(arg1, arg2);
            return Interpret(expression, args, simplify);
        }

        public static T4 Run<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function,
            T1 value1,
            T2 value2,
            T3 value3,
            bool simplify)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            var arg3 = new ZenArgumentExpr<T3>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.Id, value1);
            args = args.Add(arg2.Id, value2);
            args = args.Add(arg3.Id, value3);
            var expression = function(arg1, arg2, arg3);
            return Interpret(expression, args, simplify);
        }

        public static T5 Run<T1, T2, T3, T4, T5>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function,
            T1 value1,
            T2 value2,
            T3 value3,
            T4 value4,
            bool simplify)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            var arg3 = new ZenArgumentExpr<T3>();
            var arg4 = new ZenArgumentExpr<T4>();
            var args = ImmutableDictionary<string, object>.Empty;
            args = args.Add(arg1.Id, value1);
            args = args.Add(arg2.Id, value2);
            args = args.Add(arg3.Id, value3);
            args = args.Add(arg4.Id, value4);
            var expression = function(arg1, arg2, arg3, arg4);
            return Interpret(expression, args, simplify);
        }

        private static T Interpret<T>(Zen<T> expression, ImmutableDictionary<string, object> arguments, bool simplify)
        {
            expression = simplify ? expression.Simplify() : expression;
            var environment = new ExpressionEvaluatorEnvironment(arguments);
            var interpreter = new ExpressionEvaluator();
            return (T)expression.Accept(interpreter, environment);
        }

        public static T3 CompileRunHelper<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> function,
            T1 value1,
            T2 value2,
            ImmutableDictionary<string, object> args)
        {
            var arg1 = new ZenArgumentExpr<T1>();
            var arg2 = new ZenArgumentExpr<T2>();
            args = args.Add(arg1.Id, value1);
            args = args.Add(arg2.Id, value2);
            var expression = function(arg1, arg2);
            return Interpret(expression, args, true);
        }
    }
}
