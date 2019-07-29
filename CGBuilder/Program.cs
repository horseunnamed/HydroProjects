﻿using CommandLine;
using Core.Channels;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CGBuilder
{
    class Program
    {

        private class Options
        {
            [Option("input", Required = true)]
            public string ChannelsImagePath { get; set; }

            [Option("output", Required = true)]
            public string CGOutPath { get; set; }
        }

        private static readonly Color WHITE = Color.FromArgb(0xff, 0xff, 0xff);
        private static readonly Color ORANGE = Color.FromArgb(0xff, 0x7f, 0x27);
        private static readonly Color RED = Color.FromArgb(0xed, 0x1c, 0x24);
        private static readonly Color BLUE = Color.FromArgb(0x3f, 0x48, 0xcc);
        private static readonly Color BLACK = Color.FromArgb(0xff, 0xff, 0xff);
        private static readonly Color PINK = Color.FromArgb(0xff, 0x00, 0xdc);

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        private static void Run(Options options)
        {
            var bitmap = new Bitmap(options.ChannelsImagePath);
            var wasEnqueued = new HashSet<(int, int)>();
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y =0; y < bitmap.Height; y++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color == ORANGE && !wasEnqueued.Contains((x, y)))
                    {
                        Parse((x, y), bitmap, wasEnqueued);
                    }
                }
            }
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }

        private enum State
        {
            IN_CHAN,
            IN_TRANSITIVE,
            IN_CHAN_END
        }

        private class ParserContext
        {
            public readonly Channel channel;
            public readonly (int, int) cell;
            public readonly State state;

            public ParserContext(Channel channel, (int, int) cell, State state)
            {
                this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
                this.cell = cell;
                this.state = state;
            }
        }

        private const int IN_CHAN = 0;
        private const int IN_TRANSITIVE = 1;

        private static ChannelsGraph Parse((int, int) start, Bitmap bmp, HashSet<(int, int)> used)
        {
            var id = 0;
            var queue = new Queue<ParserContext>();
            var usedBlue = new HashSet<(int, int)>();

            Action<Channel, (int, int), State> enqueue = (channel, cell, newState) =>
            {
                queue.Enqueue(new ParserContext(channel, cell, newState));
                used.Add(cell);
            };

            enqueue(new Channel(id++), start, State.IN_CHAN);

            while (queue.Count > 0)
            {
                var ctx = queue.Dequeue();
                var ctxColor = bmp.GetPixel(ctx.cell.Item1, ctx.cell.Item2);
                var nearest = GetNearestCells(ctx.cell, used, bmp.Width, bmp.Height, ctxColor != BLUE);

                foreach (var (x, y) in nearest)
                {
                    var col = bmp.GetPixel(x, y);

                    if (col == WHITE)
                    {
                        continue;
                    }

                    switch (ctx.state)
                    {
                        case State.IN_CHAN:
                            if (col == BLACK || col == ORANGE)
                            {
                                ctx.channel.Points.Add(new ChannelPoint(x, y));
                                enqueue(ctx.channel, (x, y), State.IN_CHAN);
                            }
                            else if (col == BLUE)
                            {
                                if (!usedBlue.Contains((x, y)))
                                {
                                    UseAdjacentBlueCells(bmp, (x, y), usedBlue);
                                    ctx.channel.Points.Add(new ChannelPoint(x, y));
                                    enqueue(ctx.channel, (x, y), State.IN_CHAN_END);
                                }
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;

                        case State.IN_TRANSITIVE:
                            if (col == PINK || col == RED)
                            {
                                enqueue(ctx.channel, (x, y), State.IN_TRANSITIVE);
                            }
                            else if (col == BLUE)
                            {
                                if (!usedBlue.Contains((x, y)))
                                {
                                    UseAdjacentBlueCells(bmp, (x, y), usedBlue);
                                    var channel = new Channel(id++);
                                    ctx.channel.Connecions.Add(channel);
                                    channel.Connecions.Add(ctx.channel);
                                    channel.Points.Add(new ChannelPoint(x, y));
                                    enqueue(channel, (x, y), State.IN_CHAN_END);
                                }
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;

                        case State.IN_CHAN_END:
                            if (col == RED)
                            {
                                enqueue(ctx.channel, (x, y), State.IN_TRANSITIVE);
                            }
                            else if (col == BLUE)
                            {
                                ctx.channel.Points.Add(new ChannelPoint(x, y));
                                enqueue(ctx.channel, (x, y), State.IN_CHAN_END);
                            }
                            else if (col == BLACK)
                            {
                                ctx.channel.Points.Add(new ChannelPoint(x, y));
                                enqueue(ctx.channel, (x, y), State.IN_CHAN);
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;
                    }
                }
            }

            return null;
        }

        private static List<(int, int)> GetNearestCells(
            (int, int) cell, 
            HashSet<(int, int)> used,
            int w, 
            int h, 
            bool diag)
        {
            var candidates = new List<(int, int)>
            {
                (cell.Item1 + 1, cell.Item2),
                (cell.Item1, cell.Item2 + 1),
                (cell.Item1 - 1, cell.Item2),
                (cell.Item1, cell.Item2 - 1)
            };

            if (diag)
            {
                candidates.Add((cell.Item1 + 1, cell.Item2 + 1));
                candidates.Add((cell.Item1 - 1, cell.Item2 + 1));
                candidates.Add((cell.Item1 + 1, cell.Item2 - 1));
                candidates.Add((cell.Item1 - 1, cell.Item2 - 1));
            }

            return candidates.FindAll(
                c => !used.Contains(c) && c.Item1 < w && c.Item1 > 0 && c.Item2 < h && c.Item2 > 0);
        }

        private static void UseAdjacentBlueCells(Bitmap bitmap, (int, int) start, HashSet<(int, int)> used)
        {
            var queue = new Queue<(int, int)>();
            queue.Enqueue(start);
            used.Add(start);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var nearest = GetNearestCells(cell, used, bitmap.Width, bitmap.Height, false);
                foreach (var (x, y) in nearest)
                {
                    if (bitmap.GetPixel(x, y) == BLUE)
                    {
                        queue.Enqueue((x, y));
                        used.Add((x, y));
                    }
                }
            }
        }

    }
}