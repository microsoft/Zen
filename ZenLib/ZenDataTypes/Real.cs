// <copyright file="Real.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    /// <summary>
    /// A real number represented as a rational.
    /// </summary>
    public struct Real : IEquatable<Real>, IComparable<Real>
    {
        /// <summary>
        /// The numerator.
        /// </summary>
        internal BigInteger Numerator;

        /// <summary>
        /// The denominator.
        /// </summary>
        internal BigInteger Denominator;

        /// <summary>
        /// Convert a C# long to a Real.
        /// </summary>
        /// <param name="l">The value.</param>
        public static implicit operator Real(long l)
        {
            return new Real(new BigInteger(l), BigInteger.One);
        }

        /// <summary>
        /// Add two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static Real operator +(Real r1, Real r2)
        {
            return r1.Add(r2);
        }

        /// <summary>
        /// Subtract two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static Real operator -(Real r1, Real r2)
        {
            return r1.Subtract(r2);
        }

        /// <summary>
        /// Multiply two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static Real operator *(Real r1, Real r2)
        {
            return r1.Multiply(r2);
        }

        /// <summary>
        /// Compare two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static bool operator >=(Real r1, Real r2)
        {
            return r1.CompareTo(r2) >= 0;
        }

        /// <summary>
        /// Compare two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static bool operator <=(Real r1, Real r2)
        {
            return r1.CompareTo(r2) <= 0;
        }

        /// <summary>
        /// Compare two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static bool operator >(Real r1, Real r2)
        {
            return r1.CompareTo(r2) > 0;
        }

        /// <summary>
        /// Compare two real numbers.
        /// </summary>
        /// <param name="r1">The first real.</param>
        /// <param name="r2">The second real.</param>
        public static bool operator <(Real r1, Real r2)
        {
            return r1.CompareTo(r2) < 0;
        }

        /// <summary>
        /// Instantiate a new <see cref="Real"/> value.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        public Real(BigInteger numerator, BigInteger denominator)
        {
            CommonUtilities.ValidateIsTrue(denominator != BigInteger.Zero, "denominator can not be zero.");

            if (denominator < BigInteger.Zero)
            {
                numerator = BigInteger.Negate(numerator);
                denominator = BigInteger.Negate(denominator);
            }

            var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
            if (gcd != BigInteger.One)
            {
                numerator = BigInteger.Divide(numerator, gcd);
                denominator = BigInteger.Divide(denominator, gcd);
            }

            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        /// <summary>
        /// Add two real numbers.
        /// </summary>
        /// <param name="other">The other real number.</param>
        /// <returns>The resulting real number.</returns>
        public Real Add(Real other)
        {
            var numerator = this.Numerator * other.Denominator + other.Numerator * this.Denominator;
            var denominator = this.Denominator * other.Denominator;
            return new Real(numerator, denominator);
        }

        /// <summary>
        /// Subtract two real numbers.
        /// </summary>
        /// <param name="other">The other real number.</param>
        /// <returns>The resulting real number.</returns>
        public Real Subtract(Real other)
        {
            var numerator = this.Numerator * other.Denominator - other.Numerator * this.Denominator;
            var denominator = this.Denominator * other.Denominator;
            return new Real(numerator, denominator);
        }

        /// <summary>
        /// Multiply two real numbers.
        /// </summary>
        /// <param name="other">The other real number.</param>
        /// <returns>The resulting real number.</returns>
        public Real Multiply(Real other)
        {
            var numerator = this.Numerator * other.Numerator;
            var denominator = this.Denominator * other.Denominator;
            return new Real(numerator, denominator);
        }

        /// <summary>
        /// Equality for characters.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Real r && this.Equals(r);
        }

        /// <summary>
        /// Equality for chars.
        /// </summary>
        /// <param name="other">The other char.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Real other)
        {
            return this.Numerator.Equals(other.Numerator) && this.Denominator.Equals(other.Denominator);
        }

        /// <summary>
        /// Hashcode for characters.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            return this.Numerator.GetHashCode() + this.Denominator.GetHashCode();
        }

        /// <summary>
        /// Convert this Real to a string.
        /// </summary>
        /// <returns>A string representing the value.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (this.Denominator == BigInteger.One)
            {
                return $"{this.Numerator}.0";
            }

            return $"{this.Numerator}.0/{this.Denominator}.0";
        }

        /// <summary>
        /// Compare this character to another.
        /// </summary>
        /// <param name="other">The other character.</param>
        /// <returns>An int representing the result.</returns>
        public int CompareTo(Real other)
        {
            return (this.Numerator * other.Denominator).CompareTo(other.Numerator * this.Denominator);
        }

        /// <summary>
        /// Equality for reals.
        /// </summary>
        /// <param name="left">The left real.</param>
        /// <param name="right">The right real.</param>
        /// <returns>True or false.</returns>
        public static bool operator ==(Real left, Real right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality for reals.
        /// </summary>
        /// <param name="left">The left real.</param>
        /// <param name="right">The right real.</param>
        /// <returns>True or false.</returns>
        public static bool operator !=(Real left, Real right)
        {
            return !(left == right);
        }
    }
}
