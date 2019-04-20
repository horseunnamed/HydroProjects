using System;
using System.Collections.Generic;

namespace PlanSearch
{
    public class CofinanceInfo
    {
        public double R { get; }
        public Dictionary<long, double> ChannelsNewPrices { get; }

        public CofinanceInfo(double r, Dictionary<long, double> channelsNewPrices)
        {
            R = r;
            ChannelsNewPrices = channelsNewPrices ?? throw new ArgumentNullException(nameof(channelsNewPrices));
        }
    }
}
