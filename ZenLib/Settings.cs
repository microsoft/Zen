// <copyright file="Settings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace ZenLib
{
    /// <summary>
    /// Settings for Zen.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Use a larger stack to avoid stack overflow errors for
        /// large problem instances. Default is false.
        /// </summary>
        public static bool UseLargeStack = false;

        /// <summary>
        ///  Maximum stack size since Zen uses deep recursion.
        /// </summary>
        public static int LargeStackSize = 30_000_000;

        /// <summary>
        /// Whether we are in a separate thread with a larger stack already.
        /// </summary>
        [ThreadStatic] internal static bool InSeparateThread = false;
    }
}
