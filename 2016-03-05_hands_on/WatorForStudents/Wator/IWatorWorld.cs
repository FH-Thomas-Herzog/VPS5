using System.Drawing;

namespace VSS.Wator {
  // interface for all implementations of the wator world simulator
  public interface IWatorWorld {
    void ExecuteStep();
    Bitmap GenerateImage();
  }
}
