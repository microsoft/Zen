﻿// <copyright file="TransitionSystem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.TransitionSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A symbolic transition system.
    /// </summary>
    /// <typeparam name="T">The type of the system states.</typeparam>
    public class TransitionSystem<T>
    {
        /// <summary>
        /// The initial states.
        /// </summary>
        public Func<Zen<T>, Zen<bool>> InitialStates { get; set; }

        /// <summary>
        /// The state invariants.
        /// </summary>
        public Func<Zen<T>, Zen<bool>> Invariants { get; set; }

        /// <summary>
        /// The next state relation.
        /// </summary>
        public Func<Zen<T>, Zen<T>, Zen<bool>> NextRelation { get; set; }

        /// <summary>
        /// The specification to check.
        /// </summary>
        public Spec<T> Specification { get; set; }

        /// <summary>
        /// Model check the transition system.
        /// </summary>
        /// <param name="timeoutMs">The timeout for each depth in milliseconds.</param>
        /// <param name="useKInduction">Whether to use k induction to prove safety.</param>
        /// <returns>A counterexample, or null if none.</returns>
        public IEnumerable<SearchResult<T>> ModelCheck(int timeoutMs = -1, bool useKInduction = false)
        {
            Contract.Assert(timeoutMs >= -1);

            for (int k = 1; true; k++)
            {
                // check for a violation at the kth step.
                var watch = Stopwatch.StartNew();
                var task = Task.Factory.StartNew(() => this.CheckBoundedSafety(k));
                task.Wait(timeoutMs);
                var stats = new SearchStats(watch.ElapsedMilliseconds);

                // if there is a timeout then we are done.
                if (!task.IsCompleted)
                {
                    yield return new SearchResult<T>(k, SearchOutcome.Timeout, null, stats);
                    yield break;
                }

                // if there is no counter example.
                var trace = task.Result;
                if (trace == null)
                {
                    /* // if k-induction is enabled, check if we can prove safety.
                    if (useKInduction && k > 1)
                    {
                        // run the k-induction check.
                        var task2 = Task.Factory.StartNew(() => this.CheckKInduction(k));
                        task2.Wait(timeoutMs);

                        // If there is a timeout, then we are done.
                        if (!task2.IsCompleted)
                        {
                            yield return new SearchResult<T>(k, SearchOutcome.Timeout, null, stats);
                            yield break;
                        }
                        // otherwise if we prove safety, then we are done.
                        else if (task2.Result)
                        {
                            yield return new SearchResult<T>(k, SearchOutcome.SafetyProof, null, stats);
                            yield break;
                        }
                    } */

                    // we have neither found a counterexample nor a proof of safety, so continue.
                    yield return new SearchResult<T>(k, SearchOutcome.NoCounterExample, null, stats);
                }
                else
                {
                    // otherwise, there is a counter example, so report it.
                    yield return new SearchResult<T>(k, SearchOutcome.CounterExample, trace, stats);
                    yield break;
                }
            }
        }

        /// <summary>
        /// Model check the transition system.
        /// </summary>
        /// <param name="depth">The number of transition steps.</param>
        /// <returns>A counter example trace or null if none.</returns>
        public T[] CheckBoundedSafety(int depth)
        {
            Contract.Assert(depth >= 1);

            // create one symbolic variable for each step.
            var states = new Zen<T>[depth + 1];
            var constraints = new List<Zen<bool>>();
            for (int i = 0; i <= depth; i++)
            {
                states[i] = Zen.Symbolic<T>();
            }

            // enforce the safety and initial state invariants.
            constraints.Add(this.InitialStates(states[0]));
            for (int i = 0; i <= depth; i++)
            {
                constraints.Add(this.Invariants(states[i]));
            }

            // enforce the next state relations.
            for (int i = 0; i <= depth - 1; i++)
            {
                constraints.Add(this.NextRelation(states[i], states[i + 1]));
            }

            var c = Zen.And(constraints);

            // try to find a violation of the spec without loops.
            var withoutLoop = Zen.And(c, this.EncodeSpecWithoutLoops(states, states.Length - 2));
            var solutionNoLoop = withoutLoop.Solve();
            if (solutionNoLoop.IsSatisfiable())
            {
                return states.Select(solutionNoLoop.Get).SkipLast(1).ToArray();
            }

            // try with loops now.
            var withLoop = Zen.And(c, this.EncodeSpecWithLoop(states, states.Length - 2));
            var solutionLoop = withLoop.Solve();
            if (solutionLoop.IsSatisfiable())
            {
                return states.Select(solutionLoop.Get).ToArray();
            }

            // there is no counter example.
            return null;
        }

        /// <summary>
        /// Encodes the specification.
        /// </summary>
        /// <returns>A symbolic boolean.</returns>
        private Zen<bool> EncodeSpecWithoutLoops(Zen<T>[] states, int k)
        {
            Contract.Assert(k < states.Length);

            var spec = Spec.Not(this.Specification).Nnf();
            var result = Zen.False();
            for (int l = 0; l <= k; l++)
            {
                result = Zen.Or(result, states[k + 1] == states[l]);
            }

            return Zen.And(Zen.Not(result), spec.EncodeLoopFree(states, 0, k));
        }

        /// <summary>
        /// Encodes the specification.
        /// </summary>
        /// <returns>A symbolic boolean.</returns>
        private Zen<bool> EncodeSpecWithLoop(Zen<T>[] states, int k)
        {
            Contract.Assert(k < states.Length);

            var spec = Spec.Not(this.Specification).Nnf();

            var result = Zen.False();
            for (int l = 0; l <= k; l++)
            {
                result = Zen.Or(result, Zen.And(states[k + 1] == states[l], spec.EncodeLoop(states, l, 0, k)));
            }

            return result;
        }

        /* /// <summary>
        /// Check if the property is true from any initial state after k steps.
        /// </summary>
        /// <param name="depth">The number of transition steps.</param>
        /// <returns>True if we can prove safety for k steps.</returns>
        public bool CheckKInduction(int depth)
        {
            Contract.Assert(depth >= 1);

            // create one symbolic variable for each step.
            var states = new Zen<T>[depth];
            var constraints = new List<Zen<bool>>();
            for (int i = 0; i < depth; i++)
            {
                states[i] = Zen.Symbolic<T>();
            }

            // enforce the safety invariants.
            for (int i = 0; i < depth; i++)
            {
                constraints.Add(this.Invariants(states[i]));
            }

            // property holds for the first k-1 steps.
            for (int i = 0; i < depth - 1; i++)
            {
                constraints.Add(this.SafetyChecks(states[i]));
            }

            // but fails on the kth step.
            constraints.Add(Zen.Not(this.SafetyChecks(states[depth - 1])));

            // enforce the next state relations.
            for (int i = 0; i < depth - 1; i++)
            {
                constraints.Add(this.NextRelation(states[i], states[i + 1]));
            }

            // if there is no example of this, then we have safety.
            var solution = Zen.And(constraints).Solve();
            return !solution.IsSatisfiable();
        } */
    }
}
