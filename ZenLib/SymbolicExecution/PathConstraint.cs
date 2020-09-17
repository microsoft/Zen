// <copyright file="PathExplorerEnvironment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System.Collections.Immutable;

    /// <summary>
    /// A path constraint.
    /// </summary>
    internal class PathConstraint
    {
        /// <summary>
        /// Gets the current path constraint.
        /// </summary>
        public ImmutableHashSet<Zen<bool>> Conjuncts { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicExecution.PathConstraint"/> class.
        /// </summary>
        public PathConstraint()
        {
            this.Conjuncts = ImmutableHashSet<Zen<bool>>.Empty;
        }

        /// <summary>
        /// Appends a path constraint and returns a new environment.
        /// </summary>
        /// <param name="pathConstraint">The path constraint to add.</param>
        /// <returns>The new path constraint with the constraints added.</returns>
        public PathConstraint AddPathConstraint(PathConstraint pathConstraint)
        {
            return new PathConstraint(this.Conjuncts.Union(pathConstraint.Conjuncts));
        }

        /// <summary>
        /// Appends a path constraint and returns a new environment.
        /// </summary>
        /// <param name="conjunct">The conjunct to add.</param>
        /// <returns>The new path constraint with the conjunct added.</returns>
        public PathConstraint AddPathConstraint(Zen<bool> conjunct)
        {
            return new PathConstraint(this.Conjuncts.Add(conjunct));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicExecution.PathConstraint"/> class.
        /// </summary>
        /// <param name="pathConstraint"></param>
        public PathConstraint(ImmutableHashSet<Zen<bool>> pathConstraint)
        {
            this.Conjuncts = pathConstraint;
        }
    }
}
