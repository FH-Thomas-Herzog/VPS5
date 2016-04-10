using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationPrimitives
{
    public class PollingExample
    {
        private volatile string[] results;
        private volatile int resultsFinished;
        private object resultsLocker = new object();

        private const int MAX_RESULTS = 10;
        private const int MAX_SPIN_TIME = 10;

        public void Run()
        {
            results = new string[MAX_RESULTS];
            resultsFinished = 0;
            // Collect all threads
            IList<Task> taskList = new List<Task>();

            // start tasks 
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

            // output results 
            for (int i = 0; i < MAX_RESULTS; i++)
            {
                Console.WriteLine(results[i]);
            }
        }

        public string Magic(int i)
        {
            return $"magic_{i}";
        }
    }
}
