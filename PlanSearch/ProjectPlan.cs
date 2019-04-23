using System;
using System.Collections.Generic;
using Core.Channels;

namespace PlanSearch
{
    public class ProjectPlan
    { 
        public ISet<Channel> Donors { get; }
        public ISet<Channel> Acceptors { get; }
        public IList<Estimation> Estimations { get; }

        public ProjectPlan(ISet<Channel> donors, ISet<Channel> acceptors, IList<Estimation> estimations)
        {
            Donors = donors ?? throw new ArgumentNullException(nameof(donors));
            Acceptors = acceptors ?? throw new ArgumentNullException(nameof(acceptors));
            Estimations = estimations ?? throw new ArgumentNullException(nameof(estimations));
        }

        public class Estimation
        {
            public int S { get; }
            public double TotalV { get; }

            public Estimation(int s, double totalV)
            {
                S = s;
                TotalV = totalV;
            }
        }
    }
}
