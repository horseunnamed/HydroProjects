using System;
using System.Collections.Generic;

namespace PlanSearch
{
    public class CofinanceInfo
    {
        public double R { get; }
        public IDictionary<long, double> ChannelsPrices { get; }

        public CofinanceInfo(double r, IDictionary<long, double> channelsNewPrices)
        {
            R = r;
            ChannelsPrices = channelsNewPrices ?? throw new ArgumentNullException(nameof(channelsNewPrices));
        }
    }
}
