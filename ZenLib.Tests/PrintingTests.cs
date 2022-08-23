// <copyright file="PrintingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for the Zen bag type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PrintingTests
    {
        /// <summary>
        /// Test to string conversions.
        /// </summary>
        [TestMethod]
        public void TestToStringWorks()
        {
            // expressions
            Assert.AreEqual("1", Zen.Constant(1).ToString());
            Assert.AreEqual("[]", FSeq.Empty<int>().ToString());
            Assert.AreEqual("Cons(new Option`1(HasValue=True, Value=1), [])", FSeq.Empty<int>().AddBack(1).ToString());
            Assert.AreEqual("new Set`1(Values=Set({}, 1, new SetUnit()))", Set.Empty<int>().Add(1).ToString());
            Assert.AreEqual("Unit(1)", Seq.Empty<int>().Add(1).ToString());
            Assert.AreEqual("Concat(Unit(1), Unit(2))", (Seq.Empty<int>().Add(1) + new Seq<int>(2)).ToString());
            Assert.AreEqual("(x == y)", (Zen.Symbolic<int>("x") == Zen.Symbolic<int>("y")).ToString());
            Assert.AreEqual("(x <= y)", (Zen.Symbolic<int>("x") <= Zen.Symbolic<int>("y")).ToString());
            Assert.AreEqual("(x >= y)", (Zen.Symbolic<int>("x") >= Zen.Symbolic<int>("y")).ToString());
            Assert.AreEqual("(x + y)", (Zen.Symbolic<int>("x") + Zen.Symbolic<int>("y")).ToString());
            Assert.AreEqual("(x - y)", (Zen.Symbolic<int>("x") - Zen.Symbolic<int>("y")).ToString());
            Assert.AreEqual("(x * y)", (Zen.Symbolic<int>("x") * Zen.Symbolic<int>("y")).ToString());
            Assert.AreEqual("False", Zen.False().ToString());
            Assert.AreEqual("True", Zen.True().ToString());
            Assert.AreEqual("If(x, 1, 2)", Zen.If<int>(Zen.Symbolic<bool>("x"), 1, 2).ToString());
            Assert.AreEqual("Or(x, y)", Zen.Or(Zen.Symbolic<bool>("x"), Zen.Symbolic<bool>("y")).ToString());
            Assert.AreEqual("And(x, y)", Zen.And(Zen.Symbolic<bool>("x"), Zen.Symbolic<bool>("y")).ToString());

            // arbitrary expressions
            Assert.AreEqual("x", Zen.Arbitrary<bool>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<byte>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<short>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<ushort>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<int>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<uint>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<long>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<ulong>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<Int128>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<UInt128>("x").ToString());
            Assert.AreEqual("(x as String)", Zen.Arbitrary<string>("x").ToString());
            Assert.AreEqual("(x as String)", Zen.Arbitrary<string>("x").ToString());
            Assert.AreEqual("x", Zen.Arbitrary<Map<int, int>>("x").ToString());
            Assert.AreEqual("new Set`1(Values=x_Values)", Zen.Arbitrary<Set<int>>("x").ToString());
            Assert.AreEqual("new Option`1(HasValue=x_HasValue, Value=x_Value)", Zen.Arbitrary<Option<int>>("x").ToString());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatAndOr()
        {
            var a = Zen.Symbolic<bool>("a");
            var b = Zen.Symbolic<bool>("b");
            var c = Zen.Symbolic<bool>("c");

            Console.WriteLine(Zen.And(a, b).Format());

            var manyAnd = Zen.And(
                Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(),
                Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(),
                Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>());
            Console.WriteLine(manyAnd.Format());

            var manyOr = Zen.Or(
                Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(),
                Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(),
                Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>(), Zen.Symbolic<bool>());
            Console.WriteLine(manyOr.Format());

            Console.WriteLine(Zen.And(Zen.And(a, b), c).Format());
            Console.WriteLine(Zen.And(a, Zen.And(b, c)).Format());
            Console.WriteLine(Zen.Or(Zen.Or(a, b), c).Format());
            Console.WriteLine(Zen.Or(a, Zen.Or(b, c)).Format());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatArithmetic()
        {
            var x = Zen.Symbolic<int>("x");
            var y = Zen.Symbolic<int>("y");
            var z = Zen.Symbolic<int>("z");

            var expr =
                Zen.Symbolic<int>("a") + Zen.Symbolic<int>("b") + Zen.Symbolic<int>("c") + Zen.Symbolic<int>("d") + Zen.Symbolic<int>("e") +
                Zen.Symbolic<int>("f") + Zen.Symbolic<int>("g") + Zen.Symbolic<int>("h") + Zen.Symbolic<int>("i") + Zen.Symbolic<int>("j");
            Console.WriteLine(expr.Format());
            Console.WriteLine((expr - expr).Format());
            Console.WriteLine(((x + y) + z).Format());
            Console.WriteLine((x + (y + z)).Format());

            Console.WriteLine((Zen.Symbolic<int>("a") - Zen.Symbolic<int>("b") - 1).Format());
            Console.WriteLine((Zen.Symbolic<int>("a") & Zen.Symbolic<int>("b") & 1).Format());
            Console.WriteLine((x + 1 <= x + 1).Format());

            Console.WriteLine(((x | y) | z).Format());
            Console.WriteLine((x | (y | z)).Format());
            Console.WriteLine((~x).Format());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatSequences()
        {
            var s1 = Zen.Constant(new Seq<string>("a").Add("b"));
            var s2 = Zen.Constant(new Seq<string>("a").Add("b"));
            var s3 = Zen.Symbolic<Seq<string>>();

            Console.WriteLine(Seq.Empty<int>().Format());
            Console.WriteLine(s1.Length().Format());
            Console.WriteLine(s1.Contains(s3).Format());
            Console.WriteLine(s1.ReplaceFirst(s2, s3).Format());
            Console.WriteLine(s1.IndexOf(s3).Format());
            Console.WriteLine(s1.Slice(new BigInteger(10), Zen.Symbolic<BigInteger>()).Format());
            Console.WriteLine(s1.At(Zen.Symbolic<BigInteger>()).Format());
            Console.WriteLine(s1.Concat(s2).Concat(Zen.Symbolic<Seq<string>>("seq")).Format());
            Console.WriteLine(Seq.Concat(s1, Seq.Concat(s2, Zen.Symbolic<Seq<string>>("seq"))).Format());
            Console.WriteLine(Zen.Symbolic<Seq<byte>>().MatchesRegex(Regex.ParseAscii("abc")).Format());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatMaps()
        {
            var a = Zen.Symbolic<Map<int, int>>("a");
            var b = Zen.Symbolic<Map<int, int>>("b");

            Console.WriteLine(Map.Empty<int, int>().Format());
            Console.WriteLine(a.Get(1).Format());
            Console.WriteLine(a.Set(1, 2).Set(3, 4).Set(5, 6).Format());
            Console.WriteLine(a.Delete(1).Delete(2).Format());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatSets()
        {
            var a = Zen.Symbolic<Set<int>>("a");
            var b = Zen.Symbolic<Set<int>>("b");

            Console.WriteLine(Set.Empty<int>().Format());
            Console.WriteLine(a.Add(1).Format());
            Console.WriteLine(a.Delete(1).Delete(2).Format());
            Console.WriteLine(a.Union(b).Union(a).Format());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatOther()
        {
            var a = Zen.Symbolic<bool>("a");
            var x = Zen.Symbolic<int>("x");
            var y = Zen.Symbolic<int>("y");

            Console.WriteLine(new ZenArgumentExpr<int>().Format());
            Console.WriteLine(Zen.Not(a).Format());
            Console.WriteLine(Zen.If(a, x, y).Format());
            Console.WriteLine(Zen.Symbolic<Map<int, int>>("x").Set(1, 2).Set(3, 4).Format());
            Console.WriteLine(Zen.Arbitrary<Option<int>>("x").Format());
            Console.WriteLine(Zen.Symbolic<FSeq<int>>("x").Format());

            var l = Zen.Symbolic<FSeq<byte>>(depth: 3);
            Console.WriteLine(l.Select(x => x + 1).Format());
        }

        /// <summary>
        /// Test that formatting an expression works.
        /// </summary>
        [TestMethod]
        public void TestFormatCMap()
        {
            var a = Zen.Symbolic<CMap<string, int>>("map");
            Console.WriteLine(a.Set("a", 1).Get("b").Format());
        }
    }
}
