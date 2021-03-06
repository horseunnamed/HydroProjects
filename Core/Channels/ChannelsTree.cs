﻿using System.Collections.Generic;

namespace Core.Channels
{
    public class ChannelsTree
    {
        public Channel Root { get; }
        private IDictionary<Channel, Channel> ParentOf { get; } = new Dictionary<Channel, Channel>();
        private IDictionary<long, Channel> ChannelsById { get; } = new Dictionary<long, Channel>();

        public delegate void ChannelsVisitor(Channel channel);
        public delegate void ChannelsDepthVisitor(Channel channel, int depth);

        public ChannelsTree(Channel root)
        {
            Root = root;
            BuildChannelsById();
            BuildParentOf();
        }

        public Channel GetParentOf(Channel channel)
        {
            return ParentOf.ContainsKey(channel) ? ParentOf[channel] : null;
        }

        public Channel GetChannelById(long id)
        {
            return ChannelsById[id];
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

        public void VisitChannelsDepthFromTop(Channel baseChannel, ChannelsDepthVisitor visitor)
        {
            VisitChannelsDepthFromTopRec(baseChannel, 0, visitor);
        }

        public void VisitChannelsFromBottom(ChannelsVisitor visitor)
        {
            VisitChannelsFromBottomRec(Root, visitor);
        }

        private void BuildChannelsById()
        {
            VisitChannelsFromTop(channel => { ChannelsById[channel.Id] = channel; });
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

            foreach (var child in channel.Connecions)
            {
                BuildParentOfRec(child, channel);
            }
        }

        private void VisitChannelsFromTopRec(Channel channel, ChannelsVisitor visitor)
        {
            visitor(channel);
            foreach (var child in channel.Connecions)
            {
                VisitChannelsFromTopRec(child, visitor);
            }
        }

        private void VisitChannelsFromBottomRec(Channel channel, ChannelsVisitor visitor)
        {
            foreach (var child in channel.Connecions)
            {
                VisitChannelsFromBottomRec(child, visitor);
            }

            visitor(channel);
        }

        private void VisitChannelsDepthFromTopRec(Channel channel, int depth, ChannelsDepthVisitor visitor)
        {
            visitor(channel, depth);
            foreach (var child in channel.Connecions)
            {
                VisitChannelsDepthFromTopRec(child, depth + 1, visitor);
            }
        }
    }
}
