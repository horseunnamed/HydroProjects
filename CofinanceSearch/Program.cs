using System.Collections.Generic;
using CofinanceSearch.Stats;
using Core;
using Core.Channels;
using Core.Grid;
using PlanSearch;
using System;
using System.Linq;

namespace CofinanceSearch
{
    class Program
    {
        private static Dictionary<Channel,double> FormChannelPrices(double projectPrice,Dictionary<Channel,double> agentCofinancing)
        {
            Dictionary<Channel, double> channelPrices = new Dictionary<Channel, double>();
            foreach (Channel channel in agentCofinancing.Keys) channelPrices.Add(channel, projectPrice - agentCofinancing[channel]);
            return channelPrices;
        }

        private static double EuclidNorm(Dictionary<Channel,double> x, Dictionary<Channel,double> y)
        {
            double s = 0;
            foreach (Channel channel in x.Keys) s += (x[channel] - y[channel]) * (x[channel] - y[channel]);
            return Math.Sqrt(s);
        }

        private static Dictionary<Channel,double> FindOptimalCofinancePlan(double centerResource, double k, double alpha, double beta, double gamma, double eps)
        {
            // reading of channels tree from file:
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));

            // calculating stats:
            var floodMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("only_territory_flood_frequency0.grd"));
            var typeUseMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("TypeUseWithNonCadastr.grd"));
            var hozValues = new HashSet<double> { 37, 38, 39, 43, 44, 45 };
            var socValues = new HashSet<double> { 58, 61, 63, 66, 171, 186, 195, 203, 207 };
            var nonCadastrId = 8888d;
            var r = 10;
            var stats = StatsAggregation.GetStats(channelsTree, floodMap, typeUseMap, hozValues, socValues, nonCadastrId, r);

            // calculating agents' revenues and their initial cofinancing:
            Dictionary<Channel, double> agentRevenue = new Dictionary<Channel, double>();
            Dictionary<Channel, double> agentCofinancing = new Dictionary<Channel, double>();
            Dictionary<Channel, double> agentCofinancingNew = new Dictionary<Channel, double>();

            foreach (var channelstats in stats.ChannelsStats)
            {
                agentRevenue.Add(channelstats.Channel, k * Math.Pow(channelstats.AggrStats.Length, alpha) * Math.Pow((channelstats.AggrStats.SocNotFlooded + channelstats.AggrStats.HozNotFlooded), beta));
                agentCofinancing.Add(channelstats.Channel, 0);
                agentCofinancingNew.Add(channelstats.Channel, 0);
            }

            // calculating channel price without cofinancing:
            Dictionary<Channel, double> channelPrices = new Dictionary<Channel, double>();
            double projectPrice = gamma * agentRevenue.Values.Max();
            foreach (var channelstats in stats.ChannelsStats)
                channelPrices.Add(channelstats.Channel, projectPrice);

            // read flood series for Q=23 and t=[20, 39]:
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 20, 39);

            // read eco targets map
            var ecoTargetMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("frequencies/add_frequency_from_0,65_to_0,85.grd"));

            // initialling starting project plan without cofinancing
            var donorsAcceptors = new DonorsAcceptors(DonorsAcceptors.RatingStrategy.TargetCount, channelsTree, ecoTargetMap, floodSeries);
            var cofinanceInfo = new CofinanceInfo(centerResource, channelPrices);
            var projectPlan = donorsAcceptors.Run(cofinanceInfo);

            while (true)
            {
                foreach (var channelstats in stats.ChannelsStats)
                {
                    Channel currentChannel = channelstats.Channel;

                    if (!projectPlan.GetBestEstimation().Donors.Contains(currentChannel) && (agentCofinancing[currentChannel] < agentRevenue[currentChannel]))
                    {
                        double currentStep =agentRevenue[currentChannel] - agentCofinancing[currentChannel];
                        agentCofinancing[currentChannel] += currentStep;

                        cofinanceInfo = new CofinanceInfo(centerResource, FormChannelPrices(projectPrice,agentCofinancing));
                        projectPlan = donorsAcceptors.Run(cofinanceInfo);

                        if (!projectPlan.GetBestEstimation().Donors.Contains(currentChannel)) continue;
                        else
                        {
                           double bestAgentCofinancing = 0;
                           currentStep /= 2;
                            while (currentStep > eps)
                            {
                                if (!projectPlan.GetBestEstimation().Donors.Contains(currentChannel)) agentCofinancing[currentChannel] += currentStep;
                                else { bestAgentCofinancing = agentCofinancing[currentChannel]; agentCofinancing[currentChannel] = -currentStep; }

                                cofinanceInfo = new CofinanceInfo(centerResource, FormChannelPrices(projectPrice, agentCofinancing));
                                projectPlan = donorsAcceptors.Run(cofinanceInfo);

                                currentStep /= 2;
                            }
                            agentCofinancing[currentChannel] = bestAgentCofinancing;
                        }
                    }
                }
                if (EuclidNorm(agentCofinancing, agentCofinancingNew) < eps) break;
                else
                {
                    foreach (Channel channel in agentCofinancing.Keys) agentCofinancingNew[channel] = agentCofinancing[channel];
                }
            }
            return agentCofinancing;
        }

        private static void Main()
        {

            
        }
    }
}
