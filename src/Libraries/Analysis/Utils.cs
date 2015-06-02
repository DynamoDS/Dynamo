using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using DSCore;
using Math = System.Math;

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

        /// <summary>
        /// Create a gradient map of values on the surface. The gradient is
        /// defined by computing the weighted average of values on the surface
        /// at parameters whose locations are specified by the uSamples and vSamples 
        /// parameters.
        /// </summary>
        /// <param name="uSamples">The number of samples evenly spaced in the U direction from 0->1</param>
        /// <param name="vSamples">The number of samples evenly spaced in the V direction from 0->1</param>
        /// <returns></returns>
        public static double[][] CreateGradientValueMap(IEnumerable<double> values, IEnumerable<UV> locations, int uSamples, int vSamples)
        {
            if (!values.Any())
            {
                return new double[0][];
            }

            var min = values.Min();
            var max = values.Max();

            var domain = max - min;

            if (min == max)
            {
                domain = Math.Abs(max);
            }

            // Normalize the values in the 0->1 range
            var normalizedValues = values.Select(v => (v - min) / domain).ToList();

            // Zip the values and locations together to create an
            // enumerable of ValueLocation objects.
            var zip = locations.Zip(normalizedValues, (uv, value) => new ValueLocation(value, uv));

            var valueMap = new double[uSamples][];
            for (var i = 0; i < valueMap.Length; i++)
            {
                valueMap[i] = new double[vSamples];
            }

            Parallel.For(0, uSamples,
                         w => Parallel.For(
                             0,
                             vSamples,
                             h =>
                             {
                                 var uv = UV.ByCoordinates((double)w / uSamples, (double)h / vSamples);
                                 valueMap[w][h] = Interpolate(zip, uv);
                             }));
            return valueMap;
        }

        private static double Interpolate(IEnumerable<ValueLocation> locations, UV p)
        {
            var num = 0.0;
            var totalArea = 0.0;
            foreach (var loc in locations)
            {
                var t = loc.Location;
                var d = System.Math.Sqrt(System.Math.Pow(t.U - p.U, 2) + Math.Pow(t.V - p.V, 2));
                if (d == 0.0)
                {
                    return loc.Value;
                }
                var w = 1 / d;

                num += w;
                totalArea += w;
            }

            return num / totalArea;
        }

        /// <summary>
        /// A class for storing a value at a location.
        /// </summary>
        private class ValueLocation
        {
            public double Value { get; set; }
            public UV Location { get; set; }

            public ValueLocation(double value, UV location)
            {
                Value = value;
                Location = location;
            }
        }
    }
}
