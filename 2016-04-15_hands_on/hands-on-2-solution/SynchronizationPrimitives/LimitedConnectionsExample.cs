using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationPrimitives
{
    /// <summary>
    /// This class implements the 'DownloadFiles' functionallity with the help of a semaphore.
    /// </summary>
    public class LimitedConnectionsExample
    {
        private Random random = new Random();

        public const int MAX_PARALLEL_LOADS = 10;
        public const int MAX_THREAD_SLEEP = 50;


        /// <summary>
        /// Asynchronous implementation of 'DownloadFiles'
        /// </summary>
        /// <param name="urls">the file urls to download</param>
        public void DownloadFilesAsync(IEnumerable<string> urls)
        {
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(LimitedConnectionsExample)}#{nameof(DownloadFilesAsync)} started");
            Console.WriteLine($"----------------------------------------");

            // Initialize semaphore which can serve 'MAX_PARALLEL_LOADS' concurrent requests
            // Initialize with max count so that threads can start immediatelly
            Semaphore semaphore = new Semaphore(MAX_PARALLEL_LOADS, MAX_PARALLEL_LOADS);

            // Collect all started thread
            IList<Thread> threadList = new List<Thread>(urls.Count());
            foreach (var url in urls)
            {
                Thread t = new Thread(() => DownloadFile(semaphore, url));
                threadList.Add(t);
                t.Start();
            }

            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(LimitedConnectionsExample)}#{nameof(DownloadFilesAsync)} exit asynchronously");
            Console.WriteLine($"----------------------------------------");

            // Seamphore should get release by GC after this method finished and all thread released their reference to it
        }

        /// <summary>
        /// Synchronous implementation of 'DownloadFiles'.
        /// Threads are still invoced asynchronously but we wait for them to complete.
        /// </summary>
        /// <param name="urls">the file urls to download</param>
        public void DownloadFiles(IEnumerable<string> urls)
        {
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(LimitedConnectionsExample)}#{nameof(DownloadFilesAsync)} started");
            Console.WriteLine($"----------------------------------------");

            // Initialize semaphore which can serve 'MAX_PARALLEL_LOADS' concurrent requests
            // Initialize with max count so that threads can start immediatelly
            Semaphore semaphore = new Semaphore(MAX_PARALLEL_LOADS, MAX_PARALLEL_LOADS);

            // Collect all started thread
            IList<Thread> threadList = new List<Thread>(urls.Count());
            foreach (var url in urls)
            {
                Thread t = new Thread(() => DownloadFile(semaphore, url));
                threadList.Add(t);
                t.Start();
            }

            // Wait for all thread to complete 
            foreach (var thread in threadList)
            {
                thread.Join();
            }

            // Here we a re synchronously, therefore we can close the semaphore explicitly 
            semaphore.Close();

            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(LimitedConnectionsExample)}#{nameof(DownloadFilesAsync)} ended");
            Console.WriteLine($"----------------------------------------");
        }

        /// <summary>
        /// Simulates a file download.
        /// </summary>
        /// <param name="semaphore">the semaphore to register on</param>
        /// <param name="url">the url of the file</param>
        public void DownloadFile(Semaphore semaphore, object url)
        {
            // Requeswt semaphore
            semaphore.WaitOne();

            // Do stuff
            Console.WriteLine(url);
            Thread.Sleep(MAX_THREAD_SLEEP);

            // Release semaphore
            semaphore.Release();
        }
    }
}