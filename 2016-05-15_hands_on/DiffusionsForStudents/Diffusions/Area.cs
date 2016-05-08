
namespace Diffusions {
  public class Area {
    private int width;
    public int Width { get { return width; } set { if (value > 0) width = value; } }
    private int height;
    public int Height { get { return height; } set { if (value > 0) height = value; } }
    public double[,] Matrix { get; set; }

    public Area(int width, int height) {
      Matrix = new double[width, height];
      Width = width;
      Height = height;
    }

    public Area(int width, int height, double[,] matrix) {
      Width = width;
      Height = height;
      Matrix = matrix;
    }
  }
}
