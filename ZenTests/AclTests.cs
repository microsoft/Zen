// <copyright file="AclTests.cs" company="Microsoft">
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
    public class AclTests
    {
        private Acl ExampleAcl()
        {
            var aclLine1 = new AclLine { DstIpLow = 10, DstIpHigh = 20, SrcIpLow = 7, SrcIpHigh = 39, Permitted = true };
            var aclLine2 = new AclLine { DstIpLow = 0, DstIpHigh = 100, SrcIpLow = 0, SrcIpHigh = 100, Permitted = false };
            var lines = new AclLine[2] { aclLine1, aclLine2 };
            return new Acl { Lines = lines };
        }

        /// <summary>
        /// Test an acl evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestAclEvaluate()
        {
            var function = Function<Packet, bool>(p => ExampleAcl().Match(p));
            var result = function.Evaluate(new Packet { DstIp = 12, SrcIp = 8 });
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Check agreement for example acl.
        /// </summary>
        [TestMethod]
        public void TestAclVerify()
        {
            CheckAgreement<Packet>(p => ExampleAcl().Match(p));
        }

        /// <summary>
        /// Test acl with provenance evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TestAclWithLinesEvaluate()
        {
            var function = Function<Packet, Tuple<bool, ushort>>(p => ExampleAcl().MatchProvenance(p));
            var result = function.Evaluate(new Packet { DstIp = 12, SrcIp = 6 });
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
            CheckAgreement<Packet>(p => ExampleAcl().MatchProvenance(p).Item2() == 2);
        }

        /// <summary>
        /// benchmark.
        /// </summary>
        [TestMethod]
        public void TestAclEvaluatePerformance()
        {
            var acl = ExampleAcl();

            var function = Function<Packet, bool>(p => acl.Match(p));
            function.Compile();

            var packet = new Packet { DstIp = 12, SrcIp = 6 };
            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 10000000; i++)
            {
                function.Evaluate(packet);
            }

            Console.WriteLine($"compiled took: {watch.ElapsedMilliseconds}");
            watch.Restart();

            for (int i = 0; i < 10000000; i++)
            {
                acl.MatchLoop(packet);
            }

            Console.WriteLine($"manual took: {watch.ElapsedMilliseconds}");
            watch.Restart();
        }
    }
}
