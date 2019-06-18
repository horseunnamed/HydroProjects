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
        private readonly IDictionary<Channel, ISet<(int, int)>> _targetCells;
        private readonly IDictionary<Channel, double> _ratingValues;
        private readonly IList<(double, Channel)> _targetRating;
        private readonly IDictionary<Channel, double> _vEstimation;
        private readonly GridMap _ecoTargetMap;

        public DonorsAcceptors(RatingStrategy strategy, ChannelsTree channelsTree, GridMap ecoTargetMap, FloodSeries floodSeries, int maxS=100)
        {
            _strategy = strategy;
            _channelsTree = channelsTree ?? throw new ArgumentNullException(nameof(channelsTree));
            _maxS = maxS;
            _ecoTargetMap = ecoTargetMap;

            _channelZones = GetChannelZones(ecoTargetMap.Width, ecoTargetMap.Height);
            _targetCells = GetTargetCells(ecoTargetMap);
            _ratingValues = GetRatingValues(ecoTargetMap);
            _targetRating = ToRating(_ratingValues);
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
                    .Select(donor => cofinanceInfo.ChannelsPrices[donor.Channel])
                    .Sum();
                estimations.Add(
                    new ProjectPlan.Estimation( 
                        s: s, 
                        optimalDonorsCount: optimalDonors.Count, 
                        potentialDonorsCount: potentialDonors.Count, 
                        totalEffect: totalEffect, 
                        totalPrice: totalPrice,
                        donors: new HashSet<Channel>(optimalDonors.Select(donor => donor.Channel)),
                        acceptors: acceptors, 
                        acceptorsTargetValue: GetTargetsCountFor(acceptors),
                        acceptorZonesMap: CreateGridMapForZonesOf(acceptors),
                        donorZonesMap: CreateGridMapForZonesOf(optimalDonors.Select(donor => donor.Channel))
                    )
                );
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

        private IDictionary<Channel, ISet<(int, int)>> GetTargetCells(GridMap ecoTargetMap)
        {
            var result = new Dictionary<Channel, ISet<(int, int)>>();
            foreach (var channelZone in _channelZones)
            {
                var zoneTargetCells = new HashSet<(int, int)>();
                foreach (var (x, y) in channelZone.Value)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (ecoTargetMap[x, y] == TargetCell)
                    {
                        zoneTargetCells.Add((x, y));
                    }
                }

                result[channelZone.Key] = zoneTargetCells;
            }

            return result;
        }

        private int GetTargetsCountFor(ISet<Channel> acceptors)
        {
            var allTargets = new HashSet<(int, int)>();
            foreach (var acceptor in acceptors)
            {
                allTargets.UnionWith(_targetCells[acceptor]);
            }
            return allTargets.Count;
        }

        private double GetChannelRatingValue(Channel channel, GridMap targetMap)
        {
            var targetCount = _targetCells[channel].Count;
            var zoneSize = _channelZones[channel].Count;
            var targetRatio = zoneSize > 0 ? (double) targetCount / zoneSize : 0;
            return _strategy == RatingStrategy.TargetCount ? targetCount : targetRatio;
        }

        private IDictionary<Channel, double> GetRatingValues(GridMap targetMap)
        {
            var result = new Dictionary<Channel, double>();
            _channelsTree.VisitChannelsFromTop(channel => { result[channel] = GetChannelRatingValue(channel, targetMap); });
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
                var parent = _channelsTree.GetParentOf(channel);
                if (
                        parent != null &&
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

        private IEnumerable<Channel> GetChannelsPathTo(Channel channel)
        {
            if (channel == _channelsTree.Root)
            {
                return new HashSet<Channel>();
            }

            var result = new HashSet<Channel> {channel};
            result.UnionWith(GetChannelsPathTo(_channelsTree.GetParentOf(channel)));
            return result;
        }

        private bool IsAllowedToBeDonor(Channel channel)
        {
            var targetValue = _ratingValues[channel];
            if (_strategy == RatingStrategy.TargetCount)
            {
                return targetValue < channel.Points.Count * 2;
            }
            return targetValue < 0.5;
        }

        private GridMap CreateGridMapForZonesOf(IEnumerable<Channel> channels)
        {
            var gridMap = GridMap.CreateByParamsOf(_ecoTargetMap, 0);
            foreach (var channel in channels)
            {
                var zone = _channelZones[channel];
                foreach (var (x, y) in zone)
                {
                    gridMap[x - 1, y - 1] = channel.Id;
                }
            }

            return gridMap;
        }
    }
}
