using System.Collections.Generic;
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

            // reading of channels tree from file:
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));

            // calculating stats:
            var floodMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("only_territory_flood_frequency0.grd"));
            var typeUseMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("TypeUseWithNonCadastr.grd"));
            var hozValues = new HashSet<double> { 37, 38, 39, 43, 44, 45 };
            var socValues = new HashSet<double> { 58, 61, 63, 66, 171, 186, 195, 203, 207 };
            var nonCadastrId = 8888d;
            var r = 10;
            var stats = StatsAggregation.GetStats(channelsTree, floodMap, typeUseMap, hozValues, socValues, nonCadastrId, r);


            /* Usage of donors-acceptors algorithm */
            
            // read flood series for Q=23 and t=[20, 39]:
            var floodSeries = GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 20, 39);

            // read eco targets map
            var ecoTargetMap = GrdInteraction.ReadGridMapFromGrd(Dir.Data("frequencies/add_frequency_from_0,65_to_0,85.grd"));

            // initialization of algo:
            var donorsAcceptors = new DonorsAcceptors(
                DonorsAcceptors.RatingStrategy.TargetCount, channelsTree, ecoTargetMap, floodSeries);

            // on each iteration call method Run with CofinanceInfo:
            var cofinanceInfo = new CofinanceInfo(0, null);
            var projectPlan = donorsAcceptors.Run(cofinanceInfo);
        }
    }
}
