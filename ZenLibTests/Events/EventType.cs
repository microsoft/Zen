// <copyright file="EventType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    /// <summary>
    /// The type of an event.
    /// </summary>
    public enum EventType : byte
    {
        /// <summary>
        /// An event for a packet arrival.
        /// </summary>
        PacketBurstEvent,

        /// <summary>
        /// An event for the start of a storm.
        /// </summary>
        PfcStormStartEvent,

        /// <summary>
        /// An event for the end of a storm.
        /// </summary>
        PfcStormEndEvent,
    }
}
