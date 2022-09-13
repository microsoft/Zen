// <copyright file="And.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// A spec that takes the 'and' of two others.
    /// </summary>
    public class And<T> : LTL<T>
    {
        /// <summary>
        /// The first formula.
        /// </summary>
        public LTL<T> Formula1 { get; internal set; }

        /// <summary>
        /// The second formula.
        /// </summary>
        public LTL<T> Formula2 { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override LTL<T> Nnf()
        {
            return LTL.And(this.Formula1.Nnf(), this.Formula2.Nnf());
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k)
        {
            return Zen.And(this.Formula1.EncodeLoopFree(states, i, k), this.Formula2.EncodeLoopFree(states, i, k));
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
            return Zen.And(this.Formula1.EncodeLoop(states, l, i, k), this.Formula2.EncodeLoop(states, l, i, k));
        }
    }
}
