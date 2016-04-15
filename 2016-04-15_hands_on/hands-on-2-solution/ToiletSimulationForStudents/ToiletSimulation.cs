using System;

namespace VSS.ToiletSimulation
{
    public class ToiletSimulation
    {
        public static void Main()
        {
            // Random seed
            int randomSeed = new Random().Next();

            // .Net FIFO Qeue
            var q = new NetFIFOQueue();

            // My FIFOQueue
            //var q = new FIFOQueue(Constants.QueueContainer.List);
            //var q = new FIFOQueue(Constants.QueueContainer.PriorityQueue);

            // My ToiletQueue
            //var q = new ToiletQueue(Constants.QueueContainer.List, Constants.ConcurrentMode.Semaphore);
            //var q = new ToiletQueue(Constants.QueueContainer.List, Constants.ConcurrentMode.ResetEvent);
            //var q = new ToiletQueue(Constants.QueueContainer.List, Constants.ConcurrentMode.ThreadSleep);
            //var q = new ToiletQueue(Constants.QueueContainer.List, Constants.ConcurrentMode.ThreadSpin);
            //var q = new ToiletQueue(Constants.QueueContainer.PriorityQueue, Constants.ConcurrentMode.Semaphore);
            //var q = new ToiletQueue(Constants.QueueContainer.PriorityQueue, Constants.ConcurrentMode.ResetEvent);
            //var q = new ToiletQueue(Constants.QueueContainer.PriorityQueue, Constants.ConcurrentMode.ThreadSleep);
            //var q = new ToiletQueue(Constants.QueueContainer.PriorityQueue, Constants.ConcurrentMode.ThreadSpin);

            // Test the queue
            TestQueue(q, randomSeed);
        }

        public static void TestQueue(IQueue queue, int randomSeed)
        {
            Random random = new Random(randomSeed);

            PeopleGenerator[] producers = new PeopleGenerator[Parameters.Producers];
            for (int i = 0; i < producers.Length; i++)
                producers[i] = new PeopleGenerator("People Generator " + i, queue, random.Next());

            Toilet[] consumers = new Toilet[Parameters.Consumers];
            for (int i = 0; i < consumers.Length; i++)
                consumers[i] = new Toilet("Toilet " + i, queue);

            Console.WriteLine("Testing " + queue.GetType().Name + ":");

            Analysis.Reset();
            for (int i = 0; i < producers.Length; i++)
            {
                producers[i].Produce();
            }
            for (int i = 0; i < consumers.Length; i++)
            {
                consumers[i].Consume();
            }

            // Join all the threads
            for (int i = 0; i < consumers.Length; i++)
            {
                consumers[i]?.Join();
            }

            Analysis.Display();

            Console.Read();
        }

    }
}
