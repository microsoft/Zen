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
        /// <param name="loopStart">Variables for whether at a loop start.</param>
        /// <param name="inLoop">Variables for whehter in a loop.</param>
        internal override Zen<bool> EncodeSpec(Zen<T>[] states, Zen<bool>[] loopStart, Zen<bool>[] inLoop, int i)
        {
            return Zen.And(
                this.Formula1.EncodeSpec(states, loopStart, inLoop, i),
                this.Formula2.EncodeSpec(states, loopStart, inLoop, i));
        }
    }
}
