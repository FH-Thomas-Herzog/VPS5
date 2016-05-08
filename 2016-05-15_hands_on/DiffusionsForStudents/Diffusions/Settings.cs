using System.ComponentModel;

namespace Diffusions {
  [DefaultPropertyAttribute("MinReal")]
  public class Settings {
    public static Settings defaultSettings = new Settings();
    public static Settings DefaultSettings {
      get { return defaultSettings; }
    }

    #region Generator Settings
    private int maxIterations;
    [CategoryAttribute("Generator Settings"),
     DescriptionAttribute("Maximum number of iterations")]
    public int MaxIterations {
      get { return maxIterations; }
      set { if (value > 0) maxIterations = value; }
    }

    private int displayInterval;
    [CategoryAttribute("Generator Settings"),
     DescriptionAttribute("Display Interval")]
    public int DisplayInterval {
      get { return displayInterval; }
      set { if (value > 0) displayInterval = value; }
    }
    #endregion

    #region Parallelization Settings
    private int workers;
    [CategoryAttribute("Parallelization Settings"),
     DescriptionAttribute("Number of worker threads")]
    public int Workers {
      get { return workers; }
      set { if (value > 0) workers = value; }
    }
    #endregion

    public Settings() {
      maxIterations = 10000;
      workers = 1;
      displayInterval = 10;
    }
  }
}
