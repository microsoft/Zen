// <copyright file="Event.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using ZenLib;

    /// <summary>
    /// Class representing an event.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// A pairwise invariant for a list of elements.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="f">The pairwise invariant.</param>
        /// <returns>A zen value for the invariant.</returns>
        internal static Zen<bool> PairwiseInvariant<T>(Zen<IList<T>> list, Func<Zen<T>, Zen<T>, Zen<bool>> f)
        {
            return list.Case(
                empty: true,
                cons: (hd1, tl1) => tl1.Case(
                    empty: true,
                    cons: (hd2, tl2) => Language.And(f(hd1, hd2), PairwiseInvariant(tl1, f))));
        }
    }
}
