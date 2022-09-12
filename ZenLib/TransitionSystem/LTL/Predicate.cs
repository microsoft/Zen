// <copyright file="Predicate.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    using System;

    /// <summary>
    /// A predicate that should hold on a state.
    /// </summary>
    public class Predicate<T> : Spec<T>
    {
        /// <summary>
        /// The predicate for the state.
        /// </summary>
        public Func<Zen<T>, Zen<bool>> Spec { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override Spec<T> Nnf()
        {
            return this;
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k)
        {
            return this.Spec(states[i]);
        }

        /// <summary>
        /// Encode the loop condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="l">The loop index.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoop(Zen<T>[] states, int l, int i, int k)
        {
            return this.Spec(states[i]);
        }
    }
}
