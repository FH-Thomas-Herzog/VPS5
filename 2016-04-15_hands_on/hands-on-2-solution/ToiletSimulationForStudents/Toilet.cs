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
            // Done if queue is completed
            while (!Queue.IsCompleted)
            {
                IJob job;
                Queue.TryDequeue(out job);
                if (job != null)
                {
                    job.Process();
                }
                // Job can be null if consumer is wating loop and cannot be served anymore.
                else
                {
                    Console.WriteLine("Ups... Job was null.");
                }
            }
        }

        public void Join()
        {
            thread?.Join();
        }
    }
}
