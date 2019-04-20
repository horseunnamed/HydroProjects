using System.Linq;

namespace Core.Grid
{

    public class GridMap
    {
        public const double EmptyValue = 1.70141E+038f;

        public double MinX { get; }
        public double MaxX { get; }
        public double MinY { get; }
        public double MaxY { get; }

        public double[,] Values { get; }

        public double MinZ
        {
            get => Values.Cast<double>().Min();
        }

        public double MaxZ
        {
            get => Values.Cast<double>().Max();
        }

        public double this[int x, int y]
        {
            get => Values[x, y];
            set => Values[x, y] = value;
        }

        public int Width
        {
            get => Values.GetLength(0);
        }

        public int Height
        {
            get => Values.GetLength(1);
        }

        public double StepX
        {
            get => (MaxX - MinX) / Width;
        }

        public double StepY
        {
            get => (MaxY - MinY) / Height;
        }

        public GridMap(double[,] values, double minX, double maxX, double minY, double maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            Values = values;
        }

        public GridMap(int width, int height, double minX, double maxX, double minY, double maxY, double initialValue = 0)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            Values = new double[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    Values[x, y] = initialValue;
                }
            }
        }

        public GridMap SubMap(int x0, int y0, int w, int h)
        {
            var subValues = new double[w, h];
            for (var x = 0; x < w && x + x0 < Width; x++)
            {
                for (var y = 0; y < h && y + y0 < Height; y++)
                {
                    subValues[x, y] = Values[x + x0, y + y0];
                }
            }

            var dX = (MaxX - MinX) / (Width - 1);
            var dY = (MaxY - MinY) / (Height - 1);

            return new GridMap(subValues, 
                MinX + x0 * dX, MinX + (w + x0 - 1) * dX, 
                MinY + y0 * dY, MinY + (h + y0 - 1) * dY);
        }

    }
}
