// <copyright file="UnreachableException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// New exception type for code known to be unreachable at runtime.
    /// </summary>
    internal class UnreachableException : Exception
    {
    }
}
