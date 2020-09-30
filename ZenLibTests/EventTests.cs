// <copyright file="EventTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        /// Test solving for preconditions.
        /// </summary>
        [TestMethod]
        public void TestPrecondition()
        {
            var f = new ZenFunction<IList<Event>, bool>(es => IsValidSequence(es));
            var input = f.Find((i, o) => o, listSize: 5, checkSmallerLists: false);
            Console.WriteLine(string.Join("\n", input.Value));
        }

        internal static Zen<bool> IsValidSequence(Zen<IList<Event>> es)
        {
            var areTimesAscending = PairwiseInvariant(es, (x, y) => x.GetTimeStamp() <= y.GetTimeStamp());
            var areTimeGapsValid = PairwiseInvariant(es, (x, y) => y.GetTimeStamp() - x.GetTimeStamp() <= 2000);

            var firstEvent = es.At(0).Value();
            var initialEvent = And(firstEvent.GetTimeStamp() == 0, firstEvent.GetEventType() == new UInt3((long)EventType.PollingIntervalEvent));

            return And(areTimesAscending, areTimeGapsValid, initialEvent);
        }

        internal static Zen<bool> PairwiseInvariant<T>(Zen<IList<T>> es, Func<Zen<T>, Zen<T>, Zen<bool>> f)
        {
            return es.Case(
                empty: true,
                cons: (hd1, tl1) => tl1.Case(
                    empty: true,
                    cons: (hd2, tl2) => And(f(hd1, hd2), PairwiseInvariant(tl1, f))));
        }
    }

    /// <summary>
    /// Class representing the watchdog state.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class WatchdogState
    {
        /// <summary>
        /// Whether the watchdog is mitigating on priority 1.
        /// </summary>
        public bool DropPackets1 { get; set; }

        /// <summary>
        /// Whether the watchdog is mitigating on priority 2.
        /// </summary>
        public bool DropPackets2 { get; set; }

        /// <summary>
        /// When mitigation started on priority 1.
        /// </summary>
        public ushort StartDropTime1 { get; set; }

        /// <summary>
        /// When mitigation started on priority 2.
        /// </summary>
        public ushort StartDropTime2 { get; set; }
    }

    /// <summary>
    /// Extension methods for WatchdogState to access Zen objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class WatchdogStateExtensions
    {
        internal static Zen<bool> GetDropPackets1(this Zen<WatchdogState> wd)
        {
            return wd.GetField<WatchdogState, bool>("DropPackets1");
        }

        internal static Zen<bool> GetDropPackets2(this Zen<WatchdogState> wd)
        {
            return wd.GetField<WatchdogState, bool>("DropPackets2");
        }

        internal static Zen<ushort> GetStartDropTime1(this Zen<WatchdogState> wd)
        {
            return wd.GetField<WatchdogState, ushort>("StartDropTime1");
        }

        internal static Zen<ushort> GetStartDropTime2(this Zen<WatchdogState> wd)
        {
            return wd.GetField<WatchdogState, ushort>("StartDropTime2");
        }

        internal static Zen<WatchdogState> SetDropPackets1(this Zen<WatchdogState> wd, Zen<bool> b)
        {
            return wd.WithField("DropPackets1", b);
        }

        internal static Zen<WatchdogState> SetDropPackets2(this Zen<WatchdogState> wd, Zen<bool> b)
        {
            return wd.WithField("DropPackets2", b);
        }

        internal static Zen<WatchdogState> SetStartDropTime1(this Zen<WatchdogState> wd, Zen<ushort> t)
        {
            return wd.WithField("StartDropTime1", t);
        }

        internal static Zen<WatchdogState> SetStartDropTime2(this Zen<WatchdogState> wd, Zen<ushort> t)
        {
            return wd.WithField("StartDropTime2", t);
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
        public UInt3 EventType { get; set; }

        /// <summary>
        /// Priority class 0-3, if relevant.
        /// </summary>
        public UInt1 PriorityClass { get; set; }

        /// <summary>
        /// The payload of the event.
        /// </summary>
        public ushort Payload { get; set; }

        public override string ToString()
        {
            // var p = PriorityClass == null ? new UInt1(0) : PriorityClass;
            // Console.WriteLine(p);
            // var c = (EventType)p.ToLong();
            return $"Event(Time={TimeStamp}, Type={EventType}, PriorityClass={PriorityClass.ToLong()}, Payload={Payload})";
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

        internal static Zen<UInt3> GetEventType(this Zen<Event> e)
        {
            return e.GetField<Event, UInt3>("EventType");
        }

        public static Zen<UInt1> GetPriorityClass(this Zen<Event> e)
        {
            return e.GetField<Event, UInt1>("PriorityClass");
        }

        public static Zen<ushort> GetPayload(this Zen<Event> e)
        {
            return e.GetField<Event, ushort>("Payload");
        }
    }

    /// <summary>
    /// The type of an event.
    /// </summary>
    internal enum EventType
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