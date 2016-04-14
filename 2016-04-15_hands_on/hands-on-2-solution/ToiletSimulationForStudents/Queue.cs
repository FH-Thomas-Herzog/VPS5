using Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace VSS.ToiletSimulation
{
    public abstract class Queue : IQueue
    {
        protected volatile bool empty = true;
        protected volatile int count = 0;
        protected volatile bool addingCompleted = false;

        public int Count
        {
            get { return count; }
        }
        public abstract bool IsCompleted { get; }

        protected Queue()
        {
        }

        public abstract void Enqueue(IJob job);


        public abstract bool TryDequeue(out IJob job);


        public virtual void CompleteAdding()
        {
            throw new NotImplementedException();
        }
    }
}
