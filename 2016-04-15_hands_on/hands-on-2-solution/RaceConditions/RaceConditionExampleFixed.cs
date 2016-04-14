using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditions
{
    /// <summary>
    /// This class implements the fixed version 
    /// of the provided code smaple which had an race condition on class memeber 'buffer'
    /// </summary>
    public class RaceConditionExampleFixed
    {
        private const int N = 1000;
        private const int BUFFER_SIZE = 10;
        private double[] buffer;
        private AutoResetEvent signal;

        // the mutex object used for locking
        private readonly object mutex = new object();

        /// <summary>
        /// Reader method for reader thread
        /// </summary>
        void Reader()
        {
            var readerIndex = 0;

            // N reads in buffer
            for (int i = 0; i < N; i++)
            {
                // Not sure if it was intended to block forever !!!
                signal?.WaitOne();

                // here we use a mutex to lock the buffer index access
                lock (mutex)
                {
                    Console.WriteLine(buffer[readerIndex]);
                }

                // get new index and keep it in buffer range
                readerIndex = (readerIndex + 1) % BUFFER_SIZE;
            }
        }

        /// <summary>
        /// Write method for writer thread
        /// </summary>
        void Writer()
        {
            var writerIndex = 0;

            // N writes in buffer
            for (int i = 0; i < N; i++)
            {

                // here we use a mutex to lock the buffer index access
                lock (mutex)
                {
                    buffer[writerIndex] = (double)i;
                }

                // notify that element has bee produced
                signal.Set();

                // calculate new index and keep it in buffer range
                writerIndex = (writerIndex + 1) % BUFFER_SIZE;
            }

            // Clear signal if producer is done
            signal.Set();
            signal.Close();
            signal = null;
        }

        /// <summary>
        /// Runs teh tests.
        /// </summary>
        public void Run()
        {
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(RaceConditionExampleFixed)} started");
            Console.WriteLine($"----------------------------------------");

            buffer = new double[BUFFER_SIZE];
            signal = new AutoResetEvent(false);
            // start threads
            var t1 = new Thread(Reader);
            var t2 = new Thread(Writer);
            t1.Start();
            t2.Start();
            // wait
            Console.WriteLine("waiting for T1");
            t1.Join();
            Console.WriteLine("waiting for T2");
            t2.Join();

            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(RaceConditionExampleFixed)} ended");
            Console.WriteLine($"----------------------------------------");
        }
    }
}
