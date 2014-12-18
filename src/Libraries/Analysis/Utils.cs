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
            var colors = new[] { c1, c2, c3 };
            var parameters = new[] { 0.0, 0.5, 1.0 };
            var colorRange = ColorRange1D.ByColorsAndParameters(colors, parameters);
            return colorRange;
        }
    }
}
