// <copyright file="InterleavingClass.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

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
        [ExcludeFromCodeCoverage] // we never combine classes.
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
            var o = (InterleavingClass)other;
            var result = this.Fields;
            foreach (var fieldVariableSetPair in this.Fields)
            {
                result = result.SetItem(fieldVariableSetPair.Key, fieldVariableSetPair.Value.Union(o.Fields[fieldVariableSetPair.Key]));
            }

            return new InterleavingClass(result);
        }

        /// <summary>
        /// Combine variables.
        /// </summary>
        /// <param name="other">The other result.</param>
        /// <param name="objects">The interleaved objects.</param>
        public override void Combine(InterleavingResult other, UnionFind<object> objects)
        {
            var o = (InterleavingClass)other;
            foreach (var field in this.Fields)
            {
                var r1 = field.Value;
                var r2 = o.Fields[field.Key];
                r1.Combine(r2, objects);
            }
        }
    }
}
