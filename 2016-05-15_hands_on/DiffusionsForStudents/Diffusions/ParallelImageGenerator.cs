using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Diffusions
{
    /// <summary>
    /// Parallel implementation of the image generator which uses Parallel.For for the inner and outer iteration.
    /// </summary>
    public class ParallelImageGenerator : SyncImageGenerator
    {
        private CancellationTokenSource cts;
        private readonly object mutex = new object();

        public override Bitmap GenerateBitmap(Area area)
        {
            var matrix = area.Matrix;
            int height = area.Height;
            int width = area.Width;

            var newMatrix = new double[width, height];

            cts = new CancellationTokenSource();
            ParallelOptions outerOptions = new ParallelOptions()
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = MainForm.MAX_PARALLEL_OUTER
            };
            ParallelOptions innerOptions = new ParallelOptions()
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = MainForm.MAX_PARALLEL_INNER
            };

            try
            {
                Parallel.For(0, width, outerOptions, (i, outerState) =>
                {
                    try
                    {
                        Parallel.For(0, height, innerOptions, (j, innerState) =>
                        {
                            try
                            {
                                // Calculate the matrix
                                CalculateMatrix(i, j, height, width, matrix, newMatrix);
                            }
                            catch (OperationCanceledException) { /* Nothing to do */ }
                        });
                    }
                    catch (OperationCanceledException) {  /* Nothing to do */ }
                });
            }
            catch (OperationCanceledException)
            {
                // Nothing to do
            }

            cts = null;

            // null because image could be broken
            if (stopRequested) return null;

            // if stop request occurs here, then we let finish the image generation
            area.Matrix = newMatrix;
            Bitmap image = new Bitmap(width, height);
            ColorBitmap(newMatrix, width, height, image);


            return image;
        }

        public override void Stop()
        {
            stopRequested = true;
            cts?.Cancel();
        }
    }

}
