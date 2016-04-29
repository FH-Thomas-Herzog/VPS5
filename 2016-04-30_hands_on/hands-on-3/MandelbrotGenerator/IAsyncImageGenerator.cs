using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MandelbrotGenerator
{
    /// <summary>
    /// Interfaces for asynchronous invocation.
    /// </summary>
    interface IAsyncImageGenerator : IImageGenerator
    {
        /// <summary>
        /// Generates the image asynchronously.
        /// </summary>
        /// <param name="area">the area to generate image for</param>
        void GenerateAsync(Area area);

        /// <summary>
        /// Aborts the running generation
        /// </summary>
        void Abort();

        /// <summary>
        /// Event which gets called when generation completed
        /// </summary>
        event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> OnCompleted;
    }

}
