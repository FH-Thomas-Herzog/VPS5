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
        // complete counter to be aware if all producers have completed adding to this queue
        private volatile int completeCounter;

        // lock object for queue and related met data access
        private readonly object mutex = new object();

        // Semaphore for the producers
        private SemaphoreSlim producerSemaphore;

        // semaphore for the consumers
        private SemaphoreSlim consumerSemaphore;

        public override int Count
        {
            get
            {
                // synchronized read of count value
                return (int)Interlocked.Read(ref count);
            }
        }

        // Constants for semaphore max counts
        private readonly int PRODUCER_MAX_COUNT = Parameters.Producers;
        private readonly int CONSUMER_MAX_COUNT = Parameters.Consumers;

        public override bool IsCompleted
        {
            get
            {
                // synchronize evaluation if queue is completed
                lock (mutex)
                {
                    return ((count == 0) && (addingCompleted));
                }
            }
        }

        /// <summary>
        /// Initializes this queue and its using semaphores.
        /// The semaphores will get max count set as there are producers and consumer present.
        /// </summary>
        public FIFOQueue() : base(SortOrder.Descending)
        {
            producerSemaphore = new SemaphoreSlim(1, PRODUCER_MAX_COUNT);
            consumerSemaphore = new SemaphoreSlim(0, CONSUMER_MAX_COUNT);
        }

        public override void Enqueue(IJob job)
        {
            // close consumer semaphore because no consumer needs to wait anymore
            if (IsCompleted)
            {
                CleanupSemaphore(CONSUMER_MAX_COUNT, ref consumerSemaphore);
                return;
            }

            // Skip enqueue if completed or null job provided
            if (job != null)
            {
                // wait for an producer
                producerSemaphore?.Wait();

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

        public override bool TryDequeue(out IJob job)
        {
            job = null;
            bool dequeued = false;
            bool produceItems = false;

            // completed means cannot return anything
            if (IsCompleted)
            {
                CleanupSemaphore(CONSUMER_MAX_COUNT, ref consumerSemaphore);
                return false;
            }
            // wait for consumer (added items)
            else
            {
                consumerSemaphore?.Wait();
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
                dequeued = (job != null);
                produceItems = ((!IsCompleted));// && ((Count == 0) || (Count >= 5)));
            }

            // release for a producer
            if (produceItems)
            {
                producerSemaphore?.Release();
            }

            // Close consumer semaphore if queue is completed
            if (IsCompleted)
            {
                CleanupSemaphore(CONSUMER_MAX_COUNT, ref consumerSemaphore);
            }

            return dequeued;
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
                }
            }

            if (addingCompleted)
            {
                // close producer semaphore
                CleanupSemaphore(PRODUCER_MAX_COUNT, ref producerSemaphore);

                // set semaphore null
                producerSemaphore = null;

                if (IsCompleted)
                {
                    CleanupSemaphore(CONSUMER_MAX_COUNT, ref consumerSemaphore);
                }
            }
        }

        /// <summary>
        /// Thsi method releass the remaining consumer which are waiting for jobs, but queue is empty.
        /// </summary>
        /// <param name="maxCount">the original max count related to this semaphore</param>
        /// <param name="semaphore">the semaphore to be cleaned up</param>
        private void CleanupSemaphore(int maxCount, ref SemaphoreSlim semaphore)
        {
            if ((semaphore != null) && (maxCount > 0))
            {
                // synhronize cleanup iperation for semaphore
                lock (mutex)
                {
                    int remainingCount;
                    if ((remainingCount = (maxCount - semaphore.CurrentCount)) != 0)
                    {
                        semaphore.Release(remainingCount);
                    }
                }

                semaphore = null;
            }
        }
    }
}
