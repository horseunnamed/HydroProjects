using CommandLine;
using Core;
using Core.Channels;
using Core.Grid;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ChannelsDirectionResearch
{
    class Program
    {
        private static double Length(double vx, double vy)
        {
            return Math.Sqrt(vx * vx + vy * vy);
        }

        private static Bitmap DrawDirectionsBitmap(ChannelsTree channels, GridMap vxMap, GridMap vyMap)
        {
            var undecidedChannels = new List<Channel>();
            var channelCos = new Dictionary<Channel, double>();
            
            channels.VisitChannelsFromTop(channel =>
            {
                if (channel.Points.Count == 0)
                {
                    return;
                }

                double vxSum = 0;
                double vySum = 0;

                foreach (var point in channel.Points)
                {
                    vxSum += vxMap[point.X, point.Y];
                    vySum += vyMap[point.X, point.Y];
                }

                double vLen = Length(vxSum, vySum);

                if (Math.Abs(vLen) < 1e-6)
                {
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
                    double p2x = 0;
                    double p2y = 0;
                    foreach (var child in channel.Children)
                    {
                        p2x += child.Points[0].X;
                        p2y += child.Points[0].Y;
                    }

                    p2x /= channel.Children.Count;
                    p2y /= channel.Children.Count;

                    double px = p2x - p1.X;
                    double py = p2y - p1.Y;
                    double pLen = Length(px, py);

                    if (Math.Abs(pLen) < 1e-6)
                    {
                        undecidedChannels.Add(channel);
                        return;
                    }

                    px /= pLen;
                    py /= pLen;

                    var cosVal = px * vx + py * vy;
                    channelCos[channel] = cosVal;
                }
            });

            return Drawing.DrawBitmap(944, 944, graphics =>
            {
                Drawing.DrawChannels(graphics, undecidedChannels, new SolidBrush(Color.DarkOrchid));
                foreach (var entry in channelCos)
                {
                    var cos = entry.Value;
                    var color = cos < 0 ? Drawing.GetColorBetween(Color.Black, Color.Red, cos * -1) 
                        : Drawing.GetColorBetween(Color.Black, Color.LawnGreen, cos);
                    Drawing.DrawChannels(graphics, new []{ entry.Key }, new SolidBrush(color));
                }
            });
        }

        public class Options
        {
            [Option("flood-series", Required = true)]
            public string FloodSeriesFile { get; set; }

            [Option("channels-graph", Required = true)]
            public string ChannelsGraphFile { get; set; }

            [Option("start-day", Required = true)]
            public int StartDay { get; set; }

            [Option("end-day", Required = true)]
            public int EndDay { get; set; }

            [Option("output", Required = true)]
            public string OutputDir { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                var channels = CgInteraction.ReadChannelsTreeFromCg(o.ChannelsGraphFile);
                var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(o.FloodSeriesFile, o.StartDay, o.EndDay);
                Dir.RequireDirectory(o.OutputDir);
                foreach (var day in floodSeries.Days)
                {
                    var bitmap = DrawDirectionsBitmap(channels, day.VxMap, day.VyMap);
                    bitmap.Save($"{o.OutputDir}/{day.T}.png");
                }
            });
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}
