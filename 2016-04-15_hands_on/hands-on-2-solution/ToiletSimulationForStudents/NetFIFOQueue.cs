using System;
using System.Collections.Concurrent;
using System.Threading;

namespace VSS.ToiletSimulation
{
    class NetFIFOQueue : IQueue, IDisposable
    {
        private readonly BlockingCollection<IJob> queue = new BlockingCollection<IJob>(new ConcurrentQueue<IJob>());
        private int producersCompleted = 0;

        public int Count
        {
            get { return queue.Count; }
        }
        public void Enqueue(IJob job)
        {
            queue.Add(job);
        }

        public bool TryDequeue(out IJob job)
        {
            return queue.TryTake(out job);
        }

        public void CompleteAdding()
        {
            Interlocked.Increment(ref producersCompleted);
            // Set completed only if all producers are done
            if (producersCompleted == Parameters.Producers)
            {
                queue.CompleteAdding();
            }
        }

        public bool IsCompleted
        {
            get { return queue.IsCompleted; }
        }

        public void Dispose()
        {
            queue.Dispose();
        }
    }
}
