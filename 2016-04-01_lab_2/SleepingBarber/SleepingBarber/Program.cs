using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SleepingBarber
{
    public class Program
    {
        public class Customer
        {
            public string Name { get; set; }

            public Customer(string name)
            {
                Name = name;
            }
        }

        public class CustomerProducer
        {
            Queue<Customer> queue;
            private SemaphoreSlim semaphoreSlim;
            public Thread Thread;
            private Random random = new Random();

            public CustomerProducer(Queue<Customer> queue, SemaphoreSlim semaphoreSlim)
            {
                this.queue = queue;
                this.semaphoreSlim = semaphoreSlim;
            }

            public void Start()
            {
                Thread = new Thread(Run);
                Thread.Start();
            }

            private void Run()
            {
                // Produce max customers who are coming
                for (int i = 0; i < NumCustomers; i++)
                {
                    Thread.Sleep(random.Next(200, 600));
                    // if queue size exceeded, send customers home
                    if (queue.Count >= Program.MaxQueueSize)
                    {
                        Console.WriteLine($"Customer_{i} is going home");
                    }
                    // otherwise add them to the queue
                    else
                    {
                        Customer customer = new Customer($"Customer_{i}");
                        lock (queue)
                        {
                            queue.Enqueue(customer);
                        }
                        semaphoreSlim.Release();
                        Console.WriteLine($"{customer.Name} got a seat");
                    }
                }
                // Could be dangerous if barber hasn't served all customers until then
                Program.finished = true;
            }
        }

        public class Barber
        {
            // The thread which serves the customer in the queue
            public Thread Thread;
            // The queue holding the waiting customers
            private Queue<Customer> queue;
            // A random for random waiting
            private Random random = new Random();
            // The semaphore for the synchronization (signaling)
            SemaphoreSlim semaphoreSlim;

            /// <summary>
            /// Initializes this barber with the customer queue he is serving
            /// </summary>
            /// <param name="queue">the waiting custoemrs queue</param>
            public Barber(Queue<Customer> queue, SemaphoreSlim semaphoreSlin)
            {
                this.queue = queue;
                this.semaphoreSlim = semaphoreSlin;
            }

            public void Start()
            {
                Thread = new Thread(Run);
                Thread.Start();
            }

            private void Run()
            {
                // Wait until queue is empty and porgram has finished
                // !! queue would need to be locked if multiple barbers are present
                while ((semaphoreSlim.CurrentCount > 0) || (!Program.finished))
                {
                    Customer customer;
                    // Wait for produced customers
                    semaphoreSlim.Wait();
                    // !! No need to check for queue contains elements since we have only 'ONE' barber
                    // Lock the queue during retrieval
                    lock (queue)
                    {
                        customer = queue.Dequeue();
                    }
                    // Serve customers
                    Console.WriteLine($"Shaving customer {customer.Name}");
                    Thread.Sleep(random.Next(200, 600));
                }
            }
        }

        // The queue for the wating customers
        private static Queue<Customer> queue = new Queue<Customer>();
        // Maximum queue size
        public const int MaxQueueSize = 10;
        // Maximum custoemr count
        public const int NumCustomers = 80;
        // Flag which indicates if producer is done with producing
        public static bool finished = false;


        static void Main(string[] args)
        {
            // Semaphore for synchronization
            // Slim because max is not important here because semaphore is allowed to count up to infinite.
            // initial with '0' means semaphore blocks because queue is empty at the beginning
            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0);

            CustomerProducer producer = new CustomerProducer(queue, semaphoreSlim);
            producer.Start();
            Barber barber = new Barber(queue, semaphoreSlim);
            barber.Start();

            producer.Thread.Join();
            barber.Thread.Join();
            Console.WriteLine("Simulation Done");
            // Block program
            Console.Read();
        }
    }
}
