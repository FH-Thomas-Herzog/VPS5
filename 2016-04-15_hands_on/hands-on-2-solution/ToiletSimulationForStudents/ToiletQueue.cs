using Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.ToiletSimulation
{
    /// <summary>
    /// My implementation of the queue without the use of producer and consumer semaphores.
    /// The mode can be defined via the constructor.
    /// </summary>
    public class ToiletQueue : Queue
    {
        // completed producer counter
        private volatile int completedProducerCount = 0;
        // lock object for synchronization
        private readonly object mutex = new object();

        // The semaphore used
        private SemaphoreSlim semaphore;
        // The reset event used
        private AutoResetEvent notifyEvent;

        // The priority queue 
        private IPriorityQueue<DateTime, IJob> priorityQueue;
        // thelist queue
        private IList<IJob> queue;
        // flag indicating list or queue implemenation
        private readonly bool isPriorityQueue;
        // the random for random waiting
        private readonly Random random = new Random();
        // the defined concurrent mode
        private readonly Constants.ConcurrentMode concurrentMode;

        // the max random integer count
        private static int MAX_WAIT_COUNT = 200;
        // the max spin count
        private static int MAX_SPIN_COUNT = 5;
        // the maximum consumer count used by semaphore
        private static int MAX_CONSUMER_COUNT = Parameters.Consumers;

        /// <summary>
        /// Synchronized check for completed state
        /// </summary>
        public override bool IsCompleted
        {
            get
            {
                lock (mutex)
                {
                    return (addingCompleted) && (count == 0);
                }
            }
        }

        /// <summary>
        /// The main constructor
        /// </summary>
        /// <param name="usePriorityQueue">true if priority queue shall be used, false means use list</param>
        /// <param name="useConstants.ConcurrentMode">the intended concurrent mode</param>
        public ToiletQueue(Constants.QueueContainer container, Constants.ConcurrentMode useConstants)
        {
            isPriorityQueue = Constants.QueueContainer.PriorityQueue == container; ;
            concurrentMode = useConstants;

            // capicity is predictable, so use it
            int capicity = Parameters.Producers * Parameters.JobsPerProducer;
            // initialize intended container
            if (isPriorityQueue)
            {
                priorityQueue = new BinaryHeap<DateTime, IJob>(SortOrder.Descending, capicity);
            }
            else {
                queue = new List<IJob>(capicity);
            }

            // initialize resources for intended concurrent mode
            switch (concurrentMode)
            {
                case Constants.ConcurrentMode.Semaphore:
                    semaphore = new SemaphoreSlim(0, MAX_CONSUMER_COUNT);
                    break;
                case Constants.ConcurrentMode.ResetEvent:
                    notifyEvent = new AutoResetEvent(false);
                    break;
                default:
                    // nothing todo
                    break;
            }

        }

        public override void Enqueue(IJob job)
        {
            if (job != null)
            {
                // Block producers
                switch (concurrentMode)
                {
                    // block producer if semaphore is full
                    case Constants.ConcurrentMode.Semaphore:
                        while ((semaphore.CurrentCount == MAX_CONSUMER_COUNT) && (!IsCompleted))
                        {

                            Thread.SpinWait(random.Next(1, MAX_SPIN_COUNT));
                        }
                        break;
                    // Sleep randomly 
                    case Constants.ConcurrentMode.ThreadSleep:
                        Thread.Sleep(random.Next(MAX_WAIT_COUNT));
                        break;
                    // Spin randomly
                    case Constants.ConcurrentMode.ThreadSpin:
                        Thread.SpinWait(random.Next(1, MAX_SPIN_COUNT));
                        break;
                    // Spin randomly
                    case Constants.ConcurrentMode.ResetEvent:
                        Thread.SpinWait(random.Next(1, MAX_SPIN_COUNT));
                        break;
                    default:
                        // nothing todo
                        break;
                }

                // lock enqueue operation
                lock (mutex)
                {
                    // add to proper container
                    if (isPriorityQueue)
                    {
                        priorityQueue.Enqueue(job.DueDate, job);
                    }
                    else
                    {
                        queue.Add(job);
                    }

                    // increaseCounter
                    count++;

                    // mark not empty
                    empty = false;

                    // Semaphore and ResetEvent
                    switch (concurrentMode)
                    {
                        // release semaphore if not full
                        case Constants.ConcurrentMode.Semaphore:
                            if (semaphore.CurrentCount < MAX_CONSUMER_COUNT)
                            {
                                semaphore.Release();
                            }
                            break;
                        // notify consumer
                        case Constants.ConcurrentMode.ResetEvent:
                            notifyEvent.Set();
                            break;

                        default:
                            // nothing todo
                            break;
                    }
                }
            }
        }

        public override bool TryDequeue(out IJob job)
        {
            job = null;

            // release all waiting consumers if done
            if ((IsCompleted) && (Constants.ConcurrentMode.Semaphore == concurrentMode))
            {
                CleanupSemaphore(MAX_CONSUMER_COUNT, ref semaphore);
                return false;
            }
            else if (!IsCompleted)
            {
                switch (concurrentMode)
                {
                    // wait for semaphore
                    case Constants.ConcurrentMode.Semaphore:
                        semaphore?.Wait();
                        break;
                    // wait for event
                    case Constants.ConcurrentMode.ResetEvent:
                        notifyEvent?.WaitOne();
                        break;
                    // Sleep as long as empty
                    case Constants.ConcurrentMode.ThreadSleep:
                        while ((!IsCompleted) && (empty))
                        {
                            Thread.Sleep(random.Next(0, MAX_WAIT_COUNT));
                        }
                        break;
                    // Spin as long as empty
                    case Constants.ConcurrentMode.ThreadSpin:
                        while ((!IsCompleted) && (empty))
                        {
                            Thread.SpinWait(random.Next(1, MAX_SPIN_COUNT));
                        }
                        break;
                    default:
                        // nothing todo
                        break;
                }
            }

            // Synchronize dequeue operation
            lock (mutex)
            {
                // check if empty
                if (!empty)
                {
                    // get from proper container
                    if (isPriorityQueue)
                    {
                        job = priorityQueue.Dequeue().Value;
                    }
                    else
                    {
                        job = queue[0];
                        queue.RemoveAt(0);
                    }

                    // increase counter
                    count--;

                    // mark empty 
                    empty = (count == 0);
                }
            }

            // release all waiting consumers
            if ((Constants.ConcurrentMode.Semaphore == concurrentMode) && ((IsCompleted)))
            {
                CleanupSemaphore(Parameters.Consumers, ref semaphore);
            }

            return false;
        }

        public override void CompleteAdding()
        {
            // Synchronize complete operation
            lock (mutex)
            {
                // increase producer completed count
                completedProducerCount++;

                // if all have completed
                if (completedProducerCount == Parameters.Producers)
                {
                    // mark completed
                    addingCompleted = true;

                    switch (concurrentMode)
                    {
                        // Last notify and resetEvent cleanup
                        case Constants.ConcurrentMode.ResetEvent:
                            notifyEvent.Set();
                            notifyEvent.Close();
                            notifyEvent = null;
                            break;
                        // Release all waiting consumers
                        case Constants.ConcurrentMode.Semaphore:
                            CleanupSemaphore(MAX_CONSUMER_COUNT, ref semaphore);
                            break;
                        default:
                            // nothing todo
                            break;
                    }
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
