// <copyright file="Not.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

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
                var inner = LTL.Not(s2.Formula);
                return LTL.Always(inner.Nnf());
            }

            // not(always(x)) == eventually(not(x))
            if (Formula is Always<T> s3)
            {
                var inner = LTL.Not(s3.Formula);
                return LTL.Eventually(inner.Nnf());
            }

            // not(and(x, y)) == or(not(x), not(y))
            if (Formula is And<T> s4)
            {
                return LTL.Or(LTL.Not(s4.Formula1), LTL.Not(s4.Formula2)).Nnf();
            }

            // not(or(x, y)) == and(not(x), not(y))
            if (Formula is Or<T> s5)
            {
                return LTL.And(LTL.Not(s5.Formula1), LTL.Not(s5.Formula2)).Nnf();
            }

            // not(f(s)) = f(not(s))
            var s6 = (Predicate<T>)Formula;
            return LTL.Predicate<T>(s => Zen.Not(s6.Function(s)));
        }

        /// <summary>
        /// Encode the loop-free condition.
        /// </summary>
        /// <param name="states">The symbolic states.</param>
        /// <param name="i">The current index.</param>
        /// <param name="loopStart">Variables for whether at a loop start.</param>
        /// <param name="inLoop">Variables for whehter in a loop.</param>
        [ExcludeFromCodeCoverage]
        internal override Zen<bool> Encode(Zen<T>[] states, Zen<bool>[] loopStart, Zen<bool>[] inLoop, int i)
        {
            throw new ZenUnreachableException();
        }
    }
}
