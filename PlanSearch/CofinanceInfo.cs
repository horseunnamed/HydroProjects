using System.Collections.Generic;
using Core.Channels;

namespace PlanSearch
{
    public class CofinanceInfo
    {
        public double R { get; }
        public IDictionary<Channel, double> ChannelsPrices { get; }

        public CofinanceInfo(double r, IDictionary<Channel, double> channelsPrices)
        {
            R = r;
            ChannelsPrices = channelsPrices;
        }
    }
}
