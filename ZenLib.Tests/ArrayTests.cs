// <copyright file="ArrayTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;
    using static ZenLib.Tests.TestHelper;
    using static ZenLib.Zen;

    /// <summary>
    /// Tests for the Zen Array type.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ArrayTests
    {
        /// <summary>
        /// Test array get and set work.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TypeInitializationException))]
        public void TestArrayTypeException()
        {
            new Array<int, int>();
        }

        /// <summary>
        /// Test initialization fails.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestArraySizeInitException()
        {
            new Array<int, _5>(1, 2);
        }

        /// <summary>
        /// Test array initialization works.
        /// </summary>
        [TestMethod]
        public void TestArraySizeInitNoException1()
        {
            new Array<int, _5>(1, 2, 3, 4, 5);
        }

        /// <summary>
        /// Test array get and set work.
        /// </summary>
        [TestMethod]
        public void TestArraySizeInitNoException2()
        {
            new Array<int, _5>(new List<int> { 1, 2, 3, 4, 5 });
        }

        /// <summary>
        /// Test array length works.
        /// </summary>
        [TestMethod]
        public void TestArrayLength()
        {
            Assert.AreEqual(5, new Array<int, _5>().Length());
        }

        /// <summary>
        /// Test array get works.
        /// </summary>
        [TestMethod]
        public void TestArrayGet()
        {
            var a1 = new Array<int, _5>(0, 1, 2, 3, 4);

            Assert.AreEqual(0, a1.Get(0));
            Assert.AreEqual(1, a1.Get(1));
            Assert.AreEqual(2, a1.Get(2));
            Assert.AreEqual(3, a1.Get(3));
            Assert.AreEqual(4, a1.Get(4));
        }

        /// <summary>
        /// Test array set works.
        /// </summary>
        [TestMethod]
        public void TestArraySet()
        {
            var a1 = new Array<int, _5>(0, 1, 2, 3, 4);
            var a2 = a1.Set(0, 6).Set(3, 9);

            Assert.AreEqual(6, a2.Get(0));
            Assert.AreEqual(1, a2.Get(1));
            Assert.AreEqual(2, a2.Get(2));
            Assert.AreEqual(9, a2.Get(3));
            Assert.AreEqual(4, a2.Get(4));
        }

        /// <summary>
        /// Test array set only works with valid indices.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestArraySetException1()
        {
            var a = new Array<int, _5>(0, 1, 2, 3, 4);
            a.Set(-1, 3);
        }

        /// <summary>
        /// Test array set only works with valid indices.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestArraySetException2()
        {
            var a = new Array<int, _5>(0, 1, 2, 3, 4);
            a.Set(5, 3);
        }

        /// <summary>
        /// Test array equality and hashcode works.
        /// </summary>
        [TestMethod]
        public void TestArrayEqualityAndHashing()
        {
            var a1 = new Array<int, _5>(0, 1, 2, 3, 4);
            var a2 = new Array<int, _5>(0, 1, 2, 3, 4);
            var a3 = new Array<int, _5>(0, 1, 2, 3, 5);
            var a4 = new Array<int, _4>(0, 1, 2, 3);

            Assert.IsTrue(a1 == a2);
            Assert.IsTrue(a1 != a3);
            Assert.IsTrue(!a1.Equals(a4));
            Assert.IsTrue(!a1.Equals(new object()));
            Assert.IsTrue(a1.Equals((object)a2));
            Assert.AreEqual(a1.GetHashCode(), a2.GetHashCode());
            Assert.AreNotEqual(a1.GetHashCode(), a3.GetHashCode());
        }

        /// <summary>
        /// Test array conversions.
        /// </summary>
        [TestMethod]
        public void TestArrayConversion()
        {
            var a = new Array<int, _5>(0, 1, 2, 3, 4).ToArray();

            Assert.AreEqual(5, a.Length);
            Assert.AreEqual(0, a[0]);
            Assert.AreEqual(1, a[1]);
            Assert.AreEqual(2, a[2]);
            Assert.AreEqual(3, a[3]);
            Assert.AreEqual(4, a[4]);
        }

        /// <summary>
        /// Test array out of bounds in Zen.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestArrayIndexException1()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            (a.Get(-1) == 1).Solve();
        }

        /// <summary>
        /// Test array out of bounds in Zen.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestArrayIndexException2()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            (a.Get(4) == 1).Solve();
        }

        /// <summary>
        /// Test array get solving.
        /// </summary>
        [TestMethod]
        public void TestArrayGetSolve()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            var sol = And(a.Get(1) == 1, a.Get(2) == 2).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            var res = sol.Get(a).ToArray();
            Assert.AreEqual(1, res[1]);
            Assert.AreEqual(2, res[2]);
        }

        /// <summary>
        /// Test array set solving.
        /// </summary>
        [TestMethod]
        public void TestArraySetSolve()
        {
            var b = Zen.Symbolic<bool>();
            var a1 = Zen.Symbolic<Array<int, _4>>();
            var a2 = Zen.Symbolic<Array<int, _4>>();
            var sol = And(a1.Get(1) == 1, a2.Get(1) == 1, Zen.If(b, a1.Set(1, 99), a2.Set(1, 9)).Get(1) == 99).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            Assert.IsTrue(sol.Get(b));
            Assert.AreEqual(1, sol.Get(a1).Get(1));
            Assert.AreEqual(1, sol.Get(a2).Get(1));
        }

        /// <summary>
        /// Test array length works with Zen objects.
        /// </summary>
        [TestMethod]
        public void TestArrayLengthZen()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            Assert.AreEqual(4, a.Length());
        }

        /// <summary>
        /// Test array equality solving.
        /// </summary>
        [TestMethod]
        public void TestArrayEqualitySelectSolve()
        {
            var a1 = Zen.Symbolic<Array<int, _4>>();
            var a2 = Zen.Symbolic<Array<int, _4>>();
            var sol = (a2 == a1.Select(x => x + 1)).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            var x1 = sol.Get(a1).ToArray();
            var x2 = sol.Get(a2).ToArray();
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(x2[i], x1[i] + 1);
            }
        }

        /// <summary>
        /// Test array inequality solving.
        /// </summary>
        [TestMethod]
        public void TestArrayInequalitySolve()
        {
            var a1 = Zen.Symbolic<Array<int, _2>>();
            var a2 = Zen.Symbolic<Array<int, _2>>();
            var sol = And(a1 != a2, a1.Get(0) == a2.Get(0), a1.Get(1) == a2.Get(1)).Solve();
            Assert.IsFalse(sol.IsSatisfiable());
        }

        /// <summary>
        /// Test array any solving.
        /// </summary>
        [TestMethod]
        public void TestArrayAnySolve()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            var sol = a.Any(x => x == 99).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            Assert.IsTrue(sol.Get(a).ToArray().Contains(99));
        }

        /// <summary>
        /// Test array all solving.
        /// </summary>
        [TestMethod]
        public void TestArrayAllSolve()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            var sol = a.All(x => x == 99).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            Assert.IsTrue(sol.Get(a).ToArray().All(x => x == 99));
        }

        /// <summary>
        /// Test array to C# array solving.
        /// </summary>
        [TestMethod]
        public void TestArrayToArray()
        {
            var a = Zen.Symbolic<Array<int, _4>>();
            var elements = a.ToArray();
            var sol = (elements.Aggregate((x, y) => x + y) == 10).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            Assert.AreEqual(10, sol.Get(a).ToArray().Sum());
        }

        /// <summary>
        /// Test array in an object.
        /// </summary>
        [TestMethod]
        public void TestArrayInObject()
        {
            var a = Zen.Symbolic<TestArray>();
            var f = a.GetField<TestArray, Array<string, _3>>("Array");
            var sol = (f.All(x => x == "a")).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            Assert.IsTrue(sol.Get(a).Array.Length() == 3);
            Assert.IsTrue(sol.Get(a).Array.ToArray().All(x => x == "a"));
        }

        /// <summary>
        /// Test array in an array.
        /// </summary>
        [TestMethod]
        public void TestArrayInArray()
        {
            var a = Zen.Symbolic<Array<Array<int, _2>, _2>>();
            var sol = a.All(a2 => a2.All(x => x == 9)).Solve();

            Assert.IsTrue(sol.IsSatisfiable());
            var elts = sol.Get(a).ToArray().Select(x => x.ToArray()).ToArray();
            Assert.IsTrue(elts.All(a2 => a2.All(x => x == 9)));
        }
    }

    /// <summary>
    /// Class that holds an array.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestArray
    {
        /// <summary>
        /// An array field.
        /// </summary>
        public Array<string, _3> Array { get; set; }
    }
}