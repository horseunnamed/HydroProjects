using System.Collections.Generic;

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
        public long ChannelId { get; }
        public Stats SelfStats { get; }
        public Stats AggrStats { get; }

        public ChannelStats(long channelId, Stats selfStats, Stats aggrStats)
        {
            ChannelId = channelId;
            SelfStats = selfStats;
            AggrStats = aggrStats;
        }
    }

    public class ChannelSystemStats
    {
        public List<ChannelStats> ChannelsStats { get; }

        public ChannelSystemStats(List<ChannelStats> channelsStats)
        {
            ChannelsStats = channelsStats;
        }
    }
}
