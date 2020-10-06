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
    internal class PathConstraint : IEquatable<PathConstraint>
    {
        /// <summary>
        /// Gets the current path constraint.
        /// </summary>
        public ImmutableHashSet<Zen<bool>> Conjuncts { get; }

        /// <summary>
        /// The path constraints as a Zen expression.
        /// </summary>
        public Zen<bool> Expr { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicExecution.PathConstraint"/> class.
        /// </summary>
        public PathConstraint()
        {
            this.Conjuncts = ImmutableHashSet<Zen<bool>>.Empty;
            this.Expr = true;
        }

        /// <summary>
        /// Appends a path constraint and returns a new environment.
        /// </summary>
        /// <param name="pathConstraint">The path constraint to add.</param>
        /// <returns>The new path constraint with the constraints added.</returns>
        public PathConstraint AddPathConstraint(PathConstraint pathConstraint)
        {
            return new PathConstraint(
                this.Conjuncts.Union(pathConstraint.Conjuncts),
                ZenAndExpr.Create(this.Expr, pathConstraint.Expr));
        }

        /// <summary>
        /// Appends a path constraint and returns a new environment.
        /// </summary>
        /// <param name="conjunct">The conjunct to add.</param>
        /// <returns>The new path constraint with the conjunct added.</returns>
        public PathConstraint AddPathConstraint(Zen<bool> conjunct)
        {
            return new PathConstraint(this.Conjuncts.Add(conjunct), ZenAndExpr.Create(this.Expr, conjunct));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicExecution.PathConstraint"/> class.
        /// </summary>
        /// <param name="pathConstraint">The conjuncts.</param>
        /// <param name="expr">The path constraint expression.</param>
        private PathConstraint(ImmutableHashSet<Zen<bool>> pathConstraint, Zen<bool> expr)
        {
            this.Conjuncts = pathConstraint;
            this.Expr = expr;
        }

        /// <summary>
        /// Equality between path constraints.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True or false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is PathConstraint o && this.Equals(o);
        }

        /// <summary>
        /// Equality between path constraints.
        /// </summary>
        /// <param name="other">The other constraint.</param>
        /// <returns>True or false.</returns>
        public bool Equals(PathConstraint other)
        {
            return this.Expr.Equals(other.Expr);
        }

        /// <summary>
        /// Gets the hash code for the path constraint.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return this.Expr.GetHashCode();
        }
    }
}
