// <copyright file="FString.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing a finite string.
    /// </summary>
    public struct FString : IEquatable<FString>
    {
        /// <summary>
        /// Convert a string to a FiniteString.
        /// </summary>
        /// <param name="s">The string.</param>
        public static implicit operator FString(string s)
        {
            return new FString(s);
        }

        /// <summary>
        /// Create a new finite string from a string.
        /// </summary>
        /// <param name="s">The string.</param>
        public FString(string s)
        {
            var chars = ImmutableList.Create<ushort>();
            foreach (var c in s)
            {
                chars = chars.Add(c);
            }

            this.Characters = new FSeq<ushort>(chars);
        }

        /// <summary>
        /// Gets the underlying characters.
        /// </summary>
        public FSeq<ushort> Characters { get; set; }

        /// <summary>
        /// Convert the finite string to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var c in this.Characters.ToList())
            {
                sb.Append((char)c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Equality between finite strings.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Whether they are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is FString @string && Equals(@string);
        }

        /// <summary>
        /// Equality between finite strings.
        /// </summary>
        /// <param name="other">The other string.</param>
        /// <returns>Whether they are equal.</returns>
        public bool Equals(FString other)
        {
            return this.Characters.Equals(other.Characters);
        }

        /// <summary>
        /// Equality between finite strings.
        /// </summary>
        /// <param name="left">The left string.</param>
        /// <param name="right">The right string.</param>
        /// <returns>Whether they are equal.</returns>
        public static bool operator ==(FString left, FString right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality between finite strings.
        /// </summary>
        /// <param name="left">The left string.</param>
        /// <param name="right">The right string.</param>
        /// <returns></returns>
        public static bool operator !=(FString left, FString right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets a hash code for the finite string.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = 7;
            foreach (var c in this.Characters.ToList())
            {
                hash = 31 * hash + c;
            }

            return hash;
        }

        /// <summary>
        /// Creates an FiniteString.
        /// </summary>
        /// <returns>The finite string.</returns>
        public static Zen<FString> Create(Zen<FSeq<ushort>> values)
        {
            return Create<FString>(("Characters", values));
        }

        /// <summary>
        /// Creates a constant string value.
        /// </summary>
        /// <returns>The string value.</returns>
        public static Zen<FString> Create(string s)
        {
            var l = FSeq.Empty<ushort>();
            foreach (var c in s.Reverse())
            {
                l = l.AddFront(c);
            }

            return Create(l);
        }

        /// <summary>
        /// Creates a finite string from a character.
        /// </summary>
        /// <returns>The finite string.</returns>
        public static Zen<FString> Create(Zen<ushort> b)
        {
            return Create(FSeq.Create(b));
        }
    }

    /// <summary>
    /// Finite string extension methods for Zen.
    /// </summary>
    public static class FStringExtensions
    {
        /// <summary>
        /// Get whether a string is empty.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> IsEmpty(this Zen<FString> s)
        {
            return s.GetCharacters().IsEmpty();
        }

        /// <summary>
        /// Gets the list of bytes for a finite string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The bytes.</returns>
        public static Zen<FSeq<ushort>> GetCharacters(this Zen<FString> s)
        {
            return s.GetField<FString, FSeq<ushort>>("Characters");
        }

        /// <summary>
        /// Concatenation of two strings.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>The concatenated string.</returns>
        public static Zen<FString> Concat(this Zen<FString> s1, Zen<FString> s2)
        {
            return FString.Create(s1.GetCharacters().Append(s2.GetCharacters()));
        }

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The length.</returns>
        public static Zen<ushort> Length(this Zen<FString> s)
        {
            return s.GetCharacters().Length();
        }

        /// <summary>
        /// Whether a string is a prefix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="pre">The prefix.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> StartsWith(this Zen<FString> s, Zen<FString> pre)
        {
            return StartsWith(s.GetCharacters(), pre.GetCharacters());
        }

        /// <summary>
        /// Wheterh a string is a prefix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="pre">The prefix.</param>
        /// <returns></returns>
        private static Zen<bool> StartsWith(Zen<FSeq<ushort>> s, Zen<FSeq<ushort>> pre)
        {
            return pre.CaseStrict(
                empty: true,
                cons: (hd1, tl1) => s.CaseStrict(
                    empty: false,
                    cons: (hd2, tl2) => And(hd1 == hd2, StartsWith(tl2, tl1))));
        }

        /// <summary>
        /// Whether a string is a suffix of another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="suf">The suffix.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> EndsWith(this Zen<FString> s, Zen<FString> suf)
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
        public static Zen<FString> At(this Zen<FString> s, Zen<ushort> i)
        {
            return At(s.GetCharacters(), i, 0);
        }

        /// <summary>
        /// Substring of length 1 at the offset.
        /// Empty if the offset is invalid.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="i">The index.</param>
        /// <param name="current">The current index.</param>
        /// <returns>The substring.</returns>
        private static Zen<FString> At(Zen<FSeq<ushort>> s, Zen<ushort> i, Zen<ushort> current)
        {
            return s.Case(
                empty: FString.Create(""),
                cons: (hd, tl) =>
                    If(And(hd.IsSome(), i == current), FString.Create(hd.Value()), At(tl, i, Zen.If(hd.IsNone(), current, current + 1))));
        }

        /// <summary>
        /// Whether a string contains a substring.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="sub">The substring.</param>
        /// <returns>A boolean.</returns>
        public static Zen<bool> Contains(this Zen<FString> s, Zen<FString> sub)
        {
            return Contains(s.GetCharacters(), sub.GetCharacters());
        }

        /// <summary>
        /// Check if one sequence contains another.
        /// </summary>
        /// <param name="s">The sequence.</param>
        /// <param name="sub">The subsequence.</param>
        /// <returns></returns>
        private static Zen<bool> Contains(Zen<FSeq<ushort>> s, Zen<FSeq<ushort>> sub)
        {
            return s.Case(
                empty: sub.IsEmpty(),
                cons: (hd, tl) => Or(StartsWith(s, sub), Contains(tl, sub)));
        }

        /// <summary>
        /// Gets the substring at an offset and for a given length.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="len">The length.</param>
        /// <returns>An index.</returns>
        public static Zen<FString> SubString(this Zen<FString> s, Zen<ushort> offset, Zen<ushort> len)
        {
            return FString.Create(s.GetCharacters().Drop(offset).Take(len));
        }

        /// <summary>
        /// Replaces all occurrences of a given character with another.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="src">The source value.</param>
        /// <param name="dst">The destination value.</param>
        /// <returns>A new string.</returns>
        public static Zen<FString> ReplaceAll(this Zen<FString> s, Zen<ushort> src, Zen<ushort> dst)
        {
            return FString.Create(s.GetCharacters().Select(c => If(c == src, dst, c)));
        }

        /// <summary>
        /// Removes all occurrences of a given character.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns>A new string.</returns>
        public static Zen<FString> RemoveAll(this Zen<FString> s, Zen<ushort> value)
        {
            return Transform(s, l => l.Where(c => c != value));
        }

        /// <summary>
        /// Transform a string by modifying its characters.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="f">The transformation function.</param>
        /// <returns>A new string.</returns>
        public static Zen<FString> Transform(this Zen<FString> s, Func<Zen<FSeq<ushort>>, Zen<FSeq<ushort>>> f)
        {
            return FString.Create(f(s.GetCharacters()));
        }
    }
}
