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
        /// <param name="k">The length of the prefix.</param>
        internal abstract Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k);

        /// <summary>
        /// Encode the loop condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="l">The loop index.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal abstract Zen<bool> EncodeLoop(Zen<T>[] states, int l, int i, int k);
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
            return new Predicate<T> { Spec = spec };
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
            return new Always<T> { Spec = spec };
        }

        /// <summary>
        /// The 'eventually' of a spec.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <returns>The new spec.</returns>
        public static LTL<T> Eventually<T>(LTL<T> spec)
        {
            return new Eventually<T> { Spec = spec };
        }
    }
}
