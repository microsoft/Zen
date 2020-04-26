// <copyright file="UnreachableException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen
{
    using System;

    /// <summary>
    /// New exception type for code known to be unreachable at runtime.
    /// </summary>
    internal class UnreachableException : Exception
    {
    }
}
