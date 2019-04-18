using Core.Model;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Core
{
    class Drawing
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

        public static void DrawChannels(Graphics graphics, IEnumerable<Channel> channels, Brush brush)
        {
            foreach (var channel in channels)
            {
                for (int i = 0; i < channel.Points.Count; i++)
                {
                    var p = channel.Points[i];
                    graphics.FillRectangle(brush, p.X, p.Y, 1, 1);
                }
            }
        }

        public static void DrawPoints(Graphics graphics, IEnumerable<Point> points, Brush brush)
        {
            foreach (var p in points)
            {
                graphics.FillRectangle(brush, p.X, p.Y, 1, 1);
            }
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
