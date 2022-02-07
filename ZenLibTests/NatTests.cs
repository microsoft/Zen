// <copyright file="ClassTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

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

        private Zen<IpHeader> NatMatch(Nat nat, Zen<IpHeader> header)
        {
            return TestHelper.ApplyOrderedRules(
                header,
                header,
                ruleMatch: (l, pkt, i) => MatchNatLine(l, pkt),
                ruleAction: (l, pkt, i) => ApplyNatLine(l, pkt),
                ruleReturn: (l, pkt, i) => Option.Create(pkt),
                nat.Rules);
        }

        private Zen<bool> MatchNatLine((uint, uint, uint) rule, Zen<IpHeader> header)
        {
            var lo = rule.Item1;
            var hi = rule.Item2;
            var dstIp = header.GetDstIp().GetValue();
            return And(dstIp >= Constant<uint>(lo), dstIp <= Constant<uint>(hi));
        }

        private Zen<IpHeader> ApplyNatLine((uint, uint, uint) rule, Zen<IpHeader> header)
        {
            var newDstIp = rule.Item3;
            return header.WithField("DstIp", Ip.Create(Constant<uint>(newDstIp)));
        }

        /// <summary>
        /// Test evaluation for an example NAT.
        /// </summary>
        [TestMethod]
        public void TestNatEvaluate()
        {
            var rules = new ValueTuple<uint, uint, uint>[2] { (0, 10, 99), (11, 20, 100) };
            var nat = new Nat { Rules = rules };
            var function = new ZenFunction<IpHeader, IpHeader>(p => NatMatch(nat, p));
            Assert.AreEqual(function.Evaluate(new IpHeader { DstIp = new Ip { Value = 10 } }).DstIp.Value, 99U);
            Assert.AreEqual(function.Evaluate(new IpHeader { DstIp = new Ip { Value = 11 } }).DstIp.Value, 100U);
        }

        /// <summary>
        /// Test agreement between verification and evaluation.
        /// </summary>
        [TestMethod]
        public void TestNatVerify()
        {
            var rules = new ValueTuple<uint, uint, uint>[2] { (0, 10, 99), (11, 20, 100) };
            var nat = new Nat { Rules = rules };
            CheckAgreement<IpHeader>(p => NatMatch(nat, p).GetDstIp().GetValue() == Constant<uint>(100));
            CheckAgreement<IpHeader>(p => NatMatch(nat, p).GetDstIp().GetValue() == Constant<uint>(99));
        }
    }
}
