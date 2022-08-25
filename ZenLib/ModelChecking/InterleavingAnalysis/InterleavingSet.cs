// <copyright file="InterleavingSet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Immutable;
    using System.Linq;

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

        /// <summary>
        /// Combine variables.
        /// </summary>
        /// <param name="other">The other result.</param>
        /// <param name="objects">The interleaved objects.</param>
        public override void Combine(InterleavingResult other, UnionFind<object> objects)
        {
            var o = (InterleavingSet)other;
            var variableSet1 = this.GetAllVariables();
            var variableSet2 = o.GetAllVariables();

            if (IsBoolVariableSet(variableSet1) || IsBoolVariableSet(variableSet2))
            {
                return;
            }

            foreach (var variable1 in variableSet1)
            {
                foreach (var variable2 in variableSet2)
                {
                    var type1 = variable1.GetType().GetGenericArgumentsCached()[0];
                    var type2 = variable2.GetType().GetGenericArgumentsCached()[0];

                    if (type1 == type2)
                    {
                        objects.Union(variable1, variable2);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a set of variables is comprised only of boolean values, which do not need interleaving.
        /// </summary>
        /// <param name="variableSet">The set of variables.</param>
        /// <returns>True or false.</returns>
        private bool IsBoolVariableSet(ImmutableHashSet<object> variableSet)
        {
            return variableSet.All(x => typeof(Zen<bool>).IsAssignableFrom(x.GetType()));
        }
    }
}
