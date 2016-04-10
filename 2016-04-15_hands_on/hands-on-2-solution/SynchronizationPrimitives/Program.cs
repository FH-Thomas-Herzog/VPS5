using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationPrimitives
{
    public class Program
    {
        private static readonly LimitedConnectionsExample limitedConnection = new LimitedConnectionsExample();
        private static readonly PollingExample pollingExample = new PollingExample();

        public static void Main(string[] args)
        {
            IList<string> urls = Enumerable.Range(0, 10).Select(i => $"URL_{i}").ToList();

            // Synchronous implementation
            limitedConnection.DownloadFiles(urls);

            // Asynchronous implementation
            limitedConnection.DownloadFilesAsync(urls);

            // pooling
            pollingExample.Run();

            // block sonole
            Console.Read();
        }
    }
}
