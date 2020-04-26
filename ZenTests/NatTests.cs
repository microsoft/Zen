// <copyright file="ClassTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Research.Zen;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Microsoft.Research.Zen.Language;
    using static Microsoft.Research.ZenTests.TestHelper;

    /// <summary>
    /// Tests for Zen working with classes.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NatTests
    {
        /// <summary>
        /// A NAT class.
        /// </summary>
        public class Nat
        {
            /// <summary>
            /// Gets or sets the rules.
            /// </summary>
            public (uint, uint, uint)[] Rules { get; set; }
        }

        private Zen<Packet> NatMatch(Nat nat, Zen<Packet> packet)
        {
            return TestHelper.ApplyOrderedRules(
                packet,
                packet,
                ruleMatch: (l, pkt, i) => MatchNatLine(l, pkt),
                ruleAction: (l, pkt, i) => ApplyNatLine(l, pkt),
                ruleReturn: (l, pkt, i) => Some(pkt),
                nat.Rules);
        }

        private Zen<bool> MatchNatLine((uint, uint, uint) rule, Zen<Packet> packet)
        {
            var lo = rule.Item1;
            var hi = rule.Item2;
            var dstIp = packet.GetField<Packet, uint>("DstIp");
            return And(dstIp >= UInt(lo), dstIp <= UInt(hi));
        }

        private Zen<Packet> ApplyNatLine((uint, uint, uint) rule, Zen<Packet> packet)
        {
            var newDstIp = rule.Item3;
            return packet.WithField("DstIp", UInt(newDstIp));
        }

        /// <summary>
        /// Test evaluation for an example NAT.
        /// </summary>
        [TestMethod]
        public void TestNatEvaluate()
        {
            var rules = new ValueTuple<uint, uint, uint>[2] { (0, 10, 99), (11, 20, 100) };
            var nat = new Nat { Rules = rules };
            var function = Function<Packet, Packet>(p => NatMatch(nat, p));
            Assert.AreEqual(function.Evaluate(new Packet { DstIp = 10 }).DstIp, 99U);
            Assert.AreEqual(function.Evaluate(new Packet { DstIp = 11 }).DstIp, 100U);
        }

        /// <summary>
        /// Test agreement between verification and evaluation.
        /// </summary>
        [TestMethod]
        public void TestNatVerify()
        {
            var rules = new ValueTuple<uint, uint, uint>[2] { (0, 10, 99), (11, 20, 100) };
            var nat = new Nat { Rules = rules };
            CheckAgreement<Packet>(p => NatMatch(nat, p).GetField<Packet, uint>("DstIp") == UInt(100));
            CheckAgreement<Packet>(p => NatMatch(nat, p).GetField<Packet, uint>("DstIp") == UInt(99));
        }
    }
}
