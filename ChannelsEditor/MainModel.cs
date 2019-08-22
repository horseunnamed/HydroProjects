using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Core;
using Core.Channels;

namespace ChannelsEditor
{
    class MainModel
    {
        private readonly ChannelsGraph _channelsGraph;
        private readonly Dictionary<ChannelPoint, Channel> _channelsByPoints = new Dictionary<ChannelPoint, Channel>();
        private readonly Dictionary<long, Channel> _channelsById = new Dictionary<long, Channel>();
        private readonly List<Channel> _channels = new List<Channel>();

        public MainModel(ChannelsGraph channelsGraph)
        {
            _channelsGraph = channelsGraph;
            _channelsGraph.BFS(channel =>
            {
                _channels.Add(channel);
                _channelsById[channel.Id] = channel;
                foreach (var point in channel.Points)
                {
                    _channelsByPoints[point] = channel;
                }
            });
        }

        public Channel GetChannelById(long id)
        {
            return _channelsById[id];
        }

        public Channel GetChannelAt(ChannelPoint point)
        {
            var searchRadius = 10;

            for (var r = 1; r <= searchRadius; r++)
            {
                for (var xi = point.X - r; xi <= point.X + r; xi++)
                {
                    var channel = _channelsByPoints.GetValue(new ChannelPoint(xi, point.Y + r));
                    if (channel != null)
                    {
                        return channel;
                    }

                    channel = _channelsByPoints.GetValue(new ChannelPoint(xi, point.Y - r));
                    if (channel != null)
                    {
                        return channel;
                    }
                }

                for (var yi = point.Y - r; yi <= point.Y + r; yi++)
                {
                    var channel = _channelsByPoints.GetValue(new ChannelPoint(point.X + r, yi));
                    if (channel != null)
                    {
                        return channel;
                    }
                    channel = _channelsByPoints.GetValue(new ChannelPoint(point.X - r, yi));
                    if (channel != null)
                    {
                        return channel;
                    }
                }

            }

            return null;
        }

        public Bitmap DrawChannels(Channel selectedChannel)
        {
            var selectedSubChildren = new List<Channel>();
            if (selectedChannel != null)
            {
                selectedSubChildren.AddRange(selectedChannel.Connecions);
            }
            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, _channels, new SolidBrush(Color.Black));
                if (selectedChannel != null)
                {
                    Drawing.DrawChannels(g, new List<Channel> { selectedChannel }, new SolidBrush(Color.LawnGreen), true);
                    Drawing.DrawChannels(g, selectedSubChildren, new SolidBrush(Color.DodgerBlue), true);
                    // var selectedParent = _channelsTree.GetParentOf(selectedChannel);
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

        public List<Channel> GetAllChannels()
        { 
            return _channels;
        }

    }
}
