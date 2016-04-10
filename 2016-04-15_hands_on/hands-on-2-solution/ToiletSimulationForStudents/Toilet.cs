using System;
using System.Threading;

namespace VSS.ToiletSimulation
{
    public class Toilet
    {
        public string Name { get; private set; }
        public IQueue Queue { get; private set; }

        private Thread thread;
        private Toilet() { }
        public Toilet(string name, IQueue queue)
        {
            Name = name;
            Queue = queue;
        }

        public void Consume()
        {
            // Create and start thread
            thread = new Thread(Run);
            thread.Start();
        }

        public void Run()
        {
            while (!Queue.IsCompleted)
            {
                IJob job;
                // Blocked by queue (Uses semaphores)
                Queue.TryDequeue(out job);
                if (job != null)
                {
                    job.Process();
                }
                // Could be called at the end if not completed, but other consumer did use the last job.
                else
                {
                    Console.WriteLine("Should never be called. Meant queue.dequeue didn't return an job");
                }
            }
        }

        public void Join()
        {
            thread?.Join();
        }
    }
}
