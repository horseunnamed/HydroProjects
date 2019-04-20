using System.Collections.Generic;

namespace Core.Channels
{
    public class ChannelsTree
    {
        public Channel Root { get; }

        public delegate void ChannelsVisitor(Channel channel);

        public ChannelsTree(Channel root)
        {
            Root = root;
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

        private void VisitChannelsFromTopRec(Channel channel, ChannelsVisitor visitor)
        {
            visitor(channel);
            for (int i = 0; i < channel.Children.Count; i++)
            {
                VisitChannelsFromTopRec(channel.Children[i], visitor);
            }
        }

        public void VisitChannelsFromBottom(ChannelsVisitor visitor)
        {
            VisitChannelsFromBottomRec(Root, visitor);
        }

        private void VisitChannelsFromBottomRec(Channel channel, ChannelsVisitor visitor)
        {
            for (int i = 0; i < channel.Children.Count; i++)
            {
                VisitChannelsFromBottomRec(channel.Children[i], visitor);
            }
            visitor(channel);
        }
    }
}
