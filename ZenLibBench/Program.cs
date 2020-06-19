﻿// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLibBench
{
    using BenchmarkDotNet.Running;

    /// <summary>
    /// Run a collection of benchmarks for Zen.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<AclBench>();
            // _ = BenchmarkRunner.Run<RouteBench>();
        }
    }
}
