// <copyright file="Automaton.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A deterministic automaton with respect to a character type T.
    /// </summary>
    /// <typeparam name="T">The type of characters for the automaton.</typeparam>
    public class Automaton<T>
    {
        /// <summary>
        /// The initial state of the automaton.
        /// </summary>
        public Regex<T> InitialState { get; }

        /// <summary>
        /// The states of the automaton.
        /// </summary>
        public ISet<Regex<T>> States { get; } = new HashSet<Regex<T>>();

        /// <summary>
        /// The final states of the automaton.
        /// </summary>
        public ISet<Regex<T>> FinalStates { get; } = new HashSet<Regex<T>>();

        /// <summary>
        /// The transitions of the automaton.
        /// </summary>
        public Dictionary<Regex<T>, Dictionary<CharRange<T>, Regex<T>>> Transitions { get; } = new Dictionary<Regex<T>, Dictionary<CharRange<T>, Regex<T>>>();

        /// <summary>
        /// Creates a new instance of the <see cref="Automaton{T}"/> class.
        /// </summary>
        /// <param name="initialState">The initial automaton state.</param>
        public Automaton(Regex<T> initialState)
        {
            InitialState = initialState;
        }

        /// <summary>
        /// Add a transition to the automaton.
        /// </summary>
        /// <param name="sourceState">The source state.</param>
        /// <param name="targetState">The target state.</param>
        /// <param name="characterClass">The character class.</param>
        public void AddTransition(Regex<T> sourceState, Regex<T> targetState, CharRange<T> characterClass)
        {
            if (!this.Transitions.TryGetValue(sourceState, out var keyValuePairs))
            {
                keyValuePairs = new Dictionary<CharRange<T>, Regex<T>>();
                this.Transitions[sourceState] = keyValuePairs;
            }

            keyValuePairs[characterClass] = targetState;
        }

        /// <summary>
        /// Determines if this automaton matches a sequence.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        /// <returns>True if the sequence leads to an accepting state.</returns>
        public bool IsMatch(IEnumerable<T> sequence)
        {
            var state = this.InitialState;
            foreach (var item in sequence)
            {
                state = this.MoveState(state, item);
            }

            return this.FinalStates.Contains(state);
        }

        /// <summary>
        /// Move from one state to another given a character value.
        /// </summary>
        /// <param name="state">The current state.</param>
        /// <param name="value">The character value.</param>
        /// <returns></returns>
        public Regex<T> MoveState(Regex<T> state, T value)
        {
            return this.Transitions[state].Where(x => x.Key.Contains(value)).First().Value;
        }

        /// <summary>
        /// Convert the automaton to a string.
        /// </summary>
        /// <returns>The automaton as a string.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var states = "{" + string.Join(",", this.States.Select(x => x.Id)) + "}";
            var finalStates = "{" + string.Join(",", this.FinalStates.Select(x => x.Id)) + "}";

            var transitions = new List<string>();
            foreach (var kv1 in this.Transitions)
            {
                foreach (var kv2 in kv1.Value)
                {
                    transitions.Add($"({kv1.Key.Id},{kv2.Key}) -> {kv2.Value.Id}");
                }
            }
            var transitionString = "{" + string.Join(", ", transitions) + "}";

            return $"Automaton(init={this.InitialState.Id}, states={states}, final={finalStates}, transitions={transitionString})";
        }
    }
}
