using System.Collections.Generic;

namespace IntegratedAlgo
{
    public class ChannelsStats
    {
        class ChannelStats
        {
            public long ChannelId = 0;
            public int Length = 0;
            public int NonCadastrFlooded = 0;
            public int NonCadastrNotFlooded = 0;
            public int HozFlooded = 0;
            public int HozNotFlooded = 0;
            public int SocNotFlooded = 0;
        }

        class Stats
        {
            public ChannelStats SelfStats;
            public ChannelStats AggrStats;
        }

        public List<ChannelsStats> stats = new List<ChannelsStats>();
    }
}
