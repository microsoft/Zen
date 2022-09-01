// <copyright file="Set.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// A class representing an arbitrary sized sequence.
    /// </summary>
    public class Seq<T> : IEquatable<Seq<T>>
    {
        /// <summary>
        /// The underlying values of the backing seq.
        /// </summary>
        internal ImmutableList<T> Values;

        /// <summary>
        /// Create a new empty sequence.
        /// </summary>
        public Seq()
        {
            this.Values = ImmutableList.Create<T>();
        }

        /// <summary>
        /// Create a sequence from a single value.
        /// </summary>
        /// <param name="value">The value.</param>
        public Seq(T value)
        {
            this.Values = ImmutableList<T>.Empty.Add(value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{TKey}"/> class.
        /// </summary>
        public Seq(params T[] values) : this((IEnumerable<T>)values)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{TKey}"/> class.
        /// </summary>
        public Seq(IEnumerable<T> values)
        {
            Contract.Assert(values != null);
            this.Values = ImmutableList<T>.Empty.AddRange(values);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Seq{T}"/> class.
        /// </summary>
        /// <param name="values">The backing values.</param>
        internal Seq(ImmutableList<T> values)
        {
            Contract.AssertNotNull(values);
            this.Values = values;
        }

        /// <summary>
        /// Get the sequence as a list.
        /// </summary>
        /// <returns></returns>
        public IList<T> ToList()
        {
            return this.Values;
        }

        /// <summary>
        /// Regex match for a sequence.
        /// </summary>
        /// <param name="regex">The regex to match.</param>
        /// <returns>True if the regex matches the sequence.</returns>
        public bool MatchesRegex(Regex<T> regex)
        {
            Contract.AssertNotNull(regex);
            return regex.IsMatch(this.Values);
        }

        /// <summary>
        /// Replace the first instance of a subsequence in a seq with a new seq.
        /// </summary>
        /// <param name="match">The subseq to match.</param>
        /// <param name="replace">The replacement seq.</param>
        /// <returns>A new seq.</returns>
        public Seq<T> ReplaceFirst(Seq<T> match, Seq<T> replace)
        {
            Contract.AssertNotNull(match);
            Contract.AssertNotNull(replace);

            if (match.Length() == 0)
            {
                return replace.Concat(this);
            }

            var idx = this.IndexOf(match);
            if (idx < 0)
            {
                return this;
            }

            var afterMatch = idx + match.Length();
            return this.Slice(0, idx).Concat(replace).Concat(this.Slice(afterMatch, this.Length() - afterMatch));
        }

        /// <summary>
        /// Get the slice of a sequence at an offset and length.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>A subseq.</returns>
        public Seq<T> Slice(int offset, int length)
        {
            if (offset < 0 || offset >= this.Length() || length < 0)
            {
                return new Seq<T>();
            }

            var len = offset + length > this.Length() ? this.Length() - offset : length;
            return new Seq<T>(this.Values.GetRange(offset, len));
        }

        /// <summary>
        /// Get the slice of a sequence at an offset and length.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>A substring.</returns>
        internal Seq<T> SliceBigInteger(BigInteger offset, BigInteger length)
        {
            return this.Slice(int.Parse(offset.ToString()), int.Parse(length.ToString()));
        }

        /// <summary>
        /// Gets the index of the other subsequence after the offset.
        /// Return -1 if other is no such index exists..
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>Index of the first match or -1.</returns>
        public int IndexOf(Seq<T> other)
        {
            Contract.AssertNotNull(other);
            return this.IndexOf(other, 0);
        }

        /// <summary>
        /// Gets the index of the other subsequence after the offset.
        /// Return -1 if other is no such index exists..
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <param name="offset">The initial offset.</param>
        /// <returns>Index of the first match or -1.</returns>
        public int IndexOf(Seq<T> other, int offset)
        {
            Contract.AssertNotNull(other);

            if (offset < 0 || offset > this.Length() || other.Length() > this.Length())
            {
                return -1;
            }

            if (other.Length() == 0)
            {
                return offset;
            }

            var difference = this.Length() - other.Length();
            for (int i = offset; i < difference + 1; i++)
            {
                if (IsMatch(other, i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the other subsequence after the offset.
        /// Return -1 if other is no such index exists..
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <param name="offset">The initial offset.</param>
        /// <returns>Index of the first match or -1.</returns>
        internal BigInteger IndexOfBigInteger(Seq<T> other, BigInteger offset)
        {
            Contract.AssertNotNull(other);

            return new BigInteger(this.IndexOf(other, int.Parse(offset.ToString())));
        }

        /// <summary>
        /// Determines if this sequence contains the other subsequence.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>True if the other sequence is a subsequence of this one.</returns>
        public bool Contains(Seq<T> other)
        {
            Contract.AssertNotNull(other);

            if (other.Length() == 0)
            {
                return true;
            }

            if (0 >= this.Length() || other.Length() > this.Length())
            {
                return false;
            }

            var difference = this.Length() - other.Length();
            for (int i = 0; i < difference + 1; i++)
            {
                if (IsMatch(other, i))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if this sequence matches another at an index.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private bool IsMatch(Seq<T> other, int index)
        {
            Contract.AssertNotNull(other);

            for (int i = index; i < index + other.Length(); i++)
            {
                if (!this.Values[i].Equals(other.Values[i - index]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if this sequence has a prefix of the other subsequence.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>True if this sequence has the other sequence as a prefix.</returns>
        public bool HasPrefix(Seq<T> other)
        {
            Contract.AssertNotNull(other);

            if (other.Length() > this.Length())
            {
                return false;
            }

            for (int i = 0; i < other.Length(); i++)
            {
                if (!this.Values[i].Equals(other.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if this sequence has a suffix of the other subsequence.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>True if this sequence has the other sequence as a suffix.</returns>
        public bool HasSuffix(Seq<T> other)
        {
            Contract.AssertNotNull(other);

            if (other.Length() > this.Length())
            {
                return false;
            }

            var difference = this.Length() - other.Length();

            for (int i = 0; i < other.Length(); i++)
            {
                if (!this.Values[i + difference].Equals(other.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the value at a given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value at the index.</returns>
        public Seq<T> At(int index)
        {
            if (index >= this.Values.Count || index < 0)
            {
                return new Seq<T>();
            }

            return new Seq<T>(this.Values[index]);
        }

        /// <summary>
        /// Get the value at a given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value at the index.</returns>
        internal Seq<T> AtBigInteger(BigInteger index)
        {
            return At(int.Parse(index.ToString()));
        }

        /// <summary>
        /// Length of the sequence.
        /// </summary>
        /// <returns>The length as an integer.</returns>
        public int Length()
        {
            return this.Values.Count;
        }

        /// <summary>
        /// Concatenate this sequence with another.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>The concatenated sequence.</returns>
        public Seq<T> Concat(Seq<T> other)
        {
            Contract.AssertNotNull(other);
            return new Seq<T>(this.Values.AddRange(other.Values));
        }

        /// <summary>
        /// Concatenate this sequence with another.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The concatenated sequence.</returns>
        public Seq<T> Add(T element)
        {
            Contract.AssertNotNull(element);
            return new Seq<T>(this.Values.Add(element));
        }

        /// <summary>
        /// Convert the seq to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "[" + string.Join(", ", this.Values) + "]";
        }

        /// <summary>
        /// Equality for seqs.
        /// </summary>
        /// <param name="obj">The other seq.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Seq<T> o && Equals(o);
        }

        /// <summary>
        /// Equality for seqs.
        /// </summary>
        /// <param name="other">The other seq.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Seq<T> other)
        {
            if (this.Values.Count != other.Values.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Values.Count; i++)
            {
                if (!this.Values[i].Equals(other.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Hashcode for seq.
        /// </summary>
        /// <returns>Hashcode for seqs.</returns>
        public override int GetHashCode()
        {
            var hashCode = 1291433875;
            foreach (var v in this.Values)
            {
                hashCode += v.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Equality for seqs.
        /// </summary>
        /// <param name="left">The left seq.</param>
        /// <param name="right">The right seq.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(Seq<T> left, Seq<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for sets.
        /// </summary>
        /// <param name="left">The left seq.</param>
        /// <param name="right">The right seq.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(Seq<T> left, Seq<T> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Static factory class for seq Zen objects.
    /// </summary>
    public static class Seq
    {
        /// <summary>
        /// Create a char sequence from a string.
        /// The chars correspond to the Unicode values.
        /// </summary>
        /// <param name="s">The string value.</param>
        /// <returns>A sequence of bytes.</returns>
        public static Seq<char> FromString(string s)
        {
            var result = new Seq<char>();
            for (var i = 0; i < s.Length; i++)
            {
                result = result.Concat(new Seq<char>(s[i]));
            }

            return result;
        }

        /// <summary>
        /// Create a string from a Char sequence.
        /// The bytes correspond to the Unicode values.
        /// </summary>
        /// <param name="seq">The sequence of bytes.</param>
        /// <returns>The string for the bytes.</returns>
        public static string AsString(this Seq<char> seq)
        {
            return string.Join(string.Empty, seq.Values.Select(c => CommonUtilities.CharToString(c)));
        }

        /// <summary>
        /// The Zen value for an empty seq.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Empty<T>()
        {
            return Zen.Constant(new Seq<T>());
        }

        /// <summary>
        /// The Zen value for a singleton seq.
        /// </summary>
        /// <param name="value">The single element value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Unit<T>(Zen<T> value)
        {
            Contract.AssertNotNull(value);

            return ZenSeqUnitExpr<T>.Create(value);
        }

        /// <summary>
        /// Concatenate two Zen sequences.
        /// </summary>
        /// <param name="seqExpr1">The first sequence.</param>
        /// <param name="seqExpr2">The second sequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Concat<T>(this Zen<Seq<T>> seqExpr1, Zen<Seq<T>> seqExpr2)
        {
            Contract.AssertNotNull(seqExpr1);
            Contract.AssertNotNull(seqExpr2);

            return ZenSeqConcatExpr<T>.Create(seqExpr1, seqExpr2);
        }

        /// <summary>
        /// Concatenate two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The first sequence.</param>
        /// <param name="expr">The other expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Add<T>(this Zen<Seq<T>> seqExpr, Zen<T> expr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(expr);

            return ZenSeqConcatExpr<T>.Create(seqExpr, Seq.Unit(expr));
        }

        /// <summary>
        /// Get the length of a sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Length<T>(this Zen<Seq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return ZenSeqLengthExpr<T>.Create(seqExpr);
        }

        /// <summary>
        /// Gets the singleton subsequence at an index, or the empty sequence if out of bounds.
        /// </summary>
        /// <param name="seqExpr">The sequence expr.</param>
        /// <param name="indexExpr">The index expr.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> At<T>(this Zen<Seq<T>> seqExpr, Zen<BigInteger> indexExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(indexExpr);

            return ZenSeqAtExpr<T>.Create(seqExpr, indexExpr);
        }

        /// <summary>
        /// Determines if the sequence is empty.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsEmpty<T>(this Zen<Seq<T>> seqExpr)
        {
            Contract.AssertNotNull(seqExpr);

            return seqExpr.Length() == BigInteger.Zero;
        }

        /// <summary>
        /// Containment for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="valueExpr">The value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<Seq<T>> seqExpr, Zen<T> valueExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(valueExpr);

            return seqExpr.Contains(Seq.Unit(valueExpr));
        }

        /// <summary>
        /// Containment for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(subseqExpr);

            return ZenSeqContainsExpr<T>.Create(seqExpr, subseqExpr, SeqContainmentType.Contains);
        }

        /// <summary>
        /// HasPrefix for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> StartsWith<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(subseqExpr);

            return ZenSeqContainsExpr<T>.Create(seqExpr, subseqExpr, SeqContainmentType.HasPrefix);
        }

        /// <summary>
        /// HasSuffix for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> EndsWith<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(subseqExpr);

            return ZenSeqContainsExpr<T>.Create(seqExpr, subseqExpr, SeqContainmentType.HasSuffix);
        }

        /// <summary>
        /// IndexOf for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> IndexOf<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(subseqExpr);

            return ZenSeqIndexOfExpr<T>.Create(seqExpr, subseqExpr, BigInteger.Zero);
        }

        /// <summary>
        /// IndexOf for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> IndexOf<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr, Zen<BigInteger> offset)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(subseqExpr);
            Contract.AssertNotNull(offset);

            return ZenSeqIndexOfExpr<T>.Create(seqExpr, subseqExpr, offset);
        }

        /// <summary>
        /// Slice of a Zen sequences at an offset and with a length.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Slice<T>(this Zen<Seq<T>> seqExpr, Zen<BigInteger> offset, Zen<BigInteger> length)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(offset);
            Contract.AssertNotNull(length);

            return ZenSeqSliceExpr<T>.Create(seqExpr, offset, length);
        }

        /// <summary>
        /// ReplaceFirst for a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence to match.</param>
        /// <param name="replaceExpr">The replacement sequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> ReplaceFirst<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr, Zen<Seq<T>> replaceExpr)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(subseqExpr);
            Contract.AssertNotNull(replaceExpr);

            return ZenSeqReplaceFirstExpr<T>.Create(seqExpr, subseqExpr, replaceExpr);
        }

        /// <summary>
        /// Regex match for a Zen sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="regex">The regex to match.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> MatchesRegex<T>(this Zen<Seq<T>> seqExpr, Regex<T> regex)
        {
            Contract.AssertNotNull(seqExpr);
            Contract.AssertNotNull(regex);

            return ZenSeqRegexExpr<T>.Create(seqExpr, regex);
        }
    }
}
