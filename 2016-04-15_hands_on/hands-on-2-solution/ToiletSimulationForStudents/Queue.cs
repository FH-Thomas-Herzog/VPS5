using Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace VSS.ToiletSimulation
{
    public abstract class Queue : IQueue
    {
        protected long count = 0;
        protected volatile bool addingCompleted = false;
        protected IPriorityQueue<DateTime, IJob> queue;

        public abstract int Count { get; }
        public abstract bool IsCompleted { get; }

        protected Queue(SortOrder sortOrder)
        {
            queue = new BinaryHeap<DateTime, IJob>(sortOrder);
        }

        public abstract void Enqueue(IJob job);


        public abstract bool TryDequeue(out IJob job);


        public virtual void CompleteAdding()
        {
            throw new NotImplementedException();
        }
    }
}
