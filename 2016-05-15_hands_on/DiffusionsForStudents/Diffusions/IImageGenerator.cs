using System;
using System.Drawing;

namespace Diffusions {
  public interface IImageGenerator {
    void GenerateImage(Area area);

    event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;

    void Stop();

    bool Finished { get; }

    bool StopRequested { get; }
  }
}
