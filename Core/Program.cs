﻿using System;
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
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));
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
            GrdInteraction.ReadFloodSeriesFromZip(Dir.Data("flood/23.zip"), days);
            sw.Stop();
            Console.WriteLine($"Read {days} for {sw.Elapsed.Seconds} seconds");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            TestFloodSeries();
        }
    }
}
