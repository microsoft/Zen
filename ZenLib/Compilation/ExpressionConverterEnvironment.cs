// <copyright file="InterpreterEnvironment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Compilation
{
    using System.Collections.Immutable;
    using System.Linq.Expressions;

    /// <summary>
    /// An environment for the interpreter.
    /// </summary>
    internal class ExpressionConverterEnvironment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionConverterEnvironment"/> class.
        /// </summary>
        /// <param name="argumentAssignment">The initial argument assignment.</param>
        public ExpressionConverterEnvironment(ImmutableDictionary<long, Expression> argumentAssignment)
        {
            this.ArgumentAssignment = argumentAssignment;
        }

        /// <summary>
        /// Gets the argument assignment.
        /// </summary>
        public ImmutableDictionary<long, Expression> ArgumentAssignment { get; }
    }
}
