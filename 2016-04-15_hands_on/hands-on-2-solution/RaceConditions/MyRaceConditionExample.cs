using RaceConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditions
{
    /// <summary>
    /// Struct for holding the result of the non thread safe ooperation.
    /// </summary>
    public struct Result
    {
        /// <summary>
        /// the old value before the operation
        /// </summary>
        public int OldValue { get; set; }
        /// <summary>
        /// the new value after the operation
        /// </summary>
        public int NewValue { get; set; }
    }

    /// <summary>
    /// This class implements a cconsole application which demnstrates a race condition.
    /// The race condition will occur on the static member field 'number'.
    /// </summary>
    public class MyRaceConditionExample
    {
        // variable for race conditions
        private int number;
        private readonly object mutex = new object();

        // the counter for the repeations
        private int counter = 0;
        // flag indicating rae conditions was found
        private volatile bool raceCondititionOccurred = false;
        private Random random = new Random();

        // constants for runtime behaviour
        private const int THREAD_COUNT = 10;
        private const int THREAD_ALTER_ITERATIONS = 10;
        private const int PROGRAM_REPEATIONS = 10;
        private const int THREAD_SLEEP_MAX = 10;
        private const bool THREAD_SAFE = false;

        public void Run()
        {
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(MyRaceConditionExample)} started");
            Console.WriteLine($"----------------------------------------");

            // loop as long no race condition occurred or max repeations is reached
            while ((!raceCondititionOccurred) && (counter < PROGRAM_REPEATIONS))
            {
                // collect all threads here
                IList<Thread> taskList = new List<Thread>();

                // create the threads
                for (int i = 0; i < THREAD_COUNT; i++)
                {
                    Thread thread = new Thread(() =>
                    {
                        int threadId = Thread.CurrentThread.ManagedThreadId;
                        Console.WriteLine($"Thread: {threadId} started with {THREAD_ALTER_ITERATIONS} iterations");

                        // modify number iterativ
                        for (int j = 0; j < THREAD_ALTER_ITERATIONS; j++)
                        {
                            // some random wait increases changes for race conditions
                            Thread.Sleep(random.Next(1, THREAD_SLEEP_MAX));

                            // invoke non thread safe method
                            Result result = ModifyNumber();

                            // evaluate result
                            if ((result.NewValue - result.OldValue != 1))
                            {
                                raceCondititionOccurred = true;
                                Console.WriteLine($"Thread: {threadId} Race Condition detected. OldValue={result.OldValue}, NewValue={result.NewValue}");
                            }
                        }
                        Console.WriteLine($"Thread: {threadId} stopped with {THREAD_ALTER_ITERATIONS} iterations");
                    });

                    // start the thread
                    thread.Start();

                    // register thread
                    taskList.Add(thread);

                }

                // wait for all threads 
                foreach (var task in taskList)
                {
                    task.Join();
                }

                // increment counter for tries
                counter++;
            }


            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"{nameof(MyRaceConditionExample)} finished");
            Console.WriteLine($"----------------------------------------");

            // block cosnole window
            Console.Read();
        }

        /// <summary>
        /// Method which alters the static class level scoped number variable to simulate race conditions.
        /// </summary>
        /// <returns>the result of this operation, which can be evaluated later on</returns>
        public Result ModifyNumber()
        {
            Result result;

            // In thread-safe mode we use the mutex to span an synchronized context.
            // Only within this context the static member field 'number' gets modified.
            if (THREAD_SAFE)
            {
                lock (mutex)
                {
                    result = new Result
                    {
                        OldValue = number,
                        // non atomar operation
                        // here we get teh race conditions
                        NewValue = (number = number + 1)
                    };
                }
            }
            // In non-thread-safe mode, we alter the static member field 'number'
            // without an synchronization context.
            else {
                result = new Result
                {
                    OldValue = number,
                    // non atomar operation
                    // here we get teh race conditions
                    NewValue = (number = number + 1)
                };
            }

            return result;
        }
    }
}
