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
    using System.Text;

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

        internal Seq(ImmutableList<T> values)
        {
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
            return new BigInteger(this.IndexOf(other, int.Parse(offset.ToString())));
        }

        /// <summary>
        /// Determines if this sequence contains the other subsequence.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>True if the other sequence is a subsequence of this one.</returns>
        public bool Contains(Seq<T> other)
        {
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

        private bool IsMatch(Seq<T> other, int index)
        {
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
        public Option<T> At(int index)
        {
            if (index >= this.Values.Count || index < 0)
            {
                return Option.None<T>();
            }

            return Option.Some(this.Values[index]);
        }

        /// <summary>
        /// Get the value at a given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value at the index.</returns>
        internal Option<T> AtBigInteger(BigInteger index)
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
            return new Seq<T>(this.Values.AddRange(other.Values));
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
        /// Create a byte sequence from a string.
        /// The bytes correspond to the ASCII values.
        /// </summary>
        /// <param name="s">The string value.</param>
        /// <returns>A sequence of bytes.</returns>
        public static Seq<byte> FromString(string s)
        {
            return new Seq<byte>(ImmutableList.CreateRange(Encoding.ASCII.GetBytes(s)));
        }

        /// <summary>
        /// Create a string from a byte sequence.
        /// The bytes correspond to the ASCII values.
        /// </summary>
        /// <param name="seq">The sequence of bytes.</param>
        /// <returns>The string for the bytes.</returns>
        public static string AsString(this Seq<byte> seq)
        {
            return Encoding.ASCII.GetString(seq.Values.ToArray());
        }

        /// <summary>
        /// The Zen value for an empty seq.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Empty<T>()
        {
            return ZenSeqEmptyExpr<T>.Instance;
        }

        /// <summary>
        /// The Zen value for a singleton seq.
        /// </summary>
        /// <param name="value">The single element value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Seq<T>> Unit<T>(Zen<T> value)
        {
            CommonUtilities.ValidateNotNull(value);

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
            CommonUtilities.ValidateNotNull(seqExpr1);
            CommonUtilities.ValidateNotNull(seqExpr2);

            return ZenSeqConcatExpr<T>.Create(seqExpr1, seqExpr2);
        }

        /// <summary>
        /// Get the length of a sequence.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<BigInteger> Length<T>(this Zen<Seq<T>> seqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);

            return ZenSeqLengthExpr<T>.Create(seqExpr);
        }

        /// <summary>
        /// Gets the singleton subsequence at an index, or the empty sequence if out of bounds.
        /// </summary>
        /// <param name="seqExpr">The sequence expr.</param>
        /// <param name="indexExpr">The index expr.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> At<T>(this Zen<Seq<T>> seqExpr, Zen<BigInteger> indexExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(indexExpr);

            return ZenSeqAtExpr<T>.Create(seqExpr, indexExpr);
        }

        /// <summary>
        /// Containment for two Zen sequences.
        /// </summary>
        /// <param name="seqExpr">The sequence.</param>
        /// <param name="subseqExpr">The subsequence.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);
            CommonUtilities.ValidateNotNull(offset);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(offset);
            CommonUtilities.ValidateNotNull(length);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);
            CommonUtilities.ValidateNotNull(replaceExpr);

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
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(regex);

            return ZenSeqRegexExpr<T>.Create(seqExpr, regex);
        }
    }
}
