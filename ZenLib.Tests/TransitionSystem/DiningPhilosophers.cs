// <copyright file="DiningPhilosophers.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using ZenLib;
    using ZenLib.TransitionSystem;

    /// <summary>
    /// A model for the dining philosophers problem.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DiningPhilosophers
    {
        /// <summary>
        /// The number of philosophers.
        /// </summary>
        private int numPhilosophers;

        /// <summary>
        /// Creates a new instance of the <see cref="DiningPhilosophers"/> class.
        /// </summary>
        /// <param name="numPhilosophers">The number of philosophers.</param>
        public DiningPhilosophers(int numPhilosophers)
        {
            this.numPhilosophers = numPhilosophers;
        }

        /// <summary>
        /// Creates an instance of the problem.
        /// </summary>
        /// <returns></returns>
        public TransitionSystem<State> GetProblem()
        {
            var someoneEating = Enumerable.Range(0, this.numPhilosophers)
                .Select(i => LTL.Predicate<State>(s => Zen.And(s.GetHasLeftFork().Get(i), s.GetHasRightFork().Get(i))))
                .Aggregate(LTL.Or);

            return new TransitionSystem<State>
            {
                InitialStates = this.InitialStates,
                Invariants = (s) => true,
                NextRelation = this.NextRelation,
                Specification = LTL.Always(LTL.Eventually(someoneEating)),
            };
        }

        /// <summary>
        /// Determines what a valid initial state is.
        /// </summary>
        private Zen<bool> InitialStates(Zen<State> state)
        {
            var c1 = Zen.And(Enumerable.Range(0, numPhilosophers).Select(i => Zen.Not(state.GetHasLeftFork().Get(i))));
            var c2 = Zen.And(Enumerable.Range(0, numPhilosophers).Select(i => Zen.Not(state.GetHasRightFork().Get(i))));
            return Zen.And(c1, c2);
        }

        /// <summary>
        /// The transition relation.
        /// </summary>
        private Zen<bool> NextRelation(Zen<State> oldState, Zen<State> newState)
        {
            var transition = Zen.False();
            var allStuck = Zen.True();
            for (int i = 0; i < this.numPhilosophers; i++)
            {
                var hasLeft = oldState.GetHasLeftFork().Get(i);
                var hasRight = oldState.GetHasRightFork().Get(i);
                var leftAvailable = Zen.Not(oldState.GetHasRightFork().Get(i - 1 < 0 ? this.numPhilosophers - 1 : i - 1));
                var rightAvailable = Zen.Not(oldState.GetHasLeftFork().Get((i + 1) % this.numPhilosophers));
                var isStuck = Zen.Or(Zen.And(hasLeft, Zen.Not(rightAvailable)), Zen.And(Zen.Not(hasLeft), Zen.Not(leftAvailable)));
                allStuck = Zen.And(allStuck, isStuck);

                var philosopherTransition =
                    Zen.Or(
                        Zen.And(hasLeft, hasRight, newState == PutDownLeftFork(PutDownRightFork(oldState, i), i)),
                        Zen.And(hasLeft, rightAvailable, newState == PickUpRightFork(oldState, i)),
                        Zen.And(Zen.Not(hasLeft), leftAvailable, newState == PickUpLeftFork(oldState, i)));

                transition = Zen.Or(transition, philosopherTransition);
            }

            return Zen.Or(transition, Zen.And(allStuck, newState == oldState));
        }

        /// <summary>
        /// Put down the left fork.
        /// </summary>
        private Zen<State> PutDownLeftFork(Zen<State> state, int philosopher) => state.WithHasLeftFork(state.GetHasLeftFork().Set(philosopher, false));

        /// <summary>
        /// Pick up the left fork.
        /// </summary>
        private Zen<State> PickUpLeftFork(Zen<State> state, int philosopher) => state.WithHasLeftFork(state.GetHasLeftFork().Set(philosopher, true));

        /// <summary>
        /// Put down the right fork.
        /// </summary>
        private Zen<State> PutDownRightFork(Zen<State> state, int philosopher) => state.WithHasRightFork(state.GetHasRightFork().Set(philosopher, false));

        /// <summary>
        /// Pick up the right fork.
        /// </summary>
        private Zen<State> PickUpRightFork(Zen<State> state, int philosopher) => state.WithHasRightFork(state.GetHasRightFork().Set(philosopher, true));
    }

    /// <summary>
    /// The state of the problem.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class State
    {
        /// <summary>
        /// Whether each philosopher has the left fork.
        /// </summary>
        public CMap<int, bool> HasLeftFork { get; set; }

        /// <summary>
        /// Whether each philosopher has the right fork.
        /// </summary>
        public CMap<int, bool> HasRightFork { get; set; }

        /// <summary>
        /// String for a state.
        /// </summary>
        public override string ToString()
        {
            return $"HasLeft={this.HasLeftFork}, HasRight={this.HasRightFork}";
        }
    }

    /// <summary>
    /// Extension methods for state.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class StateExtensions
    {
        /// <summary>
        /// Get the left fork.
        /// </summary>
        /// <param name="state">The state.</param>
        public static Zen<CMap<int, bool>> GetHasRightFork(this Zen<State> state) => state.GetField<State, CMap<int, bool>>("HasRightFork");

        /// <summary>
        /// Get the left fork.
        /// </summary>
        /// <param name="state">The state.</param>
        public static Zen<CMap<int, bool>> GetHasLeftFork(this Zen<State> state) => state.GetField<State, CMap<int, bool>>("HasLeftFork");

        /// <summary>
        /// With the left fork.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="value">The new value.</param>
        public static Zen<State> WithHasLeftFork(this Zen<State> state, Zen<CMap<int, bool>> value) =>
            state.WithField("HasLeftFork", value);

        /// <summary>
        /// With the right fork.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="value">The new value.</param>
        public static Zen<State> WithHasRightFork(this Zen<State> state, Zen<CMap<int, bool>> value) =>
            state.WithField("HasRightFork", value);
    }
}
