using System.Collections.Generic;
using System.IO;
using CofinanceSearch.Stats;
using Core;
using Core.Channels;
using Core.Grid;
using PlanSearch;

namespace CofinanceSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            // reading of grd maps from file:
            var gridMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("something.grd"));

            // reading of channels tree from file:
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));

            // calculating stats:
            var hozValues = new HashSet<double> { 37, 38, 39, 43, 44, 45 };
            var socValues = new HashSet<double> { 58, 61, 63, 66, 171, 186, 195, 203, 207 };
            var stats = StatsAggregation.GetStats(channelsTree, gridMap, gridMap, hozValues, socValues);

            // usage of donors-acceptors algo
            
            // initialization of algo:
            var donorsAcceptors = new DonorsAcceptors(DonorsAcceptors.RatingStrategy.Length, 
                channelsTree, gridMap, gridMap);

            // on each iteration call method Run with CofinanceInfo:
            var cofinanceInfo = new CofinanceInfo(0, null);
            var projectPlan = donorsAcceptors.Run(cofinanceInfo);
        }
    }
}
