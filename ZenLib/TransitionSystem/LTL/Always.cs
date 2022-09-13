// <copyright file="Always.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// A property that must always hold.
    /// </summary>
    public class Always<T> : LTL<T>
    {
        /// <summary>
        /// The spec that should hold on every state.
        /// </summary>
        public LTL<T> Formula { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override LTL<T> Nnf()
        {
            return new Always<T> { Formula = this.Formula.Nnf() };
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
            if (i + 1 == states.Length)
            {
                var loopExists = inLoop[i - 1];

                var holdsInLoop = Zen.True();
                for (int j = 0; j < i; j++)
                {
                    holdsInLoop = Zen.And(holdsInLoop, Zen.Implies(inLoop[j], this.Formula.EncodeSpec(states, loopStart, inLoop, j)));
                }

                return Zen.And(loopExists, holdsInLoop);
            }
            else
            {
                return Zen.And(
                    this.Formula.EncodeSpec(states, loopStart, inLoop, i),
                    this.EncodeSpec(states, loopStart, inLoop, i + 1));
            }
        }
    }
}
