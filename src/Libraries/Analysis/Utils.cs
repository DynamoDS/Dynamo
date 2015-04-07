using System.Collections.Generic;

using DSCore;

namespace Analysis
{
    internal static class Utils
    {
        internal static ColorRange1D CreateAnalyticalColorRange()
        {
            // Build some colors for a range
            var c1 = Color.ByARGB(255, 255, 0, 0);
            var c2 = Color.ByARGB(255, 255, 255, 0);
            var c3 = Color.ByARGB(255, 0, 0, 255);
            var colors = new List<Color>(){ c1, c2, c3 };
            var parameters = new List<double>(){ 0.0, 0.5, 1.0 };
            var colorRange = ColorRange1D.ByColorsAndParameters(colors, parameters);
            return colorRange;
        }
    }
}
