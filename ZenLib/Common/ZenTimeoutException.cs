// <copyright file="ZenSolverTimeoutException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// New exception type for a timeout in a solver for Zen..
    /// </summary>
    public class ZenSolverTimeoutException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ZenSolverTimeoutException"/> class.
        /// </summary>
        public ZenSolverTimeoutException() : base()
        {
        }
    }
}
