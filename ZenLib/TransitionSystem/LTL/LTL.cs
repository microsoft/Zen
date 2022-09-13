// <copyright file="Spec.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    using System;

    /// <summary>
    /// A temporal logic specification over states of a given type.
    /// </summary>
    public abstract class LTL<T>
    {
        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal abstract LTL<T> Nnf();

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="loopStart">Variables for whether at a loop start.</param>
        /// <param name="inLoop">Variables for whehter in a loop.</param>
        internal abstract Zen<bool> Encode(Zen<T>[] states, Zen<bool>[] loopStart, Zen<bool>[] inLoop, int i);
    }

    /// <summary>
    /// Smart constructors for building specs.
    /// </summary>
    public static class LTL
    {
        /// <summary>
        /// A base predicate.
        /// </summary>
        /// <param name="spec">The spec for the state.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> Predicate<T>(Func<Zen<T>, Zen<bool>> spec)
        {
            return new Predicate<T> { Function = spec };
        }

        /// <summary>
        /// The 'not' of a spec.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> Not<T>(LTL<T> spec)
        {
            return new Not<T> { Formula = spec };
        }

        /// <summary>
        /// The 'and' of a spec.
        /// </summary>
        /// <param name="spec1">The first spec.</param>
        /// <param name="spec2">The second spec.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> And<T>(LTL<T> spec1, LTL<T> spec2)
        {
            return new And<T> { Formula1 = spec1, Formula2 = spec2 };
        }

        /// <summary>
        /// The 'or' of a spec.
        /// </summary>
        /// <param name="spec1">The first spec.</param>
        /// <param name="spec2">The second spec.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> Or<T>(LTL<T> spec1, LTL<T> spec2)
        {
            return new Or<T> { Formula1 = spec1, Formula2 = spec2 };
        }

        /// <summary>
        /// The 'always' of a spec.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> Always<T>(LTL<T> spec)
        {
            return new Always<T> { Formula = spec };
        }

        /// <summary>
        /// The 'eventually' of a spec.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> Eventually<T>(LTL<T> spec)
        {
            return new Eventually<T> { Formula = spec };
        }
    }
}
