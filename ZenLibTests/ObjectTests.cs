// <copyright file="ObjectTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using ZenLib.Tests.Network;
    using static ZenLib.Language;
    using static ZenLib.Tests.TestHelper;

    /// <summary>
    /// Tests for the Zen object type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ObjectTests
    {
        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject1()
        {
            CheckGetWithForField<Object1>("Field1");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject2()
        {
            CheckGetWithForField<Object2>("Field1");
            CheckGetWithForField<Object2>("Field2");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject3()
        {
            CheckGetWithForField<Object3>("Field1");
            CheckGetWithForField<Object3>("Field2");
            CheckGetWithForField<Object3>("Field3");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject4()
        {
            CheckGetWithForField<Object4>("Field1");
            CheckGetWithForField<Object4>("Field2");
            CheckGetWithForField<Object4>("Field3");
            CheckGetWithForField<Object4>("Field4");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject5()
        {
            CheckGetWithForField<Object5>("Field1");
            CheckGetWithForField<Object5>("Field2");
            CheckGetWithForField<Object5>("Field3");
            CheckGetWithForField<Object5>("Field4");
            CheckGetWithForField<Object5>("Field5");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject6()
        {
            CheckGetWithForField<Object6>("Field1");
            CheckGetWithForField<Object6>("Field2");
            CheckGetWithForField<Object6>("Field3");
            CheckGetWithForField<Object6>("Field4");
            CheckGetWithForField<Object6>("Field5");
            CheckGetWithForField<Object6>("Field6");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject7()
        {
            CheckGetWithForField<Object7>("Field1");
            CheckGetWithForField<Object7>("Field2");
            CheckGetWithForField<Object7>("Field3");
            CheckGetWithForField<Object7>("Field4");
            CheckGetWithForField<Object7>("Field5");
            CheckGetWithForField<Object7>("Field6");
            CheckGetWithForField<Object7>("Field7");
        }

        /// <summary>
        /// Test get and then with.
        /// </summary>
        [TestMethod]
        public void TestGetWithObject8()
        {
            CheckGetWithForField<Object8>("Field1");
            CheckGetWithForField<Object8>("Field2");
            CheckGetWithForField<Object8>("Field3");
            CheckGetWithForField<Object8>("Field4");
            CheckGetWithForField<Object8>("Field5");
            CheckGetWithForField<Object8>("Field6");
            CheckGetWithForField<Object8>("Field7");
            CheckGetWithForField<Object8>("Field8");
        }

        /// <summary>
        /// Check that a get after a with for the
        /// same field returns the same value.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="field">The field.</param>
        private void CheckGetWithForField<T>(string field)
        {
            RandomBytes(x => CheckValid<T>(o => o
                .WithField(field, Constant<int>(x))
                .GetField<T, int>(field) == Constant<int>(x)));
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith1()
        {
            CheckWithIsSet<Object1>("Field1");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith2()
        {
            CheckWithIsSet<Object2>("Field1");
            CheckWithIsSet<Object2>("Field2");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith3()
        {
            CheckWithIsSet<Object3>("Field1");
            CheckWithIsSet<Object3>("Field2");
            CheckWithIsSet<Object3>("Field3");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith4()
        {
            CheckWithIsSet<Object4>("Field1");
            CheckWithIsSet<Object4>("Field2");
            CheckWithIsSet<Object4>("Field3");
            CheckWithIsSet<Object4>("Field4");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith5()
        {
            CheckWithIsSet<Object5>("Field1");
            CheckWithIsSet<Object5>("Field2");
            CheckWithIsSet<Object5>("Field3");
            CheckWithIsSet<Object5>("Field4");
            CheckWithIsSet<Object5>("Field5");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith6()
        {
            CheckWithIsSet<Object6>("Field1");
            CheckWithIsSet<Object6>("Field2");
            CheckWithIsSet<Object6>("Field3");
            CheckWithIsSet<Object6>("Field4");
            CheckWithIsSet<Object6>("Field5");
            CheckWithIsSet<Object6>("Field6");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith7()
        {
            CheckWithIsSet<Object7>("Field1");
            CheckWithIsSet<Object7>("Field2");
            CheckWithIsSet<Object7>("Field3");
            CheckWithIsSet<Object7>("Field4");
            CheckWithIsSet<Object7>("Field5");
            CheckWithIsSet<Object7>("Field6");
            CheckWithIsSet<Object7>("Field7");
        }

        /// <summary>
        /// Test object with is set.
        /// </summary>
        [TestMethod]
        public void TestObjectWith8()
        {
            CheckWithIsSet<Object8>("Field1");
            CheckWithIsSet<Object8>("Field2");
            CheckWithIsSet<Object8>("Field3");
            CheckWithIsSet<Object8>("Field4");
            CheckWithIsSet<Object8>("Field5");
            CheckWithIsSet<Object8>("Field6");
            CheckWithIsSet<Object8>("Field7");
            CheckWithIsSet<Object8>("Field8");
        }

        /// <summary>
        /// Check that a field is set correctly.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="field">The field name.</param>
        private void CheckWithIsSet<T>(string field)
        {
            var f = new ZenFunction<T, T>(o => o.WithField(field, Constant(1)));
            var r = f.Find((i, o) => o.GetField<T, int>(field) == Constant(0));
            Assert.IsFalse(r.HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject1()
        {
            var f1 = new ZenFunction<int, Object1>(i => Create<Object1>(("Field1", Constant(1))));

            var r = f1.Evaluate(0);
            Assert.AreEqual(1, r.Field1);

            Assert.IsFalse(f1.Find((i, o) => o.GetField<Object1, int>("Field1") != Constant(1)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject2()
        {
            var f2 = new ZenFunction<int, Object2>(i => Create<Object2>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2))));

            var r = f2.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            r.Field2 = 0; // avoid dead code error

            Assert.IsFalse(f2.Find((i, o) => o.GetField<Object2, int>("Field2") != Constant(2)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject3()
        {
            var f3 = new ZenFunction<int, Object3>(i => Create<Object3>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3))));

            var r = f3.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            Assert.AreEqual(3, r.Field3);
            r.Field3 = 0; // avoid dead code error

            Assert.IsFalse(f3.Find((i, o) => o.GetField<Object3, int>("Field3") != Constant(3)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject4()
        {
            var f4 = new ZenFunction<int, Object4>(i => Create<Object4>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4))));

            var r = f4.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            Assert.AreEqual(3, r.Field3);
            Assert.AreEqual(4, r.Field4);
            r.Field4 = 0; // avoid dead code

            Assert.IsFalse(f4.Find((i, o) => o.GetField<Object4, int>("Field4") != Constant(4)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject5()
        {
            var f5 = new ZenFunction<int, Object5>(i => Create<Object5>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5))));

            var r = f5.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            Assert.AreEqual(3, r.Field3);
            Assert.AreEqual(4, r.Field4);
            Assert.AreEqual(5, r.Field5);
            r.Field5 = 0; // avoid dead code

            Assert.IsFalse(f5.Find((i, o) => o.GetField<Object5, int>("Field5") != Constant(5)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject6()
        {
            var f6 = new ZenFunction<int, Object6>(i => Create<Object6>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5)),
                ("Field6", Constant(6))));

            var r = f6.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            Assert.AreEqual(3, r.Field3);
            Assert.AreEqual(4, r.Field4);
            Assert.AreEqual(5, r.Field5);
            Assert.AreEqual(6, r.Field6);
            r.Field6 = 0; // avoid dead code error

            Assert.IsFalse(f6.Find((i, o) => o.GetField<Object6, int>("Field6") != Constant(6)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject7()
        {
            var f7 = new ZenFunction<int, Object7>(i => Create<Object7>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5)),
                ("Field6", Constant(6)),
                ("Field7", Constant(7))));

            var r = f7.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            Assert.AreEqual(3, r.Field3);
            Assert.AreEqual(4, r.Field4);
            Assert.AreEqual(5, r.Field5);
            Assert.AreEqual(6, r.Field6);
            Assert.AreEqual(7, r.Field7);
            r.Field7 = 0; // avoid dead code error

            Assert.IsFalse(f7.Find((i, o) => o.GetField<Object7, int>("Field7") != Constant(7)).HasValue);
        }

        /// <summary>
        /// Test creating an object.
        /// </summary>
        [TestMethod]
        public void TestCreateObject8()
        {
            var f8 = new ZenFunction<int, Object8>(i => Create<Object8>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5)),
                ("Field6", Constant(6)),
                ("Field7", Constant(7)),
                ("Field8", Constant(8))));

            var r = f8.Evaluate(0);
            Assert.AreEqual(1, r.Field1);
            Assert.AreEqual(2, r.Field2);
            Assert.AreEqual(3, r.Field3);
            Assert.AreEqual(4, r.Field4);
            Assert.AreEqual(5, r.Field5);
            Assert.AreEqual(6, r.Field6);
            Assert.AreEqual(7, r.Field7);
            Assert.AreEqual(8, r.Field8);
            r.Field8 = 0; // avoid dead code error

            Assert.IsFalse(f8.Find((i, o) => o.GetField<Object8, int>("Field8") != Constant(8)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject1()
        {
            var f = new ZenFunction<int, int>(i => Create<Object1>(
                ("Field1", Constant(1))).GetField<Object1, int>("Field1"));

            Assert.AreEqual(1, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(1)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject2()
        {
            var f = new ZenFunction<int, int>(i => Create<Object2>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2))).GetField<Object2, int>("Field2"));

            Assert.AreEqual(2, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(2)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject3()
        {
            var f = new ZenFunction<int, int>(i => Create<Object3>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3))).GetField<Object3, int>("Field3"));

            Assert.AreEqual(3, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(3)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject4()
        {
            var f = new ZenFunction<int, int>(i => Create<Object4>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4))).GetField<Object4, int>("Field4"));

            Assert.AreEqual(4, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(4)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject5()
        {
            var f = new ZenFunction<int, int>(i => Create<Object5>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5))).GetField<Object5, int>("Field5"));

            Assert.AreEqual(5, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(5)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject6()
        {
            var f = new ZenFunction<int, int>(i => Create<Object6>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5)),
                ("Field6", Constant(6))).GetField<Object6, int>("Field6"));

            Assert.AreEqual(6, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(6)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject7()
        {
            var f = new ZenFunction<int, int>(i => Create<Object7>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5)),
                ("Field6", Constant(6)),
                ("Field7", Constant(7))).GetField<Object7, int>("Field7"));

            Assert.AreEqual(7, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(7)).HasValue);
        }

        /// <summary>
        /// Test get from a created object.
        /// </summary>
        [TestMethod]
        public void TestGetCreateObject8()
        {
            var f = new ZenFunction<int, int>(i => Create<Object8>(
                ("Field1", Constant(1)),
                ("Field2", Constant(2)),
                ("Field3", Constant(3)),
                ("Field4", Constant(4)),
                ("Field5", Constant(5)),
                ("Field6", Constant(6)),
                ("Field7", Constant(7)),
                ("Field8", Constant(8))).GetField<Object8, int>("Field8"));

            Assert.AreEqual(8, f.Evaluate(0));
            Assert.IsTrue(f.Find((i, o) => o == Constant(8)).HasValue);
        }

        /// <summary>
        /// Test an object in an if-then-else.
        /// </summary>
        [TestMethod]
        public void TestObjectIfThenElse()
        {
            var f = new ZenFunction<bool, Object2>(b =>
            {
                var o1 = Create<Object2>(("Field1", Constant(1)), ("Field2", Constant(2)));
                var o2 = Create<Object2>(("Field1", Constant(2)), ("Field2", Constant(1)));
                return If(b, o1, o2);
            });

            var r = f.Find((i, o) => o.GetField<Object2, int>("Field1") == Constant(2));
            Assert.IsTrue(r.HasValue);
            Assert.IsFalse(r.Value);
        }

        /// <summary>
        /// Test an object in an if-then-else.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalIf()
        {
            var f = new ZenFunction<bool, Object2>(b =>
            {
                var o1 = Create<Object2>(("Field1", Constant(1)), ("Field2", Constant(2)));
                var o2 = Create<Object2>(("Field1", Constant(2)), ("Field2", Constant(1)));
                return If(b, o1, o2);
            });

            var o1 = f.Evaluate(true);
            var o2 = f.Evaluate(false);
            f.Compile();
            var o3 = f.Evaluate(true);
            var o4 = f.Evaluate(false);

            Assert.AreEqual(o1.Field1, o3.Field1);
            Assert.AreEqual(o1.Field2, o3.Field2);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate1()
        {
            var f = new ZenFunction<Object1>(() => Create<Object1>(("Field1", Constant(1))));

            Assert.AreEqual(f.Evaluate().Field1, 1);

            f.Compile();
            f.Compile();

            Assert.AreEqual(f.Evaluate().Field1, 1);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate2()
        {
            var f = new ZenFunction<Object2>(() =>
                Create<Object2>(
                    ("Field1", Constant(1)), ("Field2", Constant(2))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate3()
        {
            var f = new ZenFunction<Object3>(() =>
                Create<Object3>(
                    ("Field1", Constant(1)), ("Field2", Constant(2)), ("Field3", Constant(3))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);
            Assert.AreEqual(o1.Field3, 3);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
            Assert.AreEqual(o2.Field3, 3);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate4()
        {
            var f = new ZenFunction<Object4>(() =>
                Create<Object4>(
                    ("Field1", Constant(1)), ("Field2", Constant(2)), ("Field3", Constant(3)), ("Field4", Constant(4))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);
            Assert.AreEqual(o1.Field3, 3);
            Assert.AreEqual(o1.Field4, 4);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
            Assert.AreEqual(o2.Field3, 3);
            Assert.AreEqual(o2.Field4, 4);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate5()
        {
            var f = new ZenFunction<Object5>(() =>
                Create<Object5>(
                    ("Field1", Constant(1)), ("Field2", Constant(2)), ("Field3", Constant(3)), ("Field4", Constant(4)), ("Field5", Constant(5))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);
            Assert.AreEqual(o1.Field3, 3);
            Assert.AreEqual(o1.Field4, 4);
            Assert.AreEqual(o1.Field5, 5);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
            Assert.AreEqual(o2.Field3, 3);
            Assert.AreEqual(o2.Field4, 4);
            Assert.AreEqual(o2.Field5, 5);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate6()
        {
            var f = new ZenFunction<Object6>(() =>
                Create<Object6>(
                    ("Field1", Constant(1)), ("Field2", Constant(2)), ("Field3", Constant(3)),
                    ("Field4", Constant(4)), ("Field5", Constant(5)), ("Field6", Constant(6))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);
            Assert.AreEqual(o1.Field3, 3);
            Assert.AreEqual(o1.Field4, 4);
            Assert.AreEqual(o1.Field5, 5);
            Assert.AreEqual(o1.Field6, 6);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
            Assert.AreEqual(o2.Field3, 3);
            Assert.AreEqual(o2.Field4, 4);
            Assert.AreEqual(o2.Field5, 5);
            Assert.AreEqual(o2.Field6, 6);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate7()
        {
            var f = new ZenFunction<Object7>(() =>
                Create<Object7>(
                    ("Field1", Constant(1)), ("Field2", Constant(2)), ("Field3", Constant(3)),
                    ("Field4", Constant(4)), ("Field5", Constant(5)), ("Field6", Constant(6)),
                    ("Field7", Constant(7))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);
            Assert.AreEqual(o1.Field3, 3);
            Assert.AreEqual(o1.Field4, 4);
            Assert.AreEqual(o1.Field5, 5);
            Assert.AreEqual(o1.Field6, 6);
            Assert.AreEqual(o1.Field7, 7);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
            Assert.AreEqual(o2.Field3, 3);
            Assert.AreEqual(o2.Field4, 4);
            Assert.AreEqual(o2.Field5, 5);
            Assert.AreEqual(o2.Field6, 6);
            Assert.AreEqual(o2.Field7, 7);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate8()
        {
            var f = new ZenFunction<Object8>(() =>
                Create<Object8>(
                    ("Field1", Constant(1)), ("Field2", Constant(2)), ("Field3", Constant(3)),
                    ("Field4", Constant(4)), ("Field5", Constant(5)), ("Field6", Constant(6)),
                    ("Field7", Constant(7)), ("Field8", Constant(8))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            Assert.AreEqual(o1.Field2, 2);
            Assert.AreEqual(o1.Field3, 3);
            Assert.AreEqual(o1.Field4, 4);
            Assert.AreEqual(o1.Field5, 5);
            Assert.AreEqual(o1.Field6, 6);
            Assert.AreEqual(o1.Field7, 7);
            Assert.AreEqual(o1.Field8, 8);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            Assert.AreEqual(o2.Field2, 2);
            Assert.AreEqual(o2.Field3, 3);
            Assert.AreEqual(o2.Field4, 4);
            Assert.AreEqual(o2.Field5, 5);
            Assert.AreEqual(o2.Field6, 6);
            Assert.AreEqual(o2.Field7, 7);
            Assert.AreEqual(o2.Field8, 8);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalWith()
        {
            var f = new ZenFunction<Object2>(() =>
                Create<Object2>(("Field1", Constant(1)), ("Field2", Constant(2)))
                    .WithField("Field1", Constant(3)));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 3);
            Assert.AreEqual(o1.Field2, 2);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 3);
            Assert.AreEqual(o2.Field2, 2);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectCreateArbitrary()
        {
            var f = new ZenFunction<Object1>(() =>
            {
                var b = Arbitrary<bool>();
                var o1 = Create<Object1>(("Field1", Constant(1)));
                var o2 = Create<Object1>(("Field1", Constant(2)));
                return If(b, o1, o2);
            });

            Assert.IsTrue(f.Assert(o => o.GetField<Object1, int>("Field1") <= 2));

            var o1 = f.Evaluate();
            Assert.IsTrue(o1.Field1 == 1 || o1.Field1 == 2);

            f.Compile();
            f.Compile();

            var o2 = f.Evaluate();
            Assert.IsTrue(o2.Field1 == 1 || o2.Field1 == 2);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectEvalCreate1Field()
        {
            var f = new ZenFunction<ObjectField1>(() =>
                Create<ObjectField1>(
                    ("Field1", Constant(1))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            o1.Field1 = 0; // needed to avoid dead code compiler error.
            f.Compile();
            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
        }

        /// <summary>
        /// Test struct create evaluation.
        /// </summary>
        [TestMethod]
        public void TestStructEvalCreate1Field()
        {
            var f = new ZenFunction<StructField1>(() =>
                Create<StructField1>(
                    ("Field1", Constant(1))));

            var o1 = f.Evaluate();
            Assert.AreEqual(o1.Field1, 1);
            o1.Field1 = 0; // needed to avoid dead code compiler error.
            f.Compile();
            var o2 = f.Evaluate();
            Assert.AreEqual(o2.Field1, 1);
            o2.Field1 = 0;
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectWith1Field()
        {
            CheckValid<int, int>((a, b) =>
                Create<ObjectField1>(("Field1", a))
                    .WithField("Field1", b)
                    .GetField<ObjectField1, int>("Field1") == b);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestStructWith1Field()
        {
            CheckValid<int, int>((a, b) =>
                Create<StructField1>(("Field1", a))
                    .WithField("Field1", b)
                    .GetField<StructField1, int>("Field1") == b);
        }

        /// <summary>
        /// Test object create evaluation.
        /// </summary>
        [TestMethod]
        public void TestObjectWith1Field2()
        {
            CheckValid<ObjectField1, int>((o, i) =>
                o.WithField("Field1", i).GetField<ObjectField1, int>("Field1") == i);
        }

        /// <summary>
        /// Test object update with string.
        /// </summary>
        [TestMethod]
        public void TestObjectUpdateStringField()
        {
            // avoid warning with unused field
            StructWithString s;
            s.Field = "";

            var f = new ZenFunction<StructWithString, StructWithString>(o =>
                o.WithField<StructWithString, string>("Field", "hello"));

            var input = new StructWithString { Field = "hola" };
            var output = f.Evaluate(input);
            Assert.IsTrue(output.Field == "hello");
        }

        /// <summary>
        /// Test object agreement.
        /// </summary>
        [TestMethod]
        public void TestObjectAgreement()
        {
            CheckAgreement<ObjectField1, int>((o, i) =>
            {
                var o2 = If(i == 2, o.WithField<ObjectField1, int>("Field1", 2), o.WithField<ObjectField1, int>("Field1", 3));
                return o2.GetField<ObjectField1, int>("Field1") == 3;
            });

            CheckAgreement<ObjectField1>(o => o.GetField<ObjectField1, int>("Field1") == 2);
        }

        /// <summary>
        /// Test encapsulating a packet.
        /// </summary>
        [TestMethod]
        public void TestPacketEncapsulation()
        {
            var encap = new ZenFunction<IList<IpHeader>, IList<IpHeader>>(headers =>
            {
                var currentHeader = headers.At(0);
                var newHeader = currentHeader.Value().WithField("DstIp", Ip.Create(5));
                var newHeaders = headers.AddFront(newHeader);
                return newHeaders;
            });

            var input = encap.Find((i, o) => o.At(0).Value().GetDstIp().GetValue() == 5);
            Assert.IsTrue(input.HasValue);

            var o = encap.Evaluate(input.Value);
            Assert.IsTrue(o.Count >= 1);
            Assert.AreEqual(5U, o.First().DstIp.Value);
        }

        /// <summary>
        /// Test matrix addition.
        /// </summary>
        [TestMethod]
        public void TestMatrixAdd()
        {
            var function = new ZenFunction<Matrix3x3, Matrix3x3, Matrix3x3>(Matrix3x3.Add);
            var input = function.Find((x, y, result) => result.GetField<Matrix3x3, int>("v22") == 10);
            var x = input.Value.Item1;
            var y = input.Value.Item2;
            Assert.AreEqual(x.v22 + y.v22, 10);

            x.v11 = 1; x.v12 = 2; x.v13 = 3;
            x.v21 = 4; x.v22 = 5; x.v23 = 6;
            x.v31 = 7; x.v32 = 8; x.v33 = 9;

            y.v11 = 1; y.v12 = 2; y.v13 = 3;
            y.v21 = 4; y.v22 = 5; y.v23 = 6;
            y.v31 = 7; y.v32 = 8; y.v33 = 9;

            Assert.AreEqual(2, function.Evaluate(x, y).v11);
            Assert.AreEqual(4, function.Evaluate(x, y).v12);
            Assert.AreEqual(6, function.Evaluate(x, y).v13);
            Assert.AreEqual(8, function.Evaluate(x, y).v21);
            Assert.AreEqual(10, function.Evaluate(x, y).v22);
            Assert.AreEqual(12, function.Evaluate(x, y).v23);
            Assert.AreEqual(14, function.Evaluate(x, y).v31);
            Assert.AreEqual(16, function.Evaluate(x, y).v32);
            Assert.AreEqual(18, function.Evaluate(x, y).v33);
        }

        private struct StructField1
        {
            public int Field1;
        }

        private struct StructWithString
        {
            public string Field;
        }

        private class Matrix3x3
        {
            public int v11, v12, v13;
            public int v21, v22, v23;
            public int v31, v32, v33;

            public static Zen<Matrix3x3> Add(Zen<Matrix3x3> x, Zen<Matrix3x3> y)
            {
                return Create<Matrix3x3>(
                    ("v11", x.GetField<Matrix3x3, int>("v11") + y.GetField<Matrix3x3, int>("v11")),
                    ("v12", x.GetField<Matrix3x3, int>("v12") + y.GetField<Matrix3x3, int>("v12")),
                    ("v13", x.GetField<Matrix3x3, int>("v13") + y.GetField<Matrix3x3, int>("v13")),
                    ("v21", x.GetField<Matrix3x3, int>("v21") + y.GetField<Matrix3x3, int>("v21")),
                    ("v22", x.GetField<Matrix3x3, int>("v22") + y.GetField<Matrix3x3, int>("v22")),
                    ("v23", x.GetField<Matrix3x3, int>("v23") + y.GetField<Matrix3x3, int>("v23")),
                    ("v31", x.GetField<Matrix3x3, int>("v31") + y.GetField<Matrix3x3, int>("v31")),
                    ("v32", x.GetField<Matrix3x3, int>("v32") + y.GetField<Matrix3x3, int>("v32")),
                    ("v33", x.GetField<Matrix3x3, int>("v33") + y.GetField<Matrix3x3, int>("v33")));
            }
        }
    }
}
