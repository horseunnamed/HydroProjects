using System;
using System.IO;

namespace Core.Grid
{
    public class GrdInteraction
    {
        public static GridMap ReadGridMapFromGrd(string filename)
        {
            GridMap gridMap = null;
            using (var fs = new FileStream(filename, FileMode.Open))
            using (var binReader = new BinaryReader(fs))
            {
                var info = binReader.ReadChars(4);
                var sizeX = binReader.ReadInt16();
                var sizeY = binReader.ReadInt16();
                var minX = binReader.ReadDouble();
                var maxX = binReader.ReadDouble();
                var minY = binReader.ReadDouble();
                var maxY = binReader.ReadDouble();
                var minZ = binReader.ReadDouble();
                var maxZ = binReader.ReadDouble();

                var Z = new double[sizeX, sizeY];

                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        Z[x, y] = binReader.ReadSingle();
                    }
                }

                gridMap = new GridMap(Z, minX, maxX, minY, maxY);
            }
            return gridMap;
        }

        public static void WriteGridMapToGrd(string filename, GridMap map)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            using (var binWriter = new BinaryWriter(fs))
            {
                binWriter.Write(new char[] { 'D', 'S', 'B', 'B' });
                binWriter.Write((Int16)map.Width);
                binWriter.Write((Int16)map.Height);
                binWriter.Write(map.MinX);
                binWriter.Write(map.MaxX);
                binWriter.Write(map.MinY);
                binWriter.Write(map.MaxY);
                binWriter.Write(map.MinZ);
                binWriter.Write(map.MaxZ);

                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        binWriter.Write(map[x, y]);
                    }
                }
            }
        }
    }
}
