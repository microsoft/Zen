// <copyright file="ZenFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq.Expressions;
    using ZenLib.Compilation;
    using ZenLib.Interpretation;
    using ZenLib.ModelChecking;
    using ZenLib.SymbolicExecution;
    using static ZenLib.Language;

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    public class ZenFunction<T>
    {
        /// <summary>
        /// The function body expression.
        /// </summary>
        private Zen<T> functionBodyExpr;

        /// <summary>
        /// User provided function.
        /// </summary>
        private Func<Zen<T>> function;

        /// <summary>
        /// The compiled function as C# IL.
        /// </summary>
        private Func<T> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T>(ZenFunction<T> value)
        {
            return () => value.Evaluate();
        }

        /// <summary>
        /// Create a new instance of the <see cref="ZenFunction{T}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T>> function)
        {
            CommonUtilities.ValidateNotNull(function);
            this.function = function;
            this.functionBodyExpr = this.function();
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <returns>Result.</returns>
        public T Evaluate()
        {
            if (compiledFunction != null)
            {
                return compiledFunction();
            }

            var args = ImmutableDictionary<long, object>.Empty;
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.functionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (compiledFunction != null)
            {
                return;
            }

            var args = ImmutableDictionary<long, Expression>.Empty;
            this.compiledFunction = CodeGenerator.Compile(this.functionBodyExpr, args, maxUnrollingDepth);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public bool Assert(Func<Zen<T>, Zen<bool>> invariant, Backend backend = Backend.Z3)
        {
            var result = invariant(this.function());
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, new Dictionary<long, object>(), backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Return type.</typeparam>
    public class ZenFunction<T1, T2>
    {
        /// <summary>
        /// First argument expression.
        /// </summary>
        private ZenArgumentExpr<T1> argument1;

        /// <summary>
        /// The expression for the function body.
        /// </summary>
        private Zen<T2> functionBodyExpr;

        /// <summary>
        /// The callback for the Zen function.
        /// </summary>
        private Func<Zen<T1>, Zen<T2>> function;

        /// <summary>
        /// The compiled C# version of the function.
        /// </summary>
        private Func<T1, T2> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2>(ZenFunction<T1, T2> value)
        {
            return (v) => value.Evaluate(v);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>> function)
        {
            CommonUtilities.ValidateNotNull(function);
            this.function = function;
            this.argument1 = new ZenArgumentExpr<T1>();
            this.functionBodyExpr = this.function(this.argument1);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Result.</returns>
        public T2 Evaluate(T1 value)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value);
            }

            var args = ImmutableDictionary<long, object>.Empty
                .Add(this.argument1.ArgumentId, value);
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.functionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (compiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(this.argument1.ArgumentId, param1);
            this.compiledFunction = CodeGenerator.Compile<T1, T2>(this.functionBodyExpr, args, param1, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<T1, T2> Transformer(StateSetTransformerManager manager = null)
        {
            return CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateTransformer(this.function, manager));
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<T1> Find(
            Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant,
            Zen<T1> input = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input = CommonUtilities.GetArbitraryIfNull(input, listSize, checkSmallerLists);
            var args = new Dictionary<long, object>
            {
                { this.argument1.ArgumentId, input },
            };

            var result = invariant(this.argument1, this.functionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<T1> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant,
            Zen<T1> input = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input = CommonUtilities.GetArbitraryIfNull(input, listSize, checkSmallerLists);
            var result = invariant(input, this.function(input));

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), input, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, input == example.Value);
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<T1> GenerateInputs(
            Zen<T1> input = null,
            Func<Zen<T1>, Zen<bool>> precondition = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input = CommonUtilities.GetArbitraryIfNull(input, listSize, checkSmallerLists);
            precondition = precondition == null ? (x) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.function, precondition, input, backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Argument 2 type.</typeparam>
    /// <typeparam name="T3">Return type.</typeparam>
    public class ZenFunction<T1, T2, T3>
    {
        /// <summary>
        /// First argument expression.
        /// </summary>
        private ZenArgumentExpr<T1> argument1;

        /// <summary>
        /// Second argument expression.
        /// </summary>
        private ZenArgumentExpr<T2> argument2;

        /// <summary>
        /// Function body expression.
        /// </summary>
        private Zen<T3> functionBodyExpr;

        /// <summary>
        /// User provided function.
        /// </summary>
        private Func<Zen<T1>, Zen<T2>, Zen<T3>> function;

        /// <summary>
        /// Compiled function to C# IL.
        /// </summary>
        private Func<T1, T2, T3> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3>(ZenFunction<T1, T2, T3> value)
        {
            return (v1, v2) => value.Evaluate(v1, v2);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>> function)
        {
            CommonUtilities.ValidateNotNull(function);
            this.function = function;
            this.argument1 = new ZenArgumentExpr<T1>();
            this.argument2 = new ZenArgumentExpr<T2>();
            this.functionBodyExpr = this.function(this.argument1, this.argument2);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Result.</returns>
        public T3 Evaluate(T1 value1, T2 value2)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value1, value2);
            }

            var args = ImmutableDictionary<long, object>.Empty
                .Add(this.argument1.ArgumentId, value1)
                .Add(this.argument2.ArgumentId, value2);
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.functionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (compiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(this.argument1.ArgumentId, param1)
                .Add(this.argument2.ArgumentId, param2);
            this.compiledFunction = CodeGenerator.Compile<T1, T2, T3>(this.functionBodyExpr, args, param1, param2, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<Pair<T1, T2>, T3> Transformer()
        {
            Func<Zen<Pair<T1, T2>>, Zen<T3>> f = p => this.function(p.Item1(), p.Item2());
            return StateSetTransformerFactory.CreateTransformer(f);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="listSize">The depth bound for any given object.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            var args = new Dictionary<long, object>
            {
                { this.argument1.ArgumentId, input1 },
                { this.argument2.ArgumentId, input2 },
            };

            var result = invariant(this.argument1, this.argument2, this.functionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input1, input2, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">First input that captures structural constraints.</param>
        /// <param name="input2">Second input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2)> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            var result = invariant(input1, input2, this.function(input1, input2));

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(
                    () => SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), input1, input2, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, And(input1 == example.Value.Item1, input2 == example.Value.Item2));
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2)> GenerateInputs(
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Func<Zen<T1>, Zen<T2>, Zen<bool>> precondition = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            precondition = precondition == null ? (x, y) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.function, precondition, input1, input2, backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Argument 2 type.</typeparam>
    /// <typeparam name="T3">Argument 3 type.</typeparam>
    /// <typeparam name="T4">Return type.</typeparam>
    public class ZenFunction<T1, T2, T3, T4>
    {
        /// <summary>
        /// First argument expression.
        /// </summary>
        private ZenArgumentExpr<T1> argument1;

        /// <summary>
        /// Second argument expression.
        /// </summary>
        private ZenArgumentExpr<T2> argument2;

        /// <summary>
        /// Third argument expression.
        /// </summary>
        private ZenArgumentExpr<T3> argument3;

        /// <summary>
        /// Function body expression.
        /// </summary>
        private Zen<T4> functionBodyExpr;

        /// <summary>
        /// User-provided function.
        /// </summary>
        private Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function;

        /// <summary>
        /// Compiled function as C# IL.
        /// </summary>
        private Func<T1, T2, T3, T4> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3, T4>(ZenFunction<T1, T2, T3, T4> value)
        {
            return (v1, v2, v3) => value.Evaluate(v1, v2, v3);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function)
        {
            CommonUtilities.ValidateNotNull(function);
            this.function = function;
            this.argument1 = new ZenArgumentExpr<T1>();
            this.argument2 = new ZenArgumentExpr<T2>();
            this.argument3 = new ZenArgumentExpr<T3>();
            this.functionBodyExpr = this.function(this.argument1, this.argument2, this.argument3);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <returns>Result.</returns>
        public T4 Evaluate(T1 value1, T2 value2, T3 value3)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value1, value2, value3);
            }

            var args = ImmutableDictionary<long, object>.Empty
                .Add(this.argument1.ArgumentId, value1)
                .Add(this.argument2.ArgumentId, value2)
                .Add(this.argument3.ArgumentId, value3);
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.functionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (compiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var param3 = Expression.Parameter(typeof(T3));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(this.argument1.ArgumentId, param1)
                .Add(this.argument2.ArgumentId, param2)
                .Add(this.argument3.ArgumentId, param3);
            this.compiledFunction = CodeGenerator.Compile<T1, T2, T3, T4>(this.functionBodyExpr, args, param1, param2, param3, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<Pair<T1, T2, T3>, T4> Transformer()
        {
            Func<Zen<Pair<T1, T2, T3>>, Zen<T4>> f = p => this.function(p.Item1(), p.Item2(), p.Item3());
            return StateSetTransformerFactory.CreateTransformer(f);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="listSize">The depth bound for any given object.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2, T3)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, listSize, checkSmallerLists);
            var args = new Dictionary<long, object>
            {
                { this.argument1.ArgumentId, input1 },
                { this.argument2.ArgumentId, input2 },
                { this.argument3.ArgumentId, input3 },
            };

            var result = invariant(this.argument1, this.argument2, this.argument3, this.functionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input1, input2, input3, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">First input that captures structural constraints.</param>
        /// <param name="input2">Second input that captures structural constraints.</param>
        /// <param name="input3">Third input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3)> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, listSize, checkSmallerLists);
            var result = invariant(input1, input2, input3, this.function(input1, input2, input3));

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(
                    () => SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), input1, input2, input3, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, And(input1 == example.Value.Item1, input2 == example.Value.Item2, input3 == example.Value.Item3));
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3)> GenerateInputs(
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> precondition = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, listSize, checkSmallerLists);
            precondition = precondition == null ? (x, y, z) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.function, precondition, input1, input2, input3, backend));
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Argument 2 type.</typeparam>
    /// <typeparam name="T3">Argument 3 type.</typeparam>
    /// <typeparam name="T4">Argument 4 type.</typeparam>
    /// <typeparam name="T5">Return type.</typeparam>
    public class ZenFunction<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// First argument expression.
        /// </summary>
        private ZenArgumentExpr<T1> argument1;

        /// <summary>
        /// Second argument expression.
        /// </summary>
        private ZenArgumentExpr<T2> argument2;

        /// <summary>
        /// Third argument expression.
        /// </summary>
        private ZenArgumentExpr<T3> argument3;

        /// <summary>
        /// Fourth argument expression.
        /// </summary>
        private ZenArgumentExpr<T4> argument4;

        /// <summary>
        /// Function body expression.
        /// </summary>
        private Zen<T5> functionBodyExpr;

        /// <summary>
        /// User-provided function.
        /// </summary>
        private Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function;

        /// <summary>
        /// Compiled function as C# IL.
        /// </summary>
        private Func<T1, T2, T3, T4, T5> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3, T4, T5>(ZenFunction<T1, T2, T3, T4, T5> value)
        {
            return (v1, v2, v3, v4) => value.Evaluate(v1, v2, v3, v4);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenFunction{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function)
        {
            CommonUtilities.ValidateNotNull(function);
            this.function = function;
            this.argument1 = new ZenArgumentExpr<T1>();
            this.argument2 = new ZenArgumentExpr<T2>();
            this.argument3 = new ZenArgumentExpr<T3>();
            this.argument4 = new ZenArgumentExpr<T4>();
            this.functionBodyExpr = this.function(this.argument1, this.argument2, this.argument3, this.argument4);
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <param name="value4">The fourth value.</param>
        /// <returns>Result.</returns>
        public T5 Evaluate(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value1, value2, value3, value4);
            }

            var args = ImmutableDictionary<long, object>.Empty
                .Add(this.argument1.ArgumentId, value1)
                .Add(this.argument2.ArgumentId, value2)
                .Add(this.argument3.ArgumentId, value3)
                .Add(this.argument4.ArgumentId, value4);
            return CommonUtilities.RunWithLargeStack(() => Interpreter.Run(this.functionBodyExpr, args).Item1);
        }

        /// <summary>
        /// Compile the zen function to IL code and return
        /// an efficiently executable function.
        /// </summary>
        /// <param name="maxUnrollingDepth">The maximum unrolling depth.</param>
        /// <returns>The native function.</returns>
        public void Compile(int maxUnrollingDepth = 5)
        {
            if (compiledFunction != null)
            {
                return;
            }

            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var param3 = Expression.Parameter(typeof(T3));
            var param4 = Expression.Parameter(typeof(T4));
            var args = ImmutableDictionary<long, Expression>.Empty
                .Add(this.argument1.ArgumentId, param1)
                .Add(this.argument2.ArgumentId, param2)
                .Add(this.argument3.ArgumentId, param3)
                .Add(this.argument4.ArgumentId, param4);
            this.compiledFunction = CodeGenerator.Compile<T1, T2, T3, T4, T5>(this.functionBodyExpr, args, param1, param2, param3, param4, maxUnrollingDepth);
        }

        /// <summary>
        /// Gets the function as a state transformer.
        /// </summary>
        /// <returns>A transformer for the function.</returns>
        public StateSetTransformer<Pair<T1, T2, T3, T4>, T5> Transformer()
        {
            Func<Zen<Pair<T1, T2, T3, T4>>, Zen<T5>> f = p => this.function(p.Item1(), p.Item2(), p.Item3(), p.Item4());
            return StateSetTransformerFactory.CreateTransformer(f);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="input4">Default fourth input that captures structural constraints.</param>
        /// <param name="listSize">The depth bound for any given object.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2, T3, T4)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, listSize, checkSmallerLists);
            input4 = CommonUtilities.GetArbitraryIfNull(input4, listSize, checkSmallerLists);
            var args = new Dictionary<long, object>
            {
                { this.argument1.ArgumentId, input1 },
                { this.argument2.ArgumentId, input2 },
                { this.argument3.ArgumentId, input3 },
                { this.argument4.ArgumentId, input4 },
            };

            var result = invariant(this.argument1, this.argument2, this.argument3, this.argument4, this.functionBodyExpr);
            return CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(result, args, input1, input2, input3, input4, backend));
        }

        /// <summary>
        /// Find all inputs leading to a condition being true.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">First input that captures structural constraints.</param>
        /// <param name="input2">Second input that captures structural constraints.</param>
        /// <param name="input3">Third input that captures structural constraints.</param>
        /// <param name="input4">Fourth input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3, T4)> FindAll(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, listSize, checkSmallerLists);
            input4 = CommonUtilities.GetArbitraryIfNull(input4, listSize, checkSmallerLists);
            var result = invariant(input1, input2, input3, input4, this.function(input1, input2, input3, input4));

            Zen<bool> blocking = false;

            while (true)
            {
                var expr = And(result, Not(blocking));
                var example = CommonUtilities.RunWithLargeStack(
                    () => SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), input1, input2, input3, input4, backend));
                if (!example.HasValue)
                {
                    yield break;
                }

                yield return example.Value;
                blocking = Or(blocking, And(input1 == example.Value.Item1, input2 == example.Value.Item2, input3 == example.Value.Item3, input4 == example.Value.Item4));
            }
        }

        /// <summary>
        /// Generate inputs that exercise different program paths.
        /// </summary>
        /// <param name="precondition">A precondition for inputs to satisfy.</param>
        /// <param name="input1">Default first input that captures structural constraints.</param>
        /// <param name="input2">Default second input that captures structural constraints.</param>
        /// <param name="input3">Default third input that captures structural constraints.</param>
        /// <param name="input4">Default fourth input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public IEnumerable<(T1, T2, T3, T4)> GenerateInputs(
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> precondition = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            Backend backend = Backend.Z3)
        {
            input1 = CommonUtilities.GetArbitraryIfNull(input1, listSize, checkSmallerLists);
            input2 = CommonUtilities.GetArbitraryIfNull(input2, listSize, checkSmallerLists);
            input3 = CommonUtilities.GetArbitraryIfNull(input3, listSize, checkSmallerLists);
            input4 = CommonUtilities.GetArbitraryIfNull(input4, listSize, checkSmallerLists);
            precondition = precondition == null ? (w, x, y, z) => true : precondition;
            return CommonUtilities.RunWithLargeStack(() => InputGenerator.GenerateInputs(this.function, precondition, input1, input2, input3, input4, backend));
        }
    }
}