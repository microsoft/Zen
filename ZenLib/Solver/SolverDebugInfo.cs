// <copyright file="SolverDebugInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Solver
{
    using System;

    /// <summary>
    /// Solver debug information for Zen.
    /// </summary>
    public class SolverDebugInfo
    {
        /// <summary>
        /// The solver query that was used for Z3.
        /// </summary>
        public String SolverQuery;

        /// <summary>
        /// The solve time for Z3.
        /// </summary>
        public TimeSpan SolverTime;
    }
}