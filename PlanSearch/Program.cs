using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Core;
using Core.Channels;
using Core.Grid;

namespace PlanSearch
{
    class Program
    {
        private static void WriteProjectPlanToCsv(ProjectPlan plan, string filename)
        {
            var csvText = plan.Estimations
                .Select(estimation => $"{estimation.S},{estimation.TotalV}")
                .Aggregate((i, j) => i + '\n' + j);
            File.WriteAllText(filename, csvText);
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

        private static void TestDonorsAcceptors(DonorsAcceptors.RatingStrategy strategy, string resultsDir)
        {
            var ecoTargetMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("frequencies/add_frequency_from_0,65_to_0,85.grd"));
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 20, 39);
            var donorsAcceptors = new DonorsAcceptors(strategy, channelsTree, ecoTargetMap, floodSeries, 30);
            var cofinanceInfo = new CofinanceInfo(0, new Dictionary<long, double>());
            var projectPlan = donorsAcceptors.Run(cofinanceInfo);
            WriteProjectPlanToCsv(projectPlan, Dir.Data($"{resultsDir}/donors_estimation.csv"));
            DrawProjectPlan(projectPlan, channelsTree.GetAllChannels(), ecoTargetMap, resultsDir);
        }

        static void Main(string[] args)
        {
            TestDonorsAcceptors(DonorsAcceptors.RatingStrategy.TargetCount, Dir.Data("test_donors/estimation_count"));
            TestDonorsAcceptors(DonorsAcceptors.RatingStrategy.TargetRatio, Dir.Data("test_donors/estimation_ratio"));
        }
    }
}
