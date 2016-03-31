using System.Drawing;
using VSS.Wator.Original;

namespace VSS.Wator
{
    // interface for all implementations of the wator world simulator
    public interface IWatorWorld
    {
        MatrixItem[,] Grid { get; }
        int InitialFishEnergy { get; }
        int InitialSharkEnergy { get; }
        int FishBreedTime { get; }
        int SharkBreedEnergy { get; }
        void ExecuteStep();
        Bitmap GenerateImage();
        void SelectNeighbor(MatrixItemType type, int x, int y, out int outX, out int outY);
    }
}
