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
            var meanings = new int[]
            {
                0,
                Diff.FLOODED1,
                Diff.FLOODED2,
                Diff.FLOODED1 | Diff.FLOODED2,
                Diff.TARGET,
                Diff.TARGET | Diff.FLOODED1,
                Diff.TARGET | Diff.FLOODED2,
                Diff.TARGET | Diff.FLOODED1 | Diff.FLOODED2
            };

            var colors = new Color[]
            {
                Color.White,
                Color.LightBlue,
                Color.Blue,
                Color.Blue,
                Color.LightPink,
                Color.Red,
                Color.Green,
                Color.LightGreen
            };

            return Drawing.DrawBitmap(diffMap.Width, diffMap.Height, g =>
            {
                Drawing.DrawGridMapValues(g, diffMap, (x, y, v) =>
                {
                    var cell = (int)v;
                    for (var i = 0; i < meanings.Length; i++)
                    {
                        if (meanings[i] == cell)
                        {
                            return colors[i];
                        }
                    }
                    return Color.White;
                });

                var ly = 50;
                var lx = diffMap.Width - 200;
                for (var i = 0; i < meanings.Length; i++)
                {
                    var meaning = meanings[i];
                    var color = colors[i];
                    var str = "";
                    str += ((meaning & Diff.TARGET) == 0) ? "-T; " : "+T; ";
                    str += ((meaning & Diff.FLOODED1) == 0) ? "-F1; " : "+F1; ";
                    str += ((meaning & Diff.FLOODED2) == 0) ? "-F2; " : "+F2; ";
                    if (color != Color.White)
                    {
                        g.DrawString(str, new Font("Arial", 16, FontStyle.Bold), new SolidBrush(color), lx, ly);
                        ly += 30;
                    }
                }
            });
        }

        private static string PrepareReport(GridMap diffMap)
        {
            var result = "";
            var total = 0;
            var totalTarget = 0;
            var baseFlooded = 0;
            var baseFloodedTarget = 0;
            var newFlooded = 0;
            var newFloodedTarget = 0;

            for (var x = 0; x < diffMap.Width; x++)
            {
                for (var y = 0; y < diffMap.Height; y++)
                {
                    var cell = (int)diffMap[x, y];
                    total++;
                    if ((cell & Diff.FLOODED1) != 0)
                    {
                        baseFlooded++;
                    }
                    if ((cell & Diff.FLOODED2) != 0)
                    {
                        newFlooded++;
                    }
                    if ((cell & Diff.TARGET) != 0)
                    {
                        totalTarget++;
                        if ((cell & Diff.FLOODED1) != 0)
                        {
                            baseFloodedTarget++;
                        }
                        if ((cell & Diff.FLOODED2) != 0)
                        {
                            newFloodedTarget++;
                        }
                    }
                }
            }

            var totalEffect = (newFlooded - baseFlooded) / (float)baseFlooded * 100;
            var totalEffectTarget = (newFloodedTarget - baseFloodedTarget) / (float)baseFloodedTarget * 100;

            result += $"Total: {totalTarget} ({total})\n";
            result += $"Base flooded: {baseFloodedTarget} ({baseFlooded})\n";
            result += $"New flooded: {newFloodedTarget} ({newFlooded})\n";
            result += $"Relative effect: {totalEffectTarget:0.#}% ({totalEffect:0.#}%)";
            return result;
        }

        static void Run(Options options)
        {
            var targetmap = Grd.Read(options.TargetmapPath);
            var floodmap1 = Grd.Read(options.Floodmap1Path);
            var floodmap2 = Grd.Read(options.Floodmap2Path);

            var diff = GetDiffBetween(floodmap1, floodmap2, targetmap, options.TargetValue);
            var bitmap = DrawDiffMap(diff);
            var report = PrepareReport(diff);
            bitmap.Save($"{options.OutputDir}/diff.png");
            File.WriteAllText($"{options.OutputDir}/report.txt", report);
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}
