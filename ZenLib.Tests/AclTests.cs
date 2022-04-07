// <copyright file="AclTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for Zen working with classes.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AclTests
    {
        private Acl ExampleAcl()
        {
            var p1 = new Prefix { Length = 24, Address = Ip.Parse("72.1.2.0").Value };
            var p2 = new Prefix { Length = 24, Address = Ip.Parse("1.2.3.0").Value };
            var p3 = new Prefix { Length = 32, Address = Ip.Parse("8.8.8.8").Value };
            var p4 = new Prefix { Length = 32, Address = Ip.Parse("9.9.9.9").Value };
            var aclLine1 = new AclLine { DstIp = p1, SrcIp = p2, Permitted = true };
            var aclLine2 = new AclLine { DstIp = p3, SrcIp = p4, Permitted = false };
            var lines = new AclLine[2] { aclLine1, aclLine2 };
            return new Acl { Lines = lines };
        }

        /// <summary>
        /// Test an acl evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestAclEvaluate()
        {
            var function = new ZenFunction<IpHeader, bool>(p => ExampleAcl().Process(p, 0));
            var result = function.Evaluate(new IpHeader { DstIp = Ip.Parse("72.1.2.1"), SrcIp = Ip.Parse("1.2.3.4") });
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Check agreement for example acl.
        /// </summary>
        [TestMethod]
        public void TestAclVerify()
        {
            CheckAgreement<IpHeader>(p => ExampleAcl().Process(p, 0));
        }

        /// <summary>
        /// Test acl with provenance evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestAclWithLinesEvaluate()
        {
            var function = new ZenFunction<IpHeader, Pair<bool, ushort>>(p => ExampleAcl().ProcessProvenance(p, 0));
            var result = function.Evaluate(new IpHeader { DstIp = Ip.Parse("8.8.8.8"), SrcIp = Ip.Parse("9.9.9.9") });
            Assert.AreEqual(result.Item1, false);
            Assert.AreEqual(result.Item2, (ushort)2);
            var packet = function.Find((p, l) => l.Item2() == 3);
            Assert.AreEqual(3, function.Evaluate(packet.Value).Item2);
        }

        /// <summary>
        /// Check agreement for acl with provenance.
        /// </summary>
        [TestMethod]
        public void TestAclWithLinesVerify()
        {
            CheckAgreement<IpHeader>(p => ExampleAcl().ProcessProvenance(p, 0).Item2() == 2);
        }

        /// <summary>
        /// Test unrolling a transition function.
        /// </summary>
        [TestMethod]
        public void TestUnroll()
        {
            var function = new ZenFunction<LocatedPacket, LocatedPacket>(lp => StepMany(lp, 3));

            var input = function.Find((inputLp, outputLp) =>
                And(inputLp.GetNode() == 0,
                    outputLp.GetNode() == 2,
                    outputLp.GetHeader().GetDstIp().GetValue() == 4));

            Assert.IsTrue(input.HasValue);
        }

        private Zen<Option<LocatedPacket>> StepOnce(Zen<LocatedPacket> lp)
        {
            var location = lp.GetNode();
            var packet = lp.GetHeader();
            return If(location == 0,
                    Option.Create(LocatedPacketHelper.Create(1, packet)),
                    If(location == 1,
                        Option.Create(LocatedPacketHelper.Create(2, packet)),
                        Option.Null<LocatedPacket>()));
        }

        private Zen<LocatedPacket> StepMany(Zen<LocatedPacket> initial, int k)
        {
            if (k == 0)
            {
                return initial;
            }

            var newLp = StepOnce(initial);
            return If(newLp.IsSome(), StepMany(newLp.Value(), k - 1), initial);
        }
    }
}
