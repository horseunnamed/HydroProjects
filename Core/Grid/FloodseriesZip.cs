using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Core.Grid
{
    public class FloodseriesZip
    {
        public static FloodSeries Read(string filename, int startDay, int endDay)
        {
            var floodDays = new List<FloodDay>();
            using (var zipToOpen = new FileStream(filename, FileMode.Open))
            using (var archive = new ZipArchive(zipToOpen))
            {
                for (var day = startDay; day <= endDay; day++)
                {
                    var hEntry = archive.GetEntry(GetEntryNameForMap("H", day));
                    var hMap = Grd.Read(hEntry?.Open());

                    var vxEntry = archive.GetEntry(GetEntryNameForMap("vx", day));
                    var vxMap = Grd.Read(vxEntry?.Open());

                    var vyEntry = archive.GetEntry(GetEntryNameForMap("vy", day));
                    var vyMap = Grd.Read(vyEntry?.Open());

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

    }
}
