using System;
using System.Collections.Generic;

namespace Core.Grid
{
    public class FloodSeries
    {
        public IList<FloodDay> Days{ get; }

        public FloodSeries(IList<FloodDay> days)
        {
            Days = days ?? throw new ArgumentNullException(nameof(days));
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
