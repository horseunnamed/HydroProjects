using System;

namespace Core
{
    public static class Extensions
    {
        public delegate void MatrixVisitor<in T>(T v, int x, int y);

        public static void VisitRect<T>(this T[,] matrix, int x, int y, int r, MatrixVisitor<T> visitor)
        {
            var x0 = Math.Max(0, x - r);
            var y0 = Math.Max(0, y - r);
            var x1 = Math.Min(matrix.GetLength(0), x + r);
            var y1 = Math.Min(matrix.GetLength(1), y + r);
            for (var xi = x0; xi <= x1; xi++)
            {
                for (var yi = y0; yi <= y1; yi++)
                {
                    visitor(matrix[xi, yi], xi, yi);
                }
            }
        }

        public static void Visit<T>(this T[,] matrix, MatrixVisitor<T> visitor)
        {
            for (var x = 0; x < matrix.GetLength(0); x++)
            {
                for (var y = 0; y < matrix.GetLength(1); y++)
                {
                    visitor(matrix[x, y], x, y);
                }
            }
        }
    }
}
