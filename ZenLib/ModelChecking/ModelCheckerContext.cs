// <copyright file="ModelCheckerContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    /// <summary>
    /// The type of model checking to be performed.
    /// </summary>
    public enum ModelCheckerContext
    {
        /// <summary>
        /// Simple constraint solving.
        /// </summary>
        Solving,

        /// <summary>
        /// Constrained optimization.
        /// </summary>
        Optimization,
    }
}
