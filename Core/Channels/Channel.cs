using System.Collections.Generic;

namespace Core.Channels
{
    public class Channel
    {
        public long Id { get; }
        public Channel Parent { get; set; }
        public List<Channel> Children { get; set; } = new List<Channel>();
        public List<ChannelPoint> Points { get; set; } = new List<ChannelPoint>();

        public Channel(Channel parent, long id)
        {
            Parent = parent;
            Id = id;
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
