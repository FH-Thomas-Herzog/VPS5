using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Diffusions {
  public static class ColorSchema {
    private const int shades = 100;

    private static List<Color> colors = new List<Color>();
    private static ReadOnlyCollection<Color> readOnlyColors;
    public static IList<Color> Colors {
      get {
        if (readOnlyColors == null) readOnlyColors = colors.AsReadOnly();
        return readOnlyColors;
      }
    }

    static ColorSchema() {
      int stepWidth = (256 * 4) / shades;
      int currentValue;
      int currentClass;
      for (int i = 0; i < shades; i++) {
        currentValue = (i * stepWidth) % 256;
        currentClass = (i * stepWidth) / 256;
        switch (currentClass) {
          case 0: { colors.Add(Color.FromArgb(0, currentValue, 255)); break; }        // blue -> cyan
          case 1: { colors.Add(Color.FromArgb(0, 255, 255 - currentValue)); break; }  // cyan -> green
          case 2: { colors.Add(Color.FromArgb(currentValue, 255, 0)); break; }        // green -> yellow
          case 3: { colors.Add(Color.FromArgb(255, 255 - currentValue, 0)); break; }  // yellow -> red
        }
      }
    }

    public static Color GetColor(double iterations) {
      double red = (iterations % 32) * 3;
      if (red > 255)
        red = 255;

      double green = (iterations % 16) * 2;
      if (green > 255)
        green = 255;

      double blue = (iterations % 128) * 14;
      if (blue > 255)
        blue = 255;

      return Color.FromArgb((int)red, (int)green, (int)blue);
    }
  }
}
