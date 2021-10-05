// <copyright file="SimplifierTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

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
        /// Test hash consing of terms.
        /// </summary>
        [TestMethod]
        public void TestHashCons()
        {
            Assert.IsTrue(ReferenceEquals(~Constant<int>(10), ~Constant<int>(10)));
            Assert.IsTrue(ReferenceEquals(Constant<int>(10) - Constant<int>(10), Constant<int>(10) - Constant<int>(10)));
            Assert.IsTrue(ReferenceEquals(Constant<int>(10) * Constant<int>(10), Constant<int>(10) * Constant<int>(10)));
            Assert.IsTrue(ReferenceEquals(Some<int>(1), Some<int>(1)));
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
        /// Simplify And, Or with empty arguments.
        /// </summary>
        [TestMethod]
        public void TestAndOrEmpty()
        {
            Assert.AreEqual(And(new Zen<bool>[] { }), True());
            Assert.AreEqual(Or(new Zen<bool>[] { }), False());
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
            CheckValid<byte>(x => x + 0 == x);
            CheckValid<byte>(x => 0 + x == x);
        }

        /// <summary>
        /// Simplify for subtraction.
        /// </summary>
        [TestMethod]
        public void TestMinusSimplification()
        {
            Assert.AreEqual((Constant<byte>(1) - Constant<byte>(0)), Constant<byte>(1));
            Assert.AreEqual((Constant<BigInteger>(1) - Constant<BigInteger>(0)), Constant<BigInteger>(1));
        }

        /// <summary>
        /// Simplify for multiplication.
        /// </summary>
        [TestMethod]
        public void TestMultiplicationSimplification()
        {
            Assert.AreEqual((Constant<byte>(2) * Constant<byte>(2)), Constant<byte>(4));
            Assert.AreEqual((Constant<BigInteger>(2) * Constant<BigInteger>(2)), Constant<BigInteger>(4));
            CheckValid<byte>(x => x * 1 == x);
            CheckValid<byte>(x => 1 * x == x);
            CheckValid<byte>(x => x * 0 == 0);
            CheckValid<byte>(x => 0 * x == 0);
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
            Assert.AreEqual(Constant("hello") + Constant(""), Constant("hello"));
            Assert.AreEqual(Constant("a") + Constant("hello"), Constant("ahello"));
            CheckValid<string>(x => x + "" == x);
            CheckValid<string>(x => "" + x == x);
        }

        /// <summary>
        /// Simplify for string starts with.
        /// </summary>
        [TestMethod]
        public void TestStartsWithSimplification()
        {
            var s1 = Constant("hello");
            var s2 = Constant("he");
            var s3 = Constant("lo");

            Assert.AreEqual(s1.StartsWith(s2), True());
            Assert.AreEqual(s1.StartsWith(s3), False());
            CheckValid<string>(s => s.StartsWith(""));
            CheckValid<string>(s => Not(Constant("").StartsWith(s)));
        }

        /// <summary>
        /// Simplify for string ends with.
        /// </summary>
        [TestMethod]
        public void TestEndsWithSimplification()
        {
            var s1 = Constant("hello");
            var s2 = Constant("he");
            var s3 = Constant("lo");

            Assert.AreEqual(s1.EndsWith(s2), False());
            Assert.AreEqual(s1.EndsWith(s3), True());
            CheckValid<string>(s => s.EndsWith(""));
            CheckValid<string>(s => Not(Constant("").EndsWith(s)));
        }

        /// <summary>
        /// Simplify for string contains.
        /// </summary>
        [TestMethod]
        public void TestContainsSimplification()
        {
            var s1 = Constant("hello");
            var s2 = Constant("elo");
            var s3 = Constant("ll");

            Assert.AreEqual(s1.Contains(s2), False());
            Assert.AreEqual(s1.Contains(s3), True());
            CheckValid<string>(s => s.Contains(""));
            CheckValid<string>(s => Not(Constant("").Contains(s1)));
        }

        /// <summary>
        /// Simplify for string replace.
        /// </summary>
        [TestMethod]
        public void TestReplaceSimplification()
        {
            var s1 = Constant("hello");
            var s2 = Constant("ll");
            var s3 = Constant("ll");

            Assert.AreEqual(Constant("abc").ReplaceFirst("b", "c"), Constant("acc"));
            Assert.AreEqual(Constant("abc").ReplaceFirst("", "d"), Constant("abcd"));

            CheckValid<string, string>((s1, s2) => s1.ReplaceFirst("", s2) == s1 + s2);
            CheckValid<string, string>((s1, s2) => Constant("").ReplaceFirst(s1, s2) == "");
        }

        /// <summary>
        /// Simplify for string substring.
        /// </summary>
        [TestMethod]
        public void TestSubstringSimplification()
        {
            Assert.AreEqual(Constant("abc").Substring(new BigInteger(0), new BigInteger(1)), Constant("a"));
            Assert.AreEqual(Constant("abc").Substring(new BigInteger(0), new BigInteger(2)), Constant("ab"));
            Assert.AreEqual(Constant("abc").Substring(new BigInteger(0), new BigInteger(3)), Constant("abc"));
            Assert.AreEqual(Constant("abc").Substring(new BigInteger(0), new BigInteger(4)), Constant("abc"));
        }

        /// <summary>
        /// Simplify for string at.
        /// </summary>
        [TestMethod]
        public void TestAtSimplification()
        {
            Assert.AreEqual(Constant("abc").At(new BigInteger(0)), Constant("a"));
            Assert.AreEqual(Constant("abc").At(new BigInteger(1)), Constant("b"));
            Assert.AreEqual(Constant("abc").At(new BigInteger(2)), Constant("c"));
            Assert.AreEqual(Constant("abc").At(new BigInteger(3)), Constant(""));
            var x = Arbitrary<BigInteger>();
            Assert.AreEqual(Constant("").At(x), Constant(""));
        }

        /// <summary>
        /// Simplify for string length.
        /// </summary>
        [TestMethod]
        public void TestLengthSimplification()
        {
            Assert.AreEqual(Constant("a").Length(), Constant<BigInteger>(1));
            Assert.AreEqual(Constant("ab").Length(), Constant<BigInteger>(2));
            Assert.AreEqual(Constant("abc").Length(), Constant<BigInteger>(3));
            Assert.AreEqual(Constant("").Length(), Constant<BigInteger>(0));
        }

        /// <summary>
        /// Simplify for string indexof.
        /// </summary>
        [TestMethod]
        public void TestIndexOfSimplification()
        {
            Assert.AreEqual(Constant("abc").IndexOf(""), Constant<BigInteger>(0));
            Assert.AreEqual(Constant("abc").IndexOf("a"), Constant<BigInteger>(0));
            Assert.AreEqual(Constant("abc").IndexOf("b"), Constant<BigInteger>(1));
            Assert.AreEqual(Constant("abc").IndexOf("c"), Constant<BigInteger>(2));
            Assert.AreEqual(Constant("abc").IndexOf("d"), Constant<BigInteger>(-1));
        }

        /// <summary>
        /// Test hash consing of concat.
        /// </summary>
        [TestMethod]
        public void TestConcatHashCons()
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
        public void TestContainsHashCons()
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
        public void TestReplaceHashCons()
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
        public void TestSubstringHashCons()
        {
            var s = Arbitrary<string>();
            var e1 = s.Substring(new BigInteger(0), new BigInteger(1));
            var e2 = s.Substring(new BigInteger(0), new BigInteger(1));
            var e3 = s.Substring(new BigInteger(0), new BigInteger(2));
            var e4 = s.Substring(new BigInteger(1), new BigInteger(1));
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
            Assert.IsFalse(ReferenceEquals(e1, e4));
        }

        /// <summary>
        /// Test hash consing of substring.
        /// </summary>
        [TestMethod]
        public void TestAtHashCons()
        {
            var s = Arbitrary<string>();
            var e1 = s.At(new BigInteger(0));
            var e2 = s.At(new BigInteger(0));
            var e3 = s.At(new BigInteger(1));
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
        }

        /// <summary>
        /// Test hash consing of length.
        /// </summary>
        [TestMethod]
        public void TestLengthHashCons()
        {
            var e1 = Language.Length("abc");
            var e2 = Language.Length("abc");
            var e3 = Language.Length("ab");
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
        }

        /// <summary>
        /// Test hash consing of indexof.
        /// </summary>
        [TestMethod]
        public void TestIndexOfHashCons()
        {
            var e1 = Language.IndexOf("abc", "a", new BigInteger(0));
            var e2 = Language.IndexOf("abc", "a", new BigInteger(0));
            var e3 = Language.IndexOf("abc", "a", new BigInteger(1));
            var e4 = Language.IndexOf("abc", "b", new BigInteger(0));
            Assert.IsTrue(ReferenceEquals(e1, e2));
            Assert.IsFalse(ReferenceEquals(e1, e3));
            Assert.IsFalse(ReferenceEquals(e1, e4));
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
        public void TestHashconsingWorksForCreate()
        {
            var x = Create<Object2>(("Field1", Constant(0)), ("Field2", Constant(0)));
            var y = Create<Object2>(("Field2", Constant(0)), ("Field1", Constant(0)));
            Console.WriteLine(x.Id);
            Console.WriteLine(y.Id);
            Assert.AreEqual(x, y);
        }
    }
}
