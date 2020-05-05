// <copyright file="Backend.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Zen.ModelChecking
{
    /// <summary>
    /// Model checking backend.
    /// </summary>
    public enum Backend
    {
        /// <summary>
        /// DecisionDiagram backend.
        /// </summary>
        DecisionDiagrams,

        /// <summary>
        /// Z3 backend.
        /// </summary>
        Z3,
    }
}
