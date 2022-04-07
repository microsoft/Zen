// <copyright file="Device.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A device objet.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class Device
    {
        public string Name { get; set; }

        public ForwardingTable Table { get; set; }

        public Interface[] Interfaces { get; set; }
    }
}
