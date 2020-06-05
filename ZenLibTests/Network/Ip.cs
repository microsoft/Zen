// <copyright file="Ip.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System.Net;
    using ZenLib;

    /// <summary>
    /// A simple IP address.
    /// </summary>
    struct Ip
    {
        public uint Value { get; set; }

        public static Zen<Ip> Create(Zen<uint> value)
        {
            return Language.Create<Ip>(("Value", value));
        }

        public static Ip Parse(string s)
        {
            var bytes = IPAddress.Parse(s).GetAddressBytes();
            return FromBytes(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public override string ToString()
        {
            var x1 = (this.Value >> 0) & 0x000000FF;
            var x2 = (this.Value >> 8) & 0x000000FF;
            var x3 = (this.Value >> 16) & 0x000000FF;
            var x4 = (this.Value >> 24) & 0x000000FF;
            return $"{x4}.{x3}.{x2}.{x1}";
        }

        private static Ip FromBytes(byte x1, byte x2, byte x3, byte x4)
        {
            return new Ip { Value = (uint)(x1 << 24) | (uint)(x2 << 16) | (uint)(x3 << 8) | x4 };
        }
    }

    /// <summary>
    /// Ip extension methods.
    /// </summary>
    static class IpExtensions
    {
        public static Zen<uint> GetValue(this Zen<Ip> ip)
        {
            return ip.GetField<Ip, uint>("Value");
        }
    }
}
