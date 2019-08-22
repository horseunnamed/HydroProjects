using System;
using System.Collections.Generic;

namespace Core.Channels
{
    public delegate void ChannelsVisitor(Channel channel);

    public class ChannelsGraph
    {
        private readonly List<Channel> entrances;

        public ChannelsGraph(List<Channel> entrances)
        {
            this.entrances = entrances ?? throw new ArgumentNullException(nameof(entrances));
        }

        public void BFS(ChannelsVisitor visitor)
        {
            var wasEnqueued = new HashSet<Channel>();
            var queue = new Queue<Channel>();

            foreach (var root in entrances)
            {
                if (!wasEnqueued.Contains(root))
                {
                    queue.Enqueue(root);
                    wasEnqueued.Add(root);

                    while (queue.Count > 0)
                    {
                        var channel = queue.Dequeue();
                        foreach (var child in channel.Connecions)
                        {
                            if (!wasEnqueued.Contains(child))
                            {
                                queue.Enqueue(child);
                                wasEnqueued.Add(child);
                            }
                        }
                        visitor(channel);
                    }
                }
            }
        }

    }
}
