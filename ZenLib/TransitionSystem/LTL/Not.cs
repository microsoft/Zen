// <copyright file="Not.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// A spec that negates another.
    /// </summary>
    public class Not<T> : Spec<T>
    {
        /// <summary>
        /// The spec that negates another.
        /// </summary>
        public Spec<T> Spec { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override Spec<T> Nnf()
        {
            // not(not(s)) == s
            if (Spec is Not<T> s1)
            {
                return this.Spec.Nnf();
            }

            // not(eventually(x)) == always(not(x))
            if (Spec is Eventually<T> s2)
            {
                var inner = new Not<T> { Spec = s2.Spec };
                return new Always<T> { Spec = inner.Nnf()  };
            }

            // not(always(x)) == eventually(not(x))
            if (Spec is Always<T> s3)
            {
                var inner = new Not<T> { Spec = s3.Spec };
                return new Eventually<T> { Spec = inner.Nnf() };
            }

            // not(and(x, y)) == or(not(x), not(y))
            if (Spec is And<T> s4)
            {
                var left = new Not<T> { Spec = s4.Spec1 };
                var right = new Not<T> { Spec = s4.Spec2 };
                return new Or<T> { Spec1 = left.Nnf(), Spec2 = right.Nnf() };
            }

            // not(or(x, y)) == and(not(x), not(y))
            if (Spec is Or<T> s5)
            {
                var left = new Not<T> { Spec = s5.Spec1 };
                var right = new Not<T> { Spec = s5.Spec2 };
                return new And<T> { Spec1 = left.Nnf(), Spec2 = right.Nnf() };
            }

            // not(p(s)) == p(not(s))
            if (Spec is Predicate<T> p)
            {
                return new Predicate<T> { Spec = (s) => Zen.Not(p.Spec(s)) };
            }

            throw new ZenUnreachableException();
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="k">The length of the prefix.</param>
        internal override Zen<bool> EncodeLoopFree(Zen<T>[] states, int i, int k)
        {
            return Zen.Not(this.Spec.EncodeLoopFree(states, i, k));
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
            return Zen.Not(this.Spec.EncodeLoop(states, l, i, k));
        }
    }
}
