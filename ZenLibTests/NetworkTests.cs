// <copyright file="NetworkTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;

    /// <summary>
    /// Tests for Zen working with classes.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NetworkTests
    {
        private Network.Network ExampleNetwork()
        {
            // Simple network: [R1] --i1--i2-- [R2] --i3--i4-- [R3]

            // setup interfaces
            Interface i1 = new Interface();
            i1.Name = "i1";
            i1.GreTunnel = (Ip.Parse("1.2.3.4"), Ip.Parse("5.6.7.8"));
            i1.PortNumber = 1;

            Interface i2 = new Interface();
            i2.Name = "i2";
            i2.PortNumber = 1;

            Interface i3 = new Interface();
            i3.Name = "i3";
            i3.PortNumber = 2;

            Interface i4 = new Interface();
            i4.Name = "i4";
            i4.GreTunnel = (Ip.Parse("5.6.7.8"), Ip.Parse("1.2.3.4"));
            i4.PortNumber = 1;
            i4.InboundAcl = new Acl
            {
                Lines = new AclLine[]
                {
                    new AclLine
                    {
                        DstIp = new Prefix { Address = Ip.Parse("172.0.0.0").Value, Length = 24 },
                        SrcIp = new Prefix { Address = Ip.Parse("63.1.0.0").Value, Length = 16 },
                        Permitted = true,
                        DstPortLow = 53,
                        DstPortHigh = 53,
                    },
                },
            };

            // attach neighbors
            i1.Neighbor = i2;
            i2.Neighbor = i1;
            i3.Neighbor = i4;
            i4.Neighbor = i3;

            // setup devices
            Device d1 = new Device();
            d1.Name = "R1";
            d1.Interfaces = new Interface[] { i1 };

            Device d2 = new Device();
            d2.Name = "R2";
            d2.Interfaces = new Interface[] { i2, i3 };

            Device d3 = new Device();
            d3.Name = "R3";
            d3.Interfaces = new Interface[] { i4 };

            // associate owners
            i1.Owner = d1;
            i2.Owner = d2;
            i3.Owner = d2;
            i4.Owner = d3;

            // create and associate forwarding tables
            var table1 = new ForwardingTable
            {
                Rules = new ForwardingRule[]
                {
                    new ForwardingRule
                    {
                        DstIpLow = Ip.Parse("5.0.0.0"),
                        DstIpHigh = Ip.Parse("6.0.0.0"),
                        Interface = i1,
                    },
                },
            };

            var table2 = new ForwardingTable
            {
                Rules = new ForwardingRule[]
                {
                    new ForwardingRule
                    {
                        DstIpLow = Ip.Parse("5.0.0.0"),
                        DstIpHigh = Ip.Parse("6.0.0.0"),
                        Interface = i3,
                    },
                    new ForwardingRule
                    {
                        DstIpLow = Ip.Parse("1.0.0.0"),
                        DstIpHigh = Ip.Parse("2.0.0.0"),
                        Interface = i2,
                    },
                },
            };

            var table3 = new ForwardingTable
            {
                Rules = new ForwardingRule[]
                {
                    new ForwardingRule
                    {
                        DstIpLow = Ip.Parse("1.0.0.0"),
                        DstIpHigh = Ip.Parse("2.0.0.0"),
                        Interface = i4,
                    },
                },
            };

            d1.Table = table1;
            d2.Table = table2;
            d3.Table = table3;

            // create the network
            var network = new Network.Network { Devices = new Dictionary<string, Device>() };
            network.Devices["R1"] = d1;
            network.Devices["R2"] = d2;
            network.Devices["R3"] = d3;

            return network;
        }

        /// <summary>
        /// Test unrolling a transition function.
        /// </summary>
        [TestMethod]
        public void TestServiceMesh()
        {
            var network = ExampleNetwork();
            var d1 = network.Devices["R1"];
            var d2 = network.Devices["R2"];
            var d3 = network.Devices["R3"];

            // encoding along a single path approach
            var f = new ZenFunction<Packet, bool>(p =>
            {
                var pencap = d1.Interfaces[0].Encapsulate(p);
                var fwd1 = d1.Table.Forward(pencap, 0) == 1;
                var fwd2 = d2.Table.Forward(pencap, 0) == 2;
                var pdecap = d3.Interfaces[0].Decapsulate(pencap);
                var acl = d3.Interfaces[0].InboundAcl.Process(pdecap.GetOverlayHeader(), 0);
                return And(fwd1, fwd2, acl);
            });

            // build transformers
            var tfwd1 = new ZenFunction<Packet, bool>(p => d1.Table.Forward(p, 0) == 1).Transformer();
            var tfwd2 = new ZenFunction<Packet, bool>(p => d2.Table.Forward(p, 0) == 2).Transformer();
            var tencap = new ZenFunction<Packet, Packet>(d1.Interfaces[0].Encapsulate).Transformer();
            var tdecap = new ZenFunction<Packet, Packet>(d3.Interfaces[0].Decapsulate).Transformer();
            var tacl = new ZenFunction<Packet, bool>(p => d3.Interfaces[0].InboundAcl.Process(p.GetOverlayHeader(), 0)).Transformer();
        }
    }
}
