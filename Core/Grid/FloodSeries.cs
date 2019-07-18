using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Grid
{
    public class FloodSeries
    {
        public IList<FloodDay> Days{ get; }

        public FloodSeries(IList<FloodDay> days)
        {
            Days = days ?? throw new ArgumentNullException(nameof(days));
        }

        public GridMap CombineToFloodmap()
        {
            var result = Days[0].HMap.Copy();
            foreach (var floodDay in Days.Skip(1))
            {
                for (var x = 0; x < result.Width; x++)
                {
                    for (var y = 0; y < result.Height; y++)
                    {
                        result[x, y] = Math.Max(result[x, y], floodDay.HMap[x, y]);
                    }
                }
            }
            return result;
        }
    }

    public class FloodDay
    {
        public int T { get; }
        public GridMap HMap { get; }
        public GridMap VxMap { get; }
        public GridMap VyMap { get; }

        public FloodDay(int t, GridMap hMap, GridMap vxMap, GridMap vyMap)
        {
            T = t;
            HMap = hMap;
            VxMap = vxMap;
            VyMap = vyMap;
        }
    }
}
