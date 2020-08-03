// <copyright file="Settings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    /// <summary>
    /// Settings for Zen.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Whether to simplify expressions recursively. Default true.
        /// </summary>
        public static bool SimplifyRecursive = false;

        /// <summary>
        ///  Maximum stack size since Zen uses deep recursion.
        /// </summary>
        public static int LargeStackSize = 30_000_000;
    }
}
