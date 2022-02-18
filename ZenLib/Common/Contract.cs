// <copyright file="Contract.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// A basic contract/assertion class.
    /// </summary>
    internal static class Contract
    {
        /// <summary>
        /// Validate that an argument is true.
        /// </summary>
        /// <param name="obj">The argument.</param>
        public static void Assert(bool obj)
        {
            if (!obj)
            {
                throw new ZenException("Assertion failed");
            }
        }
    }
}
