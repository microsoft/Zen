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
        /// <param name="msg">An optional message parameter.</param>
        public static void Assert(bool obj, string msg = "Assertion failed")
        {
            if (!obj)
            {
                throw new ZenException(msg);
            }
        }
    }
}
