// <copyright file="SimplifierTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Diagnostics.CodeAnalysis;
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
            Assert.AreEqual((~~Byte(3)), Byte(3));
            Assert.AreEqual((~~Short(3)), Short(3));
            Assert.AreEqual((~~UShort(3)), UShort(3));
            Assert.AreEqual((~~Int(3)), Int(3));
            Assert.AreEqual((~~UInt(3)), UInt(3));
            Assert.AreEqual((~~Long(3)), Long(3));
            Assert.AreEqual((~~ULong(3)), ULong(3));
        }

        /// <summary>
        /// Test hash consing of terms.
        /// </summary>
        [TestMethod]
        public void TestHashCons()
        {
            Assert.IsTrue(ReferenceEquals(~Int(10), ~Int(10)));
            Assert.IsTrue(ReferenceEquals(Int(10) - Int(10), Int(10) - Int(10)));
            Assert.IsTrue(ReferenceEquals(Int(10) * Int(10), Int(10) * Int(10)));
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
            Assert.AreEqual((Byte(1) & Byte(1)), Byte(1));
            Assert.AreEqual((Short(1) & Short(1)), Short(1));
            Assert.AreEqual((UShort(1) & UShort(1)), UShort(1));
            Assert.AreEqual((Int(1) & Int(1)), Int(1));
            Assert.AreEqual((UInt(1) & UInt(1)), UInt(1));
            Assert.AreEqual((Long(1) & Long(1)), Long(1));
            Assert.AreEqual((ULong(1) & ULong(1)), ULong(1));
        }

        /// <summary>
        /// Simplify less than or equal.
        /// </summary>
        [TestMethod]
        public void TestLeqSimplification()
        {
            Assert.AreEqual((Byte(1) <= Byte(1)), True());
            Assert.AreEqual((Byte(1) <= Byte(0)), False());
            Assert.AreEqual((Short(1) <= Short(1)), True());
            Assert.AreEqual((Short(1) <= Short(0)), False());
            Assert.AreEqual((UShort(1) <= UShort(1)), True());
            Assert.AreEqual((UShort(1) <= UShort(0)), False());
            Assert.AreEqual((Int(1) <= Int(1)), True());
            Assert.AreEqual((Int(1) <= Int(0)), False());
            Assert.AreEqual((UInt(1) <= UInt(1)), True());
            Assert.AreEqual((UInt(1) <= UInt(0)), False());
            Assert.AreEqual((Long(1) <= Long(1)), True());
            Assert.AreEqual((Long(1) <= Long(0)), False());
            Assert.AreEqual((ULong(1) <= ULong(1)), True());
            Assert.AreEqual((ULong(1) <= ULong(0)), False());
        }

        /// <summary>
        /// Simplify greater than or equal.
        /// </summary>
        [TestMethod]
        public void TestGeqSimplification()
        {
            Assert.AreEqual((Byte(1) >= Byte(1)), True());
            Assert.AreEqual((Byte(0) >= Byte(1)), False());
            Assert.AreEqual((Short(1) >= Short(1)), True());
            Assert.AreEqual((Short(0) >= Short(1)), False());
            Assert.AreEqual((UShort(1) >= UShort(1)), True());
            Assert.AreEqual((UShort(0) >= UShort(1)), False());
            Assert.AreEqual((Int(1) >= Int(1)), True());
            Assert.AreEqual((Int(0) >= Int(1)), False());
            Assert.AreEqual((UInt(1) >= UInt(1)), True());
            Assert.AreEqual((UInt(0) >= UInt(1)), False());
            Assert.AreEqual((Long(1) >= Long(1)), True());
            Assert.AreEqual((Long(0) >= Long(1)), False());
            Assert.AreEqual((ULong(1) >= ULong(1)), True());
            Assert.AreEqual((ULong(0) >= ULong(1)), False());
        }

        /// <summary>
        /// Simplify for addition.
        /// </summary>
        [TestMethod]
        public void TestAdditionSimplification()
        {
            Assert.AreEqual((Byte(1) + Byte(0)), Byte(1));
            Assert.AreEqual((Byte(0) + Byte(1)), Byte(1));
            CheckValid<byte>(x => x + 0 == x);
            CheckValid<byte>(x => 0 + x == x);
        }

        /// <summary>
        /// Simplify for subtraction.
        /// </summary>
        [TestMethod]
        public void TestMinusSimplification()
        {
            Assert.AreEqual((Byte(1) - Byte(0)), Byte(1));
        }

        /// <summary>
        /// Simplify for multiplication.
        /// </summary>
        [TestMethod]
        public void TestMultiplicationSimplification()
        {
            Assert.AreEqual((Byte(2) * Byte(2)), Byte(4));
            CheckValid<byte>(x => x * 1 == x);
            CheckValid<byte>(x => 1 * x == x);
            CheckValid<byte>(x => x * 0 == 0);
            CheckValid<byte>(x => 0 * x == 0);
        }

        /// <summary>
        /// Simplify for concatenaion.
        /// </summary>
        [TestMethod]
        public void TestConcatenationSimplification()
        {
            Assert.AreEqual(String("hello") + String(""), String("hello"));
            Assert.AreEqual(String("a") + String("hello"), String("ahello"));
            CheckValid<string>(x => x + "" == x);
            CheckValid<string>(x => "" + x == x);
        }

        /// <summary>
        /// Simplify for string starts with.
        /// </summary>
        [TestMethod]
        public void TestStartsWithSimplification()
        {
            var s1 = String("hello");
            var s2 = String("he");
            var s3 = String("lo");

            Assert.AreEqual(s1.StartsWith(s2), True());
            Assert.AreEqual(s1.StartsWith(s3), False());
            CheckValid<string>(s => s.StartsWith(""));
            CheckValid<string>(s => Not(String("").StartsWith(s)));
        }

        /// <summary>
        /// Simplify for string ends with.
        /// </summary>
        [TestMethod]
        public void TestEndsWithSimplification()
        {
            var s1 = String("hello");
            var s2 = String("he");
            var s3 = String("lo");

            Assert.AreEqual(s1.EndsWith(s2), False());
            Assert.AreEqual(s1.EndsWith(s3), True());
            CheckValid<string>(s => s.EndsWith(""));
            CheckValid<string>(s => Not(String("").EndsWith(s)));
        }

        /// <summary>
        /// Simplify for string contains.
        /// </summary>
        [TestMethod]
        public void TestContainsSimplification()
        {
            var s1 = String("hello");
            var s2 = String("elo");
            var s3 = String("ll");

            Assert.AreEqual(s1.Contains(s2), False());
            Assert.AreEqual(s1.Contains(s3), True());
            CheckValid<string>(s => s.Contains(""));
            CheckValid<string>(s => Not(String("").Contains(s1)));
        }

        /// <summary>
        /// Simplify for string replace.
        /// </summary>
        [TestMethod]
        public void TestReplaceSimplification()
        {
            var s1 = String("hello");
            var s2 = String("ll");
            var s3 = String("ll");

            Assert.AreEqual(String("abc").ReplaceFirst("b", "c"), String("acc"));
            Assert.AreEqual(String("abc").ReplaceFirst("", "d"), String("abcd"));

            CheckValid<string, string>((s1, s2) => s1.ReplaceFirst("", s2) == s1 + s2);
            CheckValid<string, string>((s1, s2) => String("").ReplaceFirst(s1, s2) == "");
        }

        /// <summary>
        /// Simplify for string substring.
        /// </summary>
        [TestMethod]
        public void TestSubstringSimplification()
        {
            Assert.AreEqual(String("abc").Substring(0, 1), String("a"));
            Assert.AreEqual(String("abc").Substring(0, 2), String("ab"));
            Assert.AreEqual(String("abc").Substring(0, 3), String("abc"));
            Assert.AreEqual(String("abc").Substring(0, 4), String("abc"));
        }

        /// <summary>
        /// Simplify for string at.
        /// </summary>
        [TestMethod]
        public void TestAtSimplification()
        {
            Assert.AreEqual(String("abc").At(0), String("a"));
            Assert.AreEqual(String("abc").At(1), String("b"));
            Assert.AreEqual(String("abc").At(2), String("c"));
            Assert.AreEqual(String("abc").At(3), String(""));
            var x = Arbitrary<ushort>();
            Assert.AreEqual(String("").At(x), String(""));
        }

        /// <summary>
        /// Simplify for string length.
        /// </summary>
        [TestMethod]
        public void TestLengthSimplification()
        {
            Assert.AreEqual(String("a").Length(), UShort(1));
            Assert.AreEqual(String("ab").Length(), UShort(2));
            Assert.AreEqual(String("abc").Length(), UShort(3));
            Assert.AreEqual(String("").Length(), UShort(0));
        }

        /// <summary>
        /// Simplify for string indexof.
        /// </summary>
        [TestMethod]
        public void TestIndexOfSimplification()
        {
            Assert.AreEqual(String("abc").IndexOf(""), Short(0));
            Assert.AreEqual(String("abc").IndexOf("a"), Short(0));
            Assert.AreEqual(String("abc").IndexOf("b"), Short(1));
            Assert.AreEqual(String("abc").IndexOf("c"), Short(2));
            Assert.AreEqual(String("abc").IndexOf("d"), Short(-1));
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
            var e1 = s.Substring(0, 1);
            var e2 = s.Substring(0, 1);
            var e3 = s.Substring(0, 2);
            var e4 = s.Substring(1, 1);
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
            var e1 = s.At(0);
            var e2 = s.At(0);
            var e3 = s.At(1);
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
            var e1 = Language.IndexOf("abc", "a", 0);
            var e2 = Language.IndexOf("abc", "a", 0);
            var e3 = Language.IndexOf("abc", "a", 1);
            var e4 = Language.IndexOf("abc", "b", 0);
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
            var x = Int(1);
            var y = Int(2);
            var b = Arbitrary<bool>();
            Assert.AreEqual(If(true, x, y), x);
            Assert.AreEqual(If(false, x, y), y);
            Assert.AreEqual(If(x == 0, true, b), Or(x == 0, b));
            Assert.AreEqual(If(x == 0, false, b), And(Not(x == 0), b));
            Assert.AreEqual(If(x == 0, b, true), Or(Not(x == 0), b));
            Assert.AreEqual(If(x == 0, b, false), And(x == 0, b));
        }

        /// <summary>
        /// Simplify object get throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldGet1()
        {
            Create<Object1>(
                    ("Item1", 0))
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
                    ("Item1", 0),
                    ("Item2", 0))
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
                    ("Item1", 0),
                    ("Item2", 0),
                    ("Item3", 0))
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
                    ("Item1", 0),
                    ("Item2", 0),
                    ("Item3", 0),
                    ("Item4", 0))
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
                    ("Item1", 0),
                    ("Item2", 0),
                    ("Item3", 0),
                    ("Item4", 0),
                    ("Item5", 0))
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
                    ("Item1", 0),
                    ("Item2", 0),
                    ("Item3", 0),
                    ("Item4", 0),
                    ("Item5", 0),
                    ("Item6", 0))
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
                    ("Item1", 0),
                    ("Item2", 0),
                    ("Item3", 0),
                    ("Item4", 0),
                    ("Item5", 0),
                    ("Item6", 0),
                    ("Item7", 0))
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
                    ("Item1", 0),
                    ("Item2", 0),
                    ("Item3", 0),
                    ("Item4", 0),
                    ("Item5", 0),
                    ("Item6", 0),
                    ("Item7", 0),
                    ("Item8", 0))
                .GetField<Object8, string>("Foo");
        }

        /// <summary>
        /// Exception thrown since implicit conversion won't work with object fields.
        /// </summary>
        [TestMethod]
        public void TestHashconsingWorksForCreate()
        {
            var x = Create<Object2>(("Field1", Int(0)), ("Field2", Int(0)));
            var y = Create<Object2>(("Field2", Int(0)), ("Field1", Int(0)));
            Assert.AreEqual(x, y);
        }
    }
}
