// <copyright file="UsageTests.cs" company="Microsoft">
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
    /// Test for throwing exceptions when library is not used correctly.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UsageTests
    {
        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddBackException1()
        {
            FSeq.AddBack(null, Constant(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddBackException2()
        {
            FSeq.AddBackOption(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddFrontException1()
        {
            FSeq.AddFront(null, Constant(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAddFrontException2()
        {
            FSeq.AddFrontOption(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAllException1()
        {
            FSeq.All<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAllException2()
        {
            FSeq.All(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAnyException1()
        {
            FSeq.Any<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAnyException2()
        {
            FSeq.Any(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAndException1()
        {
            Zen.And(null, true);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAndException2()
        {
            Zen.And(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAppendException1()
        {
            FSeq.Append(null, FSeq.Empty<int>());
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAppendException2()
        {
            FSeq.Append(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAtException1()
        {
            FSeq.At<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestAtException2()
        {
            FSeq.At(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for adding big integers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseBigIntegerException()
        {
            Zen.BitwiseAnd<BigInteger>(new BigInteger(0), new BigInteger(1));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseAndException1()
        {
            Zen.BitwiseAnd<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseAndException2()
        {
            Zen.BitwiseAnd<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseNotException()
        {
            Zen.BitwiseNot<byte>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseOrException1()
        {
            Zen.BitwiseOr<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseOrException2()
        {
            Zen.BitwiseOr<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseXorException1()
        {
            Zen.BitwiseXor<byte>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestBitwiseXorException2()
        {
            Zen.BitwiseXor<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestContainsException1()
        {
            FSeq.Contains<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestContainsException2()
        {
            FSeq.Contains(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropException1()
        {
            FSeq.Drop<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropException2()
        {
            FSeq.Drop(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropWhileException1()
        {
            FSeq.DropWhile<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDropWhileException2()
        {
            FSeq.DropWhile(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDuplicatesException1()
        {
            FSeq.Duplicates<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestDuplicatesException2()
        {
            FSeq.Duplicates(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEqException1()
        {
            Zen.Eq<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestEqException2()
        {
            Zen.Eq<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFindException1()
        {
            FSeq.Find<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFindException2()
        {
            FSeq.Find(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFoldException1()
        {
            FSeq.Fold<int, int>(null, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFoldException2()
        {
            FSeq.Fold<int, int>(FSeq.Empty<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFoldException3()
        {
            FSeq.Fold<int, int>(FSeq.Empty<int>(), 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestFunctionException()
        {
            new ZenFunction<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGeqException1()
        {
            Zen.Geq<byte>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGeqException2()
        {
            Zen.Geq<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGetFieldException1()
        {
            Zen.GetField<Object1, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGetFieldException2()
        {
            Zen.GetField<Object1, int>(Zen.Create<Object1>(("Field1", Constant(0))), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGtException1()
        {
            Zen.Gt<byte>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestGtException2()
        {
            Zen.Gt<byte>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestHasValueException()
        {
            Option.IsSome<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIfException1()
        {
            Zen.If<int>(null, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIfException2()
        {
            Zen.If<int>(true, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIfException3()
        {
            Zen.If<int>(true, 3, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestImpliesException1()
        {
            Zen.Implies(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestImpliesException2()
        {
            Zen.Implies(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIndexOfException1()
        {
            FSeq.IndexOf<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIndexOfException2()
        {
            FSeq.IndexOf(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestIsEmptyException1()
        {
            FSeq.IsEmpty<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception1()
        {
            Zen<Pair<int, int>> x = null;
            Pair.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception2()
        {
            Zen<Pair<int, int, int>> x = null;
            Pair.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception3()
        {
            Zen<Pair<int, int, int, int>> x = null;
            Pair.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem1Exception4()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            Pair.Item1(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception1()
        {
            Zen<Pair<int, int>> x = null;
            Pair.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception2()
        {
            Zen<Pair<int, int, int>> x = null;
            Pair.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception3()
        {
            Zen<Pair<int, int, int, int>> x = null;
            Pair.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem2Exception4()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            Pair.Item2(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem3Exception1()
        {
            Zen<Pair<int, int, int>> x = null;
            Pair.Item3(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem3Exception2()
        {
            Zen<Pair<int, int, int, int>> x = null;
            Pair.Item3(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem3Exception3()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            Pair.Item3(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem4Exception1()
        {
            Zen<Pair<int, int, int, int>> x = null;
            Pair.Item4(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem4Exception2()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            Pair.Item4(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestItem5Exception1()
        {
            Zen<Pair<int, int, int, int, int>> x = null;
            Pair.Item5(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLengthException()
        {
            FSeq.Length<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLeqException1()
        {
            Zen.Leq<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLeqException2()
        {
            Zen.Leq<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSeqException()
        {
            Zen<int>[] x = null;
            FSeq.Create(x);
        }

        /// <summary>
        /// No exception thrown for non-null parameter.
        /// </summary>
        [TestMethod]
        public void TestSeqNoException()
        {
            Zen<int>[] x = new Zen<int>[] { 1, 2 };
            FSeq.Create(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLtException1()
        {
            Zen.Lt<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestLtException2()
        {
            Zen.Lt<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseOptionException1()
        {
            Zen.Case<int, int>(null, null, (Func<Zen<int>, Zen<int>>)null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseOptionException2()
        {
            Zen.Case<int, int>(Option.Null<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseOptionException3()
        {
            Zen.Case<int, int>(Option.Null<int>(), () => 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseSeqException1()
        {
            Zen<FSeq<int>> x = null;
            FSeq.Case<int, int>(x, null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseSeqException2()
        {
            FSeq.Case<int, int>(FSeq.Empty<int>(), null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCaseSeqException3()
        {
            FSeq.Case<int, int>(FSeq.Empty<int>(), 0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMaxException1()
        {
            Zen.Max(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMaxException2()
        {
            Zen.Max(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinException1()
        {
            Zen.Min(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinException2()
        {
            Zen.Min(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinusException1()
        {
            Zen.Minus(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMinusException2()
        {
            Zen.Minus(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplyException1()
        {
            Zen.Multiply(null, Constant<byte>(0));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplyException2()
        {
            Zen.Multiply(Constant<byte>(0), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestMultiplyException3()
        {
            Zen.Multiply(Constant(new UInt<_9>(0)), Constant(new UInt<_9>(0)));
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestNotException()
        {
            Zen.Not(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestOrException1()
        {
            Zen.Or(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestOrException2()
        {
            Zen.Or(true, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestPlusException1()
        {
            Zen.Plus<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestPlusException2()
        {
            Zen.Plus<int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestRemoveAllException1()
        {
            FSeq.RemoveAll<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestRemoveAllException2()
        {
            FSeq.RemoveAll(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestReverseException()
        {
            FSeq.Reverse<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException1()
        {
            Zen<Option<int>> x = null;
            x.Select<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException2()
        {
            Zen<Option<int>> x = Option.Null<int>();
            x.Select<int, int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException3()
        {
            Zen<FSeq<int>> x = null;
            FSeq.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSelectException4()
        {
            Zen<FSeq<int>> x = FSeq.Empty<int>();
            FSeq.Select<int, int>(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSingletonException()
        {
            Zen<int> x = null;
            FSeq.Singleton(x);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestSomeException()
        {
            Option.Create<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeException1()
        {
            FSeq.Take<int>(null, 0);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeException2()
        {
            FSeq.Take(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeWhileException1()
        {
            FSeq.TakeWhile<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTakeWhileException2()
        {
            FSeq.TakeWhile(FSeq.Empty<int>(), null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestToSeqException()
        {
            Option.ToFSeq<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTupleException1()
        {
            Pair.Create<int, int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTupleException2()
        {
            Pair.Create<int, int>(0, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestTupleValueException()
        {
            Zen.Value<int>(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestValueOrDefaultException1()
        {
            Option.ValueOrDefault<int>(null, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestValueOrDefaultException2()
        {
            Option.Null<int>().ValueOrDefault(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException1()
        {
            Zen<Option<int>> x = null;
            x.Where(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException2()
        {
            Zen<Option<int>> x = Option.Null<int>();
            x.Where(null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException3()
        {
            Zen<FSeq<int>> x = null;
            FSeq.Where(x, null);
        }

        /// <summary>
        /// Exception thrown for null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestWhereException4()
        {
            Zen<FSeq<int>> x = FSeq.Empty<int>();
            FSeq.Where(x, null);
        }

        /// <summary>
        /// Can implicitly convert to function.
        /// </summary>
        [TestMethod]
        public void TestImplicitConversions()
        {
            Func<int> a = new ZenFunction<int>(() => 1);
            Func<int, int> b = new ZenFunction<int, int>((i) => 1);
            Func<int, int, int> c = new ZenFunction<int, int, int>((i1, i2) => 1);
            Func<int, int, int, int> d = new ZenFunction<int, int, int, int>((i1, i2, i3) => 1);
            Func<int, int, int, int, int> e = new ZenFunction<int, int, int, int, int>((i1, i2, i3, i4) => 1);

            a();
            b(1);
            c(1, 2);
            d(1, 2, 3);
            e(1, 2, 3, 4);
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs1()
        {
            var f = new ZenFunction<int, int>(i => 0);
            f.Find((i, o) => true, Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs2()
        {
            var f = new ZenFunction<int, int, int>((i1, i2) => 0);
            f.Find((i1, i2, o) => true, Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs3()
        {
            var f = new ZenFunction<int, int, int, int>((i1, i2, i3) => 0);
            f.Find((i1, i2, i3, o) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs4()
        {
            var f = new ZenFunction<int, int, int, int, int>((i1, i2, i3, i4) => 0);
            f.Find((i1, i2, i3, i4, o) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestInvalidSymbolicInputs5()
        {
            Zen.Find<int, int, int, int>((i1, i2, i3, i4) => true, Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>(), Arbitrary<int>());
        }

        /// <summary>
        /// Pass explicit input.
        /// </summary>
        [TestMethod]
        public void TestExactSeqs()
        {
            var f = new ZenFunction<FSeq<int>, BigInteger>(l => l.Length());
            f.Find((i, o) => true);
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile0()
        {
            var f = new ZenFunction<int>(() => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate());
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile1()
        {
            var f = new ZenFunction<int, int>((i1) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0));
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile2()
        {
            var f = new ZenFunction<int, int, int>((i1, i2) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0, 0));
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile3()
        {
            var f = new ZenFunction<int, int, int, int>((i1, i2, i3) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0, 0, 0));
        }

        /// <summary>
        /// Check double compiling works.
        /// </summary>
        [TestMethod]
        public void TestDoubleCompile4()
        {
            var f = new ZenFunction<int, int, int, int, int>((i1, i2, i3, i4) => 1);
            f.Compile();
            f.Compile();
            Assert.AreEqual(1, f.Evaluate(0, 0, 0, 0));
        }

        /// <summary>
        /// Exception thrown since implicit conversion won't work with object fields.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestImplicitObjectFieldConversionException()
        {
            Zen.Create<Object1>(("Field1", 0));
        }

        /// <summary>
        /// Exception thrown since we provide a value with the wrong Zen type.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCreationIncorrectType1()
        {
            Zen.Create<Object1>(("Field1", Constant(false)));
        }

        /// <summary>
        /// Exception thrown since we provide a value with the wrong Zen type.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestCreationIncorrectType2()
        {
            Zen.Create<Object2>(("Field1", Constant(0)), ("Field2", Constant(false)));
        }

        /// <summary>
        /// Exception thrown since new field has the wrong type.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidFieldZenType()
        {
            Zen.Create<Object1>(("Field1", Constant(0))).WithField("Field1", Constant(true));
        }

        /// <summary>
        /// Exception thrown for invalid casts.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidCast1()
        {
            Zen.Cast<string, int>("");
        }

        /// <summary>
        /// Exception thrown for invalid casts.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidCast3()
        {
            Zen.Cast<string, Seq<byte>>(null);
        }

        /// <summary>
        /// No exception thrown for ascii strings.
        /// </summary>
        [TestMethod]
        public void TestValidAsciiOk()
        {
            Constant("hello");
        }

        /// <summary>
        /// Exception thrown for contract violation.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ZenException))]
        public void TestInvalidContract()
        {
            Contract.Assert(false);
        }
    }
}