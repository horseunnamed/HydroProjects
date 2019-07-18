using CommandLine;
using Core;
using Core.Grid;
using System.Drawing;
using System.IO;

namespace EfficiencyDiffChecker
{
    class Program
    {
        private class Options
        {
            [Option("floodmap1", Required = true)]
            public string Floodmap1Path { get; set; }

            [Option("floodmap2", Required = true)]
            public string Floodmap2Path { get; set; }

            [Option("targetmap", Required = true)]
            public string TargetmapPath { get; set; }

            [Option("targetvalue", Required = true)]
            public double TargetValue { get; set; }

            [Option("output", Required = true)]
            public string OutputDir { get; set; }
        }

        private struct Diff
        {
            public const int FLOODED1 = 0b001;
            public const int FLOODED2 = 0b010;
            public const int TARGET = 0b100;
        }

        private static GridMap GetDiffBetween(GridMap floodmap1, GridMap floodmap2, GridMap targetMap, double targetValue)
        {
            var result = floodmap1.Copy();
            for (var x = 0; x < floodmap1.Width; x++)
            {
                for (var y = 0; y < floodmap1.Height; y++)
                {
                    var target = targetMap[x, y] == targetValue ? Diff.TARGET : 0;
                    var flooded1 = floodmap1[x, y] > 0 ? Diff.FLOODED1 : 0;
                    var flooded2 = floodmap2[x, y] > 0 ? Diff.FLOODED2 : 0;

                    result[x, y] = target | flooded1 | flooded2;
                }
            }
            return result;
        }

        private static Bitmap DrawDiffMap(GridMap diffMap)
        {
            return Drawing.DrawBitmap(diffMap.Width, diffMap.Height, g =>
            {
                Drawing.DrawGridMapValues(g, diffMap, (x, y, v) => 
                {
                    var cell = (int)v;
                    if (cell == 0)
                    {
                        return Color.White;
                    }
                    else if (cell == Diff.FLOODED1)
                    {
                        return Color.LightBlue;
                    }
                    else if (cell == Diff.FLOODED2)
                    {
                        return Color.Blue;
                    }
                    else if (cell == (Diff.FLOODED1 | Diff.FLOODED2))
                    {
                        return Color.Blue;
                    }
                    else if (cell == Diff.TARGET)
                    {
                        return Color.LightPink;
                    }
                    else if (cell == (Diff.TARGET | Diff.FLOODED1))
                    {
                        return Color.Red;
                    }
                    else if (cell == (Diff.TARGET | Diff.FLOODED2))
                    {
                        return Color.Green;
                    }
                    else
                    {
                        return Color.LightGreen;
                    }
                });
            });
        }

        static void Run(Options options)
        {
            var targetmap = Grd.Read(options.TargetmapPath);
            var floodmap1 = Grd.Read(options.Floodmap1Path);
            var floodmap2 = Grd.Read(options.Floodmap2Path);

            var diff = GetDiffBetween(floodmap1, floodmap2, targetmap, options.TargetValue);
            var bitmap = DrawDiffMap(diff);
            bitmap.Save($"{options.OutputDir}/{Path.GetFileNameWithoutExtension(options.Floodmap2Path)}.png");
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}
