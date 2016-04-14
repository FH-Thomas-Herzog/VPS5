using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditions
{
    /// <summary>
    /// Illustrates a simple race condition and its solution.
    /// </summary>
    public class SimpleRaceconditionExample
    {
        // the global value accessible to all threads
        private int value = 0;
        // flag indicating synchrnoized invocation
        private bool synchrnous;

        // random for random waiting for the threads
        private readonly Random random = new Random();
        // lock object for synchrnous invocation
        private readonly object mutex = new object();

        private static int ITERATION_COUNT = 100;
        private static int THREAD_COUNT = 20;

        public SimpleRaceconditionExample(bool useSynchrnous = false)
        {
            synchrnous = useSynchrnous;
        }

        /// <summary>
        /// Performs the critical non atomic operation.
        /// </summary>
        public void DoStuff()
        {
            for (int j = 0; j < ITERATION_COUNT; j++)
            {
                int oldValue, newValue;

                // synchronus invocation
                if (synchrnous)
                {
                    lock (mutex)
                    {
                        oldValue = value;
                        newValue = value = value + 1;
                    }
                }
                // critical invocation
                else
                {
                    oldValue = value;
                    newValue = value = value + 1;
                }

                // evaluate result on method scoped values
                if ((oldValue - newValue) != -1)
                {
                    Console.WriteLine($"OldValue: {oldValue}, newValue: {value}");
                }

                // random sleep for each iteration
                Thread.Sleep(random.Next(100));
            }
        }

        /// <summary>
        /// Runs the test
        /// </summary>
        /// <param name="useSynchrnous">true if synchronized mode is intended, false otherwise</param>
        public void Run(bool useSynchrnous)
        {
            synchrnous = useSynchrnous;

            Console.WriteLine($"-----------------------------------------------------------");
            Console.WriteLine($"{nameof(SimpleRaceconditionExample)} synchrnoized={synchrnous} started");
            Console.WriteLine($"-----------------------------------------------------------");

            IList<Thread> threads = new List<Thread>(50);
            for (int i = 0; i < THREAD_COUNT; i++)
            {
                Thread thread = new Thread(DoStuff);
                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine($"-----------------------------------------------------------");
            Console.WriteLine($"{nameof(SimpleRaceconditionExample)}  synchrnoized={synchrnous} ended");
            Console.WriteLine($"-----------------------------------------------------------");
        }
    }
}
