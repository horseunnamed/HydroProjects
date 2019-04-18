using Core.Model;
using IntegratedAlgo;
using System;
using System.Collections.Generic;

namespace PlanSearch
{
    class DonorsAcceptors
    {
        public enum RatingStrategy
        {
            Length, Ratio
        }

        private static readonly int ZONE_R = 10;
        private static readonly int TARGET_ACCEPTOR_CELL = 3;
        private static readonly int TARGET_DONOR_CELL = 2;

        private readonly RatingStrategy strategy;
        private readonly ChannelsTree channelsTree;
        private readonly GridMap ecoTargetMap;
        private readonly GridMap socTargetMap;
        private readonly IDictionary<Channel, List<(int, int)>> channelZoneCache = 
            new Dictionary<Channel, List<(int, int)>>();

        public DonorsAcceptors(RatingStrategy strategy, ChannelsTree channelsTree, GridMap ecoTargetMap, GridMap socTargetMap)
        {
            this.strategy = strategy;
            this.channelsTree = channelsTree ?? throw new ArgumentNullException(nameof(channelsTree));
            this.ecoTargetMap = ecoTargetMap ?? throw new ArgumentNullException(nameof(ecoTargetMap));
            this.socTargetMap = socTargetMap ?? throw new ArgumentNullException(nameof(socTargetMap));
        }


        private List<(int, int)> GetChannelZone(Channel channel, int mapW, int mapH)
        {
            if (channelZoneCache.ContainsKey(channel))
            {
                return channelZoneCache[channel];
            }

            var visited = new HashSet<(int, int)>();
            var channelZone = new List<(int, int)>();
            foreach (var point in channel.Points)
            {
                var x0 = Math.Max(0, point.X - ZONE_R);
                var y0 = Math.Max(0, point.Y - ZONE_R);
                var x1 = Math.Min(mapW - 1, point.X + ZONE_R);
                var y1 = Math.Min(mapH - 1, point.Y + ZONE_R);

                for (var x = x0; x <= x1; x++)
                {
                    for (var y = y0; y < y1; y++)
                    {
                        if (!visited.Contains((x, y)))
                        {
                            visited.Add((x, y));
                            channelZone.Add((x, y));
                        }
                    }
                }
            }

            channelZoneCache[channel] = channelZone;
            return channelZone;
        }

        private double FindAcceptorRating(Channel channel, GridMap targetMap, RatingStrategy ratingStrategy)
        {
            var result = 0;
            var zone = GetChannelZone(channel, targetMap.Width, targetMap.Height);
            foreach ((var x, var y) in zone)
            {
                if (ecoTargetMap[x, y] == TARGET_ACCEPTOR_CELL)
                {
                    result++;
                }
            }
            double ratioRating = zone.Count > 0 ? (double) result / zone.Count : 0;
            return strategy == RatingStrategy.Length ? result : ratioRating;
        }

        ProjectPlan Run(CofinanceOutput input)
        {
            return null;
        }
    }
}
