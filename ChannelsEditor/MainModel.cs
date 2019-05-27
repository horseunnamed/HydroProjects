﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Core;
using Core.Channels;

namespace ChannelsEditor
{
    class MainModel
    {
        private readonly ChannelsTree _channelsTree;
        private readonly Dictionary<ChannelPoint, Channel> _pointsToChannels = new Dictionary<ChannelPoint, Channel>();

        public MainModel(ChannelsTree channelsTree)
        {
            _channelsTree = channelsTree;
            _channelsTree.VisitChannelsFromTop(channel =>
            {
                foreach (var point in channel.Points)
                {
                    _pointsToChannels[point] = channel;
                }
            });
        }

        public Channel GetChannelAt(ChannelPoint point)
        {
            if (_pointsToChannels.ContainsKey(point))
            {
                return _pointsToChannels[point];
            }

            return null;
        }

        public Bitmap DrawChannels(Channel selectedChannel)
        {
            var selectedSubChildren = new List<Channel>();
            if (selectedChannel != null)
            {
                _channelsTree.VisitChannelsDepthFromTop(selectedChannel, (channel, depth) =>
                {
                    if (depth > 0)
                    {
                        selectedSubChildren.Add(channel);
                    }
                });
            }
            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, _channelsTree.GetAllChannels(), new SolidBrush(Color.Black));
                if (selectedChannel != null)
                {
                    Drawing.DrawChannels(g, new List<Channel> { selectedChannel }, new SolidBrush(Color.LawnGreen), true);
                    Drawing.DrawChannels(g, selectedSubChildren, new SolidBrush(Color.DodgerBlue), true);
                    var selectedParent = _channelsTree.GetParentOf(selectedChannel);
/*
                    if (selectedParent != null)
                    {
                        Drawing.DrawChannels(g, new List<Channel> { selectedParent }, new SolidBrush(Color.Yellow));
                    }
*/
                }
            });
            return bitmap;
        }

        public IList<Channel> GetAllChannels()
        { 
            return _channelsTree.GetAllChannels().ToList();
        }

    }
}
