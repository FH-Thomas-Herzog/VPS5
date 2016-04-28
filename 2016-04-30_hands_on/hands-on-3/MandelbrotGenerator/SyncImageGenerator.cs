using System;
using System.Drawing;

namespace MandelbrotGenerator
{
    public class SyncImageGenerator : IImageGenerator
    {

        public Bitmap GenerateImage(int startIdx, int endIndex, Area area, Func<bool> cancel)
        {
            if ((startIdx > endIndex) || (startIdx < 0))
            {
                throw new ArgumentException("Starting index must not be greather than the set endIndex");
            }
            cancel = cancel ?? (() => false);
            int maxIterations;
            double zBorder;
            double cReal, cImg, zReal, zImg, zNewReal, zNewImg;

            maxIterations = Settings.DefaultSettings.MaxIterations;
            zBorder = Settings.DefaultSettings.ZBorder * Settings.DefaultSettings.ZBorder;

            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            //insert code
            for (int i = startIdx; i < endIndex; i++)
            {
                // check for cancelation
                if (cancel()) { break; }

                for (int j = 0; j < area.Height; j++)
                {
                    // check for cancelation
                    if (cancel()) { break; }

                    cReal = area.MinReal + i * area.PixelWidth;
                    cImg = area.MinImg + j * area.PixelHeight;
                    zReal = 0;
                    zImg = 0;
                    int k = 0;
                    while ((zReal * zReal + zImg * zImg < zBorder) && k < maxIterations)
                    {
                        // check for cancelation
                        if (cancel()) { break; }

                        zNewReal = zReal * zReal - zImg * zImg + cReal;
                        zNewImg = 2 * zReal * zImg + cImg;

                        zReal = zNewReal;
                        zImg = zNewImg;

                        k++;
                    }
                    bitmap.SetPixel(i, j, ColorSchema.GetColor(k));
                }
            }

            //end insert

            return bitmap;
        }

    }
}
