using System;
using System.Collections.Generic;

namespace Core.Grid
{
    public class FloodSeries
    {
        public int DaysCount { get; }
        public IList<GridMap> HByDays { get; }
        public IList<GridMap> VyByDays { get; }
        public IList<GridMap> VxByDays { get; }

        public FloodSeries(int daysCount, IList<GridMap> hByDays, IList<GridMap> vyByDays, IList<GridMap> vxByDays)
        {
            DaysCount = daysCount;
            HByDays = hByDays ?? throw new ArgumentNullException(nameof(hByDays));
            VyByDays = vyByDays ?? throw new ArgumentNullException(nameof(vyByDays));
            VxByDays = vxByDays ?? throw new ArgumentNullException(nameof(vxByDays));
        }
    }
}
