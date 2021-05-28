// <copyright file="SymbolicExecutionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
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
            var f = new ZenFunction<string, int>(x => If(x.Contains("a"), 1, If<int>(x.Contains("b"), 2, 3)));
            Assert.AreEqual(3, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for big integers.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionBigIntegers()
        {
            var f = new ZenFunction<BigInteger, int>(x => If(x == new BigInteger(10), 1, If<int>(x == new BigInteger(20), 2, 3)));
            Assert.AreEqual(3, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for logic.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionLogic()
        {
            var f1 = new ZenFunction<int, int, bool>((x, y) => And(x == 1, y == 2));
            var f2 = new ZenFunction<int, int, bool>((x, y) => Or(x == 1, y == 2));
            var f3 = new ZenFunction<int, int, bool>((x, y) => Not(Or(x == 1, y == 2)));
            var f4 = new ZenFunction<int, bool>(x => Or(x == 1, x == 2, x == 3));
            Assert.AreEqual(1, f1.GenerateInputs().Count());
            Assert.AreEqual(1, f2.GenerateInputs().Count());
            Assert.AreEqual(1, f3.GenerateInputs().Count());
            Assert.AreEqual(1, f4.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for bitvectors.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionBitvectors()
        {
            var f1 = new ZenFunction<int, int, int, int>((x, y, z) => ((x | y) & z) ^ x);
            Assert.AreEqual(1, f1.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for bitvectors.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringOperations()
        {
            var f = new ZenFunction<string, string, string, string, bool>((w, x, y, z) => If(w.EndsWith(x), True(), If(w.StartsWith(y), True(), w.Contains(z))));
            Assert.AreEqual(3, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionListContains()
        {
            var f = new ZenFunction<IList<int>, bool>(x => x.Contains(3));
            Assert.AreEqual(6, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionListSorting()
        {
            var f = new ZenFunction<IList<int>, IList<int>>(x => x.Sort());
            Assert.AreEqual(6, f.GenerateInputs(listSize: 3, checkSmallerLists: false).Count());
        }

        /// <summary>
        /// Test symbolic execution for an ite chain.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionIfThenElse()
        {
            var f = new ZenFunction<int, int, int, int>((x, y, z) =>
            {
                return If(x > 10, 1, If(y > x, If<int>(z > 10, 2, 3), If<int>(z > y, 4, 5)));
            });

            var values = f.GenerateInputs().ToList();
            var outputs = values.Select(x => f.Evaluate(x.Item1, x.Item2, x.Item3));

            Assert.AreEqual(5, values.Count);
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

            var f = new ZenFunction<IpHeader, bool>(h => acl.Process(h, 0));
            Assert.AreEqual(3, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution doesn't throw when inputs provided.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionInputsProvided()
        {
            var a = Arbitrary<int>();
            new ZenFunction<int, bool>(w => true).GenerateInputs(a);
            new ZenFunction<int, int, bool>((w, x) => true).GenerateInputs(a, a);
            new ZenFunction<int, int, int, bool>((w, x, y) => true).GenerateInputs(a, a, a);
            new ZenFunction<int, int, int, int, bool>((w, x, y, z) => true).GenerateInputs(a, a, a, a);
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionOptions()
        {
            var f = new ZenFunction<Option<int>, Option<int>>(x => x.Where(v => v == 1));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string at.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringAt()
        {
            var f = new ZenFunction<string, bool, string>((s, b) => s.At(If<BigInteger>(b, new BigInteger(1), new BigInteger(2))));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string substring.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringSubstring()
        {
            var f = new ZenFunction<string, bool, string>((s, b) => s.Substring(new BigInteger(0), If<BigInteger>(b, new BigInteger(1), new BigInteger(2))));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string replace.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringReplace()
        {
            var f = new ZenFunction<string, bool, string>((s, b) => s.ReplaceFirst("hello", If<string>(b, "x", "y")));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for string length.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringLength()
        {
            var f1 = new ZenFunction<string, int>(s => If<int>(s.Length() == new BigInteger(3), 1, 2));
            var f2 = new ZenFunction<string, int>(s => If(s.Length() == new BigInteger(3), If<int>(s.Length() == new BigInteger(2), 1, 2), 3));
            Assert.AreEqual(2, f1.GenerateInputs().Count());
            Assert.AreEqual(2, f2.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for lists.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionStringIndexOf()
        {
            var f = new ZenFunction<string, bool, BigInteger>((s, b) => s.IndexOf(If<string>(b, "hello", "world)")));
            Assert.AreEqual(2, f.GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for constants.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionConstants()
        {
            Assert.AreEqual(1, new ZenFunction<ulong, ulong>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<long, long>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<uint, uint>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<int, int>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<ushort, ushort>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<short, short>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<byte, byte>(x => 0).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<bool, bool>(x => true).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<string, string>(x => "").GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<ulong, ulong>(x => ~x).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<string, string>(x => x + x).GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for operations.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionOperations()
        {
            Assert.AreEqual(1, new ZenFunction<ulong, ulong>(x => ~x).GenerateInputs().Count());
            Assert.AreEqual(1, new ZenFunction<string, string>(x => x + x).GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution for operations.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionEmptyObject()
        {
            Assert.AreEqual(1, new ZenFunction<Object0, Object0>(o => o).GenerateInputs().Count());
        }

        /// <summary>
        /// Test symbolic execution with preconditions.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionPrecondition()
        {
            var f = new ZenFunction<IList<byte>, bool>(l => l.IsSorted());
            var f1 = new ZenFunction<IList<byte>, bool>(l1 => l1.Contains(2));
            Assert.IsTrue(f1.GenerateInputs(precondition: l => l.IsSorted()).All(i => f.Evaluate(i)));
        }

        /// <summary>
        /// Test symbolic execution with preconditions.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionPrecondition2()
        {
            var f1 = new ZenFunction<int, bool>(x => x > 10);
            var f2 = new ZenFunction<int, int, bool>((x, y) => x > 10);
            var f3 = new ZenFunction<int, int, int, bool>((x, y, z) => x > 10);
            var f4 = new ZenFunction<int, int, int, int, bool>((w, x, y, z) => x > 10);

            Assert.IsTrue(f1.GenerateInputs(precondition: x => x < 20).All(i => i < 20));
            Assert.IsTrue(f2.GenerateInputs(precondition: (x, y) => x < 20).All(i => i.Item1 < 20));
            Assert.IsTrue(f3.GenerateInputs(precondition: (x, y, z) => x < 20).All(i => i.Item1 < 20));
            Assert.IsTrue(f4.GenerateInputs(precondition: (w, x, y, z) => x < 20).All(i => i.Item1 < 20));
        }

        /// <summary>
        /// Test symbolic execution with invalid precondition.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionPreconditionInvalid()
        {
            var f = new ZenFunction<int, bool>(x => true);
            Assert.AreEqual(0, f.GenerateInputs(precondition: x => false).Count());
        }

        /// <summary>
        /// Test symbolic execution for ACLs.
        /// </summary>
        [TestMethod]
        public void TestSymbolicExecutionAcl()
        {
            var random = new Random(1);
            var lines = new List<AclLine>();

            bool parity = false;

            for (int i = 0; i < 19; i++)
            {
                parity = !parity;
                var dlow = (uint)random.Next();
                var dhigh = (uint)random.Next((int)dlow, int.MaxValue);
                var slow = (uint)random.Next();
                var shigh = (uint)random.Next((int)slow, int.MaxValue);

                var line = new AclLine
                {
                    DstIp = Prefix.Random(24, 32),
                    SrcIp = Prefix.Random(24, 32),
                    Permitted = parity,
                };

                Console.WriteLine($"{line.DstIp}, {line.SrcIp}, {line.Permitted}");

                lines.Add(line);
            }

            var acl =  new Acl { Lines = lines.ToArray() };

            var function = new ZenFunction<IpHeader, bool>(p => acl.Process(p, 0));
            Assert.AreEqual(20, function.GenerateInputs().Count());
        }
    }
}
