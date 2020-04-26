// <copyright file="ZenFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen
{
    using System;
    using Zen.Compilation;
    using Zen.Interpretation;
    using Zen.ModelChecking;

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    public class ZenFunction<T>
    {
        private Func<Zen<T>> function;

        private Func<T> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T>(ZenFunction<T> value)
        {
            return () => value.Evaluate(true);
        }

        internal ZenFunction(Func<Zen<T>> function)
        {
            this.function = function;
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="simplify">Whether to simplify first.</param>
        /// <returns>Result.</returns>
        public T Evaluate(bool simplify = true)
        {
            if (compiledFunction != null)
            {
                return compiledFunction();
            }

            return Interpreter.Run(this.function, simplify);
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

            this.compiledFunction = CodeGenerator.Compile(this.function, maxUnrollingDepth);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="simplify">Whether to simplify the expression.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public bool Assert(Func<Zen<T>, Zen<bool>> invariant = null, bool simplify = true, Backend backend = Backend.Z3)
        {
            var result = invariant(this.function());
            return SymbolicEvaluator.Find(result, backend, simplify);
        }
    }

    /// <summary>
    /// Zen function representation.
    /// </summary>
    /// <typeparam name="T1">Argument 1 type.</typeparam>
    /// <typeparam name="T2">Return type.</typeparam>
    public class ZenFunction<T1, T2>
    {
        private Func<Zen<T1>, Zen<T2>> function;

        private Func<T1, T2> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2>(ZenFunction<T1, T2> value)
        {
            return (v) => value.Evaluate(v, true);
        }

        internal ZenFunction(Func<Zen<T1>, Zen<T2>> function)
        {
            this.function = function;
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="simplify">Whether to simplify first.</param>
        /// <returns>Result.</returns>
        public T2 Evaluate(T1 value, bool simplify = true)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value);
            }

            return Interpreter.Run(this.function, value, simplify);
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

            this.compiledFunction = CodeGenerator.Compile(this.function, maxUnrollingDepth);
        }

        /// <summary>
        /// Verify that if the function satisfies the precondition,
        /// then it also satisfies the postcondition.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input">Default input that captures structural constraints.</param>
        /// <param name="listSize">The maximum number of elements to consider in an input list.</param>
        /// <param name="checkSmallerLists">Whether to check smaller list sizes as well.</param>
        /// <param name="simplify">Whether to simplify the expression first.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<T1> Find(
            Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant = null,
            Zen<T1> input = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            bool simplify = true,
            Backend backend = Backend.Z3)
        {
            input = (input is null) ? Language.Arbitrary<T1>(listSize, checkSmallerLists) : input;
            var result = invariant(input, this.function(input));
            return SymbolicEvaluator.Find(result, input, backend, simplify);
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
        private Func<Zen<T1>, Zen<T2>, Zen<T3>> function;

        private Func<T1, T2, T3> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3>(ZenFunction<T1, T2, T3> value)
        {
            return (v1, v2) => value.Evaluate(v1, v2, true);
        }

        internal ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>> function)
        {
            this.function = function;
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="simplify">Whether to simplify first.</param>
        /// <returns>Result.</returns>
        public T3 Evaluate(T1 value1, T2 value2, bool simplify = true)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value1, value2);
            }

            return Interpreter.Run(this.function, value1, value2, simplify);
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

            this.compiledFunction = CodeGenerator.Compile(this.function, maxUnrollingDepth);
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
        /// <param name="simplify">Whether to simplify the expression first.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> invariant = null,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            bool simplify = true,
            Backend backend = Backend.Z3)
        {
            input1 = (input1 is null) ? Language.Arbitrary<T1>(listSize, checkSmallerLists) : input1;
            input2 = (input2 is null) ? Language.Arbitrary<T2>(listSize, checkSmallerLists) : input2;
            var result = invariant(input1, input2, this.function(input1, input2));
            return SymbolicEvaluator.Find(result, input1, input2, backend, simplify);
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
        private Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function;

        private Func<T1, T2, T3, T4> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3, T4>(ZenFunction<T1, T2, T3, T4> value)
        {
            return (v1, v2, v3) => value.Evaluate(v1, v2, v3, true);
        }

        internal ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function)
        {
            this.function = function;
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <param name="simplify">Whether to simplify first.</param>
        /// <returns>Result.</returns>
        public T4 Evaluate(T1 value1, T2 value2, T3 value3, bool simplify = true)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value1, value2, value3);
            }

            return Interpreter.Run(this.function, value1, value2, value3, simplify);
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

            this.compiledFunction = CodeGenerator.Compile(this.function, maxUnrollingDepth);
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
        /// <param name="simplify">Whether to simplify the expression first.</param>
        /// <param name="backend">The backend.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public Option<(T1, T2, T3)> Find(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            int listSize = 5,
            bool checkSmallerLists = true,
            bool simplify = true,
            Backend backend = Backend.Z3)
        {
            input1 = (input1 is null) ? Language.Arbitrary<T1>(listSize, checkSmallerLists) : input1;
            input2 = (input2 is null) ? Language.Arbitrary<T2>(listSize, checkSmallerLists) : input2;
            input3 = (input3 is null) ? Language.Arbitrary<T3>(listSize, checkSmallerLists) : input3;
            var result = invariant(input1, input2, input3, this.function(input1, input2, input3));
            return SymbolicEvaluator.Find(result, input1, input2, input3, backend, simplify);
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
        private Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function;

        private Func<T1, T2, T3, T4, T5> compiledFunction = null;

        /// <summary>
        /// Convert implicitly to a function.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Func<T1, T2, T3, T4, T5>(ZenFunction<T1, T2, T3, T4, T5> value)
        {
            return (v1, v2, v3, v4) => value.Evaluate(v1, v2, v3, v4, true);
        }

        internal ZenFunction(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function)
        {
            this.function = function;
        }

        /// <summary>
        /// Evaluate the function against a value.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="value3">The third value.</param>
        /// <param name="value4">The fourth value.</param>
        /// <param name="simplify">Whether to simplify first.</param>
        /// <returns>Result.</returns>
        public T5 Evaluate(T1 value1, T2 value2, T3 value3, T4 value4, bool simplify = true)
        {
            if (compiledFunction != null)
            {
                return compiledFunction(value1, value2, value3, value4);
            }

            return Interpreter.Run(this.function, value1, value2, value3, value4, simplify);
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

            this.compiledFunction = CodeGenerator.Compile(this.function, maxUnrollingDepth);
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
        /// <param name="simplify">Whether to simplify the expression first.</param>
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
            bool simplify = true,
            Backend backend = Backend.Z3)
        {
            input1 = (input1 is null) ? Language.Arbitrary<T1>(listSize, checkSmallerLists) : input1;
            input2 = (input2 is null) ? Language.Arbitrary<T2>(listSize, checkSmallerLists) : input2;
            input3 = (input3 is null) ? Language.Arbitrary<T3>(listSize, checkSmallerLists) : input3;
            input4 = (input4 is null) ? Language.Arbitrary<T4>(listSize, checkSmallerLists) : input4;
            var result = invariant(input1, input2, input3, input4, this.function(input1, input2, input3, input4));
            return SymbolicEvaluator.Find(result, input1, input2, input3, input4, backend, simplify);
        }
    }
}