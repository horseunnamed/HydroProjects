using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Core.Grid
{
    public class GrdInteraction
    {
        public static GridMap ReadGridMapFromGrd(string filename)
        {
            return ReadGridMapFromStream(new FileStream(filename, FileMode.Open));
        }

        public static void WriteGridMapToGrd(string filename, GridMap map)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            using (var binWriter = new BinaryWriter(fs))
            {
                binWriter.Write(new[] { 'D', 'S', 'B', 'B' });
                binWriter.Write((short) map.Width);
                binWriter.Write((short) map.Height);
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
                        binWriter.Write((float) map[x, y]);
                    }
                }
            }
        }

        public static FloodSeries ReadFloodSeriesFromZip(string filename, int startDay, int endDay)
        {
            var floodDays = new List<FloodDay>();
            using (var zipToOpen = new FileStream(filename, FileMode.Open))
            using (var archive = new ZipArchive(zipToOpen))
            {
                for (var day = startDay; day <= endDay; day++)
                {
                    var hEntry = archive.GetEntry(GetEntryNameForMap("H", day));
                    var hMap = ReadGridMapFromStream(hEntry?.Open());

                    var vxEntry = archive.GetEntry(GetEntryNameForMap("vx", day));
                    var vxMap = ReadGridMapFromStream(vxEntry?.Open());

                    var vyEntry = archive.GetEntry(GetEntryNameForMap("vy", day));
                    var vyMap = ReadGridMapFromStream(vyEntry?.Open());

                    floodDays.Add(new FloodDay(day, hMap, vxMap, vyMap));
                }
            }

            return new FloodSeries(floodDays);
        }

        private static string GetEntryNameForMap(string prefix, int day)
        {
            var dayStr = (day < 10 ? " " : "") + day;
            return $"{prefix}_   {dayStr}.grd";
        }

        private static GridMap ReadGridMapFromStream(Stream stream)
        {
            GridMap gridMap;
            using (var binReader = new BinaryReader(stream))
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
    }
}
