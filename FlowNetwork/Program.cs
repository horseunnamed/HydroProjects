using CommandLine;
using Core.Channels;
using Core.Grid;
using System;
using System.Linq;

namespace FlowNetwork
{
    class Program
    {
        private class Options
        {
            [Option("graph", Required = true)]
            public string GraphPath { get; set; }

            [Option("flood", Required = true)]
            public string FloodPath { get; set; }

            [Option("output-graph", Required = true)]
            public string GraphOutPath { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            System.Console.WriteLine("Press any key to close...");
            System.Console.ReadKey();
        }

        class Vec
        {
            public double X{ get; set; }
            public double Y{ get; set; }
            public Vec(double X, double Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        private static void Run(Options options)
        {
            var graph = CgInteraction.ReadChannelsGraphFromCg(options.GraphPath);
            var flood = FloodseriesZip.Read(options.FloodPath, 20, 20);

            var vxMap = flood.Days[0].VxMap;
            var vyMap = flood.Days[0].VyMap;
            
            graph.BFS(channel =>
            {
                var direction = GetChannelDirection(channel, flood.Days[0]);
                channel.Connecions = channel.Connecions.Where(child => {
                    var directionBetween = GetDirectionBetweenChannels(channel, child);
                    return IsSodirected(direction, directionBetween); 
                }).ToList();
            });

            CgInteraction.WriteChannelsGraphToCg(options.GraphOutPath, graph);
        }

        private static double Length(Vec v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        private static Vec Normalize(Vec v)
        {
            double vLen = Length(v);

            if (Math.Abs(vLen) < 1e-6)
            {
                return new Vec(0, 0);
            }

            return new Vec(v.X / vLen, v.Y / vLen);
        }

        private static Vec GetChannelDirection(Channel channel, FloodDay day)
        {
            var result = new Vec(0, 0);

            foreach (var point in channel.Points)
            {
                result.X += day.VxMap[point.X, point.Y];
                result.Y += day.VyMap[point.X, point.Y];
            }

            return Normalize(result);
        }

        private static Vec GetDirectionBetweenChannels(Channel channel1, Channel channel2)
        {
            var p1 = channel1.Points[0];
            var p2 = channel2.Points[0];

            var v = new Vec(p2.X - p1.X, p2.Y - p1.Y);

            return Normalize(v);
        }

        private static bool IsSodirected(Vec v1, Vec v2)
        {
            var cosVal = v1.X * v2.X + v1.Y * v2.Y;
            return cosVal > 0.5;
        }

    }
}
