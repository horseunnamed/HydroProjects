using System;
using Core.Channels;

namespace PlanSearch
{
    internal class Donor
    {
        public Channel Channel { get; }
        public double Effect { get; }

        public Donor(Channel channel, double effect)
        {
            this.Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            this.Effect = effect;
        }
    }
}
