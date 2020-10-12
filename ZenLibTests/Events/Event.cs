// <copyright file="Event.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using ZenLib;

    /// <summary>
    /// Class representing an event.
    /// </summary>
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
            if (EventType == 3)
            {
                return $"Event(Time={TimeStamp}, Type={(EventType)EventType})";
            }

            return $"Event(Time={TimeStamp}, Type={(EventType)EventType}, PriorityClass={PriorityClass})";
        }

        public static Zen<Event> Create(Zen<ushort> time, Zen<byte> eventType, Zen<byte> priorityClass)
        {
            return Language.Create<Event>(("TimeStamp", time), ("EventType", eventType), ("PriorityClass", priorityClass));
        }

        public static byte EventTypeAsInt(EventType et)
        {
            return (byte)et;
        }
    }

    /// <summary>
    /// Extension methods for Events to access Zen objects.
    /// </summary>
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
}
