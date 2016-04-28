using System;
using System.Drawing;

namespace MandelbrotGenerator
{
    public interface IImageGenerator
    {
        /// <summary>
        /// Notifies the consumer via an event
        /// </summary>
        /// <param name="startIdx">the starting index. Must be smaller as area.Width</param>
        /// <param name="endIndex">the ending index</param>
        /// <param name="area">the area the pixel is placed on</param>
        /// <param name="cancel">function which provides cancelation flag</param>
        /// <returns>the generated bitmap</returns>
        Bitmap GenerateImage(int startIdx, int endIndex, Area area, Func<bool> cancel);
    }
}
