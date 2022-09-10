// <copyright file="SearchStats.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// The search outcome.
    /// </summary>
    public enum SearchOutcome
    {
        /// <summary>
        /// A counter example to the safety check.
        /// </summary>
        CounterExample,

        /// <summary>
        /// A proof that the system is safe.
        /// </summary>
        SafetyProof,

        /// <summary>
        /// A timeout for the verifier.
        /// </summary>
        Timeout,

        /// <summary>
        /// A lack of a counter example.
        /// </summary>
        NoCounterExample,
    }
}
