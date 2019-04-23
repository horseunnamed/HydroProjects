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

        public const int ZoneR = 10;
        public const double TargetCell = 3;

        private readonly int _maxS;
        private readonly RatingStrategy _strategy;
        private readonly ChannelsTree _channelsTree;
        private readonly IDictionary<Channel, List<(int, int)>> _channelZones;
        private readonly IDictionary<Channel, double> _targetValues;
        private readonly IList<(double, Channel)> _targetRating;
        private readonly IDictionary<Channel, double> _vEstimation;

        public DonorsAcceptors(RatingStrategy strategy, ChannelsTree channelsTree, GridMap ecoTargetMap, FloodSeries floodSeries, int maxS=100)
        {
            _strategy = strategy;
            _channelsTree = channelsTree ?? throw new ArgumentNullException(nameof(channelsTree));
            _maxS = maxS;

            _channelZones = GetChannelZones(ecoTargetMap.Width, ecoTargetMap.Height);
            _targetValues = GetTargetValues(ecoTargetMap);
            _targetRating = ToRating(_targetValues);
            _vEstimation = GetVEstimation(floodSeries);
        }

        public ProjectPlan Run(CofinanceInfo cofinanceInfo)
        {
            var estimations = new List<ProjectPlan.Estimation>();
            for (var s = 1; s < _targetRating.Count && s <= _maxS; s++)
            {
                var acceptors = new HashSet<Channel>(_targetRating.Take(s).Select(pair => pair.Item2));
                var potentialDonors = GetDonors(acceptors);
                var optimalDonors = DonorsOptimizer.FindOptimalDonors(potentialDonors, cofinanceInfo);
                var totalEffect = optimalDonors.Select(donor => donor.Effect).Sum();
                var totalPrice = optimalDonors
                    .Select(donor => donor.Channel.Id)
                    .Select(donorId => cofinanceInfo.ChannelsPrices[donorId])
                    .Sum();
                estimations.Add(new ProjectPlan.Estimation(s, optimalDonors.Count, potentialDonors.Count, totalEffect, totalPrice,
                    new HashSet<Channel>(optimalDonors.Select(donor => donor.Channel)), acceptors));
            }
            return new ProjectPlan(estimations);
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
                    foreach (var day in floodSeries.Days)
                    {
                        var vx = day.VxMap[origin.X, origin.Y];
                        var vy = day.VyMap[origin.X, origin.Y];
                        var h = day.HMap[origin.X, origin.Y];
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
            var targetRatio = zone.Count > 0 && channel.Points.Count > 10 ? (double) targetCount / zone.Count : 0;
            return _strategy == RatingStrategy.TargetCount ? targetCount : targetRatio;
        }

        private IDictionary<Channel, double> GetTargetValues(GridMap targetMap)
        {
            var result = new Dictionary<Channel, double>();
            _channelsTree.VisitChannelsFromTop(channel => { result[channel] = GetChannelTargetValue(channel, targetMap); });
            return result;
        }

        private static IList<(double, Channel)> ToRating(IDictionary<Channel, double> values)
        {
            return values.ToList()
                .Select(keyValue => (keyValue.Value, keyValue.Key))
                .OrderByDescending(pair => pair.Item1).ToList();
        }

        private ISet<Donor> GetDonors(IEnumerable<Channel> acceptors)
        {
            var result = new HashSet<Donor>();
            var transAcceptors = new HashSet<Channel>();
            foreach (var channel in acceptors)
            {
                transAcceptors.UnionWith(GetChannelsPathTo(channel));
            }
            _channelsTree.VisitChannelsFromTop(channel => {
                var parent = channel.Parent;
                if (
                        IsAllowedToBeDonor(channel) &&
                        transAcceptors.Contains(parent) && 
                        !transAcceptors.Contains(channel)
                    )
                {
                    result.Add(new Donor(channel, _vEstimation[channel]));
                }
            });
            return result;
        }

        private static IEnumerable<Channel> GetChannelsPathTo(Channel channel)
        {
            if (channel == null)
            {
                return new HashSet<Channel>();
            }

            var result = new HashSet<Channel> {channel};
            result.UnionWith(GetChannelsPathTo(channel.Parent));
            return result;
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
