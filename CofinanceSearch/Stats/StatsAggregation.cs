using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.Channels;
using Core.Grid;

namespace CofinanceSearch.Stats
{
    public class StatsAggregation
    {
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static ChannelSystemStats GetStats(
            ChannelsTree channelsTree, GridMap floodMap, GridMap typeUseMap, 
            ISet<double> hozValues, ISet<double> socValues, double nonCadastrId, int r)
        {
            var selfStatsDict = new Dictionary<Channel, Stats>();
            var aggrStatsDict = new Dictionary<Channel, Stats>();

            channelsTree.VisitChannelsFromTop(channel =>
            {
                var nonCadastrFlooded = 0;
                var nonCadastrNotFlooded = 0;
                var hozFlooded = 0;
                var hozNotFlooded = 0;
                var socNotFlooded = 0;

                var visitedCells = new HashSet<(int, int)>();
                foreach (var point in channel.Points)
                {
                    for (var x = point.X - r; x <= point.X + r; x++)
                    {
                        for (var y = point.Y - r; y <= point.Y + r; y++)
                        {
                            if (x >= 0 && x < typeUseMap.Width && y >= 0 && y < typeUseMap.Height && !visitedCells.Contains((x, y)))
                            {
                                var targetVal = typeUseMap[x, y];
                                var floodVal = floodMap[x, y];
                                if (targetVal == nonCadastrId)
                                {
                                    if (floodVal == 0)
                                    {
                                        nonCadastrNotFlooded++;
                                    }
                                    else
                                    {
                                        nonCadastrFlooded++;
                                    }
                                }
                                else if (hozValues.Contains(targetVal))
                                {
                                    if (floodVal == 0)
                                    {
                                        hozNotFlooded++;
                                    }
                                    else
                                    {
                                        hozFlooded++;
                                    }
                                }
                                else if (socValues.Contains(targetVal) && floodVal == 0)
                                {
                                    socNotFlooded++;
                                }
                            }
                            visitedCells.Add((x, y));
                        }
                    }
                }
                var selfStats = new Stats(
                   channel.Points.Count,
                   nonCadastrFlooded,
                   nonCadastrNotFlooded,
                   hozFlooded,
                   hozNotFlooded,
                   socNotFlooded);

                selfStatsDict[channel] = selfStats;
            });

            var channelsStats = new List<ChannelStats>();

            channelsTree.VisitChannelsFromBottom(channel =>
            {
                var channelStats = selfStatsDict[channel];

                var aggrLength = channelStats.Length;
                var aggrNonCadastrFlooded = channelStats.NonCadastrFlooded;
                var aggrNonCadastrNotFlooded = channelStats.NonCadastrNotFlooded;
                var aggrHozFlooded = channelStats.HozFlooded;
                var aggrHozNotFlooded = channelStats.HozNotFlooded;
                var aggrSocNotFlooded = channelStats.SocNotFlooded;

                foreach (var child in channel.Children)
                {
                    var childAggrStats = aggrStatsDict[child];
                    aggrLength += childAggrStats.Length;
                    aggrNonCadastrFlooded += childAggrStats.NonCadastrFlooded;
                    aggrNonCadastrNotFlooded += childAggrStats.NonCadastrNotFlooded;
                    aggrHozFlooded += childAggrStats.HozFlooded;
                    aggrHozNotFlooded += childAggrStats.HozNotFlooded;
                    aggrSocNotFlooded += childAggrStats.SocNotFlooded;
                }

                var aggrStats = new Stats(
                    aggrLength, 
                    aggrNonCadastrFlooded, 
                    aggrNonCadastrNotFlooded, 
                    aggrHozFlooded, 
                    aggrHozNotFlooded, 
                    aggrSocNotFlooded);

                aggrStatsDict[channel] = aggrStats;

                channelsStats.Add(new ChannelStats(channel.Id, channelStats, aggrStats));
            });


            return new ChannelSystemStats(channelsStats);
        }
    }
}
