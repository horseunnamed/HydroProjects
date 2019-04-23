using System;
using System.Collections.Generic;
using Core.Channels;

namespace CofinanceSearch.Stats
{
    public class Stats
    {
        public int Length { get; }
        public int NonCadastrFlooded { get; }
        public int NonCadastrNotFlooded { get; }
        public int HozFlooded { get; }
        public int HozNotFlooded { get; }
        public int SocNotFlooded { get; }

        public Stats(int length, int nonCadastrFlooded, int nonCadastrNotFlooded, 
            int hozFlooded, int hozNotFlooded, int socNotFlooded)
        {
            Length = length;
            NonCadastrFlooded = nonCadastrFlooded;
            NonCadastrNotFlooded = nonCadastrNotFlooded;
            HozFlooded = hozFlooded;
            HozNotFlooded = hozNotFlooded;
            SocNotFlooded = socNotFlooded;
        }
    }

    public class ChannelStats
    {
        public Channel Channel { get; }
        public Stats SelfStats { get; }
        public Stats AggrStats { get; }

        public ChannelStats(Channel channel, Stats selfStats, Stats aggrStats)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            SelfStats = selfStats ?? throw new ArgumentNullException(nameof(selfStats));
            AggrStats = aggrStats ?? throw new ArgumentNullException(nameof(aggrStats));
        }
    }

    public class ChannelSystemStats
    {
        public IList<ChannelStats> ChannelsStats { get; }

        public ChannelSystemStats(IList<ChannelStats> channelsStats)
        {
            ChannelsStats = channelsStats;
        }
    }
}
