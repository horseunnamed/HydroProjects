using System;
using System.Collections.Generic;

namespace Core.Channels
{
    public delegate void ChannelsVisitor(Channel channel);

    public class ChannelsGraph
    {
        private readonly Channel root;
        private readonly IDictionary<Channel, IEnumerable<Channel>> childrenOf;
        private readonly IDictionary<Channel, IEnumerable<Channel>> parentOf;

        public ChannelsGraph(Channel root, IDictionary<Channel, IEnumerable<Channel>> childrenOf)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
            this.childrenOf = childrenOf ?? throw new ArgumentNullException(nameof(childrenOf));
            this.parentOf = BuildParentOfDictionary();
        }

        private IDictionary<Channel, IEnumerable<Channel>> BuildParentOfDictionary() {
            var result = new Dictionary<Channel, IEnumerable<Channel>>();
            BFS(channel =>
            {
                foreach (var child in channel.Connecions)
                {
                    if (!result.ContainsKey(child))
                    {
                        result[child] = new List<Channel>();
                    }
                    (result[child] as List<Channel>).Add(channel);
                }
            });
            return result;
        }

        public void BFS(ChannelsVisitor visitor)
        {
            var wasEnqueued = new HashSet<Channel>();
            var queue = new Queue<Channel>();

            queue.Enqueue(root);
            wasEnqueued.Add(root);

            while (queue.Count > 0)
            {
                var channel = queue.Dequeue();
                visitor(channel);
                foreach (var child in childrenOf[channel])
                {
                    if (!wasEnqueued.Contains(child))
                    {
                        queue.Enqueue(child);
                        wasEnqueued.Add(child);
                    }
                }
            }
        }

    }
}
