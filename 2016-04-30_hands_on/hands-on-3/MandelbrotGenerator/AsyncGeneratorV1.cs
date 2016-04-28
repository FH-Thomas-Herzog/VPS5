using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace MandelbrotGenerator
{
    /// <summary>
    /// Implements the first version of the asynchronous image generator using a Thread.
    /// </summary>
    public class AsyncGeneratorV1 : SyncImageGenerator, IAsyncImageGenerator
    {
        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> OnCompleted;

        private Thread thread;

        public void Abort()
        {
            try
            {
                if ((thread != null) && (thread.IsAlive))
                {
                    thread?.Abort();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occurred during abort operation. {e.Message}");
            }
        }

        public void GenerateAsync(Area area)
        {
            thread = new Thread(Run);
            thread.Start(area);
        }

        private void Run(object o)
        {
            Stopwatch watch = new Stopwatch();
            Area area = o as Area;
            try
            {
                watch.Start();
                var image = GenerateImage(0, area.Width, area, () => false);
                watch.Stop();

                OnCompleted?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(
                                            new Tuple<Area, Bitmap, TimeSpan>(area, image, watch.Elapsed)));
            }
            catch (ThreadAbortException e)
            {
                watch.Stop();
                Console.WriteLine("Image generation aborted");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occurred during image generation. {e.Message}");
            }
            finally
            {
                watch.Stop();
            }
        }
    }
}
