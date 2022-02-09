// <copyright file="ReflectionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZenLib;

    /// <summary>
    /// Tests for reflection.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ReflectionTests
    {
        /// <summary>
        /// Test that creation works for non-public fields when the constructor has matching parameters.
        /// </summary>
        [TestMethod]
        public void TestCreateObjectWithNameParameterConstructor()
        {
            var fields = new string[] { "X", "Y" };
            var values = new object[] { 1, 2 };
            ReflectionUtilities.CreateInstance<Point>(fields, values);
        }

        /// <summary>
        /// Test that creation fails when there is no setter and the parameter names don't match.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCreateObjectWithInvalidConstructor1()
        {
            var fields = new string[] { "X", "Z" };
            var values = new object[] { 1, 2 };
            ReflectionUtilities.CreateInstance<Point>(fields, values);
        }

        /// <summary>
        /// Test that creation fails when there is no setter and the parameter names don't match.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCreateObjectWithInvalidConstructor2()
        {
            var fields = new string[] { "x", "X" };
            var values = new object[] { 1, 2 };
            ReflectionUtilities.CreateInstance<Point2>(fields, values);
        }

        /// <summary>
        /// Test that zen will create the object through a constructor correctly.
        /// </summary>
        [TestMethod]
        public void TestNonPublicFieldsAndProperties()
        {
            var zf = new ZenConstraint<Point>(p => Zen.And(p.GetField<Point, int>("X") == 1, p.GetField<Point, int>("Y") == 2));
            var result = zf.Find();
            Assert.AreEqual(1, result.Value.X);
            Assert.AreEqual(2, result.Value.Y);
        }

        private struct Point
        {
            public int X;

            public int Y { get; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private struct Point2
        {
            public int x;

            public int X { get; }

            public Point2(int x, int X)
            {
                this.x = x;
                this.X = X;
            }
        }
    }
}
