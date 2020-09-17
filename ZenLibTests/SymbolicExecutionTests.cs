// <copyright file="SymbolicExecutionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for symbolic execution.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SymbolicExecutionTests
    {
        /// <summary>
        /// Test symbolic execution for strings.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStrings()
        {
            var f = Function<string, int>(x => If(x.Contains("a"), 1, If<int>(x.Contains("b"), 2, 3)));
            Assert.AreEqual(3, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for logic.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionLogic()
        {
            var f1 = Function<int, int, bool>((x, y) => And(x == 1, y == 2));
            var f2 = Function<int, int, bool>((x, y) => Or(x == 1, y == 2));
            var f3 = Function<int, int, bool>((x, y) => Not(Or(x == 1, y == 2)));
            var f4 = Function<int, bool>(x => Or(x == 1, x == 2, x == 3));
            Assert.AreEqual(3, f1.GenerateInputs().Count());
            Assert.AreEqual(3, f2.GenerateInputs().Count());
            Assert.AreEqual(3, f3.GenerateInputs().Count());
            Assert.AreEqual(4, f4.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for bitvectors.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionBitvectors()
        {
            var f1 = Function<int, int, int, int>((x, y, z) => ((x | y) & z) ^ x);
            Assert.AreEqual(1, f1.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for bitvectors.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringOperations()
        {
            var f = Function<string, string, string, string, bool>((w, x, y, z) => Or(w.EndsWith(x), w.StartsWith(y), w.Contains(z)));
            Assert.AreEqual(4, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionListContains()
        {
            var f = Function<IList<int>, bool>(x => x.Contains(3));
            Assert.AreEqual(20, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionListSorting()
        {
            var f = Function<IList<int>, IList<int>>(x => x.Sort());
            Assert.AreEqual(6, f.GenerateInputs(listSize: 3, checkSmallerLists: false).Count());
        }

        /// <summary>
        /// Test symbolic execution for an ite chain.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionIfThenElse()
        {
            var f = Function<int, int, int, int>((x, y, z) =>
            {
                return If(x > 10, 1, If(y > x, If<int>(z > 10, 2, 3), If<int>(z > y, 4, 5)));
            });

            var values = f.GenerateInputs().ToList();
            var outputs = values.Select(x => f.Evaluate(x.Item1, x.Item2, x.Item3));

            Assert.AreEqual(5, f.GenerateInputs().Count());
            Assert.IsTrue(outputs.Contains(1));
            Assert.IsTrue(outputs.Contains(2));
            Assert.IsTrue(outputs.Contains(3));
            Assert.IsTrue(outputs.Contains(4));
            Assert.IsTrue(outputs.Contains(5));
        }

        /// <summary>
        /// Test symbolic execution for packets.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionPacketAcl()
        {
            var p1 = new Prefix { Length = 24, Address = Ip.Parse("72.1.2.0").Value };
            var p2 = new Prefix { Length = 24, Address = Ip.Parse("1.2.3.0").Value };
            var p3 = new Prefix { Length = 32, Address = Ip.Parse("8.8.8.8").Value };
            var p4 = new Prefix { Length = 32, Address = Ip.Parse("9.9.9.9").Value };
            var aclLine1 = new AclLine { DstIp = p1, SrcIp = p2, Permitted = true };
            var aclLine2 = new AclLine { DstIp = p3, SrcIp = p4, Permitted = true };
            var lines = new AclLine[2] { aclLine1, aclLine2 };
            var acl = new Acl { Lines = lines };

            var f = Function<IpHeader, bool>(h => acl.Process(h, 0));
            Assert.AreEqual(5, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution doesn't throw when inputs provided.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionInputsProvided()
        {
            var a = Arbitrary<int>();
            Function<int, bool>(w => true).GenerateInputs(a);
            Function<int, int, bool>((w, x) => true).GenerateInputs(a, a);
            Function<int, int, int, bool>((w, x, y) => true).GenerateInputs(a, a, a);
            Function<int, int, int, int, bool>((w, x, y, z) => true).GenerateInputs(a, a, a, a);
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionOptions()
        {
            var f = Function<Option<int>, Option<int>>(x => x.Where(v => v == 1));
            Assert.AreEqual(3, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string at.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringAt()
        {
            var f = Function<string, bool, string>((s, b) => s.At(If<ushort>(b, 1, 2)));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string substring.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringSubstring()
        {
            var f = Function<string, bool, string>((s, b) => s.Substring(0, If<ushort>(b, 1, 2)));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string replace.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringReplace()
        {
            var f = Function<string, bool, string>((s, b) => s.ReplaceFirst("hello", If<string>(b, "x", "y")));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string length.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringLength()
        {
            var f1 = Function<string, int>(s => If<int>(s.Length() == 3, 1, 2));
            var f2 = Function<string, int>(s => If(s.Length() == 3, If<int>(s.Length() == 2, 1, 2), 3));
            Assert.AreEqual(2, f1.GenerateInputs().Count());
            Assert.AreEqual(2, f2.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringIndexOf()
        {
            var f = Function<string, bool, short>((s, b) => s.IndexOf(If<string>(b, "hello", "world)")));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for constants.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionConstants()
        {
            Assert.AreEqual(1, Function<ulong, ulong>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<long, long>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<uint, uint>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<int, int>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<ushort, ushort>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<short, short>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<byte, byte>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, Function<bool, bool>(x => true).GenerateInputs().Count());
            Assert.AreEqual(1, Function<string, string>(x => "").GenerateInputs().Count());
            Assert.AreEqual(1, Function<ulong, ulong>(x => ~x).GenerateInputs().Count());
            Assert.AreEqual(1, Function<string, string>(x => x + x).GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for operations.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionOperations()
        {
            Assert.AreEqual(1, Function<ulong, ulong>(x => ~x).GenerateInputs().Count());
            Assert.AreEqual(1, Function<string, string>(x => x + x).GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for operations.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionEmptyObject()
        {
            Assert.AreEqual(0, Function<Object0, Object0>(o => o).GenerateInputs().Count());
        }
    }
}
