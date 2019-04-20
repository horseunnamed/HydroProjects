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
        public GridMap HMap { get; }
        public GridMap VxMap { get; }
        public GridMap VyMap { get; }

        public FloodDay(GridMap hMap, GridMap vxMap, GridMap vyMap)
        {
            HMap = hMap ?? throw new ArgumentNullException(nameof(hMap));
            VxMap = vxMap ?? throw new ArgumentNullException(nameof(vxMap));
            VyMap = vyMap ?? throw new ArgumentNullException(nameof(vyMap));
        }
    }
}
