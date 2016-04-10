using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationPrimitives
{
    /// <summary>
    /// This class represents the polling example which was formerly implemented with polling
    /// and now uses Task.WaitAll() for blocking this thread instead of a busy waiting
    /// </summary>
    public class PollingExample
    {
        private volatile string[] results;
        private volatile int resultsFinished;
        private object resultsLocker = new object();

        private const int MAX_RESULTS = 10;
        private const int MAX_SPIN_TIME = 10;

        /// <summary>
        /// Fills a buffer asynchronously via multiple task, one for each buffer index.
        /// </summary>
        public void Run()
        {
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(PollingExample)} started");
            Console.WriteLine($"----------------------------------------");

            // init buffer which gets filled by the task
            results = new string[MAX_RESULTS];
            resultsFinished = 0;

            // Collect all tasks
            IList<Task> taskList = new List<Task>();

            // Create and start tasks 
            for (int i = 0; i < MAX_RESULTS; i++)
            {
                var t = new Task((s) =>
                {
                    int _i = (int)s;
                    string m = Magic(_i);
                    results[_i] = m;
                    lock (resultsLocker)
                    {
                        resultsFinished++;
                    }
                }, i);
                taskList.Add(t);
                t.Start();
            }

            // Wait for all task until they are completed
            // So ww have no busy waiting, but block this thread.
            Task.WaitAll(taskList.ToArray());

            // Print results 
            for (int i = 0; i < MAX_RESULTS; i++)
            {
                Console.WriteLine(results[i]);
            }

            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(PollingExample)} ended");
            Console.WriteLine($"----------------------------------------");
        }

        /// <summary>
        /// Just for doing something
        /// </summary>
        /// <param name="i">the current index</param>
        /// <returns>the string represenation of the current index</returns>
        public string Magic(int i)
        {
            return $"magic_{i}";
        }
    }
}
