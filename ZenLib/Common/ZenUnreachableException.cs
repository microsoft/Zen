// <copyright file="ZenUnreachableException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Exception for unreachable code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ZenUnreachableException : ZenException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ZenUnreachableException"/> class.
        /// </summary>
        public ZenUnreachableException() : base("Unexpected unreachable code detected.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZenException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public ZenUnreachableException(Exception innerException) : base("Unexpected unreachable code detected.", innerException)
        {
        }
    }
}
