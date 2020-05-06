// <copyright file="ZenException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// New exception type for code known to be unreachable at runtime.
    /// </summary>
    public class ZenException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ZenException"/> class.
        /// </summary>
        /// <param name="e"></param>
        public ZenException(string e) : base(e)
        {
        }
    }
}
