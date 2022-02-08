// <copyright file="CommonUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Numerics;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// A collection of common utility functions.
    /// </summary>
    internal static class CommonUtilities
    {
        /// <summary>
        /// An empty array.
        /// </summary>
        public static object[] EmptyArray = new object[] { };

        /// <summary>
        /// constructor arguments for creating fixed integers.
        /// </summary>
        private static object[] constructorArgs = new object[] { 0L };

        /// <summary>
        /// constructor types for creating fixed integers.
        /// </summary>
        private static Type[] constructorTypes = new Type[] { typeof(long) };

        /// <summary>
        /// Merge two immutable dictionaries together by key.
        /// </summary>
        /// <typeparam name="T1">Type of the key.</typeparam>
        /// <typeparam name="T2">Type of the value.</typeparam>
        /// <typeparam name="T3">Type of the new value.</typeparam>
        /// <param name="dict1">The first dictionary.</param>
        /// <param name="dict2">The second dictionary.</param>
        /// <param name="merger">The merging function.</param>
        /// <returns></returns>
        public static ImmutableDictionary<T1, T3> Merge<T1, T2, T3>(
            ImmutableDictionary<T1, T2> dict1,
            ImmutableDictionary<T1, T2> dict2,
            Func<T1, Option<T2>, Option<T2>, Option<T3>> merger)
        {
            var result = ImmutableDictionary<T1, T3>.Empty;

            var keys = new HashSet<T1>(dict1.Keys);
            keys.UnionWith(dict2.Keys);

            foreach (var key in keys)
            {
                var b1 = dict1.TryGetValue(key, out var value1);
                var b2 = dict2.TryGetValue(key, out var value2);
                var o1 = b1 ? Option.Some(value1) : Option.None<T2>();
                var o2 = b2 ? Option.Some(value2) : Option.None<T2>();
                if (b1 || b2)
                {
                    var mergedValue = merger(key, o1, o2);
                    if (mergedValue.HasValue)
                    {
                        result = result.Add(key, mergedValue.Value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Map a function over a dictionary.
        /// </summary>
        /// <typeparam name="T1">The key type.</typeparam>
        /// <typeparam name="T2">The value type.</typeparam>
        /// <typeparam name="T3">The new value type.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="mapper">The map function.</param>
        /// <returns>The resulting dictionary.</returns>
        public static ImmutableDictionary<T1, T3> Map<T1, T2, T3>(ImmutableDictionary<T1, T2> dict, Func<T2, T3> mapper)
        {
            var result = ImmutableDictionary<T1, T3>.Empty;
            foreach (var kv in dict)
            {
                result = result.Add(kv.Key, mapper(kv.Value));
            }

            return result;
        }

        /// <summary>
        /// Split a list to peel off the first element.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The head and the rest of the list.</returns>
        public static (T, IList<T>) SplitHead<T>(ImmutableList<T> list)
        {
            var hd = list[0];
            var tl = list.Count == 1 ? ImmutableList<T>.Empty : list.GetRange(1, list.Count - 1);
            return (hd, tl);
        }

        /// <summary>
        /// Convert to an immutable list if necessary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ImmutableList<T> ToImmutableList<T>(object obj)
        {
            if (obj is ImmutableList<T>)
            {
                return (ImmutableList<T>)obj;
            }

            return ImmutableList.CreateRange((IList<T>)obj);
        }

        /// <summary>
        /// Validate that an argument is true.
        /// </summary>
        /// <param name="obj">The argument.</param>
        /// <param name="message">The error message.</param>
        public static void ValidateIsTrue(bool obj, string message)
        {
            if (!obj)
            {
                throw new ZenException(message);
            }
        }

        /// <summary>
        /// Validate that an argument is not null.
        /// </summary>
        /// <param name="obj">The argument.</param>
        public static void ValidateNotNull(object obj)
        {
            if (obj is null)
            {
                throw new ZenException($"Invalid null argument");
            }
        }

        /// <summary>
        /// Validate that a type is an integer type.
        /// </summary>
        /// <param name="type"></param>
        public static void ValidateIsIntegerType(Type type)
        {
            if (!ReflectionUtilities.IsIntegerType(type))
            {
                throw new ZenException($"Invalid non-integer type {type} used as integer.");
            }
        }

        /// <summary>
        /// Validate that a string literal is well-formed.
        /// </summary>
        /// <param name="s">The string.</param>
        internal static void ValidateStringLiteral(string s)
        {
            foreach (var c in s)
            {
                if (c > 255)
                {
                    throw new ZenException($"Invalid string literal with backslash character: {s}");
                }
            }
        }

        /// <summary>
        /// Run a function with a large stack in another thread.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="f">The function to run.</param>
        /// <returns>The result of the function.</returns>
        internal static T RunWithLargeStack<T>(Func<T> f)
        {
            // don't spawn a new thread if we are in a thread with a larger stack.
            if (!Settings.UseLargeStack || Settings.InSeparateThread)
            {
                return f();
            }

            T result = default;
            Exception exn = null;

            // start a new thread to fun the function.
            Thread t = new Thread(() =>
            {
                try
                {
                    Settings.InSeparateThread = true;
                    result = f();
                }
                catch (Exception e)
                {
                    exn = e;
                }
            }, Settings.LargeStackSize);

            t.Start();
            t.Join();

            // propagate an internal exception
            if (exn != null)
            {
                throw new ZenException("Execption thrown in RunWithLargeStack", exn);
            }

            return result;
        }

        /// <summary>
        /// Creates an arbitrary value if the input is null.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="depth">The input depth.</param>
        /// <param name="exhaustiveDepth">Whether to check smaller sizes.</param>
        /// <returns>An arbitrary Zen value.</returns>
        public static Zen<T> GetArbitraryIfNull<T>(Zen<T> input, int depth, bool exhaustiveDepth)
        {
            return (input is null) ? Basic.Arbitrary<T>(depth, exhaustiveDepth) : input;
        }

        /// <summary>
        /// Gets the integer size for a given integer type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The size of the integer.</returns>
        public static int IntegerSize(Type type)
        {
            var c = type.GetConstructor(constructorTypes);
            var integer = (dynamic)c.Invoke(constructorArgs);
            return integer.Size;
        }

        /// <summary>
        /// Converts a solver result back to a C# result.
        /// </summary>
        /// <param name="value">The solver result.</param>
        /// <returns>The C# value.</returns>
        public static T ConvertSymbolicResultToCSharp<T>(object value)
        {
            return (T)ConvertSymbolicResultToCSharp(typeof(T), value);
        }

        /// <summary>
        /// Converts a solver result back to a C# result.
        /// </summary>
        /// <param name="type">The type of the result.</param>
        /// <param name="value">The solver result.</param>
        /// <returns>The C# value.</returns>
        public static object ConvertSymbolicResultToCSharp(Type type, object value)
        {
            var objType = value.GetType();

            if (type.IsAssignableFrom(objType))
            {
                return value;
            }
            else if (type == typeof(ushort) && objType == typeof(short))
            {
                return (ushort)(short)value;
            }
            else if (type == typeof(uint) && objType == typeof(int))
            {
                return (uint)(int)value;
            }
            else if (type == typeof(ulong) && objType == typeof(long))
            {
                return (ulong)(long)value;
            }
            else if (ReflectionUtilities.IsFixedIntegerType(type))
            {
                return type.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { value });
            }

            throw new ZenException($"Internal error: invalid conversion of type {type} to {objType}");
        }

        /// <summary>
        /// Convert a C# string to a Z3 string.
        /// </summary>
        /// <param name="s">The C# string.</param>
        /// <returns>The Z3 string.</returns>
        public static string ConvertCSharpStringToZ3(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                sb.Append("\\x");
                var hex = GetHex(c);
                sb.Append(hex);
            }

            return sb.ToString();
        }

        private static string GetHex(char c)
        {
            Contract.Assert(c <= 255);
            var lo = c & 0x000F;
            var hi = c >> 4;
            return new string(new char[] { GetHex(hi), GetHex(lo) });
        }

        private static char GetHex(int x)
        {
            Contract.Assert(x <= 15);
            if (x < 10)
            {
                return (char)(48 + x);
            }

            return (char)(55 + x);
        }

        /// <summary>
        /// Unescape a Z3 string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>An unescaped string.</returns>
        public static string ConvertZ3StringToCSharp(string s)
        {
            // strip outer quotation marks
            s = s.Substring(1, s.Length - 2);

            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                // strip double quotation marks
                if ((s[i] == '\"') && (s[i + 1] == '\"'))
                {
                    sb.Append('\"');
                    i++;
                    continue;
                }

                // unescape characters if needed
                if (s[i] == '\\')
                {
                    switch (s[i + 1])
                    {
                        // Z3 escape characters
                        case 'f': sb.Append('\f'); i++; continue;
                        case 'n': sb.Append('\n'); i++; continue;
                        case 'r': sb.Append('\r'); i++; continue;
                        case 'v': sb.Append('\v'); i++; continue;
                        case '\\': sb.Append('\\'); i++; continue;
                        case 'x':
                            var v = s[i + 2].ToString() + s[i + 3].ToString();
                            var u = (char)ushort.Parse(v, NumberStyles.AllowHexSpecifier);
                            sb.Append(u.ToString());
                            i = i + 3;
                            continue;
                    }
                }

                // otherwise copy the character literal
                sb.Append(s[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replace the first instance of a substring in a string with a new string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <param name="replace">The replacement string.</param>
        /// <returns>A new string.</returns>
        public static string ReplaceFirst(string s, string sub, string replace)
        {
            if (sub == string.Empty)
            {
                return s + replace;
            }

            var idx = s.IndexOf(sub);
            if (idx < 0)
            {
                return s;
            }

            var afterMatch = idx + sub.Length;
            return s.Substring(0, idx) + replace + s.Substring(afterMatch, s.Length - afterMatch);
        }

        /// <summary>
        /// Get the substring at an offset and length. Follows SMT-LIB semantics.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>A substring.</returns>
        public static string Substring(string s, BigInteger offset, BigInteger length)
        {
            if (offset >= s.Length)
            {
                return string.Empty;
            }

            var len = offset + length > s.Length ? s.Length - offset : length;
            return s.Substring((int)offset, (int)len);
        }

        /// <summary>
        /// Get the index of a substring starting at an offset.
        /// If the substring is the empty string, returns the offset if in bounds.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The index and a match.</returns>
        public static BigInteger IndexOf(string s, string sub, BigInteger offset)
        {
            if (offset >= s.Length)
            {
                return -1;
            }

            if (sub == string.Empty)
            {
                return (short)offset;
            }

            return (short)s.IndexOf(sub, (int)offset);
        }

        /// <summary>
        /// Get the substring character at an index.
        /// Returns the empty string if out of bounds.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="index">The index.</param>
        /// <returns>A substring at that character.</returns>
        public static string At(string s, BigInteger index)
        {
            if (index >= s.Length)
            {
                return string.Empty;
            }

            return s[(int)index].ToString();
        }
    }
}
