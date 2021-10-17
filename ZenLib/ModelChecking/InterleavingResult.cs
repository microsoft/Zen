// <copyright file="InterleavingResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Immutable;

    /// <summary>
    /// Representation of a an interleaving result.
    /// </summary>
    internal abstract class InterleavingResult
    {
        /// <summary>
        /// Gets all the possible variables the result could have.
        /// </summary>
        /// <returns>The variables as a set.</returns>
        public abstract ImmutableHashSet<object> GetAllVariables();

        /// <summary>
        /// Unions the two interleaving results.
        /// </summary>
        /// <returns>A new interleaving result.</returns>
        public abstract InterleavingResult Union(InterleavingResult other);
    }
}
