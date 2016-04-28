using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MandelbrotGenerator
{
    /// <summary>
    /// This is the thrid version which implements the IAsyncImageGenerator with parallel working workers.
    /// </summary>
    public class ParallelGenerator : SyncImageGenerator, IAsyncImageGenerator
    {
        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> OnCompleted;

        // the bucket for the worker
        private int bucket;
        // the bucket offset caused by decimal division
        private int bucketOffset;
        // global canceled flag for all workers
        private volatile bool canceled = false;
        // kept reference to riginal area
        private Area area;
        // thje global image all parts get merged too
        private Bitmap image;
        // The global stop watch for time mesearement
        private Stopwatch watch;
        // the array holding the workers
        private BackgroundWorker[] workers;
        // array holding success flags for teh invoked workes
        private bool[] workersFinished;

        public void GenerateAsync(Area area)
        {
            watch = new Stopwatch();
            this.area = area;
            image = new Bitmap(area.Width, area.Height);
            workers = new BackgroundWorker[Settings.DefaultSettings.Workers];
            workersFinished = new bool[Settings.DefaultSettings.Workers];
            image = new Bitmap(area.Width, area.Height);
            bucket = area.Width / Settings.DefaultSettings.Workers;
            bucketOffset = area.Width - (bucket * Settings.DefaultSettings.Workers);

            watch.Start();
            for (int i = 0; i < Settings.DefaultSettings.Workers; i++)
            {
                // Each worker gets its own area instance since not muatable one
                var workerArea = new Area(area.MinReal, area.MinImg, area.MaxReal, area.MaxImg, area.Width, area.Height);

                workersFinished[i] = false;
                var worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = false;
                worker.DoWork += DoWork;
                worker.RunWorkerCompleted += Completed;
                worker.RunWorkerAsync(new Tuple<int, Area>(i, workerArea));
            }
        }

        public void Abort()
        {
            if (workers != null)
            {
                foreach (var worker in workers)
                {
                    if ((worker != null) && (worker.IsBusy))
                    {
                        worker.CancelAsync();
                    }
                }
                if (watch != null)
                {
                    watch.Stop();
                }

                area = null;
                image = null;
                canceled = true;
            }
        }

        private void DoWork(object o, DoWorkEventArgs evt)
        {
            Tuple<int, Area> tuple = evt.Argument as Tuple<int, Area>;
            BackgroundWorker worker = workers[tuple.Item1];
            int startIdx = (tuple.Item1 * bucket);
            int endIdx = (startIdx + bucket);
            endIdx = (tuple.Item1 == (Settings.DefaultSettings.Workers - 1)) ? (endIdx + bucketOffset) : endIdx;

            var image = GenerateImage(startIdx, endIdx, tuple.Item2, () => canceled);
            // On cancelation no result needed
            if (!canceled)
            {
                evt.Result = new Tuple<int, int, int, Bitmap>(tuple.Item1, startIdx, endIdx, image);
            }

            evt.Cancel = canceled;

        }

        private void Completed(object sneder, RunWorkerCompletedEventArgs evt)
        {
            // nothing to notify on cancelation
            if ((!canceled) && (evt.Error == null) && (evt.Result != null))
            {
                Tuple<int, int, int, Bitmap> tuple = evt.Result as Tuple<int, int, int, Bitmap>;
                workersFinished[tuple.Item1] = true;
                Bitmap workerImage = tuple.Item4;

                // merge worker image to global one
                // Collisions shouldn't occur here.
                for (int i = tuple.Item2; i < tuple.Item3; i++)
                {
                    for (int j = 0; j < area.Height; j++)
                    {
                        this.image.SetPixel(i, j, workerImage.GetPixel(i, j));
                    }
                }

                // Here we are done
                if (workersFinished.All(a => a))
                {
                    watch.Stop();

                    OnCompleted?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(
                                       new Tuple<Area, Bitmap, TimeSpan>(area, this.image, watch.Elapsed)));

                    // release references
                    area = null;
                    image = null;
                    workers = null;
                    workersFinished = null;

                }
            }
        }
    }
}
