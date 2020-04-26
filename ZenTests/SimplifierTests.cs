// <copyright file="SimplifierTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Research.Zen;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Microsoft.Research.Zen.Language;
    using static Microsoft.Research.ZenTests.TestHelper;

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
            Assert.AreEqual((~~Byte(3)).Simplify(), Byte(3));
            Assert.AreEqual((~~Short(3)).Simplify(), Short(3));
            Assert.AreEqual((~~UShort(3)).Simplify(), UShort(3));
            Assert.AreEqual((~~Int(3)).Simplify(), Int(3));
            Assert.AreEqual((~~UInt(3)).Simplify(), UInt(3));
            Assert.AreEqual((~~Long(3)).Simplify(), Long(3));
            Assert.AreEqual((~~ULong(3)).Simplify(), ULong(3));
        }

        /// <summary>
        /// Simplify and with constants.
        /// </summary>
        [TestMethod]
        public void TestAndConstant()
        {
            Assert.AreEqual(And(true, true).Simplify(), true);
            Assert.AreEqual(And(true, false).Simplify(), false);
            Assert.AreEqual(And(false, true).Simplify(), false);
            Assert.AreEqual(And(false, false).Simplify(), false);
        }

        /// <summary>
        /// Simplify or with constants.
        /// </summary>
        [TestMethod]
        public void TestOrConstant()
        {
            Assert.AreEqual(Or(true, true).Simplify(), true);
            Assert.AreEqual(Or(true, false).Simplify(), true);
            Assert.AreEqual(Or(false, true).Simplify(), true);
            Assert.AreEqual(Or(false, false).Simplify(), false);

            var x = Arbitrary<bool>();
            Assert.AreEqual(Or(x, true).Simplify(), true);
            Assert.AreEqual(Or(x, false).Simplify(), x);
            Assert.AreEqual(Or(false, x).Simplify(), x);
            Assert.AreEqual(Or(true, x).Simplify(), true);
        }

        /// <summary>
        /// Simplify not with constants.
        /// </summary>
        [TestMethod]
        public void TestNotConstant()
        {
            Assert.AreEqual(Not(true).Simplify(), false);
            Assert.AreEqual(Not(false).Simplify(), true);
        }

        /// <summary>
        /// Simplify bitwise and with constants.
        /// </summary>
        [TestMethod]
        public void TestBitwiseAndConstant()
        {
            Assert.AreEqual((Byte(1) & Byte(1)).Simplify(), Byte(1));
            Assert.AreEqual((Short(1) & Short(1)).Simplify(), Short(1));
            Assert.AreEqual((UShort(1) & UShort(1)).Simplify(), UShort(1));
            Assert.AreEqual((Int(1) & Int(1)).Simplify(), Int(1));
            Assert.AreEqual((UInt(1) & UInt(1)).Simplify(), UInt(1));
            Assert.AreEqual((Long(1) & Long(1)).Simplify(), Long(1));
            Assert.AreEqual((ULong(1) & ULong(1)).Simplify(), ULong(1));
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
                .GetField<Object1, string>("Foo")
                .Simplify();
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
                .GetField<Object1, string>("Foo")
                .Simplify();
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
                .GetField<Object3, string>("Foo")
                .Simplify();
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
                .GetField<Object8, string>("Foo")
                .Simplify();
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
                .GetField<Object5, string>("Foo")
                .Simplify();
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
                .GetField<Object6, string>("Foo")
                .Simplify();
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
                .GetField<Object7, string>("Foo")
                .Simplify();
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
                .GetField<Object8, string>("Foo")
                .Simplify();
        }

        /// <summary>
        /// Exception thrown since implicit conversion won't work with object fields.
        /// </summary>
        [TestMethod]
        public void TestHashconsingWorksForCreate()
        {
            var x = Language.Create<Object2>(("Field1", Int(0)), ("Field2", Int(0)));
            var y = Language.Create<Object2>(("Field2", Int(0)), ("Field1", Int(0)));
            Assert.AreEqual(x, y);
        }
    }
}