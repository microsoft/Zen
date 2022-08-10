// <copyright file="ZenObjectAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;

    /// <summary>
    /// Attribute annotation for classes and structs to generate zen methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class ZenObjectAttribute : Attribute
    {
    }
}