// <copyright file="Basic.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using ZenLib.Generation;

    /// <summary>
    /// Collection of helper functions for building Zen programs.
    /// </summary>
    public static class Zen
    {
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
        /// Dict equality method for reflection.
        /// </summary>
        private static MethodInfo eqDictsMethod = typeof(Zen).GetMethod("EqDicts", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Lift a C# value to a Zen value.
        /// </summary>
        /// <param name="x">The value.</param>
        public static Zen<T> Lift<T>(T x)
        {
            CommonUtilities.ValidateNotNull(x);

            if (typeof(T) == ReflectionUtilities.StringType)
            {
                CommonUtilities.ValidateStringLiteral((string)(object)x);
            }

            return (Zen<T>)ReflectionUtilities.CreateZenConstant(x);
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
        /// <param name="depth">Depth bound on the size of the object.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Symbolic<T>(int depth = 5, bool exhaustiveDepth = true)
        {
            var generator = new SymbolicInputGenerator();
            return Arbitrary<T>(generator, depth, exhaustiveDepth);
        }

        /// <summary>
        /// A Zen object representing some arbitrary value.
        /// </summary>
        /// <param name="depth">Depth bound on the size of the object.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes as well.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Arbitrary<T>(int depth = 5, bool exhaustiveDepth = true)
        {
            var generator = new SymbolicInputGenerator();
            return Arbitrary<T>(generator, depth, exhaustiveDepth);
        }

        internal static Zen<T> Arbitrary<T>(SymbolicInputGenerator generator, int depth, bool exhaustiveDepth)
        {
            var parameter = new DepthConfiguration { Depth = depth, ExhaustiveDepth = exhaustiveDepth };
            return (Zen<T>)ReflectionUtilities.ApplyTypeVisitor(generator, typeof(T), parameter);
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
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(none);
            CommonUtilities.ValidateNotNull(some);

            return If(expr.HasValue(), some(expr.Value()), none());
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Value<T>(this Zen<Option<T>> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenAndExpr.Create(expr1, expr2);
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(Zen<bool> expr1, Zen<bool> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenOrExpr.Create(expr1, expr2);
        }

        /// <summary>
        /// Compute the and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> And(params Zen<bool>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            if (exprs.Length == 0)
            {
                return True();
            }

            var result = True();
            for (int i = exprs.Length - 1; i >= 0; i--)
            {
                result = And(exprs[i], result);
            }

            return result;
        }

        /// <summary>
        /// Compute the or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Or(params Zen<bool>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            if (exprs.Length == 0)
            {
                return False();
            }

            var result = False();
            for (int i = exprs.Length - 1; i >= 0; i--)
            {
                result = Or(exprs[i], result);
            }

            return result;
        }

        /// <summary>
        /// Compute the not of a Zen value.
        /// </summary>
        /// <param name="expr">Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Not(Zen<bool> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

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
            CommonUtilities.ValidateNotNull(guardExpr);
            CommonUtilities.ValidateNotNull(thenExpr);

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
            CommonUtilities.ValidateNotNull(guardExpr);
            CommonUtilities.ValidateNotNull(trueExpr);
            CommonUtilities.ValidateNotNull(falseExpr);

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
            CommonUtilities.ValidateNotNull(deflt);
            CommonUtilities.ValidateNotNull(cases);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);
            return EqHelper<T>(expr1, expr2);
        }

        private static Zen<bool> EqLists<T>(Zen<IList<T>> expr1, Zen<IList<T>> expr2)
        {
            return ZenListCaseExpr<T, bool>.Create(
                expr1,
                ZenListCaseExpr<T, bool>.Create(expr2, True(), (hd, tl) => False()),
                (hd1, tl1) => ZenListCaseExpr<T, bool>.Create(expr2, False(), (hd2, tl2) => And(hd1 == hd2, EqLists(tl1, tl2))));
        }

        private static Zen<bool> EqDicts<TKey, TValue>(Zen<IDictionary<TKey, TValue>> expr1, Zen<IDictionary<TKey, TValue>> expr2)
        {
            return ZenDictEqualityExpr<TKey, TValue>.Create(expr1, expr2);
        }

        private static Zen<bool> EqHelper<T>(object expr1, object expr2)
        {
            var type = typeof(T);

            if (type == ReflectionUtilities.BoolType ||
                type == ReflectionUtilities.StringType ||
                ReflectionUtilities.IsIntegerType(type))
            {
                return ZenIntegerComparisonExpr<T>.Create((dynamic)expr1, (dynamic)expr2, ComparisonType.Eq);
            }

            if (ReflectionUtilities.IsIDictType(type))
            {
                var typeArgs = type.GetGenericArgumentsCached();
                var keyType = typeArgs[0];
                var valueType = typeArgs[1];
                var method = eqDictsMethod.MakeGenericMethod(keyType, valueType);
                return (Zen<bool>)method.Invoke(null, new object[] { expr1, expr2 });
            }

            if (ReflectionUtilities.IsIListType(type))
            {
                var innerType = type.GetGenericArgumentsCached()[0];
                var method = eqListsMethod.MakeGenericMethod(innerType);
                return (Zen<bool>)method.Invoke(null, new object[] { expr1, expr2 });
            }

            // some class or struct
            var acc = True();
            foreach (var field in ReflectionUtilities.GetAllFields(type))
            {
                var method = getFieldMethod.MakeGenericMethod(typeof(T), field.FieldType);
                object field1 = method.Invoke(null, new object[] { expr1, field.Name });
                object field2 = method.Invoke(null, new object[] { expr2, field.Name });
                var emethod = eqMethod.MakeGenericMethod(field.FieldType);
                acc = And(acc, (Zen<bool>)emethod.Invoke(null, new object[] { field1, field2 }));
            }

            foreach (var property in ReflectionUtilities.GetAllProperties(type))
            {
                var method = getFieldMethod.MakeGenericMethod(typeof(T), property.PropertyType);
                object prop1 = method.Invoke(null, new object[] { expr1, property.Name });
                object prop2 = method.Invoke(null, new object[] { expr2, property.Name });
                var emethod = eqMethod.MakeGenericMethod(property.PropertyType);
                acc = And(acc, (Zen<bool>)emethod.Invoke(null, new object[] { prop1, prop2 }));
            }

            return acc;
        }

        /// <summary>
        /// Compute the less than or equal of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Leq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Leq);
        }

        /// <summary>
        /// Compute the less than of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Lt<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return And(Leq(expr1, expr2), expr1 != expr2);
        }

        /// <summary>
        /// Compute the greater than of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Gt<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return Not(Leq(expr1, expr2));
        }

        /// <summary>
        /// Compute the greater than or equal of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expression.</param>
        /// <param name="expr2">Second Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Geq<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerComparisonExpr<T>.Create(expr1, expr2, ComparisonType.Geq);
        }

        /// <summary>
        /// Compute the sum of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Plus<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.Addition);
        }

        /// <summary>
        /// Compute the concatenation of two Zen strings.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> Concat(Zen<string> expr1, Zen<string> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenConcatExpr.Create(expr1, expr2);
        }

        /// <summary>
        /// Check if one string starts with another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> StartsWith(this Zen<string> str, Zen<string> substr)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);

            return ZenStringContainmentExpr.Create(str, substr, ContainmentType.PrefixOf);
        }

        /// <summary>
        /// Check if one string ends with another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> EndsWith(this Zen<string> str, Zen<string> substr)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);

            return ZenStringContainmentExpr.Create(str, substr, ContainmentType.SuffixOf);
        }

        /// <summary>
        /// Check if one string contains another.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="substr">The substring Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains(this Zen<string> str, Zen<string> substr)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);

            return ZenStringContainmentExpr.Create(str, substr, ContainmentType.Contains);
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
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(substr);
            CommonUtilities.ValidateNotNull(replace);

            return ZenStringReplaceExpr.Create(str, substr, replace);
        }

        /// <summary>
        /// Get a substring from a string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="offset">The offset Zen expression.</param>
        /// <param name="length">The length Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> Substring(this Zen<string> str, Zen<BigInteger> offset, Zen<BigInteger> length)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(offset);
            CommonUtilities.ValidateNotNull(length);

            return ZenStringSubstringExpr.Create(str, offset, length);
        }

        /// <summary>
        /// Get the character for a string at a given index.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <param name="index">The index Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<string> At(this Zen<string> str, Zen<BigInteger> index)
        {
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(index);

            return ZenStringAtExpr.Create(str, index);
        }

        /// <summary>
        /// Get the length of a string.
        /// </summary>
        /// <param name="str">The string Zen expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Length(this Zen<string> str)
        {
            CommonUtilities.ValidateNotNull(str);

            return ZenStringLengthExpr.Create(str);
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
            CommonUtilities.ValidateNotNull(str);
            CommonUtilities.ValidateNotNull(sub);
            CommonUtilities.ValidateNotNull(offset);

            return ZenStringIndexOfExpr.Create(str, sub, offset);
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
        /// Compute the difference of Zen values.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Minus<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.Subtraction);
        }

        /// <summary>
        /// Compute the multiplication of a Zen value with a constant.
        /// </summary>
        /// <param name="expr1">First Zen expressions.</param>
        /// <param name="expr2">Second Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> Multiply<T>(Zen<T> expr1, Zen<T> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.Multiplication);
        }

        /// <summary>
        /// Compute the maximum of two values.
        /// </summary>
        /// <param name="expr1">First value.</param>
        /// <param name="expr2">Second value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<byte> Max(Zen<byte> expr1, Zen<byte> expr2)
        {
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.BitwiseAnd);
        }

        /// <summary>
        /// Compute the bitwise and of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseAnd<T>(params Zen<T>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.BitwiseOr);
        }

        /// <summary>
        /// Compute the bitwise or of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseOr<T>(params Zen<T>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(BitwiseOr);
        }

        /// <summary>
        /// Compute the bitwise not of a Zen value.
        /// </summary>
        /// <param name="expr">First Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseNot<T>(Zen<T> expr)
        {
            CommonUtilities.ValidateNotNull(expr);

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
            CommonUtilities.ValidateNotNull(expr1);
            CommonUtilities.ValidateNotNull(expr2);

            return ZenIntegerBinopExpr<T>.Create(expr1, expr2, Op.BitwiseXor);
        }

        /// <summary>
        /// Compute the bitwise xor of Zen values.
        /// </summary>
        /// <param name="exprs">Zen expressions.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> BitwiseXor<T>(params Zen<T>[] exprs)
        {
            CommonUtilities.ValidateNotNull(exprs);

            return exprs.Aggregate(BitwiseXor);
        }

        /// <summary>
        /// Get a field from a Zen value.
        /// </summary>
        /// <param name="expr">Zen object expression.</param>
        /// <param name="fieldName">Object field name.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T2> GetField<T1, T2>(this Zen<T1> expr, string fieldName)
        {
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(fieldName);

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
            CommonUtilities.ValidateNotNull(expr);
            CommonUtilities.ValidateNotNull(fieldName);
            CommonUtilities.ValidateNotNull(fieldValue);

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
        /// Logical or, implemented as an if to allow better test generation.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <returns>The logical or of the two expressions.</returns>
        public static Zen<bool> OrIf(Zen<bool> expr1, Zen<bool> expr2)
        {
            return If(expr1, True(), expr2);
        }

        /// <summary>
        /// Logical and, implemented as an if to allow better test generation.
        /// </summary>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression.</param>
        /// <returns>The logical and of the two expressions.</returns>
        public static Zen<bool> AndIf(Zen<bool> expr1, Zen<bool> expr2)
        {
            return If(Not(expr1), False(), expr2);
        }

        /// <summary>
        /// The Zen value for an empty List.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<IList<T>> EmptyList<T>()
        {
            return ZenListEmptyExpr<T>.Instance;
        }

        /// <summary>
        /// The Zen value for an empty dictionary.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<IDictionary<TKey, TValue>> EmptyDict<TKey, TValue>()
        {
            return ZenDictEmptyExpr<TKey, TValue>.Instance;
        }

        /// <summary>
        /// The Zen value for an empty List.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<IDictionary<TKey, TValue>> ArbitraryDict<TKey, TValue>()
        {
            return new ZenArbitraryExpr<IDictionary<TKey, TValue>>();
        }

        /// <summary>
        /// Update a dictionary with a new value for a given key.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<IDictionary<TKey, TValue>> DictSet<TKey, TValue>(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> keyExpr, Zen<TValue> valueExpr)
        {
            return ZenDictSetExpr<TKey, TValue>.Create(dictExpr, keyExpr, valueExpr);
        }

        /// <summary>
        /// Get a value from a dictionary.
        /// </summary>
        /// <returns>Zen value.</returns>
        internal static Zen<Option<TValue>> DictGet<TKey, TValue>(Zen<IDictionary<TKey, TValue>> dictExpr, Zen<TKey> keyExpr)
        {
            return ZenDictGetExpr<TKey, TValue>.Create(dictExpr, keyExpr);
        }

        /// <summary>
        /// Create a list from some number of elements.
        /// </summary>
        /// <param name="elements">Zen elements.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<IList<T>> List<T>(params Zen<T>[] elements)
        {
            CommonUtilities.ValidateNotNull(elements);

            Zen<T>[] copy = new Zen<T>[elements.Length];
            System.Array.Copy(elements, copy, elements.Length);
            System.Array.Reverse(copy);
            var list = EmptyList<T>();
            foreach (var element in copy)
            {
                list = ZenListAddFrontExpr<T>.Create(list, element);
            }

            return list;
        }
    }
}
