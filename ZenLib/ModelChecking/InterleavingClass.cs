// <copyright file="InterleavingClass.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Immutable;

    /// <summary>
    /// Representation of interleaving information for an object result.
    /// </summary>
    internal class InterleavingClass : InterleavingResult
    {
        /// <summary>
        /// Gets the underlying decision diagram bitvector representation.
        /// </summary>
        public ImmutableDictionary<string, InterleavingResult> Fields { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="InterleavingClass"/> class.
        /// </summary>
        /// <param name="value">The mapping from field to interleaving result.</param>
        public InterleavingClass(ImmutableDictionary<string, InterleavingResult> value) : base()
        {
            this.Fields = value;
        }

        /// <summary>
        /// Gets all the possible variables the result could have.
        /// </summary>
        /// <returns>The variables as a set.</returns>
        public override ImmutableHashSet<object> GetAllVariables()
        {
            var variables = ImmutableHashSet<object>.Empty;
            foreach (var fieldResultPair in this.Fields)
            {
                variables = variables.Union(fieldResultPair.Value.GetAllVariables());
            }

            return variables;
        }

        /// <summary>
        /// Unions the two interleaving results.
        /// </summary>
        /// <returns>A new interleaving result.</returns>
        public override InterleavingResult Union(InterleavingResult other)
        {
            if (other is InterleavingClass o)
            {
                var result = this.Fields;
                foreach (var fieldVariableSetPair in this.Fields)
                {
                    result = result.SetItem(fieldVariableSetPair.Key, fieldVariableSetPair.Value.Union(o.Fields[fieldVariableSetPair.Key]));
                }

                return new InterleavingClass(result);
            }

            return new InterleavingSet(this.GetAllVariables().Union(other.GetAllVariables()));
        }
    }
}
