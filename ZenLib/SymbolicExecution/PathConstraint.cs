// <copyright file="PathConstraint.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// A path constraint.
    /// </summary>
    internal class PathConstraint
    {
        /// <summary>
        /// Gets the current path constraint.
        /// </summary>
        public ImmutableList<Zen<bool>> Conjuncts { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicExecution.PathConstraint"/> class.
        /// </summary>
        public PathConstraint()
        {
            this.Conjuncts = ImmutableList<Zen<bool>>.Empty;
        }

        /// <summary>
        /// Add a guard to the path constraint.
        /// </summary>
        /// <param name="guard">The guard.</param>
        /// <returns>A new path constraint.</returns>
        public PathConstraint Add(Zen<bool> guard)
        {
            return new PathConstraint(this.Conjuncts.Add(guard));
        }

        /// <summary>
        /// Get a new path constraint from a range of conjuncts.
        /// </summary>
        /// <param name="i">The lower index.</param>
        /// <param name="j">The upper index.</param>
        /// <returns>A new path constraint.</returns>
        public PathConstraint GetRange(int i, int j)
        {
            if (j < 0)
            {
                return new PathConstraint();
            }

            var conjuncts = this.Conjuncts.GetRange(i, j - i + 1);
            return new PathConstraint(conjuncts);
        }

        /// <summary>
        /// Gets a Zen expression from a path constraint.
        /// </summary>
        /// <returns>The Zen expression.</returns>
        public Zen<bool> GetExpr()
        {
            return Basic.And(this.Conjuncts.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicExecution.PathConstraint"/> class.
        /// </summary>
        /// <param name="pathConstraint">The conjuncts.</param>
        private PathConstraint(ImmutableList<Zen<bool>> pathConstraint)
        {
            this.Conjuncts = pathConstraint;
        }
    }
}
