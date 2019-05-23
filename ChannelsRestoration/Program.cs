using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Core;
using Core.Channels;

namespace ChannelsRestoration
{
    class Program
    {

        private static int Dist(ChannelPoint p1, ChannelPoint p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        private static ChannelPoint FindClosestPoint(Channel parent, Channel child)
        {
            var childOrigin = child.Points[0];
            ChannelPoint closestPoint = null;
            var bestDist = double.MaxValue;
            foreach (var point in parent.Points)
            {
                var dist = Dist(point, childOrigin);
                if (dist < bestDist)
                {
                    closestPoint = point;
                    bestDist = dist;
                }
            }
            return closestPoint;
        }

        private static void CheckChannels(ChannelsTree tree)
        {
            tree.VisitChannelsFromTop(channel =>
            {
                for (var i = 1; i < channel.Points.Count; i++)
                {
                    var p0 = channel.Points[i - 1];
                    var p1 = channel.Points[i];
                    var dist = Dist(p0, p1);
                    if (Dist(p0, p1) > 2)
                    {
                        Console.WriteLine($"Points {p0} and {p1} are too far in {channel.Id} with dist {dist}");
                    }
                }

                if (channel.Points.Count == 0)
                {
                    Console.WriteLine($"{channel.Id} is empty");
                }

                foreach (var child in channel.Children)
                {
                    if (child != null)
                    {
                        var p = FindClosestPoint(channel, child);
                        if (p != null)
                        {
                            var dist = Dist(p, child.Points[0]);
                            if (dist > 1)
                            {
                                Console.WriteLine($"Child {child.Id} is too far from it's parent {channel.Id} with dist {dist}");
                            }

                            if (dist == 0)
                            {
                                Console.WriteLine($"Child {child.Id} has shared point {p} with it's parent {channel.Id}");
                            }
                        }
                    }
                }
            });
        }

        private static List<ChannelPoint> Bresenham(ChannelPoint p0, ChannelPoint p1)
        {
            var result = new List<ChannelPoint>();
            var dx = Math.Abs(p1.X - p0.X);
            var dy = Math.Abs(p1.Y - p0.Y);
            var dirx = Math.Sign(p1.X - p0.X);
            var diry = Math.Sign(p1.Y - p0.Y);
            result.Add(new ChannelPoint(p0.X, p0.Y));

            var err = 0;
            var x = p0.X;
            var y = p0.Y;

            if (dy < dx)
            {
                while (x != p1.X)
                {
                    err += dy;
                    if (2 * err >= dx)
                    {
                        y += diry;
                        err -= dx;
                        result.Add(new ChannelPoint(x, y));
                    }
                    x += dirx;
                    result.Add(new ChannelPoint(x, y));
                }
            }
            else
            {
                while (y != p1.Y)
                {
                    err += dx;
                    if (2 * err >= dy)
                    {
                        x += dirx;
                        err -= dy;
                        result.Add(new ChannelPoint(x, y));
                    }
                    y += diry;
                    result.Add(new ChannelPoint(x, y));
                }
            }

            return result;
        }

        private static void RestoreHoles(ChannelsTree channelsTree)
        {
            channelsTree.VisitChannelsFromTop(channel =>
            {
                var set = new HashSet<ChannelPoint>();

                for (var i = 1; i < channel.Points.Count; i++)
                {
                    var p0 = channel.Points[i - 1];
                    var p1 = channel.Points[i];
                    var points = Bresenham(p0, p1);
                    set.UnionWith(points);
                }
                if (channel.Points.Count == 1)
                {
                    var p = channel.Points[0];
                    set.Add(new ChannelPoint(p.X, p.Y));
                }

                var result = new List<ChannelPoint>();

                while (result.Count != set.Count)
                {
                    var count = result.Count;
                    foreach (var point in set)
                    {
                        if (result.Contains(point))
                        {
                            continue;
                        }
                        if (result.Count == 0)
                        {
                            result.Add(point);
                            continue;
                        }

                        var x = point.X;
                        var y = point.Y;

                        var xs = result[0].X;
                        var ys = result[0].Y;

                        var xe = result[result.Count - 1].X;
                        var ye = result[result.Count - 1].Y;

                        if (Math.Abs(xs - x) + Math.Abs(ys - y) <= 5)
                        {
                            result.Insert(0, point);
                        }
                        else if (Math.Abs(xe - x) + Math.Abs(ye - y) <= 5)
                        {
                            result.Add(point);
                        }
                    }

                    if (result.Count == count)
                    {
                        break;
                    }
                }
                result.Reverse();
                channel.Points = result;
            });
        }

        private static List<ChannelPoint> GetSharedPoints(Channel c1, Channel c2)
        {
            var result = new List<ChannelPoint>();
            foreach (var p1 in c1.Points)
            {
                foreach (var p2 in c2.Points)
                {
                    if (Equals(p1, p2))
                    {
                        result.Add(p1);
                    }
                }
            }

            return result;
        }

        private static void RestoreChildren(ChannelsTree channelsTree)
        {
            channelsTree.VisitChannelsFromTop(channel => {
                foreach (var child in channel.Children)
                {
                    var origin = child.Points[0];
                    var closestPoint = FindClosestPoint(channel, child);
                    if (closestPoint != null)
                    {
                        var dist = Dist(origin, closestPoint);

                        if (dist >= 1)
                        {
                            // child.Points.InsertRange(0, Bresenham(closestPoint, origin));
                        }

                        var sharedPoints = GetSharedPoints(channel, child);

                        child.Points = child.Points.Except(sharedPoints).ToList();
                    }
                }
            });
        }

        private static void BinarizeChannels(ChannelsTree channelsTree)
        {
            var newId = 0L; 

            // start numeration of new channels without intersections with old channels
            channelsTree.VisitChannelsFromTop(channel => { newId = Math.Max(channel.Id, newId); });

            channelsTree.VisitChannelsFromTop(channel =>
            {
                var childrenDict = new Dictionary<ChannelPoint, List<Channel>>();
                foreach (var child in channel.Children)
                {
                    var closestPoint = FindClosestPoint(channel, child);
                    if (closestPoint != null)
                    {
                        if (!childrenDict.ContainsKey(closestPoint))
                        {
                            childrenDict[closestPoint] = new List<Channel>();
                        }
                        childrenDict[closestPoint].Add(child);
                    }
                }

                var curChannel = channel;
                var curPoints = new List<ChannelPoint>();
                var points = new List<ChannelPoint>(channel.Points);

                for (var i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    curPoints.Add(point);
                    if (childrenDict.ContainsKey(point))
                    {
                        var channelChildren = childrenDict[point]; 
                        curChannel.Points = curPoints;
                        curChannel.Children = channelChildren;
                        var newChannel = new Channel(newId++)
                        {
                            Points = new List<ChannelPoint>(curPoints),
                        };
                        if (i != points.Count - 1)
                        {
                            curChannel.Children.Add(newChannel);
                        }
                        curChannel = newChannel;
                        curPoints = new List<ChannelPoint>();
                    }
                }

                if (curPoints.Count > 0)
                {
                    curChannel.Points = curPoints;
                }

            });
        }

        static void Main(string[] args)
        {
            var channels = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels_all.cg"));
            RestoreHoles(channels);
            RestoreChildren(channels);
            RestoreHoles(channels);
            CheckChannels(channels);
            BinarizeChannels(channels);
            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, channels.GetAllChannels(), new SolidBrush(Color.Black), true);
            });
            bitmap.Save(Dir.Data("restoration_debug.png"));
            CgInteraction.WriteChannelsTreeToCg(Dir.Data("channels_binarized.cg"), channels);
            Console.ReadKey();
        }
    }
}
