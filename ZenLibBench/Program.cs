// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using BenchmarkDotNet.Running;
    using ZenLib;
    using ZenLib.Tests.Network;

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<AclBench>();
            _ = BenchmarkRunner.Run<RouteBench>();
        }
    }
}
