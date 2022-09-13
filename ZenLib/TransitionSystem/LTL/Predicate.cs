﻿// <copyright file="Predicate.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    using System;

    /// <summary>
    /// A predicate that should hold on a state.
    /// </summary>
    public class Predicate<T> : LTL<T>
    {
        /// <summary>
        /// The predicate for the state.
        /// </summary>
        public Func<Zen<T>, Zen<bool>> Function { get; internal set; }

        /// <summary>
        /// Convert the spec to negated normal form.
        /// </summary>
        /// <returns>A spec in nnf.</returns>
        internal override LTL<T> Nnf()
        {
            return this;
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
                var result = Zen.False();
                for (int j = 0; j < i; j++)
                {
                    result = Zen.Or(result, Zen.And(loopStart[j], this.Function(states[j])));
                }

                return result;
            }
            else
            {
                return this.Function(states[i]);
            }
        }
    }
}
