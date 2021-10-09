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
        internal DDManager<BDDNode> DecisionDiagramManager = new DDManager<BDDNode>(new BDDNodeFactory());

        /// <summary>
        /// Canonical variables used for a given type.
        /// </summary>
        internal Dictionary<Type, StateSetMetadata> CanonicalValues = new Dictionary<Type, StateSetMetadata>();

        /// <summary>
        /// Keep track of, for each type, if there is an output
        /// of that type that is dependency free.
        /// </summary>
        internal Dictionary<Type, VariableSet<BDDNode>> DependencyFreeOutput = new Dictionary<Type, VariableSet<BDDNode>>();
    }
}
