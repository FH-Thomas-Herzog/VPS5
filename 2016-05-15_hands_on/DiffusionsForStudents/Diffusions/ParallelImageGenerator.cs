using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diffusions
{
    /// <summary>
    /// Parallel implementation of the image generator which uses Parallel.For for the inner and outer iteration.
    /// </summary>
    public class ParallelImageGenerator : SyncImageGenerator
    {
        public override Bitmap GenerateBitmap(Area area)
        {
            var matrix = area.Matrix;
            int height = area.Height;
            int width = area.Width;

            var newMatrix = new double[width, height];

            Parallel.For(0, width, (i, outerState) =>
            {
                Parallel.For(0, height, (j, innerState) =>
                {
                    // index of directions (p=plux, m=minus)
                    int jp, jm, ip, im;
                    // in c# -1 % 50 is not 49 !!!!
                    jp = (j + height - 1) % height;
                    jm = (j + 1) % height;
                    ip = (i + 1) % width;
                    im = (i + width - 1) % width;

                    newMatrix[i, j] = (
                        matrix[i, jp] +
                        matrix[i, jm] +
                        matrix[ip, j] +
                        matrix[im, j] +
                        matrix[ip, jp] +
                        matrix[im, jm] +
                        matrix[ip, jm] +
                        matrix[im, jp]) / 8.0;

                    // stop inner loop
                    if ((stopRequested) && (!innerState.IsStopped)) { innerState.Stop(); }
                });
                // stop outer loop
                if ((stopRequested) && (!outerState.IsStopped)) { outerState.Stop(); }
            });

            // return null if stop requested, because image not needed anymore
            if (stopRequested) return null;

            // if stop request occurs here, then we let finish the image generation
            area.Matrix = newMatrix;
            Bitmap image = new Bitmap(width, height);
            ColorBitmap(newMatrix, width, height, image);

            return image;
        }
    }
}
