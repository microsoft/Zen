// <copyright file="FiniteString.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using static ZenLib.Language;

    /// <summary>
    /// A class representing a finite unicode string.
    /// </summary>
    public struct FiniteString
    {
        /// <summary>
        /// Convert a string to a FiniteString.
        /// </summary>
        /// <param name="s">The string.</param>
        public static implicit operator FiniteString(string s)
        {
            FiniteString fs = new FiniteString();
            var chars = new List<ushort>();
            foreach (var c in s)
            {
                chars.Add(c);
            }

            fs.Characters = chars;
            return fs;
        }

        /// <summary>
        /// Creates a constant string value.
        /// </summary>
        /// <returns>The string value.</returns>
        public static Zen<FiniteString> Constant(string s)
        {
            var l = EmptyList<ushort>();
            foreach (var c in s.Reverse())
            {
                l = l.AddFront(c);
            }

            return Create(l);
        }

        /// <summary>
        /// Creates an FiniteString.
        /// </summary>
        /// <returns>The empty string.</returns>
        public static Zen<FiniteString> Create(Zen<IList<ushort>> values)
        {
            return Create<FiniteString>(("Characters", values));
        }

        /// <summary>
        /// Creates an empty string.
        /// </summary>
        /// <returns>The empty string.</returns>
        public static Zen<FiniteString> Empty()
        {
            return Create(EmptyList<ushort>());
        }

        /// <summary>
        /// Creates an empty string.
        /// </summary>
        /// <returns>The empty string.</returns>
        public static Zen<FiniteString> Singleton(Zen<ushort> b)
        {
            return Create(Language.Singleton(b));
        }

        /// <summary>
        /// Gets the underlying characters.
        /// </summary>
        public IList<ushort> Characters { get; set; }

        /// <summary>
        /// Convert the finite string to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var c in this.Characters)
            {
                sb.Append((char)c);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Extension Zen methods for FiniteString types.
    /// </summary>
    public static class FiniteStringExtensions
    {
        /// <summary>
        /// Get whether a string is empty.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> IsEmpty(this Zen<FiniteString> s)
        {
            return s.GetCharacters().IsEmpty();
        }

        /// <summary>
        /// Gets the list of bytes for a finite string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The bytes.</returns>
        public static Zen<IList<ushort>> GetCharacters(this Zen<FiniteString> s)
        {
            return s.GetField<FiniteString, IList<ushort>>("Characters");
        }

        /// <summary>
        /// Concatenation of two strings.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>The concatenated string.</returns>
        public static Zen<FiniteString> Concat(this Zen<FiniteString> s1, Zen<FiniteString> s2)
        {
            return FiniteString.Create(s1.GetCharacters().Append(s2.GetCharacters()));
        }

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The length.</returns>
        public static Zen<ushort> Length(this Zen<FiniteString> s)
        {
            return s.GetCharacters().Length();
        }

        /// <summary>
        /// Whether a string is a prefix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="pre">The prefix.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> StartsWith(this Zen<FiniteString> s, Zen<FiniteString> pre)
        {
            return StartsWith(s.GetCharacters(), pre.GetCharacters());
        }

        private static Zen<bool> StartsWith(Zen<IList<ushort>> s, Zen<IList<ushort>> pre)
        {
            return pre.Case(
                empty: true,
                cons: (hd1, tl1) => s.Case(
                    empty: false,
                    cons: (hd2, tl2) => And(hd1 == hd2, StartsWith(tl2, tl1))));
        }

        /// <summary>
        /// Whether a string is a suffix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="suf">The suffix.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> EndsWith(this Zen<FiniteString> s, Zen<FiniteString> suf)
        {
            return StartsWith(s.GetCharacters().Reverse(), suf.GetCharacters().Reverse());
        }

        /// <summary>
        /// Substring of length 1 at the offset.
        /// Empty if the offset is invalid.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="i">The index.</param>
        /// <returns>The substring.</returns>
        public static Zen<FiniteString> At(this Zen<FiniteString> s, Zen<ushort> i)
        {
            return At(s.GetCharacters(), i, 0);
        }

        private static Zen<FiniteString> At(Zen<IList<ushort>> s, Zen<ushort> i, int current)
        {
            return s.Case(
                empty: FiniteString.Empty(),
                cons: (hd, tl) =>
                    If(i == (ushort)current, FiniteString.Singleton(hd), At(tl, i, current + 1)));
        }

        /// <summary>
        /// Whether a string contains a substring.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> Contains(this Zen<FiniteString> s, Zen<FiniteString> sub)
        {
            return Contains(s.GetCharacters(), sub.GetCharacters());
        }

        private static Zen<bool> Contains(Zen<IList<ushort>> s, Zen<IList<ushort>> sub)
        {
            return s.Case(
                empty: sub.IsEmpty(),
                cons: (hd, tl) => Or(StartsWith(s, sub), Contains(tl, sub)));
        }

        /// <summary>
        /// Gets the first index of a substring in a string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <returns>An index.</returns>
        public static Zen<Option<ushort>> IndexOf(this Zen<FiniteString> s, Zen<FiniteString> sub)
        {
            return IndexOf(s.GetCharacters(), sub.GetCharacters(), 0);
        }

        private static Zen<Option<ushort>> IndexOf(Zen<IList<ushort>> s, Zen<IList<ushort>> sub, int current)
        {
            return s.Case(
                empty: If(sub.IsEmpty(), Some<ushort>(current), Null<ushort>()),
                cons: (hd, tl) => If(StartsWith(s, sub), Some<ushort>(current), IndexOf(tl, sub, current + 1)));
        }

        /// <summary>
        /// Gets the first index of a substring in a
        /// string starting at an offset.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>An index.</returns>
        public static Zen<Option<ushort>> IndexOf(this Zen<FiniteString> s, Zen<FiniteString> sub, Zen<ushort> offset)
        {
            var trimmed = s.GetCharacters().Drop(offset);
            var idx = IndexOf(trimmed, sub.GetCharacters(), 0);
            return If(idx.HasValue(), Some(idx.Value() + offset), idx);
        }

        /// <summary>
        /// Gets the substring at an offset and for a given length..
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="len">The length.</param>
        /// <returns>An index.</returns>
        public static Zen<FiniteString> SubString(this Zen<FiniteString> s, Zen<ushort> offset, Zen<ushort> len)
        {
            return FiniteString.Create(s.GetCharacters().Drop(offset).Take(len));
        }

        /// <summary>
        /// Replaces all occurrences of a given character with another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="src">The source value.</param>
        /// <param name="dst">The destination value.</param>
        /// <returns>A new string.</returns>
        public static Zen<FiniteString> ReplaceAll(this Zen<FiniteString> s, Zen<ushort> src, Zen<ushort> dst)
        {
            return FiniteString.Create(s.GetCharacters().Select(c => If(c == src, dst, c)));
        }

        /// <summary>
        /// Removes all occurrences of a given character.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns>A new string.</returns>
        public static Zen<FiniteString> RemoveAll(this Zen<FiniteString> s, Zen<ushort> value)
        {
            return Transform(s, l => l.Where(c => c != value));
        }

        /// <summary>
        /// Transform a string by modifying its characters.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="f">The transformation function.</param>
        /// <returns>A new string.</returns>
        public static Zen<FiniteString> Transform(this Zen<FiniteString> s, Func<Zen<IList<ushort>>, Zen<IList<ushort>>> f)
        {
            return FiniteString.Create(f(s.GetCharacters()));
        }
    }
}
