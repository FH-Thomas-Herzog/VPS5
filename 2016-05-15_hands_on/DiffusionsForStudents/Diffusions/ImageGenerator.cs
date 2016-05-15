using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Diffusions
{
    public abstract class ImageGenerator : IImageGenerator
    {
        protected bool stopRequested = false;
        protected bool finished = false;

        public bool StopRequested => stopRequested;
        public bool Finished => finished;

        /// <summary>
        /// The event where listeners can register on
        /// </summary>
        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;


        public abstract Bitmap GenerateBitmap(Area area);

        public async void GenerateImage(Area area)
        {
            // reset for restart
            finished = false;
            stopRequested = false;

            Stopwatch watch = new Stopwatch();
            Task t = new Task(() =>
            {
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

            // Notify the last time 
            stopRequested = true;
            OnImageGenerated(area, null, watch.Elapsed);
        }    

        public virtual void Stop()
        {
            stopRequested = true;
        }

        #region Protected Util Methods
        protected virtual void ColorBitmap(double[,] array, int width, int height, Bitmap bm)
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

        /// <summary>
        /// Calculates the new matrix from teh old one
        /// </summary>
        /// <param name="i">the index of the first dimension</param>
        /// <param name="j">the index of the second dimension</param>
        /// <param name="height">the height of the screen</param>
        /// <param name="width">the width of the screen</param>
        /// <param name="oldMatrix">the old matrix</param>
        /// <param name="newMatrix">the new matrix</param>
        protected void CalculateMatrix(int i, int j, int height, int width, double[,] oldMatrix, double[,] newMatrix)
        {
            // index of directions (p=plux, m=minus)
            int jp, jm, ip, im;
            // in c# -1 % 50 is not 49 !!!!
            jp = (j + height - 1) % height;
            jm = (j + 1) % height;
            ip = (i + 1) % width;
            im = (i + width - 1) % width;

            newMatrix[i, j] = (
                oldMatrix[i, jp] +
                oldMatrix[i, jm] +
                oldMatrix[ip, j] +
                oldMatrix[im, j] +
                oldMatrix[ip, jp] +
                oldMatrix[im, jm] +
                oldMatrix[ip, jm] +
                oldMatrix[im, jp]) / 8.0;
        }

        /// <summary>
        /// Notifies the registered event handler of the finished image generation
        /// </summary>
        /// <param name="area">the area to notify</param>
        /// <param name="bitmap">the generate image to notify</param>
        /// <param name="timespan">the timespan the generation took to notify</param>
        protected void OnImageGenerated(Area area, Bitmap bitmap, TimeSpan timespan)
        {
            finished = stopRequested;
            ImageGenerated?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(new Tuple<Area, Bitmap, TimeSpan>(area, bitmap, timespan)));
        }
        #endregion
    }
}
