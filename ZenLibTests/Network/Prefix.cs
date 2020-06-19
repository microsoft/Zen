// <copyright file="Prefix.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using ZenLib;

    /// <summary>
    /// Simple packet class for testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Prefix
    {
        /// <summary>
        /// The destination IP address.
        /// </summary>
        public uint Address { get; set; }

        /// <summary>
        /// The length of the prefix.
        /// </summary>
        public byte Length { get; set; }

        /// <summary>
        /// Check if this prefix contains a packet.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns>Whether contained.</returns>
        public Zen<bool> Match(Zen<Ip> ip)
        {
            var mask = 0xFFFFFFFF << (32 - this.Length);
            return (ip.GetValue() & mask) == (this.Address & mask);
        }

        /// <summary>
        /// Create a random prefix.
        /// </summary>
        /// <returns></returns>
        public static Prefix Random(int low, int high)
        {
            var rnd = new Random();
            var addr = (uint)rnd.Next();
            var l = low + (rnd.Next() % (high - low + 1));
            var mask = 0xFFFFFFFF << (32 - l);
            return new Prefix { Length = (byte)l, Address = addr & mask };
        }

        /// <summary>
        /// Convert the prefix to a string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var x1 = (Address >> 24) & 0x000000FF;
            var x2 = (Address >> 16) & 0x000000FF;
            var x3 = (Address >> 8) & 0x000000FF;
            var x4 = (Address >> 0) & 0x000000FF;

            return $"{x1}.{x2}.{x3}.{x4}/{Length}";
        }
    }
}
