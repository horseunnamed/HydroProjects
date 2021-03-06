﻿using CommandLine;
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

        static void DrawFloodSeries(FloodSeries floodSeries, string outputDir)
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

        static void DrawFloodMap(GridMap floodmap, string outputFile)
        {
            var bitmap = Drawing.DrawBitmap(floodmap.Width, floodmap.Height, graphics =>
            {
                Drawing.DrawGridMapValues(graphics, floodmap, (x, y, v) => v > 0, new SolidBrush(Color.Blue));
            });
            bitmap.Save(outputFile);
        }

        static void Run(Options options)
        {
            Dir.RequireDirectory(options.OutputDir);
            var floodVisOutput = $"{options.OutputDir}/flood_vis";
            Dir.RequireClearDirectory(floodVisOutput);
            var floodseries = FloodseriesZip.Read(options.FloodSeriesPath, options.StartDay, options.EndDay);
            var floodmap = floodseries.CombineToFloodmap();
            DrawFloodSeries(floodseries, floodVisOutput);
            DrawFloodMap(floodmap, $"{options.OutputDir}/floodmap.png");
            Grd.Write($"{options.OutputDir}/floodmap.grd", floodmap);
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
