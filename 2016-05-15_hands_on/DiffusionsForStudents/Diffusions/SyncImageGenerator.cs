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
                    // Calculate the matrix
                    CalculateMatrix(i, j, height, width, matrix, newMatrix);

                    // break inner loop
                    if (stopRequested) { break; }
                }
                // break outer loop
                if (stopRequested) { break; }
            }

            // null because image could be broken
            if (stopRequested) { return null; }

            // If stop request occurs here, let finish the image generation
            area.Matrix = newMatrix;
            Bitmap image = new Bitmap(width, height);
            ColorBitmap(newMatrix, width, height, image);

            return image;
        }
    }
}
