using System.Collections.Generic;

namespace Core.Channels
{
    public class ChannelsTree
    {
        public Channel Root { get; }
        private IDictionary<Channel, Channel> ParentOf { get; } = new Dictionary<Channel, Channel>();

        public delegate void ChannelsVisitor(Channel channel);

        public ChannelsTree(Channel root)
        {
            Root = root;
            BuildParentOf();
        }

        public Channel GetParentOf(Channel channel)
        {
            return ParentOf.ContainsKey(channel) ? ParentOf[channel] : null;
        }

        public IEnumerable<Channel> GetAllChannels() 
        {
            var result = new List<Channel>();
            VisitChannelsFromTop((channel) =>
            {
                result.Add(channel);
            });
            return result;
        }

        public void VisitChannelsFromTop(ChannelsVisitor visitor)
        {
            VisitChannelsFromTopRec(Root, visitor);
        }

        public void VisitChannelsFromTop(Channel baseChannel, ChannelsVisitor visitor)
        {
            VisitChannelsFromTopRec(baseChannel, visitor);
        }

        public void VisitChannelsFromBottom(ChannelsVisitor visitor)
        {
            VisitChannelsFromBottomRec(Root, visitor);
        }

        private void BuildParentOf()
        {
            BuildParentOfRec(Root, null);
        }

        private void BuildParentOfRec(Channel channel, Channel parent)
        {
            if (parent != null)
            {
                ParentOf[channel] = parent;
            }

            foreach (var child in channel.Children)
            {
                BuildParentOfRec(child, channel);
            }
        }

        private void VisitChannelsFromTopRec(Channel channel, ChannelsVisitor visitor)
        {
            visitor(channel);
            foreach (var child in channel.Children)
            {
                VisitChannelsFromTopRec(child, visitor);
            }
        }

        private void VisitChannelsFromBottomRec(Channel channel, ChannelsVisitor visitor)
        {
            foreach (var child in channel.Children)
            {
                VisitChannelsFromBottomRec(child, visitor);
            }

            visitor(channel);
        }
    }
}
