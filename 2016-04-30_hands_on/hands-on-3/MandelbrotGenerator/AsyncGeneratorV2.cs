using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace MandelbrotGenerator
{
    /// <summary>
    /// Implements the second version of the asynchronous image generator using BackgroundWorker .
    /// </summary>
    public class AsyncGeneratorV2 : SyncImageGenerator, IAsyncImageGenerator
    {
        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> OnCompleted;

        private BackgroundWorker worker;

        public void Abort()
        {
            try
            {
                worker.CancelAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occurred during abort operation. {e.Message}");
            }
        }

        public void GenerateAsync(Area area)
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = false;
            worker.RunWorkerCompleted += (sender, evt) =>
            {
                // no notification in case on cancelation
                if ((!evt.Cancelled) && (evt.Error == null) && (evt.Result != null))
                {
                    OnCompleted?.Invoke(this, (EventArgs<Tuple<Area, Bitmap, TimeSpan>>)evt.Result);
                }
            };
            worker.DoWork += DoWork;
            worker.RunWorkerAsync(area);
        }

        private void DoWork(object o, DoWorkEventArgs evt)
        {
            try
            {
                Area area = evt.Argument as Area;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var image = GenerateImage(0, area.Width, area, () => worker.CancellationPending);
                watch.Stop();

                // On cancelation no result needed
                if (!worker.CancellationPending)
                {
                    evt.Result = new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(
                                            new Tuple<Area, Bitmap, TimeSpan>(area, image, watch.Elapsed));
                }

                evt.Cancel = worker.CancellationPending;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occurred during image generation. {e.Message}");
            }
        }
    }
}
