using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using Core;
using Core.Channels;
using Core.Grid;

namespace PlanSearch
{
    internal class Program
    {
        private static void WriteProjectPlanToCsv(ProjectPlan plan, string filename)
        {
            var csvHeader = "s,pdc,odc,effect,target_val,price";
            var csvRows = plan.Estimations
                .Select(estimation => $"{estimation.S},{estimation.PotentialDonorsCount},{estimation.OptimalDonorsCount}" +
                                      $",{estimation.TotalEffect},{estimation.AcceptorsTargetValue},{estimation.TotalPrice}")
                .Aggregate((i, j) => i + '\n' + j);
            File.WriteAllText(filename, csvHeader + '\n' + csvRows);
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static void DrawProjectPlan(ProjectPlan plan, IEnumerable<Channel> allChannels, GridMap targetMap, string dir)
        {
            foreach (var estimation in plan.Estimations)
            {
                var bitmap = Drawing.DrawBitmap(944, 944, graphics =>
                {
                    Drawing.DrawGridMapValues(graphics, targetMap, DonorsAcceptors.TargetCell, new SolidBrush(Color.PowderBlue));
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
                bitmap.Save($"{dir}/s_{estimation.S}.png");
            }
        }

        private static void SetDamsFor(ISet<Channel> channels, GridMap relief, string outputFile)
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
                        damRelief[originPoint.X - 1, originPoint.Y + i - 1] = damHeight;
                    }
                } 
                else if (horizontalAhtubaDams.Contains(channel.Id))
                {
                    var originPoint = channel.Points[0];
                    for (var i = -5; i < 8; i++)
                    {
                        damRelief[originPoint.X + i - 1, originPoint.Y - 1] = damHeight;
                    }
                }
                else
                {
                    for (var i = 0; i < 6 && i < channel.Points.Count; i++)
                    {
                        var originPoint = channel.Points[i];
                        damRelief[originPoint.X - 1, originPoint.Y - 1] = damHeight;
                    }
                }
            }

            GrdInteraction.WriteGridMapToGrd(outputFile, damRelief);
        }

        private static void DrawFloodMapWithTargets(FloodSeries floodSeries, GridMap ecoTargetMap, string dir)
        {
            var floodMap = GridMap.CreateByParamsOf(ecoTargetMap);
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

            for (var x = 0; x < ecoTargetMap.Width; x++)
            {
                for (var y = 0; y < ecoTargetMap.Height; y++)
                {
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    if (ecoTargetMap[x, y] == DonorsAcceptors.TargetCell && floodMap[x, y] != 1)
                    {
                        floodMap[x, y] = 2;
                    }
                }
            }

            var bitmap = Drawing.DrawBitmap(944, 944, graphics =>
            {
                Drawing.DrawGridMapValues(graphics, floodMap, 1, new SolidBrush(Color.Aquamarine));
                Drawing.DrawGridMapValues(graphics, floodMap, 2, new SolidBrush(Color.HotPink));
            });
            bitmap.Save($"{dir}/floodmap.png");
        }

        private static void DrawFloodMapSeries(string q)
        {
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data($"flood/{q}.zip"), 0, 13);

            var outputDir = Dir.Data($"floodmap_vis/Q={q}"); 
            Dir.RequireDirectory(outputDir);

            foreach (var floodDay in floodSeries.Days)
            {
                var hMap = floodDay.HMap;
                var bitmap = Drawing.DrawBitmap(hMap.Width, hMap.Height, graphics =>
                {
                    Drawing.DrawGridMapValues(graphics, hMap, (x, y, v) => v > 0, new SolidBrush(Color.Blue));
                });
                bitmap.Save(Dir.Data($"{outputDir}/{floodDay.T}_day.png"));
            }
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

        private static void TestDonorsAcceptors(int q, int t0, int t1)
        {
            var relief = GrdInteraction.ReadGridMapFromGrd(Dir.Data("relief.grd"));
            var ecoTargetMap =
                GrdInteraction.ReadGridMapFromGrd(Dir.Data("frequencies/add_frequency_from_0,65_to_0,85.grd"));
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels_tree.cg"));
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data($"flood/{q}.zip"), t0, t1);
            var cofinanceInfo = GenerateCofinanceInfo(channelsTree.GetAllChannels());

            var baseOutputDir = Dir.Data($"test_donors/Q={q}");
            var drawingsDir = $"{baseOutputDir}/drawings";
            var mapsDir = $"{baseOutputDir}/maps";

            Dir.RequireDirectory(Dir.Data(baseOutputDir));
            Dir.RequireDirectory(Dir.Data(drawingsDir));
            Dir.RequireDirectory(Dir.Data(mapsDir));

            DrawFloodMapWithTargets(floodSeries, ecoTargetMap, baseOutputDir);

            var strategies = new[]
            {
                (DonorsAcceptors.RatingStrategy.TargetCount, "count"), 
                (DonorsAcceptors.RatingStrategy.TargetRatio, "ratio")
            };

            foreach (var (strategy, strategyName) in strategies)
            {
                var strategyDrawingsDir = $"{drawingsDir}/{strategyName}";
                var strategyMapsDir = $"{mapsDir}/{strategyName}";

                Dir.RequireDirectory(Dir.Data(strategyDrawingsDir));
                Dir.RequireDirectory(Dir.Data(strategyMapsDir));

                var donorsAcceptors = new DonorsAcceptors(strategy, channelsTree, ecoTargetMap, floodSeries, 150);
                var projectPlan = donorsAcceptors.Run(cofinanceInfo);

                WriteProjectPlanToCsv(projectPlan, Dir.Data($"{baseOutputDir}/{strategyName}.csv"));

                DrawProjectPlan(projectPlan, channelsTree.GetAllChannels(), ecoTargetMap, strategyDrawingsDir);
                var uniqueDonorsSets = GetUniqueSetsOfDonors(projectPlan);
                foreach (var donorsSet in uniqueDonorsSets)
                {
                    SetDamsFor(donorsSet.Item2, relief, $"{strategyMapsDir}/s={donorsSet.Item1}.grd");
                }
            }

        }

        private static void Main()
        {
            // TestDonorsAcceptors(23, 10, 23);
            // DrawFloodMapSeries(21);
            DrawFloodMapSeries("21_test");
            System.Console.WriteLine("Press any key to close...");
            System.Console.ReadKey();
        }
    }
}
