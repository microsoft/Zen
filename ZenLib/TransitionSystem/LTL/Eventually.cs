// <copyright file="Eventually.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    using System;

    /// <summary>
    /// A property that must eventually hold.
    /// </summary>
    public class Eventually<T> : Spec<T>
    {
        /// <summary>
        /// The spec that should hold on a future state.
        /// </summary>
        public Spec<T> Spec { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override Spec<T> Nnf()
        {
            return new Eventually<T> { Spec = this.Spec.Nnf() };
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k)
        {
            var result = Zen.False();
            for (int j = i; j <= k; j++)
            {
                result = Zen.Or(result, this.Spec.EncodeLoopFree(states, j, k));
            }

            return result;
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
            var result = Zen.False();
            for (int j = Math.Min(i, l); j <= k; j++)
            {
                result = Zen.Or(result, this.Spec.EncodeLoop(states, l, j, k));
            }

            return result;
        }
    }
}
