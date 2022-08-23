// <copyright file="InterleavingSetEmptyVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class to help generate an empty interleaving set based on a type.
    /// </summary>
    internal class InterleavingSetEmptyVisitor : TypeVisitor<InterleavingResult, Unit>
    {
        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitBigInteger(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitBool(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitByte(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitChar(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="intType">The integer type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitFixedInteger(Type intType, Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitInt(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="innerType">The list element type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitList(Type listType, Type innerType, Unit parameter)
        {
            return this.Visit(typeof(Option<>).MakeGenericType(innerType), parameter);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitLong(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="mapType">The dictionary type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            throw new ZenException("Maps not supported in BDDs");
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="mapType">The dictionary type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitConstMap(Type mapType, Type keyType, Type valueType, Unit parameter)
        {
            throw new ZenException("Maps not supported in BDDs");
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name="fields">The fields and their types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitObject(Type objectType, SortedDictionary<string, Type> fields, Unit parameter)
        {
            var result = ImmutableDictionary<string, InterleavingResult>.Empty;
            foreach (var kv in fields)
            {
                result = result.Add(kv.Key, this.Visit(kv.Value, parameter));
            }

            return new InterleavingClass(result);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitReal(Unit parameter)
        {
            throw new ZenException("Reals not supported in BDDs");
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="sequenceType">The sequence type.</param>
        /// <param name="innerType">The type of the sequence elements.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitSeq(Type sequenceType, Type innerType, Unit parameter)
        {
            throw new ZenException("Seqs not supported in BDDs");
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitShort(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        [ExcludeFromCodeCoverage]
        public override InterleavingResult VisitString(Unit parameter)
        {
            throw new ZenException("Strings not supported in BDDs");
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitUint(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitUlong(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }

        /// <summary>
        /// Generate an empty set result.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>An empty interleaving result set.</returns>
        public override InterleavingResult VisitUshort(Unit parameter)
        {
            return new InterleavingSet(ImmutableHashSet<object>.Empty);
        }
    }
}
