using System;

namespace Core
{
    public static class Extensions
    {
        public delegate void MatrixVisitor<T>(T v, int x, int y);

        public static void VisitRect<T>(this T[,] arr, int x, int y, int r, MatrixVisitor<T> visitor)
        {
            var x0 = Math.Max(0, x - r);
            var y0 = Math.Max(0, y - r);
            var x1 = Math.Min(arr.GetLength(0), x + r);
            var y1 = Math.Min(arr.GetLength(1), y + r);
            for (var xi = x0; xi <= x1; xi++)
            {
                for (var yi = y0; yi <= y1; yi++)
                {
                    visitor(arr[xi, yi], xi, yi);
                }
            }
        }
    }
}
