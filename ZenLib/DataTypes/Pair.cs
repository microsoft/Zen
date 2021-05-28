// <copyright file="Tuple.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        public bool Equals(Pair<T1, T2> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2})";
        }
    }

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2, T3> : IEquatable<Pair<T1, T2, T3>>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     The third item.
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2, T3> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        public bool Equals(Pair<T1, T2, T3> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2) &&
                   this.Item3.Equals(other.Item3);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2}, {this.Item3})";
        }
    }

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2, T3, T4> : IEquatable<Pair<T1, T2, T3, T4>>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     The third item.
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        ///     The fourth item.
        /// </summary>
        public T4 Item4 { get; set; }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2, T3, T4> p && Equals(p);
        }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        public bool Equals(Pair<T1, T2, T3, T4> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2) &&
                   this.Item3.Equals(other.Item3) &&
                   this.Item4.Equals(other.Item4);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            hashCode = hashCode * -1521134295 + EqualityComparer<T4>.Default.GetHashCode(Item4);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4})";
        }
    }

    /// <summary>
    /// A simple tuple type that works with Zen.
    /// </summary>
    public class Pair<T1, T2, T3, T4, T5>
    {
        /// <summary>
        ///     The first item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        ///     The second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        ///     The third item.
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        ///     The fourth item.
        /// </summary>
        public T4 Item4 { get; set; }

        /// <summary>
        ///     The fifth item.
        /// </summary>
        public T5 Item5 { get; set; }

        /// <summary>
        ///     Equality for pairs.
        /// </summary>
        /// <param name="other">The other pair.</param>
        /// <returns></returns>
        public bool Equals(Pair<T1, T2, T3, T4, T5> other)
        {
            return other != null &&
                   this.Item1.Equals(other.Item1) &&
                   this.Item2.Equals(other.Item2) &&
                   this.Item3.Equals(other.Item3) &&
                   this.Item4.Equals(other.Item4) &&
                   this.Item5.Equals(other.Item5);
        }

        /// <summary>
        ///     Hashcode for pair.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            hashCode = hashCode * -1521134295 + EqualityComparer<T4>.Default.GetHashCode(Item4);
            hashCode = hashCode * -1521134295 + EqualityComparer<T5>.Default.GetHashCode(Item5);
            return hashCode;
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5})";
        }
    }
}
