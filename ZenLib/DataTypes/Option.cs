// <copyright file="Option.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using ZenLib.Generation;
    using static ZenLib.Zen;

    /// <summary>
    /// A simple option type to parameterize over nullable
    /// values for both structs and classes.
    /// </summary>
    public struct Option<T> : IEquatable<Option<T>>
    {
        internal Option(bool b, T value) : this()
        {
            this.HasValue = b;
            this.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this option has a value.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Return the underlying value or another value if none.
        /// </summary>
        /// <param name="other">The default value.</param>
        /// <returns>A value of the underlying type.</returns>
        public T ValueOrDefault(T other)
        {
            if (this.HasValue)
            {
                return this.Value;
            }

            return other;
        }

        /// <summary>
        /// Map a function over an option.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>A new option with the function mapped over the value.</returns>
        public Option<TResult> Select<TResult>(Func<T, TResult> function)
        {
            Contract.AssertNotNull(function);
            return this.HasValue ? Option.Some(function(this.Value)) : Option.None<TResult>();
        }

        /// <summary>
        /// Filter an option with a predicate.
        /// </summary>
        /// <param name="function">The predicate function.</param>
        /// <returns>A new option that is filtered to None if the predicate matches.</returns>
        public Option<T> Where(Func<T, bool> function)
        {
            Contract.AssertNotNull(function);

            if (this.HasValue && !function(this.Value))
            {
                return Option.None<T>();
            }

            return this;
        }

        /// <summary>
        /// Determines if this option is none.
        /// </summary>
        /// <returns>True if the option has no value.</returns>
        public bool IsNone()
        {
            return !this.HasValue;
        }

        /// <summary>
        /// Determines if this option is some.
        /// </summary>
        /// <returns>True if the option has a value.</returns>
        public bool IsSome()
        {
            return this.HasValue;
        }

        /// <summary>
        /// Convert the option to a sequence with zero or one element.
        /// </summary>
        /// <returns>A sequence.</returns>
        public FSeq<T> ToSequence()
        {
            return this.HasValue ? new FSeq<T>().AddFront(this.Value)  : new FSeq<T>();
        }

        /// <summary>
        /// Convert the option to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (this.HasValue)
            {
                return $"Some({this.Value})";
            }

            return "None";
        }

        /// <summary>
        /// Equality between option types.
        /// </summary>
        /// <param name="obj">The other option.</param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is Option<T> o && this.Equals(o);
        }

        /// <summary>
        /// Equality between option types.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns>True or false.</returns>
        public bool Equals(Option<T> other)
        {
            if (!this.HasValue && !other.HasValue)
            {
                return true;
            }

            if (this.HasValue && other.HasValue && this.Value.Equals(other.Value))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the hashcode for the optional value.
        /// </summary>
        /// <returns>An integer.</returns>
        public override int GetHashCode()
        {
            int hashCode = 299404170;
            hashCode = hashCode * -1521134295 + HasValue.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            return hashCode;
        }
    }

    /// <summary>
    /// Builder class for options.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Return some value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>An option that has some value.</returns>
        public static Option<T> Some<T>(T value)
        {
            return new Option<T>(true, value);
        }

        /// <summary>
        /// Return some value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>An option that has some value.</returns>
        public static Option<T> None<T>()
        {
            return new Option<T>(false, default);
        }

        /// <summary>
        /// The Zen value for null.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Null<T>()
        {
            return Zen.Create<Option<T>>(("HasValue", False()), ("Value", Zen.Default<T>()));
        }

        /// <summary>
        /// The Zen expression for Some value.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Create<T>(Zen<T> expr)
        {
            Contract.AssertNotNull(expr);

            return Zen.Create<Option<T>>(("HasValue", True()), ("Value", expr));
        }

        /// <summary>
        /// The Zen expression for mapping over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T2>> Select<T1, T2>(this Zen<Option<T1>> expr, Func<Zen<T1>, Zen<T2>> function)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(function);

            return If(expr.IsSome(), Option.Create(function(expr.Value())), Option.Null<T2>());
        }

        /// <summary>
        /// The Zen expression for filtering over an option.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="function">The function.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Option<T>> Where<T>(this Zen<Option<T>> expr, Func<Zen<T>, Zen<bool>> function)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(function);

            return If(And(expr.IsSome(), function(expr.Value())), expr, Option.Null<T>());
        }

        /// <summary>
        /// The Zen expression for an option or a default if no value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="deflt">The default value.</param>
        /// <returns>Zen value.</returns>
        public static Zen<T> ValueOrDefault<T>(this Zen<Option<T>> expr, Zen<T> deflt)
        {
            Contract.AssertNotNull(expr);
            Contract.AssertNotNull(deflt);

            return If(expr.IsSome(), expr.Value(), deflt);
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsSome<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            return expr.GetField<Option<T>, bool>("HasValue");
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> IsNone<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            return Not(expr.IsSome());
        }

        /// <summary>
        /// The Zen expression for whether an option has a value.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<FSeq<T>> ToSequence<T>(this Zen<Option<T>> expr)
        {
            Contract.AssertNotNull(expr);

            var l = FSeq.Empty<T>();
            return If(expr.IsSome(), l.AddFront(expr.Value()), l);
        }
    }
}
