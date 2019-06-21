using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Core;
using Core.Channels;
using Core.Grid;

namespace FlowNetwork
{
    class Program
    {

        private static double Length(double vx, double vy)
        {
            return Math.Sqrt(vx * vx + vy * vy);
        }

        private static void TestChannelsTreeDirections()
        {
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels_tree.cg"));
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 25, 25);
            var undecidedChannels = new List<Channel>();
            var channelCos = new Dictionary<Channel, double>();
            
            channelsTree.VisitChannelsFromTop(channel =>
            {
                if (channel.Points.Count == 0)
                {
                    return;
                }

                double vxSum = 0;
                double vySum = 0;
                var day = floodSeries.Days.First(x => x.T == 25);
                foreach (var point in channel.Points)
                {
                    vxSum += day.VxMap[point.X - 1, point.Y - 1];
                    vySum += day.VyMap[point.X - 1, point.Y - 1];
                }

                double vLen = Length(vxSum, vySum);

                if (Math.Abs(vLen) < 1e-6)
                {
                    Console.WriteLine($"Unknown for channel {channel.Id}");
                    undecidedChannels.Add(channel);
                    return;
                }

                double vx = vxSum / vLen;
                double vy = vySum / vLen;

                var p1 = channel.Points[0];

                if (channel.Children.Count == 0)
                {
                    undecidedChannels.Add(channel);
                }
                else
                {
                    var child = channel.Children[0];
                    var p2 = child.Points[0];
                    double px = p2.X - p1.X;
                    double py = p2.Y - p1.Y;
                    double pLen = Length(px, py);

                    if (Math.Abs(pLen) < 1e-6)
                    {
                        Console.WriteLine($"Unknown for connection {channel.Id}-{child.Id}");
                        undecidedChannels.Add(channel);
                        return;
                    }

                    px /= pLen;
                    py /= pLen;

                    var cosVal = px * vx + py * vy;
                    channelCos[channel] = cosVal;

                    Console.WriteLine($"Cos for {channel.Id}-{child.Id} = {cosVal}");
                }
            });

            Drawing.DrawBitmap(944, 944, graphics =>
            {
                Drawing.DrawChannels(graphics, undecidedChannels, new SolidBrush(Color.DarkOrchid));
                foreach (var entry in channelCos)
                {
                    var cos = entry.Value;
                    var color = cos < 0 ? Drawing.GetColorBetween(Color.Black, Color.Red, cos * -1) 
                        : Drawing.GetColorBetween(Color.Black, Color.LawnGreen, cos);
                    Drawing.DrawChannels(graphics, new []{ entry.Key }, new SolidBrush(color));
                }
            }).Save(Dir.Data("test_channels_tree.png"));
        }

        static void Main(string[] args)
        {
            TestChannelsTreeDirections();
            System.Console.WriteLine("Press any key to close...");
            System.Console.ReadKey();
        }
    }
}
