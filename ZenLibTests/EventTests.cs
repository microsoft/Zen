// <copyright file="EventTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static ZenLib.Language;

    /// <summary>
    /// Tests for primitive types.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EventTests
    {
        /// <summary>
        /// How many polling events to encode.
        /// </summary>
        private static int numPollingIntervals = 1;

        /// <summary>
        /// The maximum gap in time between events.
        /// </summary>
        private static ushort maxEventTimeGap = 1000;

        /// <summary>
        /// Time in ms between polling intervals.
        /// </summary>
        private static ushort pollingInterval = 200;

        /// <summary>
        /// Time before a detection is triggered.
        /// </summary>
        private static ushort detectionInterval = 200;

        /// <summary>
        /// Time before a restoration is triggered.
        /// </summary>
        private static ushort restorationInterval = 200;

        /// <summary>
        /// Test solving for preconditions.
        /// </summary>
        [TestMethod]
        public void TestInputGeneration()
        {
            var initialState = Create<SwitchState>(
                ("WatchdogDropPackets1", Constant(false)),
                ("WatchdogDropPackets2", Constant(false)),
                ("WatchdogStartDropTime1", Constant<ushort>(0)),
                ("WatchdogStartDropTime2", Constant<ushort>(0)),
                ("StormStartedTime1", Constant<ushort>(0)),
                ("StormStartedTime2", Constant<ushort>(0)),
                ("StormEndedTime1", Constant<ushort>(0)),
                ("StormEndedTime2", Constant<ushort>(0)));

            var f = new ZenFunction<IList<Event>, SwitchState>(es => If(IsValidSequence(es), ProcessEvents(es, initialState), initialState));
            var fValid = new ZenFunction<IList<Event>, bool>(IsValidSequence);
            // var input = f.Find(Invariant, listSize: 5, checkSmallerLists: true);

            foreach (var x in f.GenerateInputs(listSize: 3, checkSmallerLists: false).Take(10))
            {
                if (fValid.Evaluate(x))
                    Console.WriteLine($"[{string.Join(",", x)}]");
            }

            /* var inputs = f.FindAll((es, state) => IsValidSequence(es), listSize: 5, checkSmallerLists: false).Take(20);
            foreach (var input in inputs)
            {
                Console.WriteLine($"[{string.Join(",", input)}]");
            } */

            // Assert.IsTrue(input.HasValue);
            // Assert.IsTrue(f.Evaluate(input.Value).WatchdogDropPackets1);
        }

        internal static Zen<bool> Invariant(Zen<IList<Event>> es, Zen<SwitchState> resultState)
        {
            return And(IsValidSequence(es), resultState.GetWatchdogDropPackets1());
        }

        /// <summary>
        /// Constraint that defines whether a sequence of events is valid.
        /// </summary>
        /// <param name="es">The events.</param>
        /// <returns>A Zen value for if the sequence is valid.</returns>
        internal static Zen<bool> IsValidSequence(Zen<IList<Event>> es)
        {
            var areTimesAscending = PairwiseInvariant(es, (x, y) => x.GetTimeStamp() < y.GetTimeStamp());
            var areTimeGapsValid = PairwiseInvariant(es, (x, y) => y.GetTimeStamp() - x.GetTimeStamp() <= maxEventTimeGap);
            var arePrioritiesValid = es.All(e => e.GetPriorityClass() <= 1);

            var hasPollingEvents = True();
            for (int i = 0; i < numPollingIntervals; i++)
            {
                var et = Event.EventTypeAsInt(EventType.PollingIntervalEvent);
                var value = (ushort)(pollingInterval * i);
                var hasPollingEvent = es.Any(e => And(e.GetTimeStamp() == value, e.GetEventType() == et));
                hasPollingEvents = And(hasPollingEvent, hasPollingEvents);
            }

            return And(areTimesAscending, areTimeGapsValid, arePrioritiesValid, hasPollingEvents);
        }

        /// <summary>
        /// A pairwise invariant for a list of elements.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="f">The pairwise invariant.</param>
        /// <returns>A zen value for the invariant.</returns>
        internal static Zen<bool> PairwiseInvariant<T>(Zen<IList<T>> list, Func<Zen<T>, Zen<T>, Zen<bool>> f)
        {
            return list.Case(
                empty: true,
                cons: (hd1, tl1) => tl1.Case(
                    empty: true,
                    cons: (hd2, tl2) => And(f(hd1, hd2), PairwiseInvariant(tl1, f))));
        }

        /// <summary>
        /// Process all of the events.
        /// </summary>
        /// <param name="es">The events.</param>
        /// <param name="initialState">The initial state.</param>
        /// <returns>The updated switch state.</returns>
        internal static Zen<SwitchState> ProcessEvents(Zen<IList<Event>> es, Zen<SwitchState> initialState)
        {
            return Language.Fold(es, initialState, (e, acc) => ProcessEvent(e, acc));
        }

        /// <summary>
        /// Processes an event and updates the current state.
        /// </summary>
        /// <param name="e">The event to process.</param>
        /// <param name="currentState">The current watchdog state.</param>
        /// <returns>The new watchdog state.</returns>
        internal static Zen<SwitchState> ProcessEvent(Zen<Event> e, Zen<SwitchState> currentState)
        {
            var stormStartEvent = Event.EventTypeAsInt(EventType.PfcStormStartEvent);
            var stormEndEvent = Event.EventTypeAsInt(EventType.PfcStormEndEvent);
            var pollingIntervalEvent = Event.EventTypeAsInt(EventType.PollingIntervalEvent);

            var isStormStartOn1 = And(e.GetEventType() == stormStartEvent, e.GetPriorityClass() == 0);
            var isStormStartOn2 = And(e.GetEventType() == stormStartEvent, e.GetPriorityClass() == 1);

            var stateStormStartOn1 = currentState.SetStormStartedTime1(e.GetTimeStamp());
            var stateStormStartOn2 = currentState.SetStormStartedTime2(e.GetTimeStamp());

            var isStormEndOn1 = And(e.GetEventType() == stormEndEvent, e.GetPriorityClass() == 0);
            var isStormEndOn2 = And(e.GetEventType() == stormEndEvent, e.GetPriorityClass() == 1);

            var stateStormEndOn1 = currentState.SetStormEndedTime1(e.GetTimeStamp());
            var stateStormEndOn2 = currentState.SetStormEndedTime2(e.GetTimeStamp());

            var isDetectionEvent1 = And(
                e.GetEventType() == pollingIntervalEvent,
                currentState.GetStormStartedTime1() > currentState.GetStormEndedTime1(),
                e.GetTimeStamp() - currentState.GetStormStartedTime1() >= detectionInterval);

            var isDetectionEvent2 = And(
                e.GetEventType() == pollingIntervalEvent,
                currentState.GetStormStartedTime2() > currentState.GetStormEndedTime2(),
                e.GetTimeStamp() - currentState.GetStormStartedTime2() >= detectionInterval);

            var stateDetectionOn1 = currentState.SetWatchdogStartDropTime1(e.GetTimeStamp()).SetWatchdogDropPackets1(true);
            var stateDetectionOn2 = currentState.SetWatchdogStartDropTime2(e.GetTimeStamp()).SetWatchdogDropPackets2(true);

            var isRestorationEvent1 = And(
                e.GetEventType() == pollingIntervalEvent,
                currentState.GetStormEndedTime1() > currentState.GetStormStartedTime1(),
                e.GetTimeStamp() - currentState.GetWatchdogStartDropTime1() >= restorationInterval);

            var isRestorationEvent2 = And(
                e.GetEventType() == pollingIntervalEvent,
                currentState.GetStormEndedTime2() > currentState.GetStormStartedTime2(),
                e.GetTimeStamp() - currentState.GetWatchdogStartDropTime2() >= restorationInterval);

            var stateRestorationOn1 = currentState.SetWatchdogDropPackets1(false);
            var stateRestorationOn2 = currentState.SetWatchdogDropPackets2(false);

            return Cases(currentState,
                    (isStormStartOn1, stateStormStartOn1),
                    (isStormStartOn2, stateStormStartOn2),
                    (isStormEndOn1, stateStormEndOn1),
                    (isStormEndOn2, stateStormEndOn2),
                    (isDetectionEvent1, stateDetectionOn1),
                    (isDetectionEvent2, stateDetectionOn2),
                    (isRestorationEvent1, stateRestorationOn1),
                    (isRestorationEvent2, stateRestorationOn2));
        }
    }

    /// <summary>
    /// Class representing the watchdog state.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class SwitchState
    {
        /// <summary>
        /// Whether the watchdog is mitigating on priority 1.
        /// </summary>
        public bool WatchdogDropPackets1 { get; set; }

        /// <summary>
        /// Whether the watchdog is mitigating on priority 2.
        /// </summary>
        public bool WatchdogDropPackets2 { get; set; }

        /// <summary>
        /// When mitigation started on priority 1.
        /// </summary>
        public ushort WatchdogStartDropTime1 { get; set; }

        /// <summary>
        /// When mitigation started on priority 2.
        /// </summary>
        public ushort WatchdogStartDropTime2 { get; set; }

        /// <summary>
        /// When a storm started on priority 1.
        /// </summary>
        public ushort StormStartedTime1 { get; set; }

        /// <summary>
        /// When a storm started on priority 2.
        /// </summary>
        public ushort StormStartedTime2 { get; set; }

        /// <summary>
        /// When a storm ended on priority 1.
        /// </summary>
        public ushort StormEndedTime1 { get; set; }

        /// <summary>
        /// When a storm started on priority 2.
        /// </summary>
        public ushort StormEndedTime2 { get; set; }
    }

    /// <summary>
    /// Extension methods for WatchdogState to access Zen objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class WatchdogStateExtensions
    {
        internal static Zen<bool> GetWatchdogDropPackets1(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, bool>("WatchdogDropPackets1");
        }

        internal static Zen<bool> GetWatchdogDropPackets2(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, bool>("WatchdogDropPackets2");
        }

        internal static Zen<ushort> GetWatchdogStartDropTime1(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, ushort>("WatchdogStartDropTime1");
        }

        internal static Zen<ushort> GetWatchdogStartDropTime2(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, ushort>("WatchdogStartDropTime2");
        }

        internal static Zen<ushort> GetStormStartedTime1(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, ushort>("StormStartedTime1");
        }

        internal static Zen<ushort> GetStormStartedTime2(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, ushort>("StormStartedTime2");
        }

        internal static Zen<ushort> GetStormEndedTime1(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, ushort>("StormEndedTime1");
        }

        internal static Zen<ushort> GetStormEndedTime2(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, ushort>("StormEndedTime2");
        }

        internal static Zen<SwitchState> SetWatchdogDropPackets1(this Zen<SwitchState> wd, Zen<bool> b)
        {
            return wd.WithField("WatchdogDropPackets1", b);
        }

        internal static Zen<SwitchState> SetWatchdogDropPackets2(this Zen<SwitchState> wd, Zen<bool> b)
        {
            return wd.WithField("WatchdogDropPackets2", b);
        }

        internal static Zen<SwitchState> SetWatchdogStartDropTime1(this Zen<SwitchState> wd, Zen<ushort> t)
        {
            return wd.WithField("WatchdogStartDropTime1", t);
        }

        internal static Zen<SwitchState> SetWatchdogStartDropTime2(this Zen<SwitchState> wd, Zen<ushort> t)
        {
            return wd.WithField("WatchdogStartDropTime2", t);
        }

        internal static Zen<SwitchState> SetStormStartedTime1(this Zen<SwitchState> wd, Zen<ushort> t)
        {
            return wd.WithField("StormStartedTime1", t);
        }

        internal static Zen<SwitchState> SetStormStartedTime2(this Zen<SwitchState> wd, Zen<ushort> t)
        {
            return wd.WithField("StormStartedTime2", t);
        }

        internal static Zen<SwitchState> SetStormEndedTime1(this Zen<SwitchState> wd, Zen<ushort> t)
        {
            return wd.WithField("StormEndedTime1", t);
        }

        internal static Zen<SwitchState> SetStormEndedTime2(this Zen<SwitchState> wd, Zen<ushort> t)
        {
            return wd.WithField("StormEndedTime2", t);
        }
    }

    /// <summary>
    /// Class representing an event.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class Event
    {
        /// <summary>
        /// The time the event arrives.
        /// </summary>
        public ushort TimeStamp { get; set; }

        /// <summary>
        /// The event type.
        /// </summary>
        public byte EventType { get; set; }

        /// <summary>
        /// Priority class 0-1, if relevant.
        /// </summary>
        public byte PriorityClass { get; set; }

        public override string ToString()
        {
            return $"Event(Time={TimeStamp}, Type={(EventType)EventType}, PriorityClass={PriorityClass})";
        }

        public static byte EventTypeAsInt(EventType et)
        {
            return (byte)et;
        }
    }

    /// <summary>
    /// Extension methods for Events to access Zen objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class EventExtensions
    {
        internal static Zen<ushort> GetTimeStamp(this Zen<Event> e)
        {
            return e.GetField<Event, ushort>("TimeStamp");
        }

        internal static Zen<byte> GetEventType(this Zen<Event> e)
        {
            return e.GetField<Event, byte>("EventType");
        }

        public static Zen<byte> GetPriorityClass(this Zen<Event> e)
        {
            return e.GetField<Event, byte>("PriorityClass");
        }
    }

    /// <summary>
    /// The type of an event.
    /// </summary>
    internal enum EventType : byte
    {
        /// <summary>
        /// An event for a packet arrival.
        /// </summary>
        PacketArrivesEvent,

        /// <summary>
        /// An event for the start of a storm.
        /// </summary>
        PfcStormStartEvent,

        /// <summary>
        /// An event for the end of a storm.
        /// </summary>
        PfcStormEndEvent,

        /// <summary>
        /// A timer event, goes off every 200ms
        /// </summary>
        PollingIntervalEvent,
    }
}