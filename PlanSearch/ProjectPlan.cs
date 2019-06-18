using System;
using System.Collections.Generic;
using Core.Channels;
using Core.Grid;

namespace PlanSearch
{
    public class ProjectPlan
    { 
        public IList<Estimation> Estimations { get; }

        public ProjectPlan(IList<Estimation> estimations)
        {
            Estimations = estimations ?? throw new ArgumentNullException(nameof(estimations));
        }

        public Estimation GetBestEstimation()
        {
            Estimation bestEstimation = null;
            foreach (var estimation in Estimations)
            {
                if (bestEstimation == null || estimation.TotalEffect > bestEstimation.TotalEffect)
                {
                    bestEstimation = estimation;
                }
            }
            return bestEstimation;
        }

        public class Estimation
        {
            public int S { get; }
            public int OptimalDonorsCount { get; }
            public int PotentialDonorsCount { get; }
            public double TotalEffect { get; }
            public double TotalPrice { get; }
            public double AcceptorsTargetValue { get; }
            public ISet<Channel> Donors { get; }
            public ISet<Channel> Acceptors { get; }
            public GridMap AcceptorZonesMap { get; }
            public GridMap DonorZonesMap { get; }

            public Estimation(int s, int optimalDonorsCount, int potentialDonorsCount, double totalEffect, 
                double totalPrice, double acceptorsTargetValue, ISet<Channel> donors, ISet<Channel> acceptors, GridMap acceptorZonesMap, GridMap donorZonesMap)
            {
                S = s;
                OptimalDonorsCount = optimalDonorsCount;
                PotentialDonorsCount = potentialDonorsCount;
                TotalEffect = totalEffect;
                TotalPrice = totalPrice;
                AcceptorsTargetValue = acceptorsTargetValue;
                Donors = donors;
                Acceptors = acceptors;
                AcceptorZonesMap = acceptorZonesMap;
                DonorZonesMap = donorZonesMap;
            }
        }
    }
}
