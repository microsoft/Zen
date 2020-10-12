// <copyright file="PfcModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// Class representing the model for Pfc and the watchdog.
    /// </summary>
    internal sealed class PfcModel
    {
        /// <summary>
        /// Time before a detection is triggered.
        /// </summary>
        private static ushort detectionInterval = 200;

        /// <summary>
        /// Time before a restoration is triggered.
        /// </summary>
        private static ushort restorationInterval = 200;

        /// <summary>
        /// Gets the Zen function for the model.
        /// </summary>
        /// <returns>An input if one exists.</returns>
        public static IEnumerable<IList<Event>> GenerateTests()
        {
            var f = new ZenFunction<IList<Event>, SwitchState>(es => ProcessEvents(es, InitialState()).Item1());
            return f.GenerateInputs(precondition: IsValidSequence, listSize: 3, checkSmallerLists: false);
        }

        /// <summary>
        /// Gets the initial switch state.
        /// </summary>
        /// <returns>The state.</returns>
        private static Zen<SwitchState> InitialState()
        {
            return Create<SwitchState>(
                ("WatchdogDropPackets1", Constant(false)),
                ("WatchdogDropPackets2", Constant(false)),
                ("WatchdogStartDropTime1", Constant<ushort>(0)),
                ("WatchdogStartDropTime2", Constant<ushort>(0)),
                ("StormStartedTime1", Constant<ushort>(0)),
                ("StormStartedTime2", Constant<ushort>(0)),
                ("StormEndedTime1", Constant<ushort>(0)),
                ("StormEndedTime2", Constant<ushort>(0)));
        }

        /// <summary>
        /// Constraint that defines whether a sequence of events is valid.
        /// </summary>
        /// <param name="es">The events.</param>
        /// <returns>A Zen value for if the sequence is valid.</returns>
        private static Zen<bool> IsValidSequence(Zen<IList<Event>> es)
        {
            var areTimesAscending = Utilities.PairwiseInvariant(es, (x, y) => x.GetTimeStamp() < y.GetTimeStamp());

            var validPriorities = es.All(e => e.GetPriorityClass() <= 1);
            var validTypes = es.All(e => e.GetEventType() <= 2);

            var validStorms0 = AreStartEndStormsValid(es, false, 0, 0);
            var validStorms1 = AreStartEndStormsValid(es, false, 0, 1);

            var timesBounded = es.All(e => e.GetTimeStamp() <= 2000);

            return And(areTimesAscending, timesBounded, validPriorities, validTypes, validStorms0, validStorms1);
        }

        /// <summary>
        /// Ensure all storms are ended.
        /// </summary>
        /// <param name="es">The events.</param>
        /// <param name="isPreviousStart">Whether a start is open and waiting to be closed.</param>
        /// <param name="prevTime">The previous time of a start or end event.</param>
        /// <param name="priorityClass">The priority class.</param>
        /// <returns></returns>
        private static Zen<bool> AreStartEndStormsValid(Zen<IList<Event>> es, Zen<bool> isPreviousStart, Zen<ushort> prevTime, byte priorityClass)
        {
            var startType = Event.EventTypeAsInt(EventType.PfcStormStartEvent);
            var endType = Event.EventTypeAsInt(EventType.PfcStormEndEvent);

            return es.Case(empty: Not(isPreviousStart), cons: (hd, tl) =>
            {
                var isCorrectPriority = hd.GetPriorityClass() == priorityClass;
                var isStart = And(hd.GetEventType() == startType, isCorrectPriority);
                var isEnd = And(hd.GetEventType() == endType, isCorrectPriority);
                var isEvent = Or(isStart, isEnd);
                var isInvalid = Or(And(isPreviousStart, isStart), And(Not(isPreviousStart), isEnd));
                var diff = hd.GetTimeStamp() - prevTime;
                var isTimeGapValid = Implies(isEvent, Or(diff > 400, diff < 200));
                var previousStart = If(isStart, true, If(isEnd, false, isPreviousStart));
                var previousTime = If(isEvent, hd.GetTimeStamp(), prevTime);
                return And(Not(isInvalid), isTimeGapValid, AreStartEndStormsValid(tl, previousStart, previousTime, priorityClass));
            });
        }

        /// <summary>
        /// Process all of the events.
        /// </summary>
        /// <param name="es">The events.</param>
        /// <param name="initialState">The initial state.</param>
        /// <returns>The updated switch state.</returns>
        private static Zen<(SwitchState, IList<(ushort, byte)>)> ProcessEvents(Zen<IList<Event>> es, Zen<SwitchState> initialState)
        {
            return Language.Fold(es, ValueTuple(initialState, EmptyList<(ushort, byte)>()), ProcessEvent);
        }

        /// <summary>
        /// Processes an event and updates the current state.
        /// </summary>
        /// <param name="e">The event to process.</param>
        /// <param name="currentState">The current watchdog state.</param>
        /// <returns>The new watchdog state.</returns>
        private static Zen<(SwitchState, IList<(ushort, byte)>)> ProcessEvent(
            Zen<Event> e,
            Zen<(SwitchState, IList<(ushort, byte)>)> currentState)
        {
            var switchState = currentState.Item1();
            var packets = currentState.Item2();

            var stormStartEvent = Event.EventTypeAsInt(EventType.PfcStormStartEvent);
            var stormEndEvent = Event.EventTypeAsInt(EventType.PfcStormEndEvent);

            var isStormStartOn1 = And(e.GetEventType() == stormStartEvent, e.GetPriorityClass() == 0);
            var isStormStartOn2 = And(e.GetEventType() == stormStartEvent, e.GetPriorityClass() == 1);

            var stateStormStartOn1 = switchState.SetStormStartedTime1(e.GetTimeStamp());
            var stateStormStartOn2 = switchState.SetStormStartedTime2(e.GetTimeStamp());

            var isStormEndOn1 = And(e.GetEventType() == stormEndEvent, e.GetPriorityClass() == 0);
            var isStormEndOn2 = And(e.GetEventType() == stormEndEvent, e.GetPriorityClass() == 1);

            var stateStormEndOn1 = switchState.SetStormEndedTime1(e.GetTimeStamp());
            var stateStormEndOn2 = switchState.SetStormEndedTime2(e.GetTimeStamp());

            var conservativeDetectionTime = (ushort)(2 * detectionInterval);
            var conservativeRestorationTime = (ushort)(2 * restorationInterval);

            var isDetectionEvent1 = And(
                switchState.GetStormStartedTime1() > switchState.GetStormEndedTime1(),
                e.GetTimeStamp() - switchState.GetStormStartedTime1() >= conservativeDetectionTime);

            var isDetectionEvent2 = And(
                switchState.GetStormStartedTime2() > switchState.GetStormEndedTime2(),
                e.GetTimeStamp() - switchState.GetStormStartedTime2() >= conservativeDetectionTime);

            var isRestorationEvent1 = And(
                switchState.GetStormEndedTime1() > switchState.GetStormStartedTime1(),
                e.GetTimeStamp() - switchState.GetWatchdogStartDropTime1() >= conservativeRestorationTime);

            var isRestorationEvent2 = And(
                switchState.GetStormEndedTime2() > switchState.GetStormStartedTime2(),
                e.GetTimeStamp() - switchState.GetWatchdogStartDropTime2() >= conservativeRestorationTime);

            switchState = If(isRestorationEvent1, switchState.SetWatchdogDropPackets1(false), switchState);
            switchState = If(isRestorationEvent2, switchState.SetWatchdogDropPackets2(false), switchState);
            switchState = If(isDetectionEvent1, switchState.SetWatchdogStartDropTime1(e.GetTimeStamp()).SetWatchdogDropPackets1(true), switchState);
            switchState = If(isDetectionEvent2, switchState.SetWatchdogStartDropTime2(e.GetTimeStamp()).SetWatchdogDropPackets2(true), switchState);

            return Cases(ValueTuple(switchState, packets),
                    (isStormStartOn1, ValueTuple(stateStormStartOn1, packets)),
                    (isStormStartOn2, ValueTuple(stateStormStartOn2, packets)),
                    (isStormEndOn1, ValueTuple(stateStormEndOn1, packets)),
                    (isStormEndOn2, ValueTuple(stateStormEndOn2, packets)));
        }
    }
}
