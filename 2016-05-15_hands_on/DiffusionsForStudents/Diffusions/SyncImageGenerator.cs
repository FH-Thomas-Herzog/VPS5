using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Diffusions
{
    /// <summary>
    /// The syncchronous version of the image generator.
    /// </summary>
    public class SyncImageGenerator : ImageGenerator
    {
        public override Bitmap GenerateBitmap(Area area)
        {
            var matrix = area.Matrix;
            int height = area.Height;
            int width = area.Width;

            var newMatrix = new double[width, height];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
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
                    // break inner loop
                    if (stopRequested) { break; }
                }
                // break outer loop
                if (stopRequested) { break; }
            }

            if (stopRequested) { return null; }

            // If stop request occurs here, let finish the image generation
            area.Matrix = newMatrix;
            Bitmap image = new Bitmap(width, height);
            ColorBitmap(newMatrix, width, height, image);

            return image;
        }
    }
}
