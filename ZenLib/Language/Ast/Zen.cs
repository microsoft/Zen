﻿// <copyright file="Zen.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Threading;
    using ZenLib.Generation;
    using ZenLib.Interpretation;
    using ZenLib.ModelChecking;
    using ZenLib.Solver;

    /// <summary>
    /// A Zen expression object parameterized over the C# type.
    /// </summary>
    /// <typeparam name="T">Return type as a C# type.</typeparam>
    public abstract class Zen<T>
    {
        /// <summary>
        /// The unique id for the given Zen expression.
        /// </summary>
        public long Id = Interlocked.Increment(ref IdHolder.NextId);

        /// <summary>
        /// Accept a visitor for the ZenExpr object.
        /// </summary>
        /// <returns>A value of the return type.</returns>
        internal abstract TReturn Accept<TParam, TReturn>(ZenExprVisitor<TParam, TReturn> visitor, TParam parameter);

        /// <summary>
        /// Accept a visitor for the ZenExpr object.
        /// </summary>
        /// <returns>A value of the return type.</returns>
        internal abstract void Accept(ZenExprActionVisitor visitor);

        /// <summary>
        /// Format an expression to be more readable.
        /// </summary>
        /// <returns></returns>
        public string Format()
        {
            return CommonUtilities.RunWithLargeStack(() => new ZenFormatVisitor().Format(this));
        }

        /// <summary>
        /// Convert a C# value to a Zen value.
        /// </summary>
        /// <param name="x">The value.</param>
        public static implicit operator Zen<T>(T x)
        {
            return (Zen<T>)ReflectionUtilities.CreateZenConstant(x);
        }

        /// <summary>
        /// Equality between two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator ==(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Eq(expr1, expr2);
        }

        /// <summary>
        /// Inequality between two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator !=(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Not(Zen.Eq(expr1, expr2));
        }

        /// <summary>
        /// Less than or equal for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator <=(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Leq(expr1, expr2);
        }

        /// <summary>
        /// Less than for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator <(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Lt(expr1, expr2);
        }

        /// <summary>
        /// Greater than for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator >(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Gt(expr1, expr2);
        }

        /// <summary>
        /// Greater than or equal for two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<bool> operator >=(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Geq(expr1, expr2);
        }

        /// <summary>
        /// Sum of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator +(Zen<T> expr1, Zen<T> expr2)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.StringType)
            {
                return Zen.Concat(expr1 as Zen<string>, expr2 as Zen<string>) as Zen<T>;
            }

            if (ReflectionUtilities.IsSeqType(type))
            {
                var innerType = type.GetGenericArguments()[0];
                var method = typeof(Seq).GetMethod("Concat").MakeGenericMethod(innerType);
                return (Zen<T>)method.Invoke(null, new object[] { expr1, expr2 });
            }

            return Zen.Plus(expr1, expr2);
        }

        /// <summary>
        /// Subtraction of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator -(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Minus(expr1, expr2);
        }

        /// <summary>
        /// Multiplication of a Zen object with a constant.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator *(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.Multiply(expr1, expr2);
        }

        /// <summary>
        /// Bitwise and of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator &(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.BitwiseAnd(expr1, expr2);
        }

        /// <summary>
        /// Bitwise or of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator |(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.BitwiseOr(expr1, expr2);
        }

        /// <summary>
        /// Bitwise xor of two Zen objects.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator ^(Zen<T> expr1, Zen<T> expr2)
        {
            return Zen.BitwiseXor(expr1, expr2);
        }

        /// <summary>
        /// Bitwise negation of a Zen object.
        /// </summary>
        /// <param name="expr">Zen expression.</param>
        /// <returns>Zen expression.</returns>
        public static Zen<T> operator ~(Zen<T> expr)
        {
            return Zen.BitwiseNot(expr);
        }

        /// <summary>
        /// Reference equality for Zen objects.
        /// </summary>
        /// <param name="obj">Other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Hash code for a Zen object.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Class to store a shared id for Zen objects.
    /// </summary>
    internal class IdHolder
    {
        /// <summary>
        /// The next id to use.
        /// </summary>
        internal static long NextId;
    }

    /// <summary>
    /// Collection of helper functions for building Zen programs.
    /// </summary>
    public static class Zen
    {
        /// <summary>
        /// The next unique arbitrary id.
        /// </summary>
        private static long nextArbitraryId = 0;

        /// <summary>
        /// Get field method for reflection.
        /// </summary>
        private static MethodInfo getFieldMethod = typeof(Zen).GetMethod("GetField");

        /// <summary>
        /// Equality method for reflection.
        /// </summary>
        private static MethodInfo eqMethod = typeof(Zen).GetMethod("Eq");

        /// <summary>
        /// Boolean equality method for reflection.
        /// </summary>
        private static MethodInfo eqBoolMethod = typeof(Zen).GetMethod("Eq").MakeGenericMethod(typeof(bool));

        /// <summary>
        /// List equality method for reflection.
        /// </summary>
        private static MethodInfo eqListsMethod = typeof(Zen).GetMethod("EqLists", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// The empty state set cache.
        /// </summary>
        private static Dictionary<(Type, object), object> emptyStateSetCache = new Dictionary<(Type, object), object>();

        /// <summary>
        /// The full state set cache.
        /// </summary>
        private static Dictionary<(Type, object), object> fullStateSetCache = new Dictionary<(Type, object), object>();

        /// <summary>
        /// Lift a C# value to a Zen value.
        /// </summary>
        /// <param name="x">The value.</param>
        public static Zen<T> Lift<T>(T x)
        {
            Contract.AssertNotNull(x);
            return (Zen<T>)ReflectionUtilities.CreateZenConstant(x);
        }

        /// <summary>
        /// Gets the default value for a type as a zen value.
        /// </summary>
        /// <returns>The default expression.</returns>
        public static Zen<T> Default<T>()
        {
            return Constant(ReflectionUtilities.GetDefaultValue<T>());
        }

        /// <summary>
        /// The Zen value for false.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<bool> False()
        {
            return ZenConstantExpr<bool>.Create(false);
        }

        /// <summary>
        /// The Zen value for true.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<bool> True()
        {
            return ZenConstantExpr<bool>.Create(true);
        }

        /// <summary>
        /// The Zen value for a constant.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Constant<T>(T value)
        {
            return Lift(value);
        }

        /// <summary>
        /// A Zen object representing some symbolic value.
        /// </summary>
        /// <param name="name">An optional name for the expression.</param>
        /// <param name="depth">Depth bound on the size of the object.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Symbolic<T>(string name = "k!", int depth = 8)
        {
            return Arbitrary<T>(name, depth);
        }

        /// <summary>
        /// A Zen object representing some arbitrary value.
        /// </summary>
        /// <param name="name">An optional name for the expression.</param>
        /// <param name="depth">Depth bound on the size of the object.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Arbitrary<T>(string name = "k!", int depth = 8)
        {
            var generator = new SymbolicInputVisitor();
            return Arbitrary<T>(generator, name, depth);
        }

        /// <summary>
        /// A Zen object representing some arbitrary value.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="name">An optional name of the expression.</param>
        /// <param name="depth">Depth bound on the size of the object.</param>
        /// <returns></returns>
        internal static Zen<T> Arbitrary<T>(SymbolicInputVisitor generator, string name = "k!", int depth = 8)
        {
            var parameter = new ZenGenerationConfiguration { Depth = depth, Name = GetName(name, typeof(T)) };
            return (Zen<T>)generator.Visit(typeof(T), parameter);
        }

        /// <summary>
        /// Get a name for a given type.
        /// </summary>
        /// <param name="name">The base name.</param>
        /// <param name="type">The type of the expression assocaited with the name.</param>
        /// <returns>A descriptive name with the type attached.</returns>
        [ExcludeFromCodeCoverage]
        internal static string GetName(string name, Type type)
        {
            return name == "k!" ? name + Interlocked.Increment(ref nextArbitraryId) + "_" + type : name;
        }

        /// <summary>
        /// The Zen expression for an option match.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="none">The none case.</param>
        /// <param name="some">The some case.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> Case<T1, T2>(this Zen<Option<T1>> expr, Func<Zen<T2>> none, Func<Zen<T1>, Zen<T2>> some)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(none);
            Contract.AssertNotNull(some);

            return If(expr.IsSome(), some(expr.Value()), none());
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Value<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Option<T>, T>("Value");
        }

        /// <summary>
        /// Compute the and of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> And(Zen<bool> expr1, Zen<bool> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenLogicalBinopExpr.Create(expr1, expr2, ZenLogicalBinopExpr.LogicalOp.And);
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(Zen<bool> expr1, Zen<bool> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenLogicalBinopExpr.Create(expr1, expr2, ZenLogicalBinopExpr.LogicalOp.Or);
        }

        /// <summary>
        /// Compute the and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> And(IEnumerable<Zen<bool>> exprs)
        {
            Contract.AssertNotNull(exprs);

            return exprs.Aggregate(And);
        }

        /// <summary>
        /// Compute the and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> And(params Zen<bool>[] exprs)
        {
            Contract.AssertNotNull(exprs);

            return And((IEnumerable<Zen<bool>>)exprs);
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(IEnumerable<Zen<bool>> exprs)
        {
            Contract.AssertNotNull(exprs);

            return exprs.Aggregate(Or);
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(params Zen<bool>[] exprs)
        {
            Contract.AssertNotNull(exprs);

            return Or((IEnumerable<Zen<bool>>)exprs);
        }

        /// <summary>
        /// Compute the not of a Zen value.
        /// </summary>
        /// <param name="expr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Not(Zen<bool> expr)
        {
            Contract.AssertNotNull(expr);

            return ZenNotExpr.Create(expr);
        }

        /// <summary>
        /// Compute the implication of a Zen value.
        /// </summary>
        /// <param name="guardExpr">Zen guard expression.</param>
        /// <param name="thenExpr">Zen then expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Implies(Zen<bool> guardExpr, Zen<bool> thenExpr)
        {
            Contract.AssertNotNull(guardExpr);
            Contract.AssertNotNull(thenExpr);

            return Or(Not(guardExpr), thenExpr);
        }

        /// <summary>
        /// Compute the if-then-else of a Zen value.
        /// </summary>
        /// <param name="guardExpr">Zen guard expression.</param>
        /// <param name="trueExpr">Zen true expression.</param>
        /// <param name="falseExpr">Zen false expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> If<T>(Zen<bool> guardExpr, Zen<T> trueExpr, Zen<T> falseExpr)
        {
            Contract.AssertNotNull(guardExpr);
            Contract.AssertNotNull(trueExpr);
            Contract.AssertNotNull(falseExpr);

            return ZenIfExpr<T>.Create(guardExpr, trueExpr, falseExpr);
        }

        /// <summary>
        /// Compute a switch statement.
        /// </summary>
        /// <param name="deflt">The default value.</param>
        /// <param name="cases">A collection of ordered cases.</param>
        /// <returns>The resulting Zen value.</returns>
        public static Zen<T> Cases<T>(Zen<T> deflt, params (Zen<bool>, Zen<T>)[] cases)
        {
            Contract.AssertNotNull(deflt);
            Contract.AssertNotNull(cases);

            for (int i = cases.Length - 1; i >= 0; i--)
            {
                var (guard, value) = cases[i];
                deflt = If(guard, value, deflt);
            }

            return deflt;
        }

        /// <summary>
        /// Compute the equality of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Eq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenEqualityExpr<T>.Create(expr1, expr2);
        }

        /// <summary>
        /// Compute the less than or equal of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Leq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Leq);
        }

        /// <summary>
        /// Compute the less than of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Lt<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Lt);
        }

        /// <summary>
        /// Compute the greater than of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Gt<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Gt);
        }

        /// <summary>
        /// Compute the greater than or equal of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Geq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Geq);
        }

        /// <summary>
        /// Compute the sum of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Plus<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithBinopExpr<T>.Create(expr1, expr2, ArithmeticOp.Addition);
        }

        /// <summary>
        /// Compute the concatenation of two Zen strings.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> Concat(Zen<string> expr1, Zen<string> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            var e1 = Cast<string, Seq<char>>(expr1);
            var e2 = Cast<string, Seq<char>>(expr2);
            return Cast<Seq<char>, string>(ZenSeqConcatExpr<char>.Create(e1, e2));
        }

        /// <summary>
        /// Check if one string starts with another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> StartsWith(this Zen<string> str, Zen<string> substr)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(substr);

            var e1 = Cast<string, Seq<char>>(str);
            var e2 = Cast<string, Seq<char>>(substr);
            return ZenSeqContainsExpr<char>.Create(e1, e2, SeqContainmentType.HasPrefix);
        }

        /// <summary>
        /// Check if one string ends with another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> EndsWith(this Zen<string> str, Zen<string> substr)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(substr);

            var e1 = Cast<string, Seq<char>>(str);
            var e2 = Cast<string, Seq<char>>(substr);
            return ZenSeqContainsExpr<char>.Create(e1, e2, SeqContainmentType.HasSuffix);
        }

        /// <summary>
        /// Check if one string contains another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains(this Zen<string> str, Zen<string> substr)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(substr);

            var e1 = Cast<string, Seq<char>>(str);
            var e2 = Cast<string, Seq<char>>(substr);
            return ZenSeqContainsExpr<char>.Create(e1, e2, SeqContainmentType.Contains);
        }

        /// <summary>
        /// Replace a substring with another string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <param name="replace">The replacement string.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> ReplaceFirst(this Zen<string> str, Zen<string> substr, Zen<string> replace)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(substr);
            Contract.AssertNotNull(replace);

            var e1 = Cast<string, Seq<char>>(str);
            var e2 = Cast<string, Seq<char>>(substr);
            var e3 = Cast<string, Seq<char>>(replace);
            return Cast<Seq<char>, string>(ZenSeqReplaceFirstExpr<char>.Create(e1, e2, e3));
        }

        /// <summary>
        /// Get a substring from a string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="offset">The offset Zen expression.</param>
        /// <param name="length">The length Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> Slice(this Zen<string> str, Zen<BigInteger> offset, Zen<BigInteger> length)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(offset);
            Contract.AssertNotNull(length);

            var e1 = Cast<string, Seq<char>>(str);
            return Cast<Seq<char>, string>(ZenSeqSliceExpr<char>.Create(e1, offset, length));
        }

        /// <summary>
        /// Get the character for a string at a given index.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="index">The index Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> At(this Zen<string> str, Zen<BigInteger> index)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(index);

            var e1 = Cast<string, Seq<char>>(str);
            return Cast<Seq<char>, string>(ZenSeqAtExpr<char>.Create(e1, index));
        }

        /// <summary>
        /// Get the length of a string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Length(this Zen<string> str)
        {
            Contract.AssertNotNull(str);

            var e1 = Cast<string, Seq<char>>(str);
            return ZenSeqLengthExpr<char>.Create(e1);
        }

        /// <summary>
        /// Get the index of a substring from an offset.
        /// If the substring is empty and the offset is in bounds then returns the offset.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="sub">The substring Zen expression.</param>
        /// <param name="offset">The offset Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> IndexOf(this Zen<string> str, Zen<string> sub, Zen<BigInteger> offset)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(sub);
            Contract.AssertNotNull(offset);

            var e1 = Cast<string, Seq<char>>(str);
            var e2 = Cast<string, Seq<char>>(sub);
            return ZenSeqIndexOfExpr<char>.Create(e1, e2, offset);
        }

        /// <summary>
        /// Get the index of a substring.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="sub">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> IndexOf(this Zen<string> str, Zen<string> sub)
        {
            return IndexOf(str, sub, new BigInteger(0));
        }

        /// <summary>
        /// Determines if the string matches a regular expression.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="regex">The unicode regular expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> MatchesRegex(this Zen<string> str, Regex<char> regex)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(regex);

            return Cast<string, Seq<char>>(str).MatchesRegex(regex);
        }

        /// <summary>
        /// Determines if the string matches a regular expression.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="regex">The unicode regular expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> MatchesRegex(this Zen<string> str, string regex)
        {
            Contract.AssertNotNull(str);
            Contract.AssertNotNull(regex);

            return str.MatchesRegex(Regex.Parse(regex));
        }

        /// <summary>
        /// Cast one value to another.
        /// </summary>
        /// <param name="expr">Source Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TTarget> Cast<TSource, TTarget>(Zen<TSource> expr)
        {
            Contract.AssertNotNull(expr);
            Contract.Assert(CommonUtilities.IsSafeCast(typeof(TSource), typeof(TTarget)), "Invalid cast");

            return ZenCastExpr<TSource, TTarget>.Create(expr);
        }

        /// <summary>
        /// Compute the difference of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Minus<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithBinopExpr<T>.Create(expr1, expr2, ArithmeticOp.Subtraction);
        }

        /// <summary>
        /// Compute the multiplication of a Zen value with a constant.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Multiply<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenArithBinopExpr<T>.Create(expr1, expr2, ArithmeticOp.Multiplication);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<byte> Max(Zen<byte> expr1, Zen<byte> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> Max(Zen<short> expr1, Zen<short> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Max(Zen<ushort> expr1, Zen<ushort> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<int> Max(Zen<int> expr1, Zen<int> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<uint> Max(Zen<uint> expr1, Zen<uint> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<long> Max(Zen<long> expr1, Zen<long> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ulong> Max(Zen<ulong> expr1, Zen<ulong> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Max(Zen<BigInteger> expr1, Zen<BigInteger> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 >= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<byte> Min(Zen<byte> expr1, Zen<byte> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<short> Min(Zen<short> expr1, Zen<short> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ushort> Min(Zen<ushort> expr1, Zen<ushort> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<int> Min(Zen<int> expr1, Zen<int> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<uint> Min(Zen<uint> expr1, Zen<uint> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<long> Min(Zen<long> expr1, Zen<long> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<ulong> Min(Zen<ulong> expr1, Zen<ulong> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the minimum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Min(Zen<BigInteger> expr1, Zen<BigInteger> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return If(expr1 <= expr2, expr1, expr2);
        }

        /// <summary>
        /// Compute the bitwise and of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseAnd<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenBitwiseBinopExpr<T>.Create(expr1, expr2, BitwiseOp.BitwiseAnd);
        }

        /// <summary>
        /// Compute the bitwise and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseAnd<T>(params Zen<T>[] exprs)
        {
            Contract.AssertNotNull(exprs);
            return exprs.Aggregate(BitwiseAnd);
        }

        /// <summary>
        /// Compute the bitwise or of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseOr<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenBitwiseBinopExpr<T>.Create(expr1, expr2, BitwiseOp.BitwiseOr);
        }

        /// <summary>
        /// Compute the bitwise or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseOr<T>(params Zen<T>[] exprs)
        {
            Contract.AssertNotNull(exprs);

            return exprs.Aggregate(BitwiseOr);
        }

        /// <summary>
        /// Compute the bitwise not of a Zen value.
        /// </summary>
        /// <param name="expr">First Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseNot<T>(Zen<T> expr)
        {
            Contract.AssertNotNull(expr);

            return ZenBitwiseNotExpr<T>.Create(expr);
        }

        /// <summary>
        /// Compute the bitwise xor of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseXor<T>(Zen<T> expr1, Zen<T> expr2)
        {
            Contract.AssertNotNull(expr1);
            Contract.AssertNotNull(expr2);

            return ZenBitwiseBinopExpr<T>.Create(expr1, expr2, BitwiseOp.BitwiseXor);
        }

        /// <summary>
        /// Compute the bitwise xor of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseXor<T>(params Zen<T>[] exprs)
        {
            Contract.AssertNotNull(exprs);

            return exprs.Aggregate(BitwiseXor);
        }

        /// <summary>
        /// Apply a lambda to an argument.
        /// </summary>
        /// <param name="lambda">A lambda function.</param>
        /// <param name="argument">The function argument.</param>
        /// <returns>Zen value.</returns>
        public static Zen<TDst> Apply<TSrc, TDst>(this ZenLambda<TSrc, TDst> lambda, Zen<TSrc> argument)
        {
            Contract.AssertNotNull(argument);

            return ZenApplyExpr<TSrc, TDst>.Create(lambda, argument);
        }

        /// <summary>
        /// Create an uninitialized lambda function.
        /// </summary>
        /// <returns>Zen lambda.</returns>
        public static ZenLambda<TSrc, TDst> Lambda<TSrc, TDst>()
        {
            return new ZenLambda<TSrc, TDst>();
        }

        /// <summary>
        /// Create an initialized lambda function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>Zen lambda.</returns>
        public static ZenLambda<TSrc, TDst> Lambda<TSrc, TDst>(Func<Zen<TSrc>, Zen<TDst>> function)
        {
            Contract.AssertNotNull(function);

            var lambda = new ZenLambda<TSrc, TDst>();
            lambda.Initialize(function);
            return lambda;
        }

        /// <summary>
        /// Get a field from a Zen value.
        /// </summary>
        /// <param name="expr">Zen object expression.</param>
        /// <param name="fieldName">Object field name.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> GetField<T1, T2>(this Zen<T1> expr, string fieldName)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(fieldName);

            return ZenGetFieldExpr<T1, T2>.Create(expr, fieldName);
        }

        /// <summary>
        /// Create a new Zen value a field set to a new value.
        /// </summary>
        /// <param name="expr">Zen object expression.</param>
        /// <param name="fieldName">Object field name.</param>
        /// <param name="fieldValue">New Zen field value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T1> WithField<T1, T2>(this Zen<T1> expr, string fieldName, Zen<T2> fieldValue)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(fieldName);
            Contract.AssertNotNull(fieldValue);

            return ZenWithFieldExpr<T1, T2>.Create(expr, fieldName, fieldValue);
        }

        /// <summary>
        /// Create a new Zen object from a constructor.
        /// </summary>
        /// <param name="fields">The fields as Zen values.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Create<T>(params (string, object)[] fields)
        {
            return ZenCreateObjectExpr<T>.Create(fields);
        }

        /// <summary>
        /// The Zen value for an empty List.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<FSeq<T>> EmptyList<T>()
        {
            return Constant(new FSeq<T>());
        }

        /// <summary>
        /// The Zen value for an empty map.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, TValue>> EmptyMap<TKey, TValue>()
        {
            return ZenConstantExpr<Map<TKey, TValue>>.Create(new Map<TKey, TValue>());
        }

        /// <summary>
        /// The Zen value for an empty cmap.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<TKey, TValue>> EmptyCMap<TKey, TValue>()
        {
            return ZenConstantExpr<CMap<TKey, TValue>>.Create(new CMap<TKey, TValue>());
        }

        /// <summary>
        /// The union of two zen maps.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, SetUnit>> Union<TKey>(Zen<Map<TKey, SetUnit>> d1, Zen<Map<TKey, SetUnit>> d2)
        {
            return ZenMapCombineExpr<TKey>.Create(d1, d2, ZenMapCombineExpr<TKey>.SetCombineType.Union);
        }

        /// <summary>
        /// The union of two zen maps.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<TKey, bool>> Union<TKey>(Zen<CMap<TKey, bool>> d1, Zen<CMap<TKey, bool>> d2)
        {
            return ZenConstMapCombineExpr<TKey>.Create(d1, d2, ZenConstMapCombineExpr<TKey>.CSetCombineType.Union);
        }

        /// <summary>
        /// The intersection of two zen maps.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, SetUnit>> Intersect<TKey>(Zen<Map<TKey, SetUnit>> d1, Zen<Map<TKey, SetUnit>> d2)
        {
            return ZenMapCombineExpr<TKey>.Create(d1, d2, ZenMapCombineExpr<TKey>.SetCombineType.Intersect);
        }

        /// <summary>
        /// The intersection of two zen maps.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<TKey, bool>> Intersect<TKey>(Zen<CMap<TKey, bool>> d1, Zen<CMap<TKey, bool>> d2)
        {
            return ZenConstMapCombineExpr<TKey>.Create(d1, d2, ZenConstMapCombineExpr<TKey>.CSetCombineType.Intersect);
        }

        /// <summary>
        /// The difference of two zen maps.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, SetUnit>> Difference<TKey>(Zen<Map<TKey, SetUnit>> d1, Zen<Map<TKey, SetUnit>> d2)
        {
            return ZenMapCombineExpr<TKey>.Create(d1, d2, ZenMapCombineExpr<TKey>.SetCombineType.Difference);
        }

        /// <summary>
        /// The difference of two zen maps.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<TKey, bool>> Difference<TKey>(Zen<CMap<TKey, bool>> d1, Zen<CMap<TKey, bool>> d2)
        {
            return ZenConstMapCombineExpr<TKey>.Create(d1, d2, ZenConstMapCombineExpr<TKey>.CSetCombineType.Difference);
        }

        /// <summary>
        /// The Zen value for an arbitrary map.
        /// </summary>
        /// <param name="name">An optional name for the expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, TValue>> ArbitraryMap<TKey, TValue>(string name = null)
        {
            return new ZenArbitraryExpr<Map<TKey, TValue>>(name);
        }

        /// <summary>
        /// The Zen value for an arbitrary map.
        /// </summary>
        /// <param name="name">An optional name for the expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<TKey, TValue>> ArbitraryConstMap<TKey, TValue>(string name = null)
        {
            return new ZenArbitraryExpr<CMap<TKey, TValue>>(name);
        }

        /// <summary>
        /// The Zen value for an empty seq.
        /// </summary>
        /// <param name="name">An optional name for the expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Seq<T>> ArbitrarySeq<T>(string name = null)
        {
            return new ZenArbitraryExpr<Seq<T>>(name);
        }

        /// <summary>
        /// Update a map with a new value for a given key.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="keyExpr">The key expression.</param>
        /// <param name="valueExpr">The value expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, TValue>> MapSet<TKey, TValue>(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            return ZenMapSetExpr<TKey, TValue>.Create(mapExpr, keyExpr, valueExpr);
        }

        /// <summary>
        /// Update a map by removing a given key.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="keyExpr">The key expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<TKey, TValue>> MapDelete<TKey, TValue>(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            return ZenMapDeleteExpr<TKey, TValue>.Create(mapExpr, keyExpr);
        }

        /// <summary>
        /// Get a value from a map.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="keyExpr">The key expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Option<TValue>> MapGet<TKey, TValue>(Zen<Map<TKey, TValue>> mapExpr, Zen<TKey> keyExpr)
        {
            return ZenMapGetExpr<TKey, TValue>.Create(mapExpr, keyExpr);
        }

        /// <summary>
        /// Update a map with a new value for a given key.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueExpr">The value expression.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<CMap<TKey, TValue>> ConstMapSet<TKey, TValue>(Zen<CMap<TKey, TValue>> mapExpr, TKey key, Zen<TValue> valueExpr)
        {
            return ZenConstMapSetExpr<TKey, TValue>.Create(mapExpr, key, valueExpr);
        }

        /// <summary>
        /// Get a value from a map.
        /// </summary>
        /// <param name="mapExpr">The map expression.</param>
        /// <param name="key">The key.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<TValue> ConstMapGet<TKey, TValue>(Zen<CMap<TKey, TValue>> mapExpr, TKey key)
        {
            return ZenConstMapGetExpr<TKey, TValue>.Create(mapExpr, key);
        }

        /// <summary>
        /// Create a list from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<FSeq<T>> List<T>(params Zen<Option<T>>[] elements)
        {
            Contract.AssertNotNull(elements);

            Zen<Option<T>>[] copy = new Zen<Option<T>>[elements.Length];
            System.Array.Copy(elements, copy, elements.Length);
            System.Array.Reverse(copy);
            var list = EmptyList<T>();
            foreach (var element in copy)
            {
                list = ZenFSeqAddFrontExpr<T>.Create(list, element);
            }

            return list;
        }

        /// <summary>
        /// Solves for an assignment to Arbitrary variables in a boolean expression.
        /// </summary>
        /// <param name="expr">The boolean expression.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>Mapping from arbitrary expressions to C# objects.</returns>
        public static ZenSolution Solve(this Zen<bool> expr, SolverConfig config = null)
        {
            config = config ?? new SolverConfig();
            var model = CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Find(expr, new Dictionary<long, object>(), config));
            return new ZenSolution(model);
        }

        /// <summary>
        /// Maximize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The objective function.</param>
        /// <param name="subjectTo">The boolean expression constraints.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>Mapping from arbitrary expressions to C# objects.</returns>
        public static ZenSolution Maximize<T>(Zen<T> objective, Zen<bool> subjectTo, SolverConfig config = null)
        {
            Contract.Assert(ReflectionUtilities.IsArithmeticType(typeof(T)));
            config = config ?? new SolverConfig();
            var model = CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Maximize(objective, subjectTo, new Dictionary<long, object>(), config));
            return new ZenSolution(model);
        }

        /// <summary>
        /// Minimize an objective subject to constraints.
        /// </summary>
        /// <param name="objective">The objective function.</param>
        /// <param name="subjectTo">The boolean expression constraints.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>Mapping from arbitrary expressions to C# objects.</returns>
        public static ZenSolution Minimize<T>(Zen<T> objective, Zen<bool> subjectTo, SolverConfig config = null)
        {
            Contract.Assert(ReflectionUtilities.IsArithmeticType(typeof(T)));
            config = config ?? new SolverConfig();
            var model = CommonUtilities.RunWithLargeStack(() => SymbolicEvaluator.Minimize(objective, subjectTo, new Dictionary<long, object>(), config));
            return new ZenSolution(model);
        }

        /// <summary>
        /// Alias for new ZenFunction.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T> Function<T>(Func<Zen<T>> f)
        {
            return new ZenFunction<T>(f);
        }

        /// <summary>
        /// Alias for new ZenFunction.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2> Function<T1, T2>(Func<Zen<T1>, Zen<T2>> f)
        {
            return new ZenFunction<T1, T2>(f);
        }

        /// <summary>
        /// Alias for new ZenFunction.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2, T3> Function<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> f)
        {
            return new ZenFunction<T1, T2, T3>(f);
        }

        /// <summary>
        /// Alias for new ZenFunction.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2, T3, T4> Function<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> f)
        {
            return new ZenFunction<T1, T2, T3, T4>(f);
        }

        /// <summary>
        /// Alias for new ZenFunction.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenFunction<T1, T2, T3, T4, T5> Function<T1, T2, T3, T4, T5>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> f)
        {
            return new ZenFunction<T1, T2, T3, T4, T5>(f);
        }

        /// <summary>
        /// Alias for new ZenConstraint.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenConstraint<T> Constraint<T>(Func<Zen<T>, Zen<bool>> f)
        {
            return new ZenConstraint<T>(f);
        }

        /// <summary>
        /// Alias for new ZenConstraint.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenConstraint<T1, T2> Constraint<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> f)
        {
            return new ZenConstraint<T1, T2>(f);
        }

        /// <summary>
        /// Alias for new ZenConstraint.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenConstraint<T1, T2, T3> Constraint<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> f)
        {
            return new ZenConstraint<T1, T2, T3>(f);
        }

        /// <summary>
        /// Alias for new ZenConstraint.
        /// </summary>
        /// <param name="f">The function expression.</param>
        /// <returns>The Zen function.</returns>
        public static ZenConstraint<T1, T2, T3, T4> Constraint<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> f)
        {
            return new ZenConstraint<T1, T2, T3, T4>(f);
        }

        /// <summary>
        /// Evaluate a Zen function.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <returns>The value from running the function.</returns>
        public static T Evaluate<T>(Func<Zen<T>> f)
        {
            return new ZenFunction<T>(f).Evaluate();
        }

        /// <summary>
        /// Evaluate a Zen function.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <param name="input1">The input.</param>
        /// <returns>The value from running the function.</returns>
        public static T2 Evaluate<T1, T2>(Func<Zen<T1>, Zen<T2>> f, T1 input1)
        {
            return new ZenFunction<T1, T2>(f).Evaluate(input1);
        }

        /// <summary>
        /// Evaluate a Zen function.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <param name="input1">The first input.</param>
        /// <param name="input2">The second input.</param>
        /// <returns>The value from running the function.</returns>
        public static T3 Evaluate<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> f, T1 input1, T2 input2)
        {
            return new ZenFunction<T1, T2, T3>(f).Evaluate(input1, input2);
        }

        /// <summary>
        /// Evaluate a Zen function.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <param name="input1">The first input.</param>
        /// <param name="input2">The second input.</param>
        /// <param name="input3">The third input.</param>
        /// <returns>The value from running the function.</returns>
        public static T4 Evaluate<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> f, T1 input1, T2 input2, T3 input3)
        {
            return new ZenFunction<T1, T2, T3, T4>(f).Evaluate(input1, input2, input3);
        }

        /// <summary>
        /// Evaluate a Zen function.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <param name="input1">The first input.</param>
        /// <param name="input2">The second input.</param>
        /// <param name="input3">The third input.</param>
        /// <param name="input4">The fourth input.</param>
        /// <returns>The value from running the function.</returns>
        public static T5 Evaluate<T1, T2, T3, T4, T5>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> f, T1 input1, T2 input2, T3 input3, T4 input4)
        {
            return new ZenFunction<T1, T2, T3, T4, T5>(f).Evaluate(input1, input2, input3, input4);
        }

        /// <summary>
        /// Compile a Zen function to a C# function.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <returns>The compiled C# function.</returns>
        public static Func<T> Compile<T>(Func<Zen<T>> f)
        {
            var zf = new ZenFunction<T>(f);
            zf.Compile();
            return zf.Evaluate;
        }

        /// <summary>
        /// Compile a Zen function to a C# function.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <returns>The compiled C# function.</returns>
        public static Func<T1, T2> Compile<T1, T2>(Func<Zen<T1>, Zen<T2>> f)
        {
            var zf = new ZenFunction<T1, T2>(f);
            zf.Compile();
            return zf.Evaluate;
        }

        /// <summary>
        /// Compile a Zen function to a C# function.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <returns>The compiled C# function.</returns>
        public static Func<T1, T2, T3> Compile<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> f)
        {
            var zf = new ZenFunction<T1, T2, T3>(f);
            zf.Compile();
            return zf.Evaluate;
        }

        /// <summary>
        /// Compile a Zen function to a C# function.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <returns>The compiled C# function.</returns>
        public static Func<T1, T2, T3, T4> Compile<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> f)
        {
            var zf = new ZenFunction<T1, T2, T3, T4>(f);
            zf.Compile();
            return zf.Evaluate;
        }

        /// <summary>
        /// Compile a Zen function to a C# function.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <returns>The compiled C# function.</returns>
        public static Func<T1, T2, T3, T4, T5> Compile<T1, T2, T3, T4, T5>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> f)
        {
            var zf = new ZenFunction<T1, T2, T3, T4, T5>(f);
            zf.Compile();
            return zf.Evaluate;
        }

        /// <summary>
        /// Find an input satisfying the invariant.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input">The input.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public static Option<T1> Find<T1>(
            Func<Zen<T1>, Zen<bool>> invariant,
            Zen<T1> input = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return Zen.Constraint<T1>(invariant).Find(input, depth, config);
        }

        /// <summary>
        /// Find an input satisfying the invariant.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">The first input.</param>
        /// <param name="input2">The second input.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public static Option<(T1, T2)> Find<T1, T2>(
            Func<Zen<T1>, Zen<T2>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return Zen.Constraint<T1, T2>(invariant).Find(input1, input2, depth, config);
        }

        /// <summary>
        /// Find an input satisfying the invariant.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">The first input.</param>
        /// <param name="input2">The second input.</param>
        /// <param name="input3">The third input.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public static Option<(T1, T2, T3)> Find<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return Zen.Constraint<T1, T2, T3>(invariant).Find(input1, input2, input3, depth, config);
        }

        /// <summary>
        /// Find an input satisfying the invariant.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <param name="input1">The first input.</param>
        /// <param name="input2">The second input.</param>
        /// <param name="input3">The third input.</param>
        /// <param name="input4">The fourth input.</param>
        /// <param name="depth">The maximum depth of elements to consider in an input.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>An input if one exists satisfying the constraints.</returns>
        public static Option<(T1, T2, T3, T4)> Find<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> invariant,
            Zen<T1> input1 = null,
            Zen<T2> input2 = null,
            Zen<T3> input3 = null,
            Zen<T4> input4 = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return Zen.Constraint<T1, T2, T3, T4>(invariant).Find(input1, input2, input3, input4, depth, config);
        }

        /// <summary>
        /// Generate inputs using symbolic execution.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <param name="precondition">The precondition.</param>
        /// <param name="depth">The maximum depth.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>The input values.</returns>
        public static IEnumerable<T1> GenerateInputs<T1, T2>(
            Func<Zen<T1>, Zen<T2>> f,
            Func<Zen<T1>, Zen<bool>> precondition = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return new ZenFunction<T1, T2>(f).GenerateInputs(null, precondition, depth, config);
        }

        /// <summary>
        /// Generate inputs using symbolic execution.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <param name="precondition">The precondition.</param>
        /// <param name="depth">The maximum depth.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>The input values.</returns>
        public static IEnumerable<(T1, T2)> GenerateInputs<T1, T2, T3>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>> f,
            Func<Zen<T1>, Zen<T2>, Zen<bool>> precondition = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return new ZenFunction<T1, T2, T3>(f).GenerateInputs(null, null, precondition, depth, config);
        }

        /// <summary>
        /// Generate inputs using symbolic execution.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <param name="precondition">The precondition.</param>
        /// <param name="depth">The maximum depth.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>The input values.</returns>
        public static IEnumerable<(T1, T2, T3)> GenerateInputs<T1, T2, T3, T4>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> f,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> precondition = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return new ZenFunction<T1, T2, T3, T4>(f).GenerateInputs(null, null, null, precondition, depth, config);
        }

        /// <summary>
        /// Generate inputs using symbolic execution.
        /// </summary>
        /// <param name="f">The Zen function.</param>
        /// <param name="precondition">The precondition.</param>
        /// <param name="depth">The maximum depth.</param>
        /// <param name="config">The solver configuration.</param>
        /// <returns>The input values.</returns>
        public static IEnumerable<(T1, T2, T3, T4)> GenerateInputs<T1, T2, T3, T4, T5>(
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> f,
            Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> precondition = null,
            int depth = 8,
            SolverConfig config = null)
        {
            return new ZenFunction<T1, T2, T3, T4, T5>(f).GenerateInputs(null, null, null, null, precondition, depth, config);
        }

        /// <summary>
        /// Evaluates a Zen expression given an assignment from arbitrary variable to C# object.
        /// </summary>
        /// <returns>Mapping from arbitrary expressions to C# objects.</returns>
        public static T Evaluate<T>(this Zen<T> expr, Dictionary<object, object> assignment)
        {
            Zen<bool> constraints = true;
            foreach (var kv in assignment)
            {
                var keyType = kv.Key.GetType();
                var valueType = kv.Value.GetType();
                Contract.Assert(ReflectionUtilities.IsZenType(keyType));
                var innerType = keyType.BaseType.GetGenericArgumentsCached()[0];
                Contract.Assert(innerType.IsAssignableFrom(valueType), "Type mismatch in assignment between key and value");
                constraints = Zen.And(constraints, Zen.Eq((dynamic)kv.Key, (dynamic)kv.Value));
            }

            var solution = constraints.Solve();
            var environment = new ExpressionEvaluatorEnvironment { ArbitraryAssignment = System.Collections.Immutable.ImmutableDictionary<object, object>.Empty.AddRange(solution.VariableAssignment) };
            var interpreter = new ExpressionEvaluatorVisitor(false);
            return (T)interpreter.Visit(expr, environment);
        }

        /// <summary>
        /// Gets the set containing all values.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<T> FullSet<T>(StateSetTransformerManager manager = null)
        {
            var key = (typeof(T), manager);
            if (!fullStateSetCache.TryGetValue(key, out var set))
            {
                set = Zen.StateSet<T>(x => true, manager);
                fullStateSetCache[key] = set;
            }

            return (StateSet<T>)set;
        }

        /// <summary>
        /// Gets the set containing no values.
        /// </summary>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<T> EmptySet<T>(StateSetTransformerManager manager = null)
        {
            var key = (typeof(T), manager);
            if (!emptyStateSetCache.TryGetValue(key, out var set))
            {
                set = Zen.StateSet<T>(x => false, manager);
                emptyStateSetCache[key] = set;
            }

            return (StateSet<T>)set;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<T> StateSet<T>(Func<Zen<T>, Zen<bool>> function, StateSetTransformerManager manager = null)
        {
            return Zen.StateSet<T>(new ZenFunction<T, bool>(function), manager);
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<Pair<T1, T2>> StateSet<T1, T2>(Func<Zen<T1>, Zen<T2>, Zen<bool>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2, bool>(function).StateSet(manager);
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<Pair<T1, T2, T3>> StateSet<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<bool>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2, T3, bool>(function).StateSet(manager);
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<Pair<T1, T2, T3, T4>> StateSet<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<bool>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2, T3, T4, bool>(function).StateSet(manager);
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<T> StateSet<T>(this ZenFunction<T, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(T), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<T>)stateSet;
            }

            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(function.Function, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<Pair<T1, T2>> StateSet<T1, T2>(this ZenFunction<T1, T2, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(Pair<T1, T2>), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<Pair<T1, T2>>)stateSet;
            }

            Func<Zen<Pair<T1, T2>>, Zen<bool>> f = p => function.Function(p.Item1(), p.Item2());
            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(f, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<Pair<T1, T2, T3>> StateSet<T1, T2, T3>(this ZenFunction<T1, T2, T3, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(Pair<T1, T2, T3>), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<Pair<T1, T2, T3>>)stateSet;
            }

            Func<Zen<Pair<T1, T2, T3>>, Zen<bool>> f = p => function.Function(p.Item1(), p.Item2(), p.Item3());
            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(f, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a state set.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A state set for the function.</returns>
        public static StateSet<Pair<T1, T2, T3, T4>> StateSet<T1, T2, T3, T4>(this ZenFunction<T1, T2, T3, T4, bool> function, StateSetTransformerManager manager = null)
        {
            manager = StateSetTransformerFactory.GetOrDefaultManager(manager);

            var key = (typeof(Pair<T1, T2, T3, T4>), function.FunctionBodyExpr.Id);
            if (manager.StateSetCache.TryGetValue(key, out var stateSet))
            {
                return (StateSet<Pair<T1, T2, T3, T4>>)stateSet;
            }

            Func<Zen<Pair<T1, T2, T3, T4>>, Zen<bool>> f = p => function.Function(p.Item1(), p.Item2(), p.Item3(), p.Item4());
            var result = CommonUtilities.RunWithLargeStack(() => StateSetTransformerFactory.CreateStateSet(f, manager));
            manager.StateSetCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// Gets the function as a transformer.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSetTransformer<T1, T2> Transformer<T1, T2>(Func<Zen<T1>, Zen<T2>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2>(function).Transformer(manager);
        }

        /// <summary>
        /// Gets the function as a transformer.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSetTransformer<Pair<T1, T2>, T3> Transformer<T1, T2, T3>(Func<Zen<T1>, Zen<T2>, Zen<T3>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2, T3>(function).Transformer(manager);
        }

        /// <summary>
        /// Gets the function as a transformer.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSetTransformer<Pair<T1, T2, T3>, T4> Transformer<T1, T2, T3, T4>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2, T3, T4>(function).Transformer(manager);
        }

        /// <summary>
        /// Gets the function as a transformer.
        /// </summary>
        /// <param name="function">The zen function.</param>
        /// <param name="manager">An optional manager object.</param>
        /// <returns>A transformer for the function.</returns>
        public static StateSetTransformer<Pair<T1, T2, T3, T4>, T5> Transformer<T1, T2, T3, T4, T5>(Func<Zen<T1>, Zen<T2>, Zen<T3>, Zen<T4>, Zen<T5>> function, StateSetTransformerManager manager = null)
        {
            return new ZenFunction<T1, T2, T3, T4, T5>(function).Transformer(manager);
        }
    }
}
