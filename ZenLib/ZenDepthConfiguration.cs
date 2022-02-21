// <copyright file="ZenDepthConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// A generation configuration.
    /// </summary>
    internal class ZenDepthConfiguration
    {
        /// <summary>
        /// The depth at which to generate objects.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Whether to exhaustively enumerate objects up to a given depth.
        /// </summary>
        public bool ExhaustiveDepth { get; set; }
    }
}