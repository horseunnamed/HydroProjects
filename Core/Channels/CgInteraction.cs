using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Channels
{
    public class CgInteraction
    {
        public static ChannelsTree ReadChannelsTreeFromCg(string filename)
        {
            var channelsDict = new Dictionary<long, Channel>();
            using (var sr = new StreamReader(filename))
            {
                var channelsCount = int.Parse(sr.ReadLine());
                for (var i = 0; i < channelsCount; i++)
                {
                    var parts = sr.ReadLine().Split(' ');
                    var channelId = long.Parse(parts[0]);
                    var pointsCount = int.Parse(parts[1]);
                    var channel = new Channel(null, channelId);
                    for (var j = 0; j < pointsCount; j++)
                    {
                        var pointParts = sr.ReadLine().Split(' ');
                        var x = int.Parse(pointParts[0]);
                        var y = int.Parse(pointParts[1]);
                        channel.Points.Add(new ChannelPoint(x, y));
                    }
                    channelsDict[channelId] = channel;
                }
                var connectionsCount = int.Parse(sr.ReadLine());
                for (var i = 0; i < connectionsCount; i++)
                {
                    var parts = sr.ReadLine().Split(' ');
                    var parentId = long.Parse(parts[0]);
                    var childId = long.Parse(parts[1]);
                    var parent = channelsDict[parentId];
                    var child = channelsDict[childId];
                    parent.Children.Add(child);
                    child.Parent = parent;
                }
            }
            var tree = new ChannelsTree(channelsDict[1]);
            return tree;
        }

        public static void WriteChannelsTreeToCg(string filename, ChannelsTree tree)
        {
            var resultBuilder = new StringBuilder();
            var pointsBuilder = new StringBuilder();
            var connectionsBuilder = new StringBuilder();
            var channelsCount = 0;
            var connectionsCount = 0;
            tree.VisitChannelsFromTop(channel =>
            {
                channelsCount++;
                connectionsCount += channel.Children.Count;
                pointsBuilder.AppendLine($"{channel.Id} {channel.Points.Count}");
                foreach (var point in channel.Points)
                {
                    pointsBuilder.AppendLine($"{point.X} {point.Y}");
                }
                foreach (var child in channel.Children)
                {
                    connectionsBuilder.AppendLine($"{channel.Id} {child.Id}");
                }
            });
            resultBuilder.AppendLine($"{channelsCount}");
            resultBuilder.Append(pointsBuilder.ToString());
            resultBuilder.AppendLine($"{connectionsCount}");
            resultBuilder.Append(connectionsBuilder.ToString());
            File.WriteAllText(filename, resultBuilder.ToString());
        }
    }
}
