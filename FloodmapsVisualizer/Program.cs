using CommandLine;
using Core;
using Core.Grid;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FloodmapsVisualizer
{
    class Program
    {
        private class Options
        {
            [Option("floodseries", Required = true)]
            public string FloodSeriesPath { get; set; }

            [Option("start_day", Required = true)]
            public int StartDay { get; set; }

            [Option("end_day", Required = true)]
            public int EndDay { get; set; }

            [Option("output-dir", Required = true)]
            public string OutputDir { get; set; }
        }

        static void DrawFloodMapSeries(FloodSeries floodSeries, string outputDir)
        {
            foreach (var floodDay in floodSeries.Days)
            {
                var hMap = floodDay.HMap;
                var bitmap = Drawing.DrawBitmap(hMap.Width, hMap.Height, graphics =>
                {
                    Drawing.DrawGridMapValues(graphics, hMap, (x, y, v) => v > 0, new SolidBrush(Color.Blue));
                });
                bitmap.Save($"{outputDir}/{floodDay.T}_day.png");
            }
        }

        static void DrawFloodMap(GridMap floodmap, string outputDir)
        {
            var bitmap = Drawing.DrawBitmap(floodmap.Width, floodmap.Height, graphics =>
            {
                Drawing.DrawGridMapValues(graphics, floodmap, (x, y, v) => v > 0, new SolidBrush(Color.Blue));
            });
            bitmap.Save($"{outputDir}/floodmap.png");
        }

        static GridMap CombineFloodMapSeries(FloodSeries floodSeries)
        {
            var result = floodSeries.Days[0].HMap.Copy();
            foreach (var floodDay in floodSeries.Days.Skip(1))
            {
                for (var x = 0; x < result.Width; x++)
                {
                    for (var y = 0; y < result.Height; y++)
                    {
                        result[x, y] = Math.Max(result[x, y], floodDay.HMap[x, y]);
                    }
                }
            }
            return result;
        }

        static void Run(Options options)
        {
            var output = $"{options.OutputDir}/{Path.GetFileNameWithoutExtension(options.FloodSeriesPath)}";
            Dir.RequireDirectory(output);
            var floodseries = FloodseriesZip.Read(options.FloodSeriesPath, options.StartDay, options.EndDay);
            var floodmap = CombineFloodMapSeries(floodseries);
            DrawFloodMapSeries(floodseries, output);
            DrawFloodMap(floodmap, output);
            Grd.Write($"{output}/floodmap.grd", floodmap);
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}
