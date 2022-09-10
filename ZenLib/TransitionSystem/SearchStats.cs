// <copyright file="SearchStats.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    /// <summary>
    /// The search statistics.
    /// </summary>
    public class SearchStats
    {
        /// <summary>
        /// The time taken in milliseconds.
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SearchStats"/> class.
        /// </summary>
        /// <param name="time">The time.</param>
        public SearchStats(long time)
        {
            Time = time;
        }
    }
}
