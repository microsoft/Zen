// <copyright file="ZenGenerationConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// A generation configuration.
    /// </summary>
    internal class ZenGenerationConfiguration
    {
        /// <summary>
        /// The depth at which to generate objects.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// An optional name to use for variables.
        /// </summary>
        public string Name { get; set; }
    }
}