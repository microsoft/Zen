// <copyright file="Set.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static ZenLib.Zen;

    /// <summary>
    /// A class representing an arbitrary sized set.
    /// </summary>
    public class Set<T>
    {
        /// <summary>
        /// Gets the underlying values of the backing map.
        /// </summary>
        public Map<T, bool> Values { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Set{TKey}"/> class.
        /// </summary>
        public Set()
        {
            this.Values = new Map<T, bool>();
        }

        private Set(Map<T, bool> map)
        {
            this.Values = map;
        }

        /// <summary>
        /// The number of elements in the map.
        /// </summary>
        public int Count() { return this.Values.Count(); }

        /// <summary>
        /// Add an element to the set.
        /// </summary>
        /// <param name="elt">The element to add.</param>
        public Set<T> Add(T elt)
        {
            return new Set<T>(this.Values.Set(elt, true));
        }

        /// <summary>
        /// Delete an element from the Set.
        /// </summary>
        /// <param name="elt">The element to remove.</param>
        public Set<T> Delete(T elt)
        {
            return new Set<T>(this.Values.Delete(elt));
        }

        /// <summary>
        /// Check if the set contains a value.
        /// </summary>
        /// <param name="elt">The given element.</param>
        /// <returns>True or false.</returns>
        public bool Contains(T elt)
        {
            return this.Values.ContainsKey(elt);
        }

        /// <summary>
        /// Convert the set to a string.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{" + string.Join(", ", this.Values.Values.Select(kv => kv.Key)) + "}";
        }
    }

    /// <summary>
    /// Static factory class for map Zen objects.
    /// </summary>
    public static class Set
    {
        /// <summary>
        /// The Zen value for an empty set.
        /// </summary>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Empty<T>()
        {
            return Create<Set<T>>(("Values", Map.Empty<T, bool>()));
        }
    }

    /// <summary>
    /// Extension methods for Zen set objects.
    /// </summary>
    public static class SetExtensions
    {
        /// <summary>
        /// The underlying map.
        /// </summary>
        /// <param name="setExpr">The set expr.</param>
        /// <returns>Zen value.</returns>
        internal static Zen<Map<T, bool>> Values<T>(this Zen<Set<T>> setExpr)
        {
            return setExpr.GetField<Set<T>, Map<T, bool>>("Values");
        }

        /// <summary>
        /// Add a value to a Zen set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="elementExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Add<T>(this Zen<Set<T>> setExpr, Zen<T> elementExpr)
        {
            CommonUtilities.ValidateNotNull(setExpr);
            CommonUtilities.ValidateNotNull(elementExpr);

            return Create<Set<T>>(("Values", setExpr.Values().Set(elementExpr, true)));
        }

        /// <summary>
        /// Delete a value from a Zen set.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="elementExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<Set<T>> Delete<T>(this Zen<Set<T>> setExpr, Zen<T> elementExpr)
        {
            CommonUtilities.ValidateNotNull(setExpr);
            CommonUtilities.ValidateNotNull(elementExpr);

            return Create<Set<T>>(("Values", setExpr.Values().Delete(elementExpr)));
        }

        /// <summary>
        /// Check if a Zen set contains an element.
        /// </summary>
        /// <param name="setExpr">Zen set expression.</param>
        /// <param name="elementExpr">Zen key expression.</param>
        /// <returns>Zen value.</returns>
        public static Zen<bool> Contains<T>(this Zen<Set<T>> setExpr, Zen<T> elementExpr)
        {
            CommonUtilities.ValidateNotNull(setExpr);
            CommonUtilities.ValidateNotNull(elementExpr);

            // TODO: need to use unit type for values or this will not always work as expected.
            return setExpr.Values().ContainsKey(elementExpr);
        }
    }
}
