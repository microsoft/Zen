// <copyright file="UnrollingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Basic;

    /// <summary>
    /// Test unrolling expressions.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UnrollingTests
    {
        /// <summary>
        /// Test unrolling is simplified correctly.
        /// </summary>
        [TestMethod]
        public void TestUnrolling()
        {
            var a = Arbitrary<bool>();
            var b = Arbitrary<bool>();
            var x = Arbitrary<int>();
            var y = Arbitrary<int>();
            var byte1 = Arbitrary<byte>();
            var byte2 = Arbitrary<byte>();
            var short1 = Arbitrary<short>();
            var short2 = Arbitrary<short>();
            var ushort1 = Arbitrary<ushort>();
            var ushort2 = Arbitrary<ushort>();
            var uint1 = Arbitrary<uint>();
            var uint2 = Arbitrary<uint>();
            var long1 = Arbitrary<long>();
            var long2 = Arbitrary<long>();
            var ulong1 = Arbitrary<ulong>();
            var ulong2 = Arbitrary<ulong>();
            var bigint1 = Arbitrary<BigInteger>();
            var bigint2 = Arbitrary<BigInteger>();
            var string1 = Arbitrary<string>();
            var string2 = Arbitrary<string>();
            var string3 = Arbitrary<string>();
            var list1 = Arbitrary<Seq<byte>>();
            var list2 = Arbitrary<Seq<byte>>();
            var h1 = Arbitrary<IpHeader>();
            var h2 = Arbitrary<IpHeader>();
            var opt = Arbitrary<Option<int>>();
            var arg = new ZenArgumentExpr<int>();

            var header = IpHeader.Create(Ip.Create(1), Ip.Create(2), 3, 4, 5);

            CheckEqual(True(), True());
            CheckEqual(False(), False());
            CheckEqual(And(a, b), And(a, b));
            CheckEqual(Or(a, b), Or(a, b));
            CheckEqual(Not(a), Not(a));
            CheckEqual(x | y, x | y);
            CheckEqual(x & y, x & y);
            CheckEqual(x ^ y, x ^ y);
            CheckEqual(x + y, x + y);
            CheckEqual(~x, ~x);
            CheckEqual(byte1 + byte2, byte1 + byte2);
            CheckEqual(short1 + short2, short1 + short2);
            CheckEqual(ushort1 + ushort2, ushort1 + ushort2);
            CheckEqual(uint1 + uint2, uint1 + uint2);
            CheckEqual(long1 + long2, long1 + long2);
            CheckEqual(ulong1 + ulong2, ulong1 + ulong2);
            CheckEqual(x - y, x - y);
            CheckEqual(x * y, x * y);
            CheckEqual(x < y, x < y);
            CheckEqual(x > y, x > y);
            CheckEqual(x <= y, x <= y);
            CheckEqual(x >= y, x >= y);
            CheckEqual(bigint1 + bigint2, bigint1 + bigint2);
            CheckEqual(string1 + string2, string1 + string2);
            CheckEqual(string1.At(bigint1), string1.At(bigint1));
            CheckEqual(string1.Contains(string2), string1.Contains(string2));
            CheckEqual(string1.StartsWith(string2), string1.StartsWith(string2));
            CheckEqual(string1.EndsWith(string2), string1.EndsWith(string2));
            CheckEqual(string1.IndexOf(string2, bigint1), string1.IndexOf(string2, bigint1));
            CheckEqual(string1.Length(), string1.Length());
            CheckEqual(string1.ReplaceFirst(string2, string3), string1.ReplaceFirst(string2, string3));
            CheckEqual(string1.Substring(bigint1, bigint2), string1.Substring(bigint1, bigint2));
            CheckEqual(opt.HasValue(), opt.HasValue());
            CheckEqual(arg, arg);
            CheckEqual(If(a, x, y), If(a, x, y));
            CheckEqual(header, header);
            CheckEqual(header.WithField("DstIp", Ip.Create(99)), header.WithField("DstIp", Ip.Create(99)));
            CheckEqual(list1.AddFront(byte1), list1.AddFront(byte1));

            list1.Case(Constant(1), (hd, tl) => 2).Unroll();
            If(b, list1, list2).Case(Constant(1), (hd, tl) => 2).Unroll();
            If(b, h1, h2).GetDstIp().Unroll();
        }

        /// <summary>
        /// Check that two expression are equal.
        /// </summary>
        private void CheckEqual<T>(Zen<T> expression1, Zen<T> expression2)
        {
            Assert.AreEqual(expression1, expression2.Unroll());
        }
    }
}