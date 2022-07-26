// <copyright file="InterleavingSet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Immutable;

    /// <summary>
    /// Representation of interleaving information for some base type.
    /// </summary>
    internal class InterleavingSet : InterleavingResult
    {
        /// <summary>
        /// The set of possible variables.
        /// </summary>
        public ImmutableHashSet<object> Variables { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="InterleavingSet"/> class.
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        public InterleavingSet(ImmutableHashSet<object> variables) : base()
        {
            this.Variables = variables;
        }

        /// <summary>
        /// Gets all the possible variables the result could have.
        /// </summary>
        /// <returns>The variables as a set.</returns>
        public override ImmutableHashSet<object> GetAllVariables()
        {
            return this.Variables;
        }

        /// <summary>
        /// Unions the two interleaving results.
        /// </summary>
        /// <returns>A new interleaving result.</returns>
        public override InterleavingResult Union(InterleavingResult other)
        {
            var o = (InterleavingSet)other;
            return new InterleavingSet(this.Variables.Union(o.Variables));
        }
    }
}
