using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Diffusions
{
    public abstract class ImageGenerator : IImageGenerator
    {
        protected bool stopRequested = false;
        public bool StopRequested => stopRequested;

        protected bool finished = false;
        public bool Finished => finished;


        public abstract Bitmap GenerateBitmap(Area area);

        public async void GenerateImage(Area area)
        {
            // reset for restart
            finished = false;
            stopRequested = false;

            Task t = new Task(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                for (int i = 0; i < Settings.DefaultSettings.MaxIterations; i++)
                {
                    watch.Start();
                    Bitmap image = GenerateBitmap(area);
                    watch.Stop();
                    OnImageGenerated(area, image, watch.Elapsed);
                    watch.Reset();

                    // break loop
                    if (stopRequested) { break; }
                }
            });
            t.Start();
            await t;
        }

        public virtual void ColorBitmap(double[,] array, int width, int height, Bitmap bm)
        {
            int maxColorIndex = ColorSchema.Colors.Count - 1;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int colorIndex = (int)array[i, j] % maxColorIndex;
                    bm.SetPixel(i, j, ColorSchema.Colors[colorIndex]);
                }
            }
        }

        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;

        protected void OnImageGenerated(Area area, Bitmap bitmap, TimeSpan timespan)
        {
            finished = stopRequested;
            ImageGenerated?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(new Tuple<Area, Bitmap, TimeSpan>(area, bitmap, timespan)));
        }

        public virtual void Stop()
        {
            stopRequested = true;
        }
    }
}
