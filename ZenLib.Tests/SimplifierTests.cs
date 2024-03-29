﻿// <copyright file="SimplifierTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for simplification.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SimplifierTests
    {
        /// <summary>
        /// Double negation simplifies.
        /// </summary>
        [TestMethod]
        public void TestBitwiseNotNot()
        {
            Assert.AreEqual((~~Constant<byte>(3)), Constant<byte>(3));
            Assert.AreEqual((~~Constant<short>(3)), Constant<short>(3));
            Assert.AreEqual((~~Constant<ushort>(3)), Constant<ushort>(3));
            Assert.AreEqual((~~Constant<int>(3)), Constant<int>(3));
            Assert.AreEqual((~~Constant<uint>(3)), Constant<uint>(3));
            Assert.AreEqual((~~Constant<long>(3)), Constant<long>(3));
            Assert.AreEqual((~~Constant<ulong>(3)), Constant<ulong>(3));
        }

        /// <summary>
        /// Double arithmetic negation.
        /// </summary>
        [TestMethod]
        public void TestArithmeticNot()
        {
            var x = Zen.Symbolic<int>();
            var y = Zen.Symbolic<int>();
            Assert.AreEqual(x > y, Not(x <= y));
            Assert.AreEqual(x >= y, Not(x < y));
            Assert.AreEqual(x < y, Not(x >= y));
            Assert.AreEqual(x <= y, Not(x > y));

            var i = Zen.Symbolic<BigInteger>();
            var j = Zen.Symbolic<BigInteger>();
            Assert.AreEqual(i > j, Not(i <= j));
            Assert.AreEqual(i >= j, Not(i < j));
            Assert.AreEqual(i < j, Not(i >= j));
            Assert.AreEqual(i <= j, Not(i > j));
        }

        /// <summary>
        /// Test demorgan's simplification.
        /// </summary>
        [TestMethod]
        public void TestDemorgan()
        {
            var a = Zen.Symbolic<bool>();
            var b = Zen.Symbolic<bool>();

            Assert.AreEqual(Not(And(a, b)), Or(Not(a), Not(b)));
            Assert.AreEqual(Not(Or(a, b)), And(Not(a), Not(b)));
        }

        /// <summary>
        /// Simplify and with constants.
        /// </summary>
        [TestMethod]
        public void TestAndConstant()
        {
            Assert.AreEqual(And(true, true), true);
            Assert.AreEqual(And(true, false), false);
            Assert.AreEqual(And(false, true), false);
            Assert.AreEqual(And(false, false), false);
        }

        /// <summary>
        /// Simplify And, Or idempotent.
        /// </summary>
        [TestMethod]
        public void TestAndOrIdempotent()
        {
            var x = Arbitrary<bool>();
            Assert.AreEqual(And(x, x), x);
            Assert.AreEqual(Or(x, x), x);
        }

        /// <summary>
        /// Simplify or with constants.
        /// </summary>
        [TestMethod]
        public void TestOrConstant()
        {
            Assert.AreEqual(Or(true, true), true);
            Assert.AreEqual(Or(true, false), true);
            Assert.AreEqual(Or(false, true), true);
            Assert.AreEqual(Or(false, false), false);

            var x = Arbitrary<bool>();
            Assert.AreEqual(Or(x, true), true);
            Assert.AreEqual(Or(x, false), x);
            Assert.AreEqual(Or(false, x), x);
            Assert.AreEqual(Or(true, x), true);
        }

        /// <summary>
        /// Simplify not with constants.
        /// </summary>
        [TestMethod]
        public void TestNotConstant()
        {
            Assert.AreEqual(Not(true), false);
            Assert.AreEqual(Not(false), true);
        }

        /// <summary>
        /// Simplify bitwise and with constants.
        /// </summary>
        [TestMethod]
        public void TestBitwiseAndConstant()
        {
            Assert.AreEqual((Constant<byte>(1) & Constant<byte>(1)), Constant<byte>(1));
            Assert.AreEqual((Constant<short>(1) & Constant<short>(1)), Constant<short>(1));
            Assert.AreEqual((Constant<ushort>(1) & Constant<ushort>(1)), Constant<ushort>(1));
            Assert.AreEqual((Constant<int>(1) & Constant<int>(1)), Constant<int>(1));
            Assert.AreEqual((Constant<uint>(1) & Constant<uint>(1)), Constant<uint>(1));
            Assert.AreEqual((Constant<long>(1) & Constant<long>(1)), Constant<long>(1));
            Assert.AreEqual((Constant<ulong>(1) & Constant<ulong>(1)), Constant<ulong>(1));
        }

        /// <summary>
        /// Simplify bitwise or with constants.
        /// </summary>
        [TestMethod]
        public void TestBitwiseOrConstant()
        {
            Assert.AreEqual((Constant<byte>(1) | Constant<byte>(1)), Constant<byte>(1));
            Assert.AreEqual((Constant<short>(1) | Constant<short>(1)), Constant<short>(1));
            Assert.AreEqual((Constant<ushort>(1) | Constant<ushort>(1)), Constant<ushort>(1));
            Assert.AreEqual((Constant<int>(1) | Constant<int>(1)), Constant<int>(1));
            Assert.AreEqual((Constant<uint>(1) | Constant<uint>(1)), Constant<uint>(1));
            Assert.AreEqual((Constant<long>(1) | Constant<long>(1)), Constant<long>(1));
            Assert.AreEqual((Constant<ulong>(1) | Constant<ulong>(1)), Constant<ulong>(1));
        }

        /// <summary>
        /// Simplify bitwise xor with constants.
        /// </summary>
        [TestMethod]
        public void TestBitwiseXorConstant()
        {
            Assert.AreEqual((Constant<byte>(1) ^ Constant<byte>(1)), Constant<byte>(0));
            Assert.AreEqual((Constant<short>(1) ^ Constant<short>(1)), Constant<short>(0));
            Assert.AreEqual((Constant<ushort>(1) ^ Constant<ushort>(1)), Constant<ushort>(0));
            Assert.AreEqual((Constant<int>(1) ^ Constant<int>(1)), Constant<int>(0));
            Assert.AreEqual((Constant<uint>(1) ^ Constant<uint>(1)), Constant<uint>(0));
            Assert.AreEqual((Constant<long>(1) ^ Constant<long>(1)), Constant<long>(0));
            Assert.AreEqual((Constant<ulong>(1) ^ Constant<ulong>(1)), Constant<ulong>(0));
        }

        /// <summary>
        /// Simplify less than or equal.
        /// </summary>
        [TestMethod]
        public void TestLeqSimplification()
        {
            Assert.AreEqual((Constant<byte>(1) <= Constant<byte>(1)), True());
            Assert.AreEqual((Constant<byte>(1) <= Constant<byte>(0)), False());
            Assert.AreEqual((Constant<short>(1) <= Constant<short>(1)), True());
            Assert.AreEqual((Constant<short>(1) <= Constant<short>(0)), False());
            Assert.AreEqual((Constant<ushort>(1) <= Constant<ushort>(1)), True());
            Assert.AreEqual((Constant<ushort>(1) <= Constant<ushort>(0)), False());
            Assert.AreEqual((Constant<int>(1) <= Constant<int>(1)), True());
            Assert.AreEqual((Constant<int>(1) <= Constant<int>(0)), False());
            Assert.AreEqual((Constant<uint>(1) <= Constant<uint>(1)), True());
            Assert.AreEqual((Constant<uint>(1) <= Constant<uint>(0)), False());
            Assert.AreEqual((Constant<long>(1) <= Constant<long>(1)), True());
            Assert.AreEqual((Constant<long>(1) <= Constant<long>(0)), False());
            Assert.AreEqual((Constant<ulong>(1) <= Constant<ulong>(1)), True());
            Assert.AreEqual((Constant<ulong>(1) <= Constant<ulong>(0)), False());
            Assert.AreEqual((Constant<BigInteger>(1) <= Constant<BigInteger>(1)), True());
            Assert.AreEqual((Constant<BigInteger>(1) <= Constant<BigInteger>(0)), False());
            Assert.AreEqual((Constant<Real>(1) <= Constant<Real>(1)), True());
            Assert.AreEqual((Constant<Real>(1) <= Constant<Real>(0)), False());
        }

        /// <summary>
        /// Simplify less than.
        /// </summary>
        [TestMethod]
        public void TestLtSimplification()
        {
            Assert.AreEqual((Constant<byte>(1) < Constant<byte>(1)), False());
            Assert.AreEqual((Constant<byte>(0) < Constant<byte>(1)), True());
            Assert.AreEqual((Constant<short>(1) < Constant<short>(1)), False());
            Assert.AreEqual((Constant<short>(0) < Constant<short>(1)), True());
            Assert.AreEqual((Constant<ushort>(1) < Constant<ushort>(1)), False());
            Assert.AreEqual((Constant<ushort>(0) < Constant<ushort>(1)), True());
            Assert.AreEqual((Constant<int>(1) < Constant<int>(1)), False());
            Assert.AreEqual((Constant<int>(0) < Constant<int>(1)), True());
            Assert.AreEqual((Constant<uint>(1) < Constant<uint>(1)), False());
            Assert.AreEqual((Constant<uint>(0) < Constant<uint>(1)), True());
            Assert.AreEqual((Constant<long>(1) < Constant<long>(1)), False());
            Assert.AreEqual((Constant<long>(0) < Constant<long>(1)), True());
            Assert.AreEqual((Constant<ulong>(1) < Constant<ulong>(1)), False());
            Assert.AreEqual((Constant<ulong>(0) < Constant<ulong>(1)), True());
            Assert.AreEqual((Constant<BigInteger>(1) < Constant<BigInteger>(1)), False());
            Assert.AreEqual((Constant<BigInteger>(0) < Constant<BigInteger>(1)), True());
            Assert.AreEqual((Constant<Real>(1) < Constant<Real>(1)), False());
            Assert.AreEqual((Constant<Real>(0) < Constant<Real>(1)), True());
        }

        /// <summary>
        /// Simplify greater than or equal.
        /// </summary>
        [TestMethod]
        public void TestGeqSimplification()
        {
            Assert.AreEqual((Constant<byte>(1) >= Constant<byte>(1)), True());
            Assert.AreEqual((Constant<byte>(0) >= Constant<byte>(1)), False());
            Assert.AreEqual((Constant<short>(1) >= Constant<short>(1)), True());
            Assert.AreEqual((Constant<short>(0) >= Constant<short>(1)), False());
            Assert.AreEqual((Constant<ushort>(1) >= Constant<ushort>(1)), True());
            Assert.AreEqual((Constant<ushort>(0) >= Constant<ushort>(1)), False());
            Assert.AreEqual((Constant<int>(1) >= Constant<int>(1)), True());
            Assert.AreEqual((Constant<int>(0) >= Constant<int>(1)), False());
            Assert.AreEqual((Constant<uint>(1) >= Constant<uint>(1)), True());
            Assert.AreEqual((Constant<uint>(0) >= Constant<uint>(1)), False());
            Assert.AreEqual((Constant<long>(1) >= Constant<long>(1)), True());
            Assert.AreEqual((Constant<long>(0) >= Constant<long>(1)), False());
            Assert.AreEqual((Constant<ulong>(1) >= Constant<ulong>(1)), True());
            Assert.AreEqual((Constant<ulong>(0) >= Constant<ulong>(1)), False());
            Assert.AreEqual((Constant<BigInteger>(1) >= Constant<BigInteger>(1)), True());
            Assert.AreEqual((Constant<BigInteger>(0) >= Constant<BigInteger>(1)), False());
            Assert.AreEqual((Constant<Real>(1) >= Constant<Real>(1)), True());
            Assert.AreEqual((Constant<Real>(0) >= Constant<Real>(1)), False());
        }

        /// <summary>
        /// Simplify greater than.
        /// </summary>
        [TestMethod]
        public void TestGtSimplification()
        {
            Assert.AreEqual((Constant<byte>(2) > Constant<byte>(1)), True());
            Assert.AreEqual((Constant<byte>(1) > Constant<byte>(1)), False());
            Assert.AreEqual((Constant<short>(2) > Constant<short>(1)), True());
            Assert.AreEqual((Constant<short>(1) > Constant<short>(1)), False());
            Assert.AreEqual((Constant<ushort>(2) > Constant<ushort>(1)), True());
            Assert.AreEqual((Constant<ushort>(1) > Constant<ushort>(1)), False());
            Assert.AreEqual((Constant<int>(2) > Constant<int>(1)), True());
            Assert.AreEqual((Constant<int>(1) > Constant<int>(1)), False());
            Assert.AreEqual((Constant<uint>(2) > Constant<uint>(1)), True());
            Assert.AreEqual((Constant<uint>(1) > Constant<uint>(1)), False());
            Assert.AreEqual((Constant<long>(2) > Constant<long>(1)), True());
            Assert.AreEqual((Constant<long>(1) > Constant<long>(1)), False());
            Assert.AreEqual((Constant<ulong>(2) > Constant<ulong>(1)), True());
            Assert.AreEqual((Constant<ulong>(1) > Constant<ulong>(1)), False());
            Assert.AreEqual((Constant<BigInteger>(2) > Constant<BigInteger>(1)), True());
            Assert.AreEqual((Constant<BigInteger>(1) > Constant<BigInteger>(1)), False());
            Assert.AreEqual((Constant<Real>(2) > Constant<Real>(1)), True());
            Assert.AreEqual((Constant<Real>(1) > Constant<Real>(1)), False());
        }

        /// <summary>
        /// Simplify for addition.
        /// </summary>
        [TestMethod]
        public void TestAdditionSimplification()
        {
            Assert.AreEqual((Constant<byte>(1) + Constant<byte>(0)), Constant<byte>(1));
            Assert.AreEqual((Constant<byte>(0) + Constant<byte>(1)), Constant<byte>(1));
            Assert.AreEqual((Constant<BigInteger>(1) + Constant<BigInteger>(0)), Constant<BigInteger>(1));
            Assert.AreEqual((Constant<BigInteger>(0) + Constant<BigInteger>(1)), Constant<BigInteger>(1));
            Assert.AreEqual((Constant<Real>(1) + Constant<Real>(0)), Constant<Real>(1));
            Assert.AreEqual((Constant<Real>(0) + Constant<Real>(1)), Constant<Real>(1));
            CheckValid<byte>(x => x + 0 == x);
            CheckValid<byte>(x => 0 + x == x);
        }

        /// <summary>
        /// Simplify for addition.
        /// </summary>
        [TestMethod]
        public void TestArithmeticSimplification()
        {
            var x = Zen.Symbolic<Real>("x");
            var y = Zen.Symbolic<BigInteger>("y");

            Assert.AreEqual(x, x + new Real(0));
            Assert.AreEqual(x, new Real(0) + x);
            Assert.AreEqual(y, y + BigInteger.Zero);
            Assert.AreEqual(y, BigInteger.Zero + y);

            Assert.AreEqual(x, x - new Real(0));
            Assert.AreEqual(y, y - BigInteger.Zero);

            Assert.AreEqual(x, x * new Real(1));
            Assert.AreEqual(x, new Real(1) * x);
            Assert.AreEqual(new Real(0), x * new Real(0));
            Assert.AreEqual(new Real(0), new Real(0) * x);
            Assert.AreEqual(y, y * BigInteger.One);
            Assert.AreEqual(y, BigInteger.One * y);
            Assert.AreEqual(Constant(BigInteger.Zero), y * BigInteger.Zero);
            Assert.AreEqual(Constant(BigInteger.Zero), BigInteger.Zero * y);
        }

        /// <summary>
        /// Simplify for subtraction.
        /// </summary>
        [TestMethod]
        public void TestMinusSimplification()
        {
            Assert.AreEqual((Constant<byte>(1) - Constant<byte>(0)), Constant<byte>(1));
            Assert.AreEqual((Constant<BigInteger>(1) - Constant<BigInteger>(0)), Constant<BigInteger>(1));
            Assert.AreEqual((Constant<Real>(1) - Constant<Real>(0)), Constant<Real>(1));
        }

        /// <summary>
        /// Simplify for multiplication.
        /// </summary>
        [TestMethod]
        public void TestMultiplicationSimplification()
        {
            Assert.AreEqual((Constant<byte>(2) * Constant<byte>(2)), Constant<byte>(4));
            Assert.AreEqual((Constant<BigInteger>(2) * Constant<BigInteger>(2)), Constant<BigInteger>(4));
            Assert.AreEqual((Constant<Real>(2) * Constant<Real>(2)), Constant<Real>(4));
            CheckValid<byte>(x => x * 1 == x);
            CheckValid<byte>(x => 1 * x == x);
            CheckValid<byte>(x => x * 0 == 0);
            CheckValid<byte>(x => 0 * x == 0);
        }

        /// <summary>
        /// Simplify FSeq operations.
        /// </summary>
        [TestMethod]
        public void TestFSeqSimplification()
        {
            var l1 = Zen.EmptyList<int>().AddFront(1).AddFront(2);
            var l2 = Zen.EmptyList<int>();
            Assert.AreEqual(False(), l1.Case(true, Zen.Lambda<Pair<Option<int>, FSeq<int>>, bool>(arg => false)));
            Assert.AreEqual(True(), l2.Case(true, Zen.Lambda<Pair<Option<int>, FSeq<int>>, bool>(arg => false)));
        }

        /// <summary>
        /// Simplify unsigned integer operations.
        /// </summary>
        [TestMethod]
        public void TestUnsignedSimplification()
        {
            ulong a = 0xffffffffffffffff;
            ulong b = 0x1000000000000000;
            Assert.AreEqual(Constant(a + b), Constant(a) + Constant(b));
            Assert.AreEqual(Constant(b - a), Constant(b) - Constant(a));
            Assert.AreEqual(Constant(a * b), Constant(a) * Constant(b));
        }

        /// <summary>
        /// Simplify for concatenaion.
        /// </summary>
        [TestMethod]
        public void TestConcatenationSimplification()
        {
            Assert.AreEqual(Seq.Empty<int>() + new Seq<int>(1), Constant(new Seq<int>(1)));
            Assert.AreEqual(new Seq<int>(1) + Seq.Empty<int>(), Constant(new Seq<int>(1)));
        }

        /// <summary>
        /// Simplify if conditions.
        /// </summary>
        [TestMethod]
        public void TestIfSimplification()
        {
            var x = Constant(1);
            var y = Constant(2);
            var b = Arbitrary<bool>();
            Assert.AreEqual(If(true, x, y), x);
            Assert.AreEqual(If(false, x, y), y);
            Assert.AreEqual(If(x == 0, true, b), Or(x == 0, b));
            Assert.AreEqual(If(x == 0, false, b), And(Not(x == 0), b));
            Assert.AreEqual(If(x == 0, b, true), Or(Not(x == 0), b));
            Assert.AreEqual(If(x == 0, b, false), And(x == 0, b));
            Assert.AreEqual(If<bool>(x == 0, false, false), False());
            Assert.AreEqual(If<bool>(x == 0, true, b), Or(x == 0, b));
            Assert.AreEqual(If<bool>(x == 0, false, b), And(Not(x == 0), b));
            Assert.AreEqual(If<bool>(x == 0, b, true), Or(Not(x == 0), b));
            Assert.AreEqual(If<bool>(x == 0, b, false), And(x == 0, b));
        }

        /// <summary>
        /// Test that map simplification is working.
        /// </summary>
        [TestMethod]
        public void TestMapSimplification()
        {
            Assert.AreEqual(Map.Empty<int, int>().Get(10), Option.Null<int>());
            Assert.AreEqual(Map.Empty<int, int>().Delete(10).Get(10), Option.Null<int>());
            Assert.AreEqual(Map.Empty<int, int>().Set(10, 11).Get(10), Option.Create<int>(11));
            Assert.AreEqual(Map.Empty<int, int>().Set(10, 11).Set(10, 12), Map.Empty<int, int>().Set(10, 12));
            Assert.AreEqual(Map.Empty<int, int>().Set(10, 11).Delete(15).Set(15, 10), Map.Empty<int, int>().Set(10, 11).Set(15, 10));
        }

        /// <summary>
        /// Test that const map simplification is working.
        /// </summary>
        [TestMethod]
        public void TestCMapSimplification()
        {
            var x = Zen.Symbolic<CMap<int, int>>();
            Assert.AreEqual(x.Set(1, 2), x.Set(1, 3).Set(1, 2));
        }

        /// <summary>
        /// Test that set simplification is working.
        /// </summary>
        [TestMethod]
        public void TestSetSimplification()
        {
            var x = Set.Empty<int>().Add(10);
            var y = Set.Empty<int>().Add(11);

            // equalities
            Assert.AreEqual(x.Intersect(y).Intersect(y), x.Intersect(y));
            Assert.AreEqual(x.Intersect(y).Intersect(x), x.Intersect(y));
            Assert.AreEqual(x.Intersect(y.Intersect(x)), y.Intersect(x));
            Assert.AreEqual(x.Union(y).Union(y), x.Union(y));
            Assert.AreEqual(x.Union(y).Union(x), x.Union(y));
            Assert.AreEqual(x.Union(y.Union(x)), y.Union(x));
            Assert.AreEqual(x.Union(x), x);
            Assert.AreEqual(x.Intersect(x), x);
            Assert.AreEqual(x.Union(Set.Empty<int>()), x);
            Assert.AreEqual(Set.Empty<int>().Union(x), x);
            Assert.AreEqual(x.Intersect(Set.Empty<int>()), Set.Empty<int>());
            Assert.AreEqual(Set.Empty<int>().Intersect(x), Set.Empty<int>());
            Assert.AreEqual(x.Difference(Set.Empty<int>()), x);
            Assert.AreEqual(x.Difference(x), Set.Empty<int>());
            Assert.AreEqual(Set.Empty<int>().Difference(x), Set.Empty<int>());
            Assert.AreEqual(x.Difference(y).Difference(x), Set.Empty<int>());
            Assert.AreEqual(x.Difference(y).Difference(y), x.Difference(y));

            // inequalities
            Assert.AreNotEqual(x.Union(y).Intersect(y), x.Union(y));
            Assert.AreNotEqual(x.Intersect(y).Union(x), x.Intersect(y));
        }

        /// <summary>
        /// Test that cset simplification is working.
        /// </summary>
        [TestMethod]
        public void TestCSetSimplification()
        {
            var x = CSet.Empty<int>().Add(10);
            var y = CSet.Empty<int>().Add(11);

            // equalities
            Assert.AreEqual(x.Intersect(y).Intersect(y), x.Intersect(y));
            Assert.AreEqual(x.Intersect(y).Intersect(x), x.Intersect(y));
            Assert.AreEqual(x.Intersect(y.Intersect(x)), y.Intersect(x));
            Assert.AreEqual(x.Union(y).Union(y), x.Union(y));
            Assert.AreEqual(x.Union(y).Union(x), x.Union(y));
            Assert.AreEqual(x.Union(y.Union(x)), y.Union(x));
            Assert.AreEqual(x.Union(x), x);
            Assert.AreEqual(x.Intersect(x), x);
            Assert.AreEqual(x.Union(CSet.Empty<int>()), x);
            Assert.AreEqual(CSet.Empty<int>().Union(x), x);
            Assert.AreEqual(x.Intersect(CSet.Empty<int>()), CSet.Empty<int>());
            Assert.AreEqual(CSet.Empty<int>().Intersect(x), CSet.Empty<int>());
            Assert.AreEqual(x.Difference(CSet.Empty<int>()), x);
            Assert.AreEqual(x.Difference(x), CSet.Empty<int>());
            Assert.AreEqual(CSet.Empty<int>().Difference(x), CSet.Empty<int>());
            Assert.AreEqual(x.Difference(y).Difference(x), CSet.Empty<int>());
            Assert.AreEqual(x.Difference(y).Difference(y), x.Difference(y));

            // inequalities
            Assert.AreNotEqual(x.Union(y).Intersect(y), x.Union(y));
            Assert.AreNotEqual(x.Intersect(y).Union(x), x.Intersect(y));
        }

        /// <summary>
        /// Simplify equality conditions.
        /// </summary>
        [TestMethod]
        public void TestEqualsSimplification()
        {
            var f = Constant(false);
            var t = Constant(true);
            var b = Symbolic<bool>();
            Assert.AreEqual(b, b == t);
            Assert.AreEqual(b, t == b);
            Assert.AreEqual(Zen.Not(b), b == f);
            Assert.AreEqual(Zen.Not(b), f == b);
        }

        /// <summary>
        /// Simplify get field.
        /// </summary>
        [TestMethod]
        public void TestObjectGetSimplification0()
        {
            var x = If(Symbolic<bool>(), Symbolic<Object2>(), Symbolic<Object2>());
            var y = x.WithField("Field1", Constant(3));
            Assert.AreEqual(y.GetField<Object2, int>("Field1"), Constant(3));
            Assert.AreEqual(y.GetField<Object2, int>("Field2"), x.GetField<Object2, int>("Field2"));
        }

        /// <summary>
        /// Simplify get field.
        /// </summary>
        [TestMethod]
        public void TestObjectGetSimplification1()
        {
            var x = Create<Object2>(("Field1", Constant(1)), ("Field2", Constant(2)));
            var y = x.WithField("Field1", Constant(3));
            Assert.AreEqual(y.GetField<Object2, int>("Field1"), Constant(3));
            Assert.AreEqual(y.GetField<Object2, int>("Field2"), x.GetField<Object2, int>("Field2"));
        }

        /// <summary>
        /// Simplify get field.
        /// </summary>
        [TestMethod]
        public void TestObjectGetSimplification2()
        {
            // suppress warning
            Object2Different o = new Object2Different();
            o.Field2 = 0;

            var x = Create<Object2Different>(("Field1", Constant(1)), ("Field2", Constant((short)2)));
            var y = x.WithField("Field1", Constant(3));
            Assert.AreEqual(y.GetField<Object2Different, int>("Field1"), Constant(3));
            Assert.AreEqual(y.GetField<Object2Different, short>("Field2"), x.GetField<Object2Different, short>("Field2"));
        }

        /// <summary>
        /// Simplify get field.
        /// </summary>
        [TestMethod]
        public void TestObjectWithCreateSimplification()
        {
            var x = Create<Object2>(("Field1", Constant(1)), ("Field2", Constant(2)));
            var y = x.WithField("Field1", Constant(3));
            var z = Create<Object2>(("Field1", Constant(3)), ("Field2", Constant(2)));
            Assert.AreEqual(y, z);
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet1()
        {
            Create<Object1>(
                    ("Item1", Constant(0)))
                .GetField<Object1, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet2()
        {
            Create<Object1>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)))
                .GetField<Object1, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet3()
        {
            Create<Object3>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)),
                    ("Item3", Constant(0)))
                .GetField<Object3, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet4()
        {
            Create<Object8>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)),
                    ("Item3", Constant(0)),
                    ("Item4", Constant(0)))
                .GetField<Object8, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet5()
        {
            Create<Object5>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)),
                    ("Item3", Constant(0)),
                    ("Item4", Constant(0)),
                    ("Item5", Constant(0)))
                .GetField<Object5, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet6()
        {
            Create<Object6>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)),
                    ("Item3", Constant(0)),
                    ("Item4", Constant(0)),
                    ("Item5", Constant(0)),
                    ("Item6", Constant(0)))
                .GetField<Object6, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet7()
        {
            Create<Object7>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)),
                    ("Item3", Constant(0)),
                    ("Item4", Constant(0)),
                    ("Item5", Constant(0)),
                    ("Item6", Constant(0)),
                    ("Item7", Constant(0)))
                .GetField<Object7, string>("Foo");
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet8()
        {
            Create<Object8>(
                    ("Item1", Constant(0)),
                    ("Item2", Constant(0)),
                    ("Item3", Constant(0)),
                    ("Item4", Constant(0)),
                    ("Item5", Constant(0)),
                    ("Item6", Constant(0)),
                    ("Item7", Constant(0)),
                    ("Item8", Constant(0)))
                .GetField<Object8, string>("Foo");
        }

        /// <summary>
        /// Exception thrown since implicit conversion won't work with object fields.
        /// </summary>
        [TestMethod]
        public void TestFlyweightWorksForCreate()
        {
            var x = Create<Object2>(("Field1", Constant(0)), ("Field2", Constant(0)));
            var y = Create<Object2>(("Field2", Constant(0)), ("Field1", Constant(0)));
            Assert.AreEqual(x, y);
        }

        /// <summary>
        /// Test hash consing of terms.
        /// </summary>
        [TestMethod]
        public void TestFlyweight()
        {
            Assert.IsTrue(ReferenceEquals(~Constant<int>(10), ~Constant<int>(10)));
            Assert.IsTrue(ReferenceEquals(Constant<int>(10) - Constant<int>(10), Constant<int>(10) - Constant<int>(10)));
            Assert.IsTrue(ReferenceEquals(Constant<int>(10) * Constant<int>(10), Constant<int>(10) * Constant<int>(10)));
            Assert.IsTrue(ReferenceEquals(Option.Create<int>(1), Option.Create<int>(1)));
        }

        /// <summary>
        /// Test hash consing of concat.
        /// </summary>
        [TestMethod]
        public void TestConcatFlyweight()
        {
            var s = Arbitrary<string>();
            var e1 = s + "ll";
            var e2 = s + "ll";
            var e3 = s + "lll";
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
        }

        /// <summary>
        /// Test hash consing of containment.
        /// </summary>
        [TestMethod]
        public void TestContainsFlyweight()
        {
            var s = Arbitrary<string>();
            var e1 = s.Contains("ll");
            var e2 = s.Contains("ll");
            var e3 = s.EndsWith("ll");
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
        }

        /// <summary>
        /// Test hash consing of replacement.
        /// </summary>
        [TestMethod]
        public void TestReplaceFlyweight()
        {
            var s = Arbitrary<string>();
            var e1 = s.ReplaceFirst("xx", "yy");
            var e2 = s.ReplaceFirst("xx", "yy");
            var e3 = s.ReplaceFirst("xx", "zz");
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
        }

        /// <summary>
        /// Test hash consing of substring.
        /// </summary>
        [TestMethod]
        public void TestSubstringFlyweight()
        {
            var s = Arbitrary<string>();
            var e1 = s.Slice(new BigInteger(0), new BigInteger(1));
            var e2 = s.Slice(new BigInteger(0), new BigInteger(1));
            var e3 = s.Slice(new BigInteger(0), new BigInteger(2));
            var e4 = s.Slice(new BigInteger(1), new BigInteger(1));
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
            Assert.IsFalse(ReferenceEquals(e1, e4));
        }

        /// <summary>
        /// Test hash consing of length.
        /// </summary>
        [TestMethod]
        public void TestLengthFlyweight()
        {
            var e1 = Zen.Length("abc");
            var e2 = Zen.Length("abc");
            var e3 = Zen.Length("ab");
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
        }

        /// <summary>
        /// Test hash consing of indexof.
        /// </summary>
        [TestMethod]
        public void TestIndexOfFlyweight()
        {
            var e1 = Zen.IndexOf("abc", "a", new BigInteger(0));
            var e2 = Zen.IndexOf("abc", "a", new BigInteger(0));
            var e3 = Zen.IndexOf("abc", "a", new BigInteger(1));
            var e4 = Zen.IndexOf("abc", "b", new BigInteger(0));
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
            Assert.IsFalse(ReferenceEquals(e1, e4));
        }
    }
}
