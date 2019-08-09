using System.Collections.Generic;

namespace Core.Channels
{
    public class Channel
    {
        public long Id { get; }
        public bool IsEntrance { get; set; }
        public List<ChannelPoint> Points { get; set; } = new List<ChannelPoint>();
        public List<Channel> Connecions { get; set; } = new List<Channel>();

        public Channel(long id, bool isEntrance = false)
        {
            Id = id;
            IsEntrance = isEntrance;
        }

        protected bool Equals(Channel other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
