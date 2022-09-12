// <copyright file="Always.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    using System;
    using System.Linq;

    /// <summary>
    /// A property that must always hold.
    /// </summary>
    public class Always<T> : Spec<T>
    {
        /// <summary>
        /// The spec that should hold on every state.
        /// </summary>
        public Spec<T> Spec { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override Spec<T> Nnf()
        {
            return new Always<T> { Spec = this.Spec.Nnf() };
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k)
        {
            return Zen.False();
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
            var result = Zen.True();
            for (int j = Math.Min(i, l); j <= k; j++)
            {
                result = Zen.And(result, this.Spec.EncodeLoop(states, l, j, k));
            }

            return result;
        }
    }
}
