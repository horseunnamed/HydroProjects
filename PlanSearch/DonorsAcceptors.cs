using System;
using System.Collections.Generic;
using System.Linq;
using Core.Channels;
using Core.Grid;

namespace PlanSearch
{
    public class DonorsAcceptors
    {
        public enum RatingStrategy
        {
            TargetCount, TargetRatio
        }

        private const int ZoneR = 10;
        private const double TargetCell = 3;

        private readonly RatingStrategy _strategy;
        private readonly ChannelsTree _channelsTree;
        private readonly IDictionary<Channel, List<(int, int)>> _channelZones;
        private readonly IDictionary<Channel, double> _targetValues;
        private readonly IList<(double, Channel)> _targetRating;
        private readonly IDictionary<Channel, double> _vEstimation;

        public DonorsAcceptors(RatingStrategy strategy, ChannelsTree channelsTree, GridMap ecoTargetMap, FloodSeries floodSeries)
        {
            _strategy = strategy;
            _channelsTree = channelsTree ?? throw new ArgumentNullException(nameof(channelsTree));

            _channelZones = GetChannelZones(ecoTargetMap.Width, ecoTargetMap.Height);
            _targetValues = GetTargetValues(ecoTargetMap);
            _targetRating = ToRating(_targetValues);
            _vEstimation = GetVEstimation(floodSeries);
        }

        public ProjectPlan Run(CofinanceInfo input)
        {
            var bestS = -1;
            var bestEstimation = -1.0;
            ISet<Channel> bestAcceptors = null;
            ISet<Channel> bestDonors = null;
            for (var s = 1; s < _targetRating.Count && s < 100; s++)
            {
                var acceptors = new HashSet<Channel>(_targetRating.Take(s).Select(pair => pair.Item2));
                var donors = GetDonors(_channelsTree, acceptors);
                var estimation = donors.Select(channel => _vEstimation[channel]).Sum();
                if (estimation > bestEstimation)
                {
                    bestS = s;
                    bestEstimation = estimation;
                    bestAcceptors = acceptors;
                    bestDonors = donors;
                }
            }
            return new ProjectPlan(bestDonors, bestAcceptors);
        }

        private IDictionary<Channel, double> GetVEstimation(FloodSeries floodSeries)
        {
            var result = new Dictionary<Channel, double>();
            _channelsTree.VisitChannelsFromTop(channel =>
            {
                var sumV = 0d;
                if (channel.Points.Count > 3)
                {
                    var origin = channel.Points[2];
                    for (var day = 0; day < floodSeries.DaysCount; day++)
                    {
                        var vx = floodSeries.VxByDays[day][origin.X, origin.Y];
                        var vy = floodSeries.VyByDays[day][origin.X, origin.Y];
                        var h = floodSeries.HByDays[day][origin.X, origin.Y];
                        sumV += Math.Sqrt(vx * vx + vy * vy) * h * 24 * 60 * 60 / 1e6;
                    }
                }
                result[channel] = Math.Max(sumV - channel.Points.Count * 2 * 50 * 50 / 1e6, 0);
            });
            return result;
        }

        private IDictionary<Channel, List<(int, int)>> GetChannelZones(int mapW, int mapH)
        {
            var result = new Dictionary<Channel, List<(int, int)>>();
            _channelsTree.VisitChannelsFromTop(channel =>
            {
                var visited = new HashSet<(int, int)>();
                var channelZone = new List<(int, int)>();
                foreach (var point in channel.Points)
                {
                    var x0 = Math.Max(0, point.X - ZoneR);
                    var y0 = Math.Max(0, point.Y - ZoneR);
                    var x1 = Math.Min(mapW - 1, point.X + ZoneR);
                    var y1 = Math.Min(mapH - 1, point.Y + ZoneR);

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

                result[channel] = channelZone;
            });
            return result;
        }

        private double GetChannelTargetValue(Channel channel, GridMap targetMap)
        {
            var targetCount = 0;
            var zone = _channelZones[channel];
            foreach (var (x, y) in zone)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (targetMap[x, y] == TargetCell)
                {
                    targetCount++;
                }
            }
            var targetRatio = zone.Count > 0 ? (double) targetCount / zone.Count : 0;
            return _strategy == RatingStrategy.TargetCount ? targetCount : targetRatio;
        }

        private IDictionary<Channel, double> GetTargetValues(GridMap targetMap)
        {
            var result = new Dictionary<Channel, double>();
            _channelsTree.VisitChannelsFromTop(channel => { result[channel] = GetChannelTargetValue(channel, targetMap); });
            return result;
        }

        private IList<(double, Channel)> ToRating(IDictionary<Channel, double> values)
        {
            return values.ToList()
                .Select(keyValue => (keyValue.Value, keyValue.Key))
                .OrderByDescending(pair => pair.Item1).ToList();
        }

        private ISet<Channel> GetDonors(ChannelsTree tree, IEnumerable<Channel> acceptors)
        {
            var result = new HashSet<Channel>();
            var transAcceptors = new HashSet<Channel>();
            foreach (var channel in acceptors)
            {
                transAcceptors.UnionWith(GetChannelsPathTo(channel));
            }
            tree.VisitChannelsFromTop(channel => {
                var parent = channel.Parent;
                if (
                        IsAllowedToBeDonor(channel) &&
                        transAcceptors.Contains(parent) && 
                        !transAcceptors.Contains(channel)
                    )
                {
                    result.Add(channel);
                }
            });
            return result;
        }

        private ISet<Channel> GetChannelsPathTo(Channel channel)
        {
            if (channel == null)
            {
                return new HashSet<Channel>();
            }
            else
            {
                var result = new HashSet<Channel> {channel};
                result.UnionWith(GetChannelsPathTo(channel.Parent));
                return result;
            }
        }

        private bool IsAllowedToBeDonor(Channel channel)
        {
            var targetValue = _targetValues[channel];
            if (_strategy == RatingStrategy.TargetCount)
            {
                return targetValue < channel.Points.Count * 2;
            }
            return targetValue < 0.5;
        }
    }
}
