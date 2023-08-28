// <copyright file="SolverConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;

    /// <summary>
    /// Solver configuration for Zen.
    /// </summary>
    public class SolverConfig
    {
        /// <summary>
        /// The solver type to use.
        /// </summary>
        public SolverType SolverType { get; set; } = SolverType.Z3;

        /// <summary>
        /// The solver timeout to use.
        /// </summary>
        public TimeSpan? SolverTimeout { get; set; } = null;

        /// <summary>
        /// An optional debugging callback with the .
        /// </summary>
        public Action<SolverDebugInfo> Debug { get; set; } = null;
    }
}