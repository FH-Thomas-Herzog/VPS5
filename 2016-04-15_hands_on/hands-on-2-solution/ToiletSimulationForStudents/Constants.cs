using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.ToiletSimulation
{
    public class Constants
    {
        /// <summary>
        /// Enumeration which specifies the available concurrent mode
        /// </summary>
        public enum ConcurrentMode
        {
            Semaphore,
            ResetEvent,
            ThreadSpin,
            ThreadSleep
        }

        /// <summary>
        /// Enumeration which specifies the available queue container
        /// </summary>
        public enum QueueContainer
        {
            PriorityQueue,
            List
        }
    }
}
