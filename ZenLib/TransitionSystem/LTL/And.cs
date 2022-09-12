// <copyright file="And.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// A spec that takes the 'and' of two others.
    /// </summary>
    public class And<T> : Spec<T>
    {
        /// <summary>
        /// The first spec.
        /// </summary>
        public Spec<T> Spec1 { get; internal set; }

        /// <summary>
        /// The second spec.
        /// </summary>
        public Spec<T> Spec2 { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override Spec<T> Nnf()
        {
            return Spec.And(this.Spec1.Nnf(), this.Spec2.Nnf());
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k)
        {
            return Zen.And(this.Spec1.EncodeLoopFree(states, i, k), this.Spec2.EncodeLoopFree(states, i, k));
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
            return Zen.And(this.Spec1.EncodeLoop(states, l, i, k), this.Spec2.EncodeLoop(states, l, i, k));
        }
    }
}
