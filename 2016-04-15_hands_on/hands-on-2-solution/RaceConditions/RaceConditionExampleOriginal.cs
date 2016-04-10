using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditions
{
    public class RaceConditionExampleOriginal
    {
        private const int N = 1000;
        private const int BUFFER_SIZE = 10;
        private double[] buffer;
        private AutoResetEvent signal;

        void Reader()
        {
            var readerIndex = 0;
            for (int i = 0; i < N; i++)
            {
                signal.WaitOne();
                Console.WriteLine(buffer[readerIndex]);
                readerIndex = (readerIndex + 1) % BUFFER_SIZE;
            }
        }
        void Writer()
        {
            var writerIndex = 0;
            for (int i = 0; i < N; i++)
            {
                buffer[writerIndex] = (double)i;
                signal.Set();
                writerIndex = (writerIndex + 1) % BUFFER_SIZE;
            }
        }

        public void Run()
        {
            buffer = new double[BUFFER_SIZE];
            signal = new AutoResetEvent(false);
            // start threads
            var t1 = new Thread(Reader);
            var t2 = new Thread(Writer);
            t1.Start();
            t2.Start();
            // wait
            t1.Join();
            t2.Join();
        }
    }
}
