// <copyright file="Network.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Collections.Generic;

    /// <summary>
    /// Network object as a collection of devices.
    /// </summary>
    class Network
    {
        public Dictionary<string, Device> Devices { get; set; }
    }
}
