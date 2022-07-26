// <copyright file="ZenSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// Settings for Zen.
    /// </summary>
    public static class ZenSettings
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
        /// Whether or not the preserve the structure of If branches.
        /// </summary>
        public static bool PreserveBranches = false;

        /// <summary>
        /// Whether we are in a separate thread with a larger stack already.
        /// </summary>
        [ThreadStatic] internal static bool InSeparateThread = false;
    }
}
