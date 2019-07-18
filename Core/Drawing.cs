using System;
using System.Collections.Generic;
using System.Drawing;
using Core.Channels;
using Core.Grid;

namespace Core
{
    public class Drawing
    {
        private static Color RGB(int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }

        public static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
        {
            int stepA = (end.A - start.A) / (steps - 1);
            int stepR = (end.R - start.R) / (steps - 1);
            int stepG = (end.G - start.G) / (steps - 1);
            int stepB = (end.B - start.B) / (steps - 1);


            for (int i = 0; i < steps; i++)
            {
                yield return Color.FromArgb(start.A + (stepA * i),
                                            start.R + (stepR * i),
                                            start.G + (stepG * i),
                                            start.B + (stepB * i));
            }
        }

        public static Color GetColorBetween(Color start, Color end, double at)
        {
            int a = start.A + (byte) ((end.A - start.A) * at);
            int r = start.R + (byte) ((end.R - start.R) * at);
            int g = start.G + (byte) ((end.G - start.G) * at);
            int b = start.B + (byte) ((end.B - start.B) * at);

            return Color.FromArgb(a, r, g, b);
        }

        public static void DrawChannels(Graphics graphics, IEnumerable<Channel> channels, Brush brush, bool withOrigins = false)
        {
            foreach (var channel in channels)
            {
                foreach (var p in channel.Points)
                {
                    graphics.FillRectangle(brush, p.X, p.Y, 1, 1);
                }
                if (withOrigins)
                {
                    if (channel.Points.Count > 0)
                    {
                        var origin = channel.Points[0];
                        // graphics.DrawEllipse(new Pen(Color.Blue), origin.X - 2, origin.Y - 2, 4, 4);
                        graphics.FillRectangle(new SolidBrush(Color.Red), origin.X, origin.Y, 1, 1);
                    }
                }
            }
        }

        public static void DrawPoints(Graphics graphics, IEnumerable<(int, int)> points, Brush brush)
        {
            foreach (var (x, y) in points)
            {
                graphics.FillRectangle(brush, x, y, 1, 1);
            }
        }

        public static void DrawGridMapValues(Graphics graphics, GridMap gridMap, double value, Brush brush)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            DrawGridMapValues(graphics, gridMap, (x, y, v) => v == value, brush);
        }


        public delegate bool GridMapCellsSelector(int x, int y, double value);

        public static void DrawGridMapValues(Graphics graphics, GridMap gridMap, GridMapCellsSelector selector, Brush brush)
        {
            gridMap.Values.Visit((v, x, y) =>
            {
                if (selector(x, y, v))
                {
                    graphics.FillRectangle(brush, x, y, 1, 1);
                }
            });
        }

        public delegate Color CellColorSupplier(int x, int y, double value);

        public static void DrawGridMapValues(Graphics graphics, GridMap gridMap, CellColorSupplier colorSupplier)
        {
            gridMap.Values.Visit((v, x, y) =>
            {
                graphics.FillRectangle(new SolidBrush(colorSupplier(x, y, v)), x, y, 1, 1);
            });
        }

        public static void DrawChannelsOrigins(Graphics graphics, IEnumerable<Channel> channels)
        {
            foreach (var channel in channels)
            {
                if (channel.Points.Count > 0)
                {
                    var firstPoint = channel.Points[0];
                    graphics.DrawEllipse(new Pen(Color.Blue), firstPoint.X - 2, firstPoint.Y - 2, 4, 4);
                }
            }
        }

        public static Bitmap DrawBitmap(int width, int height, Action<Graphics> draw)
        {
            var bitmap = new Bitmap(width, height);
            var backgroundBrush = new SolidBrush(Color.White);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(backgroundBrush, new Rectangle(0, 0, width, height));
                draw(g);
                g.Dispose();
            }

            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

    }
}
