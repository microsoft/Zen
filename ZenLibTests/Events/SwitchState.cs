// <copyright file="SwitchState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using ZenLib;

    /// <summary>
    /// Class representing the watchdog state.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class SwitchState
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

        /// <summary>
        /// The packet outcomes for bursts.
        /// </summary>
        public IList<(ushort, (byte, bool))> Packets { get; set; }
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

        internal static Zen<IList<(ushort, (byte, bool))>> GetPackets(this Zen<SwitchState> wd)
        {
            return wd.GetField<SwitchState, IList<(ushort, (byte, bool))>>("Packets");
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

        internal static Zen<SwitchState> SetPackets(this Zen<SwitchState> wd, Zen<IList<(ushort, (byte, bool))>> p)
        {
            return wd.WithField("Packets", p);
        }

        internal static Zen<SwitchState> AddPacket(this Zen<SwitchState> wd, Zen<(ushort, (byte, bool))> p)
        {
            return wd.SetPackets(wd.GetPackets().AddFront(p));
        }
    }
}
