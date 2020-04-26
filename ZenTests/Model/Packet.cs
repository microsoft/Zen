// <copyright file="Packet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Simple packet class for testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Packet
    {
        /// <summary>
        /// The destination IP address.
        /// </summary>
        public uint DstIp { get; set; }

        /// <summary>
        /// The source IP address.
        /// </summary>
        public uint SrcIp { get; set; }

        /// <summary>
        /// Convert a packet to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"DstIp={DstIp}, SrcIp={SrcIp}";
        }
    }
}
