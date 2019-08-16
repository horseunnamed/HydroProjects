using System;
using System.Collections.Generic;
using System.Linq;
using Core.Channels;
using Core.Grid;

namespace PlanSearch
{
    using ChannelCells = Dictionary<Channel, IEnumerable<(int, int)>>;

    public class DonorsAcceptors
    {
        public enum RatingStrategy
        {
            TargetCount, TargetRatio
        }

        private class Targets
        {
            public const int ZoneR = 10;

            public readonly ChannelCells zones;
            public readonly ChannelCells needFlood;
            public readonly ChannelCells alreadyFlooded;

            public Targets(ChannelCells zones, ChannelCells needFlood, ChannelCells alreadyFlooded)
            {
                this.zones = zones ?? throw new ArgumentNullException(nameof(zones));
                this.needFlood = needFlood ?? throw new ArgumentNullException(nameof(needFlood));
                this.alreadyFlooded = alreadyFlooded ?? throw new ArgumentNullException(nameof(alreadyFlooded));
            }

            public static Targets Create(ChannelsTree channelsTree, GridMap targetMap, double targetValue, GridMap floodMap)
            {
                var zones = GetChannelZones(channelsTree, targetMap.Width, targetMap.Height);
                var needFlood = new ChannelCells();
                var alreadyFlooded = new ChannelCells();

                foreach (var channelZone in zones)
                {
                    var zoneFloodedCells = new List<(int, int)>();
                    var zoneDryCells = new List<(int, int)>();
                    foreach (var (x, y) in channelZone.Value)
                    {
                        if (targetMap[x, y] == targetValue)
                        {
                            if (floodMap[x, y] == 0)
                            {
                                zoneDryCells.Add((x, y));
                            }
                            else
                            {
                                zoneFloodedCells.Add((x, y));
                            }
                        }
                    }

                    needFlood[channelZone.Key] = zoneDryCells;
                    alreadyFlooded[channelZone.Key] = zoneFloodedCells;
                }

                return new Targets(zones, needFlood, alreadyFlooded);
            }

            private static ChannelCells GetChannelZones(ChannelsTree channelsTree, int mapW, int mapH)
            {
                var result = new Dictionary<Channel, IEnumerable<(int, int)>>();
                channelsTree.VisitChannelsFromTop(channel =>
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
        }

        private class Rating
        {
            public readonly Dictionary<Channel, double> acceptorsRating;
            public readonly List<(double, Channel)> orderedByAcceptorsRating;
            public readonly RatingStrategy strategy;

            public Rating(Dictionary<Channel, double> acceptorsRating, List<(double, Channel)> orderedByAcceptorsRating, RatingStrategy strategy)
            {
                this.acceptorsRating = acceptorsRating ?? throw new ArgumentNullException(nameof(acceptorsRating));
                this.orderedByAcceptorsRating = orderedByAcceptorsRating ?? throw new ArgumentNullException(nameof(orderedByAcceptorsRating));
                this.strategy = strategy;
            }

            public static Rating Create(ChannelsTree channelsTree, Targets targets, RatingStrategy strategy)
            {
                var acceptorsRating = new Dictionary<Channel, double>();

                channelsTree.VisitChannelsFromTop(channel => 
                {
                    acceptorsRating[channel] = GetAcceptorRatingValue(channel, targets, strategy);
                });

                var orderedByRating = acceptorsRating.ToList() 
                    .Select(keyValue => (keyValue.Value, keyValue.Key))
                    .OrderByDescending(pair => pair.Item1)
                    .ToList();

                return new Rating(acceptorsRating, orderedByRating, strategy);
            }

            private static double GetAcceptorRatingValue(Channel channel, Targets targets, RatingStrategy strategy)
            {
                var targetCount = targets.needFlood[channel].Count();
                var zoneSize = targets.zones[channel].Count();
                var targetRatio = zoneSize > 0 ? (double) targetCount / zoneSize : 0;
                return strategy == RatingStrategy.TargetCount ? targetCount : targetRatio;
            }

        }

        private readonly ChannelsTree _channelsTree;
        private readonly Dictionary<Channel, double> _vEstimation;
        private readonly Targets _targets;
        private readonly Rating _rating;

        private DonorsAcceptors(
            ChannelsTree channelsTree,
            Dictionary<Channel, double> vEstimation,
            Targets targets,
            Rating rating)
        {
            _channelsTree = channelsTree ?? throw new ArgumentNullException(nameof(channelsTree));
            _vEstimation = vEstimation ?? throw new ArgumentNullException(nameof(vEstimation));
            _targets = targets ?? throw new ArgumentNullException(nameof(targets));
            _rating = rating ?? throw new ArgumentNullException(nameof(rating));
        }

        public static DonorsAcceptors Create(
            RatingStrategy strategy,
            ChannelsTree channelsTree,
            GridMap targetMap,
            double targetValue,
            FloodSeries floodSeries)
        {
            var targets = Targets.Create(channelsTree, targetMap, targetValue, floodSeries.CombineToFloodmap());
            var rating = Rating.Create(channelsTree, targets, strategy);
            var vEstimation = GetVEstimation(channelsTree, floodSeries);
            return new DonorsAcceptors(channelsTree, vEstimation, targets, rating);
        }

        private static Dictionary<Channel, double> GetVEstimation(ChannelsTree channelsTree, FloodSeries floodSeries)
        {
            var result = new Dictionary<Channel, double>();
            channelsTree.VisitChannelsFromTop(channel =>
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

                result[channel] = sumV; // Math.Max(sumV - channel.Points.Count * 2 * 50 * 50 / 1e6, 0);
            });
            return result;
        }

        public ProjectPlan Run(CofinanceInfo cofinanceInfo, int maxS, IEnumerable<long> blackList) 
        {
            var estimations = new List<ProjectPlan.Estimation>();
            for (var s = 1; s < _rating.orderedByAcceptorsRating.Count && s <= maxS; s++)
            {
                var acceptors = _rating.orderedByAcceptorsRating
                    .Take(s)
                    .Select(pair => pair.Item2)
                    // .Where(channel => !blackList.Contains(channel.Id))
                    .ToHashSet();

                var potentialDonors = GetDonors(acceptors)
                    .Where(donor => !blackList.Contains(donor.Channel.Id))
                    .ToHashSet();

                var optimalDonors = DonorsOptimizer.FindOptimalDonors(potentialDonors, cofinanceInfo);

                var totalEffect = optimalDonors
                    .Select(donor => donor.Effect)
                    .Sum();

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
                        acceptorsTargetValue: GetTargetsCountFor(acceptors)
                    )
                );
            }
            return new ProjectPlan(_targets.zones, estimations);
        }

        private int GetTargetsCountFor(ISet<Channel> acceptors)
        {
            var allTargets = new HashSet<(int, int)>();
            foreach (var acceptor in acceptors)
            {
                allTargets.UnionWith(_targets.needFlood[acceptor]);
            }
            return allTargets.Count;
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
            return _targets.alreadyFlooded[channel].Count() < channel.Points.Count * 2;
        }

    }
}
