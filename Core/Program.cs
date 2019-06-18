using System;
using System.Diagnostics;
using System.Drawing;
using Core.Channels;
using Core.Grid;

namespace Core
{
    class Program
    {
        private static void TestChannels()
        {
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels_all.cg"));
            CgInteraction.WriteChannelsTreeToCg(Dir.Data("channels_copy.cg"), channelsTree);
            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, channelsTree.GetAllChannels(), new SolidBrush(Color.Black));
            });
            bitmap.Save(Dir.Data("image1.png"));
        }

        private static void TestFloodSeries()
        {
            var days = 20;
            var sw = new Stopwatch();
            sw.Start();
            GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), 0, days - 1);
            sw.Stop();
            Console.WriteLine($"Read {days} for {sw.Elapsed.Seconds} seconds");
            Console.ReadKey();
        }

        private static void TestGrdReadWrite()
        {
            var relief = GrdInteraction.ReadGridMapFromGrd(Dir.Data("relief.grd"));
            GrdInteraction.WriteGridMapToGrd(Dir.Data("relief_copy.grd"), relief);
        }

        static void Main(string[] args)
        {
            TestGrdReadWrite();
            // TestFloodSeries();
            // TestChannels();
        }
    }
}
