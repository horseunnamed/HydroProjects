using CommandLine;
using Core;
using Core.Channels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ComponentsVisualizer
{
    class Program
    {

        private class Options
        {
            [Option("input", Required = true)]
            public string CgPath { get; set; }

            [Option("output", Required = true)]
            public string ImageOutputPath { get; set; }

        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void Run(Options options)
        {
            var graph = CgInteraction.ReadChannelsTreeFromCg(options.CgPath);

            var contrastColors = new[]
            {
                Color.DeepPink,
                Color.LightGreen,
                Color.Black,
                Color.Violet,
                Color.Blue,
                Color.Cyan,
                Color.Magenta,
                Color.Aqua,
                Color.Azure,
                Color.Coral,
                Color.DarkSalmon,
                Color.Firebrick,
                Color.Green,
                Color.Red,
                Color.LightGreen,
                Color.Black,
                Color.Violet,
                Color.DeepSkyBlue,
                Color.DodgerBlue
            };

            Console.WriteLine($"Components count: {graph.Root.Connecions.Count}");

            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                foreach ((var color, var baseChannel) in graph.Root.Connecions.Zip(
                    contrastColors, (color, channel) => (channel, color)))
                {
                    var channels = new List<Channel>();
                    graph.VisitChannelsDepthFromTop(baseChannel, (channel, depth) =>
                    {
                        channels.Add(channel);
                    });
                    Drawing.DrawChannels(g, channels, new SolidBrush(color));
                }
            });

            bitmap.Save(options.ImageOutputPath);
        }
    }
}
