// <copyright file="Automaton.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A deterministic automaton with respect to a character type T.
    /// </summary>
    /// <typeparam name="T">The type of characters for the automaton.</typeparam>
    public class Automaton<T> where T : IComparable<T>
    {
        /// <summary>
        /// The initial state of the automaton.
        /// </summary>
        internal Regex<T> InitialState { get; set; }

        /// <summary>
        /// The states of the automaton.
        /// </summary>
        internal ISet<Regex<T>> States { get; set; }

        /// <summary>
        /// The final states of the automaton.
        /// </summary>
        internal ISet<Regex<T>> FinalStates { get; set; }

        /// <summary>
        /// The transitions of the automaton.
        /// </summary>
        internal Dictionary<Regex<T>, Dictionary<T, IList<Regex<T>>>> Transitions { get; set; }
    }

    /// <summary>
    /// Static factory class for automata.
    /// </summary>
    public static class Automaton
    {
        /// <summary>
        /// Create an automaton from a regular expression.
        /// </summary>
        /// <param name="regex">The regular expresson.</param>
        /// <returns>An automaton accepting the same strings as the regex.</returns>
        public static Automaton<T> FromRegex<T>(Regex<T> regex) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }
    }
}
