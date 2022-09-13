// <copyright file="TransitionSystem.cs" company="Microsoft">
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
        public LTL<T> Specification { get; set; }

        /// <summary>
        /// Model check the transition system.
        /// </summary>
        /// <param name="timeoutMs">The timeout for each depth in milliseconds.</param>
        /// <param name="useKInduction">Whether to use k induction to prove safety.</param>
        /// <returns>A counterexample, or null if none.</returns>
        public IEnumerable<SearchResult<T>> ModelCheck(int timeoutMs = -1, bool useKInduction = false)
        {
            Contract.Assert(timeoutMs >= -1);
            var timer = new Stopwatch();

            for (int k = 1; true; k++)
            {
                // compute the timer for this iteration.
                var timeLeft = timeoutMs >= 0 ? Math.Max(0, timeoutMs - (int)timer.ElapsedMilliseconds) : timeoutMs;

                // run the kth iteration and add to the current time.
                timer.Start();
                var task = Task.Factory.StartNew(() => this.CheckBoundedSafety(k));
                task.Wait(timeLeft);
                timer.Stop();

                // record any statistics.
                var stats = new SearchStats(timer.ElapsedMilliseconds);

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
        /// <param name="k">The number of transition steps.</param>
        /// <returns>A counter example trace or null if none.</returns>
        public T[] CheckBoundedSafety(int k)
        {
            Contract.Assert(k >= 1);

            // create one symbolic variable for each step.
            // there is a special last step k + 1 that is used to check for loops.
            var states = new Zen<T>[k + 1];

            var constraints = new List<Zen<bool>>();
            for (int i = 0; i <= k; i++)
            {
                states[i] = Zen.Symbolic<T>($"state_{i}");
            }

            // enforce the safety and initial state invariants.
            constraints.Add(this.InitialStates(states[0]));
            for (int i = 0; i <= k; i++)
            {
                constraints.Add(this.Invariants(states[i]));
            }

            // enforce the next state relations.
            for (int i = 0; i <= k - 1; i++)
            {
                constraints.Add(this.NextRelation(states[i], states[i + 1]));
            }

            // try to find a violation of the spec without loops.
            var property = this.EncodeSpec(LTL.Not(this.Specification), states);
            Console.WriteLine("==========");
            Console.WriteLine(property.Encoding.Format());

            var solution = Zen.And(Zen.And(constraints), property.Encoding).Solve();

            if (solution.IsSatisfiable())
            {
                var trace = states.Select(solution.Get);

                if (solution.Get(property.LoopExistsVar))
                {
                    return trace.ToArray();
                }

                return trace.SkipLast(1).ToArray();
            }

            return null;
        }

        /// <summary>
        /// Creates the loop tracking variables and constraints.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <returns>The encoding used.</returns>
        private (Zen<bool>[] LoopStart, Zen<bool>[] InLoop, Zen<bool> Constraints) CreateLoopTracking(Zen<T>[] states)
        {
            var k = states.Length - 1;
            var loopStart = new Zen<bool>[k];
            var inLoop = new Zen<bool>[k];

            for (var i = 0; i < k; i++)
            {
                loopStart[i] = Zen.Symbolic<bool>($"loop_start_{i}");
                inLoop[i] = Zen.Symbolic<bool>($"in_loop_{i}");
            }

            var constraints = new List<Zen<bool>>();

            for (int i = 0; i < k; i++)
            {
                constraints.Add(Zen.Implies(loopStart[i], states[k] == states[i]));
                constraints.Add(inLoop[i] == Zen.Or(i == 0 ? Zen.False() : inLoop[i - 1], loopStart[i]));

                if (i < k - 1)
                {
                    constraints.Add(Zen.Implies(inLoop[i], Zen.Not(loopStart[i + 1])));
                }
            }

            return (loopStart, inLoop, Zen.And(constraints));
        }

        /// <summary>
        /// Encodes the specification.
        /// </summary>
        /// <param name="specification">The specification to find an example for.</param>
        /// <param name="states">The symbolic states.</param>
        /// <returns>A symbolic boolean.</returns>
        private (Zen<bool> Encoding, Zen<bool> LoopExistsVar) EncodeSpec(LTL<T> specification, Zen<T>[] states)
        {
            var spec = specification.Nnf();
            var loops = this.CreateLoopTracking(states);
            var specHolds = Zen.And(loops.Constraints, spec.EncodeSpec(states, loops.LoopStart, loops.InLoop, 0));
            return (specHolds, loops.InLoop[states.Length - 2]);
        }
    }
}
