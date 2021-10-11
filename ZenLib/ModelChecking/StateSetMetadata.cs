// <copyright file="StateSetCanonicalData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;
    using DecisionDiagrams;

    /// <summary>
    /// Represents a canonical set of BDD variables and Zen variables for state
    /// set and transformation operations. To allow for different variable orderings,
    /// a type T may have its BDD variables interleaved or not with other variables.
    /// However, to ensure operations can be performed we convert to and from a canonical
    /// variable set per type before doing the operation. This class carries that information.
    /// </summary>
    internal class StateSetMetadata
    {
        /// <summary>
        /// Represents the Zen input or output AST, which carries the arbitrary variables.
        /// </summary>
        public object ZenParameter { get; set; }

        /// <summary>
        /// The BDD variable set associated with the Zen Parameter. Used for quantification.
        /// </summary>
        public VariableSet<BDDNode> VariableSet { get; set; }

        /// <summary>
        /// The mappping from Zen Arbitrary expression to BDD variable doing the encoding.
        /// </summary>
        public Dictionary<object, Variable<BDDNode>> ZenArbitraryMapping { get; set; }
    }
}
