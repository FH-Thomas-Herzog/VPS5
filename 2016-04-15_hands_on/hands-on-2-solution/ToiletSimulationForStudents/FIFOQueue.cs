using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Collections.Generic;

namespace VSS.ToiletSimulation
{
    /// <summary>
    /// FIFO Queue implementation which uses semaphores for syncchronization between cosumers and producers.
    /// </summary>
    public class FIFOQueue : Queue
    {
        // lock object for queue and related met data access
        private readonly object mutex = new object();

        // Semaphore for the producers
        private Semaphore producerSemaphore;

        // semaphore for the consumers
        private Semaphore consumerSemaphore;

        public override int Count { get { return (int)Interlocked.Read(ref count); } }

        public override bool IsCompleted
        {
            get
            {
                lock (mutex)
                {
                    return ((count == 0) && (addingCompleted));
                }
            }
        }

        public FIFOQueue(int capicity) : base(SortOrder.Descending)
        {
            producerSemaphore = new Semaphore(0, Parameters.Producers);
            consumerSemaphore = new Semaphore(0, Parameters.Consumers);

            producerSemaphore.Release();
        }

        public override void Enqueue(IJob job)
        {
            // close consumer semaphore because no consumer needs to wait anymore
            if (IsCompleted)
            {
                consumerSemaphore?.Close();
                consumerSemaphore = null;
            }
            // Skip enqueue if completed or null job provided
            else {
                if (job != null)
                {
                    // wait for an producer
                    producerSemaphore?.WaitOne();

                    // lock for adding on backed list
                    lock (mutex)
                    {
                        // enqueue job
                        queue.Enqueue(job.DueDate, job);

                        // increase counter
                        count++;
                    }

                    // release for an consumer
                    consumerSemaphore?.Release();
                }
            }
        }

        public override bool TryDequeue(out IJob job)
        {
            job = null;

            // wait for an consumer
            if (IsCompleted)
            {
                consumerSemaphore?.Close();
                consumerSemaphore = null;
            }
            else
            {
                consumerSemaphore?.WaitOne();
            }

            // synchronize dequeue from backed queue 
            lock (mutex)
            {
                if (!queue.IsEmpty)
                {
                    // dequeue job
                    job = queue.Dequeue().Value;
                    // decrease counter
                    count--;
                }
            }

            bool result = job != null;

            if (!IsCompleted)
            {
                // release for a producer
                if (result)
                {
                    producerSemaphore?.Release();
                }
            }
            // Close consumer semaphore if queue is completed
            else if (Count == 0)
            {
                consumerSemaphore?.Close();
                consumerSemaphore = null;
            }

            return result;
        }

        public override void CompleteAdding()
        {
            // synhronize complete operation
            lock (mutex)
            {
                // Increase complete counter
                completeCounter++;

                // check if all producers have completed
                if (completeCounter == Parameters.Producers)
                {
                    // Mark adding completed
                    addingCompleted = true;

                    // close producer semaphore
                    producerSemaphore?.Close();

                    // set semaphore null
                    producerSemaphore = null;
                }
            }
        }
    }
}
