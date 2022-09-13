// <copyright file="SearchResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// The search result.
    /// </summary>
    public class SearchResult<T>
    {
        /// <summary>
        /// The outcome of the search.
        /// </summary>
        public SearchOutcome SearchOutcome { get; }

        /// <summary>
        /// The counter example, or null if none.
        /// </summary>
        public T[] CounterExample { get; }

        /// <summary>
        /// The depth of the search.
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// The statistics for the search.
        /// </summary>
        public SearchStats Stats { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SearchResult{T}"/> class.
        /// </summary>
        /// <param name="depth">The depth of the search.</param>
        /// <param name="searchOutcome">The search outcome.</param>
        /// <param name="counterExample">The counter example.</param>
        /// <param name="stats">The statistics.</param>
        public SearchResult(int depth, SearchOutcome searchOutcome, T[] counterExample, SearchStats stats)
        {
            this.Depth = depth;
            this.SearchOutcome = searchOutcome;
            this.CounterExample = counterExample;
            this.Stats = stats;
        }
    }
}
