// <copyright file="Set.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
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

        private Seq(ImmutableList<T> values)
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
        /// Determines if this sequence contains the other subsequence.
        /// </summary>
        /// <param name="other">The other sequence.</param>
        /// <returns>True if the other sequence is a subsequence of this one.</returns>
        public bool Contains(Seq<T> other)
        {
            if (other.Length() > this.Length())
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
        private Option<T> AtBigInteger(BigInteger index)
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
    }

    /// <summary>
    /// Extension methods for Zen seq objects.
    /// </summary>
    public static class SeqExtensions
    {
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
        public static Zen<bool> HasPrefix<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
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
        public static Zen<bool> HasSuffix<T>(this Zen<Seq<T>> seqExpr, Zen<Seq<T>> subseqExpr)
        {
            CommonUtilities.ValidateNotNull(seqExpr);
            CommonUtilities.ValidateNotNull(subseqExpr);

            return ZenSeqContainsExpr<T>.Create(seqExpr, subseqExpr, SeqContainmentType.HasSuffix);
        }
    }
}
