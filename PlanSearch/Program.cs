using System.Collections.Generic;
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

        private static void DrawProjectPlan(ProjectPlan plan, IEnumerable<Channel> allChannels, GridMap targetMap, string dir)
        {
            foreach (var estimation in plan.Estimations)
            {
                var bitmap = Drawing.DrawBitmap(944, 944, graphics =>
                {
                    Drawing.DrawGridMapValues(graphics, targetMap, DonorsAcceptors.TargetCell, new SolidBrush(Color.PowderBlue));
                    Drawing.DrawChannels(graphics, allChannels, new SolidBrush(Color.Black));
                    Drawing.DrawChannels(graphics, estimation.Acceptors, new SolidBrush(Color.LimeGreen));
                    Drawing.DrawChannels(graphics, estimation.Acceptors, new SolidBrush(Color.LimeGreen));
                    Drawing.DrawChannels(graphics, estimation.Donors, new SolidBrush(Color.Red));
                });
                bitmap.Save($"{dir}/s_{estimation.S}.png");
            }
        }

        private static void GenerateProjectPlanMaps(ProjectPlan plan, GridMap relief, string dir)
        {
            var verticalAhtubaDams = new long[] { 242, 245, 246, 247, 248 };
            var horizontalAhtubaDams = new long[] { 243, 244, 249 };
            var damHeight = 12;
            for (var estimationInd = 9; estimationInd < plan.Estimations.Count; estimationInd += 10)
            {
                var estimation = plan.Estimations[estimationInd];
                var damRelief = relief.Copy();
                foreach (var donor in estimation.Donors)
                {
                    if (verticalAhtubaDams.Contains(donor.Id))
                    {
                        var originPoint = donor.Points[0];
                        for (var i = 0; i < 5; i++)
                        {
                            damRelief[originPoint.X - 1, originPoint.Y + i - 1] = damHeight;
                        }
                    } 
                    else if (horizontalAhtubaDams.Contains(donor.Id))
                    {
                        var originPoint = donor.Points[0];
                        for (var i = 0; i < 5; i++)
                        {
                            damRelief[originPoint.X + i - 1, originPoint.Y - 1] = damHeight;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < 4 && i < donor.Points.Count; i++)
                        {
                            var originPoint = donor.Points[i];
                            damRelief[originPoint.X - 1, originPoint.Y - 1] = damHeight;
                        }
                    }
                    GrdInteraction.WriteGridMapToGrd($"{dir}/s_{estimation.S}.grd", damRelief);
                }
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

        private static void TestDonorsAcceptors(DonorsAcceptors.RatingStrategy strategy, string resultsDir)
        {
            var relief = GrdInteraction.ReadGridMapFromGrd(Dir.Data("relief.grd"));
            var ecoTargetMap =
                GrdInteraction.ReadGridMapFromGrd(Dir.Data("frequencies/add_frequency_from_0,65_to_0,85.grd"));
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels_binarized.cg"));
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 20, 39);
            var donorsAcceptors = new DonorsAcceptors(strategy, channelsTree, ecoTargetMap, floodSeries, 150);
            var cofinanceInfo = GenerateCofinanceInfo(channelsTree.GetAllChannels());
            var projectPlan = donorsAcceptors.Run(cofinanceInfo);
            WriteProjectPlanToCsv(projectPlan, Dir.Data($"{resultsDir}/donors_estimation.csv"));
            DrawProjectPlan(projectPlan, channelsTree.GetAllChannels(), ecoTargetMap, $"{resultsDir}/draw");
            GenerateProjectPlanMaps(projectPlan, relief, $"{resultsDir}/maps");
        }

        private static void Main()
        {
            // TestDonorsAcceptors(DonorsAcceptors.RatingStrategy.TargetCount, Dir.Data("test_donors/estimation_count"));
            TestDonorsAcceptors(DonorsAcceptors.RatingStrategy.TargetRatio, Dir.Data("test_donors"));
            System.Console.WriteLine("Press any key to close...");
            System.Console.ReadKey();
        }
    }
}
