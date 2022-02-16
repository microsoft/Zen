// <copyright file="Set.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

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
    }
}
