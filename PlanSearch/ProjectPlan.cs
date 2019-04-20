using System.Collections.Generic;

namespace PlanSearch
{
    public class ProjectPlan
    {
        public HashSet<long> SelectedChannels { get; }

        public ProjectPlan(HashSet<long> selectedChannels)
        {
            SelectedChannels = selectedChannels;
        }
    }
}
