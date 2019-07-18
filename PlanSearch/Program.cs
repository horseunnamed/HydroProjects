using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CommandLine;
using Core;
using Core.Channels;
using Core.Grid;

namespace PlanSearch
{
    public class Program
    { 
        private class Options
        {
            [Option("relief", Required = true)]
            public string ReliefFile { get; set; }

            [Option("target-map", Required = true)]
            public string TargetMapFile { get; set; }

            [Option("target-value", Required = true)]
            public double TargetValue { get; set; }

            [Option("flood-series", Required = true)]
            public string FloodSeriesFile { get; set; }

            [Option("channels-graph", Required = true)]
            public string ChannelsGraphFile { get; set; }

            [Option("start-day", Required = true)]
            public int StartDay { get; set; }

            [Option("end-day", Required = true)]
            public int EndDay { get; set; }

            [Option("max-s", Required = true)]
            public int MaxS { get; set; }

            [Option("output-dir", Required = true)]
            public string OutputDir { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
        
        private static void Run(Options options)
        {
            var relief = Grd.Read(options.ReliefFile);
            var targetMap = Grd.Read(options.TargetMapFile);
            var floodSeries = FloodseriesZip.Read(options.FloodSeriesFile, options.StartDay, options.EndDay);
            var channels = CgInteraction.ReadChannelsTreeFromCg(options.ChannelsGraphFile);

            var drawingsDir = $"{options.OutputDir}/vis";
            var mapsDir = $"{options.OutputDir}/maps";

            Dir.RequireDirectory(drawingsDir);
            Dir.RequireDirectory(mapsDir);

            var floodMap = DrawFloodMapWithTargets(floodSeries, targetMap, options.TargetValue);
            floodMap.Save($"{options.OutputDir}/floodmap.png");

            var strategies = new[]
            {
                (DonorsAcceptors.RatingStrategy.TargetCount, "count"), 
                (DonorsAcceptors.RatingStrategy.TargetRatio, "ratio")
            };

            foreach (var (strategy, strategyName) in strategies)
            {
                var strategyDrawingsDir = $"{drawingsDir}/{strategyName}";
                var strategyMapsDir = $"{mapsDir}/{strategyName}";

                Dir.RequireDirectory(strategyDrawingsDir);
                Dir.RequireDirectory(strategyMapsDir);

                var donorsAcceptors = new DonorsAcceptors(strategy, channels, targetMap, 
                    options.TargetValue, floodSeries, options.MaxS);

                var projectPlan = donorsAcceptors.Run(GenerateCofinanceInfo(channels.GetAllChannels()));

                var projectCsv = GenerateProjectPlanCsv(projectPlan);
                File.WriteAllText($"{options.OutputDir}/{strategyName}.csv", projectCsv);

                foreach (var estimation in projectPlan.Estimations)
                {
                    var bitmap = DrawProjectPlanEstimation(projectPlan, estimation, 
                        channels.GetAllChannels(), targetMap, options.TargetValue);
                    bitmap.Save($"{strategyDrawingsDir}/s={estimation.S}.png");
                }

                var uniqueDonorsSets = GetUniqueSetsOfDonors(projectPlan);
                foreach (var donorsSet in uniqueDonorsSets)
                {
                    var reliefWithDams = SetDamsFor(donorsSet.Item2, relief);
                    Grd.Write($"{strategyMapsDir}/s={donorsSet.Item1}.grd", reliefWithDams);
                }
            }
        }

        private static string GenerateProjectPlanCsv(ProjectPlan plan)
        {
            var csvHeader = "s,pdc,odc,effect,target_val,price";
            var csvRows = plan.Estimations
                .Select(estimation => $"{estimation.S},{estimation.PotentialDonorsCount},{estimation.OptimalDonorsCount}" +
                                      $",{estimation.TotalEffect},{estimation.AcceptorsTargetValue},{estimation.TotalPrice}")
                .Aggregate((i, j) => i + '\n' + j);
            return csvHeader + '\n' + csvRows;
        }

        private static Bitmap DrawProjectPlanEstimation(ProjectPlan plan, ProjectPlan.Estimation estimation, 
            IEnumerable<Channel> allChannels, GridMap targetMap, double targetValue)
        {
            return Drawing.DrawBitmap(targetMap.Width, targetMap.Height, graphics =>
            {
                Drawing.DrawGridMapValues(graphics, targetMap, targetValue, new SolidBrush(Color.PowderBlue));
                foreach (var acceptor in estimation.Acceptors)
                {
                    Drawing.DrawPoints(graphics, plan.Zones[acceptor], new SolidBrush(Color.Aquamarine));
                }
                foreach (var donor in estimation.Donors)
                {
                    Drawing.DrawPoints(graphics, plan.Zones[donor], new SolidBrush(Color.HotPink));
                }
                Drawing.DrawChannels(graphics, allChannels, new SolidBrush(Color.Black));
                Drawing.DrawChannels(graphics, estimation.Acceptors, new SolidBrush(Color.LimeGreen));
                Drawing.DrawChannels(graphics, estimation.Donors, new SolidBrush(Color.Red));
            });
        }

        private static GridMap SetDamsFor(ISet<Channel> channels, GridMap relief)
        {
            var verticalAhtubaDams = new long[] { 242, 245, 246, 247 };
            var horizontalAhtubaDams = new long[] { 243, 244, 248, 249 };
            var damHeight = 12;

            var damRelief = relief.Copy();
            foreach (var channel in channels)
            {
                if (verticalAhtubaDams.Contains(channel.Id))
                {
                    var originPoint = channel.Points[0];
                    for (var i = -5; i < 8; i++)
                    {
                        damRelief[originPoint.X, originPoint.Y + i] = damHeight;
                    }
                } 
                else if (horizontalAhtubaDams.Contains(channel.Id))
                {
                    var originPoint = channel.Points[0];
                    for (var i = -5; i < 8; i++)
                    {
                        damRelief[originPoint.X + i, originPoint.Y] = damHeight;
                    }
                }
                else
                {
                    for (var i = 0; i < 15 && i < channel.Points.Count; i++)
                    {
                        var originPoint = channel.Points[i];
                        damRelief[originPoint.X, originPoint.Y] = damHeight;
                    }
                }
            }

            return damRelief;
        }

        private static Bitmap DrawFloodMapWithTargets(FloodSeries floodSeries, GridMap targetMap, double targetValue)
        {
            var floodMap = GridMap.CreateByParamsOf(targetMap);
            foreach (var floodDay in floodSeries.Days)
            {
                var hMap = floodDay.HMap;
                for (var x = 0; x < hMap.Width; x++)
                {
                    for (var y = 0; y < hMap.Height; y++)
                    {
                        if (hMap[x, y] > 0)
                        {
                            floodMap[x, y] = 1;
                        }
                    }
                }
            }

            for (var x = 0; x < targetMap.Width; x++)
            {
                for (var y = 0; y < targetMap.Height; y++)
                {
                    if (targetMap[x, y] == targetValue && floodMap[x, y] != 1)
                    {
                        floodMap[x, y] = 2;
                    }
                }
            }

            return Drawing.DrawBitmap(floodMap.Width, floodMap.Height, graphics =>
            {
                Drawing.DrawGridMapValues(graphics, floodMap, 1, new SolidBrush(Color.Aquamarine));
                Drawing.DrawGridMapValues(graphics, floodMap, 2, new SolidBrush(Color.HotPink));
            });
        }

        private static CofinanceInfo GenerateCofinanceInfo(IEnumerable<Channel> channels)
        {
            var channelsPrices = new Dictionary<Channel, double>();
            foreach (var channel in channels)
            {
                channelsPrices[channel] = 0;
            }
            return new CofinanceInfo(1, channelsPrices);
        }

        private static IEnumerable<(int, ISet<Channel>)> GetUniqueSetsOfDonors(ProjectPlan projectPlan)
        {
            return projectPlan.Estimations
                .DistinctBy(est => est.TotalEffect)
                .Select(est => (est.S, est.Donors));
        }
    }
}
