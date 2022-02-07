﻿// <copyright file="Route.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Tests.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using ZenLib;
    using static ZenLib.Language;

    /// <summary>
    /// Simple packet class for testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Route
    {
        /// <summary>
        /// The destination prefix.
        /// </summary>
        public Prefix Prefix { get; set; }

        /// <summary>
        /// The list of communities.
        /// </summary>
        public IList<uint> Communities { get; set; }

        /// <summary>
        /// The AS path.
        /// </summary>
        public IList<uint> AsPath { get; set; }

        /// <summary>
        /// Convert a route to a string.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            var comms = string.Join(",", Communities);
            var aspath = string.Join(",", AsPath);
            return $"Route(Prefix={Prefix}, Communities=[{comms}], AsPath=[{aspath}])";
        }
    }

    /// <summary>
    /// A route map.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct RouteMap
    {
        /// <summary>
        /// The list of route map lines.
        /// </summary>
        public List<RouteMapLine> Lines { get; set; }

        /// <summary>
        /// Process a route with this route map.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>A new route and the matching line number.</returns>
        public Zen<Option<Route>> Process(Zen<Route> route)
        {
            return TestHelper.ApplyOrderedRules<Route, Option<Route>, RouteMapLine>(
                input: route,
                deflt: Option.Null<Route>(),
                ruleMatch: (l, r, i) => l.Matches(r),
                ruleAction: (l, r, i) => l.ApplyAction(r),
                ruleReturn: (l, r, i) =>
                {
                    var line = Constant<int>(i);
                    if (l.Disposition == Disposition.Deny)
                    {
                        return Option.Create(Option.Null<Route>());
                    }

                    if (l.Disposition == Disposition.Allow)
                    {
                        return Option.Create(Option.Create(r));
                    }

                    return Option.Null<Option<Route>>();
                },
                this.Lines.ToArray());
        }

        /// <summary>
        /// Process a route with this route map.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>A new route and the matching line number.</returns>
        public Zen<Pair<Option<Route>, int>> ProcessProvenance(Zen<Route> route)
        {
            return TestHelper.ApplyOrderedRules<Route, Pair<Option<Route>, int>, RouteMapLine>(
                input: route,
                deflt: Pair.Create(Option.Null<Route>(), Constant<int>(this.Lines.Count)),
                ruleMatch: (l, r, i) => l.Matches(r),
                ruleAction: (l, r, i) => l.ApplyAction(r),
                ruleReturn: (l, r, i) =>
                {
                    var line = Constant<int>(i);
                    if (l.Disposition == Disposition.Deny)
                    {
                        return Option.Create(Pair.Create(Option.Null<Route>(), line));
                    }

                    if (l.Disposition == Disposition.Allow)
                    {
                        return Option.Create(Pair.Create(Option.Create(r), line));
                    }

                    return Option.Null<Pair<Option<Route>, int>>();
                },
                this.Lines.ToArray());
        }
    }

    /// <summary>
    /// A route map.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct RouteMapLine
    {
        /// <summary>
        /// Gets the line disposition.
        /// </summary>
        public Disposition Disposition { get; set; }

        /// <summary>
        /// A prefix filter for the line.
        /// </summary>
        public (uint, byte, byte) PrefixGuard { get; set; }

        /// <summary>
        /// Protocol filters.
        /// </summary>
        public List<byte> ProtocolGuard { get; set; }

        /// <summary>
        /// As path filter guard.
        /// </summary>
        public List<uint> AsPathContainsGuard { get; set; }

        /// <summary>
        /// A community filter for the line.
        /// </summary>
        public List<uint> CommunityGuard { get; set; }

        /// <summary>
        /// Gets the list of Asns to prepend to the path.
        /// </summary>
        public List<uint> AsPathPrepends { get; set; }

        /// <summary>
        /// Gets the list of communities to add if matching.
        /// </summary>
        public List<uint> CommunityAdds { get; set; }

        /// <summary>
        /// Gets the list of communities to delete if matching.
        /// </summary>
        public List<uint> CommunityDeletes { get; set; }

        /// <summary>
        /// Computes whether this line matches a route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>Whether the route matches.</returns>
        public Zen<bool> Matches(Zen<Route> route)
        {
            var prefix = route.GetField<Route, Prefix>("Prefix");
            var dstIp = prefix.GetField<Prefix, uint>("Address");
            var prefixLen = prefix.GetField<Prefix, byte>("Length");
            var communities = route.GetField<Route, IList<uint>>("Communities");

            var (addr, lenLow, lenHi) = this.PrefixGuard;
            var mask = (uint)(0xFFFFFF << (32 - lenLow));

            var bitsMatch = (dstIp & mask) == (addr & mask);
            var lenMatch = And(prefixLen >= lenLow, prefixLen <= lenHi);

            var comms = this.CommunityGuard.Select(c => communities.Contains(c)).ToArray();
            var commsMatch = this.CommunityGuard.Count == 0 ? true : And(comms);

            return And(bitsMatch, lenMatch, commsMatch);
        }

        /// <summary>
        /// Applies the actions of this line to a route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>A new modified route.</returns>
        public Zen<Route> ApplyAction(Zen<Route> route)
        {
            var communities = route.GetField<Route, IList<uint>>("Communities");
            var aspath = route.GetField<Route, IList<uint>>("AsPath");

            foreach (var c in this.CommunityDeletes)
            {
                communities = communities.RemoveAll(c);
            }

            foreach (var c in this.CommunityAdds)
            {
                communities = communities.AddFront(c);
            }

            foreach (var asn in this.AsPathPrepends)
            {
                aspath = aspath.AddFront(asn);
            }

            return route.WithField("Communities", communities).WithField("AsPath", aspath);
        }
    }

    /// <summary>
    /// A route map line disposition.
    /// </summary>
    public enum Disposition
    {
        /// <summary>
        /// Allow the route through the route map.
        /// </summary>
        Allow,

        /// <summary>
        /// Drop the route.
        /// </summary>
        Deny,

        /// <summary>
        /// Continue to the next term with modifications.
        /// </summary>
        NextTerm,
    }
}
