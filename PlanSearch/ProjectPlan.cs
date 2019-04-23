using System;
using System.Collections.Generic;
using Core.Channels;

namespace PlanSearch
{
    public class ProjectPlan
    { 
        public IList<Estimation> Estimations { get; }

        public Estimation BestEstimation
        {
            get
            {
                Estimation bestEstimation = null;
                foreach (var estimation in Estimations)
                {
                    if (bestEstimation == null || estimation.TotalV > bestEstimation.TotalV)
                    {
                        bestEstimation = estimation;
                    }
                }
                return bestEstimation;
            }
        }

        public ProjectPlan(IList<Estimation> estimations)
        {
            Estimations = estimations ?? throw new ArgumentNullException(nameof(estimations));
        }

        public class Estimation
        {
            public int S { get; }
            public double TotalV { get; }
            public ISet<Channel> Donors { get; }
            public ISet<Channel> Acceptors { get; }

            public Estimation(int s, double totalV, ISet<Channel> donors, ISet<Channel> acceptors)
            {
                S = s;
                TotalV = totalV;
                Donors = donors ?? throw new ArgumentNullException(nameof(donors));
                Acceptors = acceptors ?? throw new ArgumentNullException(nameof(acceptors));
            }
        }
    }
}
