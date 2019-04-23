using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private static void TestDonorsAcceptors()
        {
            var ecoTargetMap = GrdInteraction.ReadGridMapFromGrd(
                Dir.Data("frequencies/add_frequency_from_0,65_to_0,85.grd"));
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 20);
            var donorsAcceptors = new DonorsAcceptors(
                DonorsAcceptors.RatingStrategy.TargetCount, channelsTree, ecoTargetMap, floodSeries);
            var cofinanceInfo = new CofinanceInfo(0, new Dictionary<long, double>());
            var projectPlan = donorsAcceptors.Run(cofinanceInfo);
            WriteProjectPlanToCsv(projectPlan, Dir.Data("donors_estimation.csv"));
        }

        static void Main(string[] args)
        {
            TestDonorsAcceptors();
        }
    }
}
