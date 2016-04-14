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

        // prioritiy queue to use
        protected IPriorityQueue<DateTime, IJob> priorityQueue;
        // queue list to use
        IList<IJob> queue;

        // maximum producer count
        private readonly int PRODUCER_MAX_COUNT = Parameters.Producers;
        // maximum consumer count
        private readonly int CONSUMER_MAX_COUNT = Parameters.Consumers;
        // true if priority queue shall be used isntead of list
        private readonly bool isPriorityQueue;

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
        public FIFOQueue(Constants.QueueContainer container)
        {
            isPriorityQueue = Constants.QueueContainer.List == container;

            int capicity = Parameters.Producers * Parameters.JobsPerProducer;
            if (isPriorityQueue)
            {
                priorityQueue = new BinaryHeap<DateTime, IJob>(SortOrder.Descending, capicity);
            }
            else {
                queue = new List<IJob>(capicity);
            }

            producerSemaphore = new SemaphoreSlim(1, PRODUCER_MAX_COUNT);
            consumerSemaphore = new SemaphoreSlim(0, CONSUMER_MAX_COUNT);
        }

        public override void Enqueue(IJob job)
        {
            // Skip enqueue if completed or null job provided
            if (job != null)
            {
                // wait for an producer
                producerSemaphore?.Wait();

                // lock for adding on backed list
                lock (mutex)
                {
                    // enqueue job
                    if (isPriorityQueue)
                    {
                        priorityQueue.Enqueue(job.DueDate, job);
                    }
                    else {
                        queue.Add(job);
                    }

                    // increase counter
                    count++;

                    // cannot be empty here
                    empty = false;
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
                if (!empty)
                {
                    // dequeue job
                    if (isPriorityQueue)
                    {
                        job = priorityQueue.Dequeue().Value;
                    }
                    else {
                        job = queue[0];
                        queue.RemoveAt(0);
                    }

                    // decrease counter
                    count--;

                    // mark is empty
                    empty = (count == 0);

                }

                dequeued = (job != null);
                produceItems = !IsCompleted;
            }

            // release for a producer
            if (produceItems)
            {
                producerSemaphore?.Release();
            }
            // Close consumer semaphore if queue is completed
            else
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
