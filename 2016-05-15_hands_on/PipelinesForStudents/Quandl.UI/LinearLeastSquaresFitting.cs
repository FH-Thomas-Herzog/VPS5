
using System.Linq;

namespace Quandl.UI
{
    public class LinearLeastSquaresFitting
    {
        public static void Calculate(double[] dataPoints, out double a1, out double a0)
        {
            double sxy = 0.0;
            double sxx = 0.0;
            int n = dataPoints.Count();
            double sy = dataPoints.Sum();
            double sx = ((n - 1) * n) / 2;
            double avgy = sy / n;
            double avgx = sx / n;

            for (int i = 0; i < n; i++)
            {
                sxy += i * dataPoints[i];
                sxx += i * i;
            }

            a1 = (sxy - (n * avgx * avgy)) / (sxx - (n * avgx * avgx));
            a0 = avgy - a1 * avgx;
        }
    }
}
