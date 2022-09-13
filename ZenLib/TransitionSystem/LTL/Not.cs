// <copyright file="Not.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// A spec that negates another.
    /// </summary>
    public class Not<T> : LTL<T>
    {
        /// <summary>
        /// The formula that negates another.
        /// </summary>
        public LTL<T> Formula { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override LTL<T> Nnf()
        {
            // not(not(s)) == s
            if (Formula is Not<T> s1)
            {
                return this.Formula.Nnf();
            }

            // not(eventually(x)) == always(not(x))
            if (Formula is Eventually<T> s2)
            {
                var inner = LTL.Not(s2.Spec);
                return LTL.Always(inner.Nnf());
            }

            // not(always(x)) == eventually(not(x))
            if (Formula is Always<T> s3)
            {
                var inner = LTL.Not(s3.Spec);
                return LTL.Eventually(inner.Nnf());
            }

            // not(and(x, y)) == or(not(x), not(y))
            if (Formula is And<T> s4)
            {
                return LTL.Or(LTL.Not(s4.Formula1).Nnf(), LTL.Not(s4.Formula2).Nnf());
            }

            // not(or(x, y)) == and(not(x), not(y))
            if (Formula is Or<T> s5)
            {
                return LTL.And(LTL.Not(s5.Formula1).Nnf(), LTL.Not(s5.Formula2).Nnf());
            }

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
            return Zen.Not(this.Formula.EncodeLoopFree(states, i, k));
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
            return Zen.Not(this.Formula.EncodeLoop(states, l, i, k));
        }
    }
}
