using System;
using System.Collections.Generic;
using Core.Channels;

namespace PlanSearch
{
    public class ProjectPlan
    { 
        public ISet<Channel> Donors { get; }
        public ISet<Channel> Acceptors { get; }

        public ProjectPlan(ISet<Channel> donors, ISet<Channel> acceptors)
        {
            Donors = donors ?? throw new ArgumentNullException(nameof(donors));
            Acceptors = acceptors ?? throw new ArgumentNullException(nameof(acceptors));
        }
    }
}
