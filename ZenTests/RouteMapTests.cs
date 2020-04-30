// <copyright file="AclTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.ZenTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Research.Zen;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Microsoft.Research.Zen.Language;

    /// <summary>
    /// Tests for Zen with route maps.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RouteMapTests
    {
        /// <summary>
        /// Test verification for a route map.
        /// </summary>
        [TestMethod]
        public void TestRouteMapVerify()
        {
            var routeMap = ExampleRouteMap();

            var timer = System.Diagnostics.Stopwatch.StartNew();
            var function = Function<Route, Tuple<Option<Route>, int>>(r => routeMap.Process(r));
            Console.WriteLine($"time build: {timer.ElapsedMilliseconds}");

            var result = function.Find((route, outputRoute) => outputRoute.Item2() == 3);
            Console.WriteLine($"time find: {timer.ElapsedMilliseconds}");

            var input = result.Value;
            Console.WriteLine($"Found input satisfying conditions: {input}");

            var output = function.Evaluate(input);
            Console.WriteLine($"The resulting route is: {output}");

            Console.WriteLine($"time evaluate: {timer.ElapsedMilliseconds}");
        }

        /// <summary>
        /// Creates an example route map.
        /// </summary>
        /// <returns></returns>
        private RouteMap ExampleRouteMap()
        {
            var line1 = new RouteMapLine
            {
                PrefixGuard = (0, 0, 32),
                CommunityGuard = new List<uint> { 5U },
                CommunityAdds = new List<uint> { },
                CommunityDeletes = new List<uint> { },
                AsPathPrepends = new List<uint> { },
                Disposition = Disposition.Deny,
            };

            var line2 = new RouteMapLine
            {
                PrefixGuard = (0, 16, 32),
                CommunityGuard = new List<uint> { 4U },
                CommunityAdds = new List<uint> { 5U },
                CommunityDeletes = new List<uint> { 4U },
                AsPathPrepends = new List<uint> { },
                Disposition = Disposition.NextTerm,
            };

            var line3 = new RouteMapLine
            {
                PrefixGuard = (0, 0, 32),
                CommunityGuard = new List<uint> { 5U },
                CommunityAdds = new List<uint> { },
                CommunityDeletes = new List<uint> { },
                AsPathPrepends = new List<uint> { 100U, 100U },
                Disposition = Disposition.Allow,
            };

            var line4 = new RouteMapLine
            {
                PrefixGuard = (0, 0, 32),
                CommunityGuard = new List<uint> { },
                CommunityAdds = new List<uint> { },
                CommunityDeletes = new List<uint> { },
                AsPathPrepends = new List<uint> { },
                Disposition = Disposition.Deny,
            };

            return new RouteMap { Lines = new List<RouteMapLine> { line1, line2, line3, line4 } };
        }
    }
}
