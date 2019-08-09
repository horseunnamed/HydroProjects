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
                    var channel = new Channel(channelId);
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
                    parent.Connecions.Add(child);
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
                connectionsCount += channel.Connecions.Count;
                pointsBuilder.AppendLine($"{channel.Id} {channel.Points.Count}");
                foreach (var point in channel.Points)
                {
                    pointsBuilder.AppendLine($"{point.X} {point.Y}");
                }
                foreach (var child in channel.Connecions)
                {
                    connectionsBuilder.AppendLine($"{channel.Id} {child.Id}");
                }
            });
            resultBuilder.AppendLine($"{channelsCount}");
            resultBuilder.Append(pointsBuilder);
            resultBuilder.AppendLine($"{connectionsCount}");
            resultBuilder.Append(connectionsBuilder);
            File.WriteAllText(filename, resultBuilder.ToString());
        }

        public static ChannelsGraph ReadChannelsGraphFromCg(string filename)
        {
            var channelsDict = new Dictionary<long, Channel>();
            var entrances = new List<Channel>();
            using (var sr = new StreamReader(filename))
            {
                var channelsCount = int.Parse(sr.ReadLine());
                for (var i = 0; i < channelsCount; i++)
                {
                    var parts = sr.ReadLine().Split(' ');
                    var channelId = long.Parse(parts[0]);
                    var pointsCount = int.Parse(parts[1]);
                    var isEntrance = int.Parse(parts[2]);
                    var channel = new Channel(channelId, isEntrance != 0);
                    if (channel.IsEntrance)
                    {
                        entrances.Add(channel);
                    }
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
                    parent.Connecions.Add(child);
                }
            }
            var graph = new ChannelsGraph(entrances);
            return graph;
        }

        public static void WriteChannelsGraphToCg(string filename, ChannelsGraph graph)
        {
            var resultBuilder = new StringBuilder();
            var pointsBuilder = new StringBuilder();
            var connectionsBuilder = new StringBuilder();
            var channelsCount = 0;
            var connectionsCount = 0;

            graph.BFS(channel =>
            {
                channelsCount++;
                connectionsCount += channel.Connecions.Count;
                pointsBuilder.AppendLine($"{channel.Id} {channel.Points.Count} {(channel.IsEntrance ? 1 : 0)}");
                foreach (var point in channel.Points)
                {
                    pointsBuilder.AppendLine($"{point.X} {point.Y}");
                }
                foreach (var child in channel.Connecions)
                {
                    connectionsBuilder.AppendLine($"{channel.Id} {child.Id}");
                }
            });

            resultBuilder.AppendLine($"{channelsCount}");
            resultBuilder.Append(pointsBuilder);
            resultBuilder.AppendLine($"{connectionsCount}");
            resultBuilder.Append(connectionsBuilder);
            File.WriteAllText(filename, resultBuilder.ToString());
        }
    }
}
