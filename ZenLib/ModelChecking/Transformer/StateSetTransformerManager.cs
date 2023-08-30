// <copyright file="StateSetTransformerManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System;
    using System.Collections.Generic;
    using DecisionDiagrams;

    /// <summary>
    /// Manager object for transformers. Operations are only allowed between
    /// state sets and transformers with the same manager.
    /// </summary>
    public class StateSetTransformerManager
    {
        /// <summary>
        /// Manager object for building transformers.
        /// </summary>
        internal DDManager<BDDNode> DecisionDiagramManager;

        /// <summary>
        /// Canonical variables used for a given type.
        /// </summary>
        internal Dictionary<Type, StateSetMetadata> CanonicalValues;

        /// <summary>
        /// Keep track of, for each type, if there is an output
        /// of that type that is dependency free.
        /// </summary>
        internal Dictionary<Type, VariableSet<BDDNode>> DependencyFreeOutput;

        /// <summary>
        /// Cache for transformers.
        /// </summary>
        internal FiniteCache<(Type, long), object> TransformerCache;

        /// <summary>
        /// Cache for state sets.
        /// </summary>
        internal FiniteCache<(Type, long), object> StateSetCache;

        /// <summary>
        /// Creates a new instance of the <see cref="StateSetTransformerManager"/> class.
        /// </summary>
        /// <param name="cacheSize">The maximum cache size.</param>
        public StateSetTransformerManager(int cacheSize = 1024)
        {
            this.DecisionDiagramManager = new DDManager<BDDNode>();
            this.CanonicalValues = new Dictionary<Type, StateSetMetadata>();
            this.DependencyFreeOutput = new Dictionary<Type, VariableSet<BDDNode>>();
            this.TransformerCache = new FiniteCache<(Type, long), object>(cacheSize);
            this.StateSetCache = new FiniteCache<(Type, long), object>(cacheSize);
        }
    }
}
