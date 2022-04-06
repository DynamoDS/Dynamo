using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DSCore;
using Math = System.Math;

namespace Analysis
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Utils
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

        public static double[][] CreateGradientValueMap(IEnumerable<double> values, IEnumerable<UV> locations, int uSamples, int vSamples)
        {
            if (!values.Any())
            {
                return new double[0][];
            }

            // Zip the values and locations together to create an
            // enumerable of ValueLocation objects.
            var zip = locations.Zip(values, (uv, value) => new ValueLocation<double>(value, uv));

            // The size of the map will be +1 in both directions
            // than the number of samples, to ensure that we are
            // sampling all the way to the edges.

            var w = uSamples + 1;
            var h = vSamples + 1;

            var valueMap = new double[w][];
            for (var i = 0; i < w; i++)
            {
                valueMap[i] = new double[h];
            }

            Parallel.For(0,w,
                         u => Parallel.For(
                             0,
                             h,
                             v =>
                             {
                                 var uv = UV.ByCoordinates((double)u / uSamples, (double)v / vSamples);
                                 valueMap[v][u] = Interpolate(zip, uv);
                             }));
            return valueMap;
        }

        public static Color[][] CreateGradientColorMap(Color[] colors, IEnumerable<UV> locations, int uSamples, int vSamples)
        {
            if (!colors.Any())
            {
                return new Color[0][];
            }

            // Zip the values and locations together to create an
            // enumerable of ValueLocation objects.
            var zip = locations.Zip(colors, (uv, value) => new ValueLocation<Color>(value, uv));

            // The size of the map will be +1 in both directions
            // than the number of samples, to ensure that we are
            // sampling all the way to the edges.

            var w = uSamples;
            var h = vSamples;

            var valueMap = new Color[w][];
            for (var i = 0; i < w; i++)
            {
                valueMap[i] = new Color[h];
            }

            Parallel.For(0, w,
                         u => Parallel.For(
                             0,
                             h,
                             v =>
                             {
                                 var uv = UV.ByCoordinates((double)u / uSamples, (double)v / vSamples);
                                 valueMap[v][u] = Interpolate(zip, uv);
                             }));
            return valueMap;
        }

        private static double Interpolate(IEnumerable<ValueLocation<double>> locations, UV p)
        {
            var num = 0.0;
            var totalArea = 0.0;

            foreach (var loc in locations)
            {
                var t = loc.Location;

                // Use the sum of the distances squared between components. No need to use the
                // square root as the relative weight will be the same.
                var d = Math.Pow(t.U - p.U, 2) + Math.Pow(t.V - p.V, 2);

                if (d == 0.0)
                {
                    return loc.Value;
                }
                var w = 1 / d;

                num += loc.Value * w;
                totalArea += w;
            }

            return num / totalArea;
        }

        private static Color Interpolate(IEnumerable<ValueLocation<Color>> locations, UV p)
        {
            var num = new double[4];
            var totalArea = 0.0;

            //var sw = new Stopwatch();
            //sw.Start();

            var locationArr = locations.ToArray();

            // Find uvs in the locations set that are within a certain domain
            // around p. Start with something small like +-0.1. If you don't
            // find enough sample points, keep increasing the size of the search
            // domain until you do.

            ValueLocation<Color>[] searchLocations = null;
            var s = 0.025;
            const double step = 0.025;
            while (s <= 1.0)
            {
                searchLocations = locationArr.Where(
                    l => l.Location.U < p.U + s &&
                         l.Location.U > p.U - s &&
                         l.Location.V < p.V + s &&
                         l.Location.V > p.V - s).ToArray();

                if (searchLocations.Count() >= 5)
                {
                    break;
                }
                s += step;
            }

            foreach (var loc in searchLocations)
            {
                var t = loc.Location;
                var d = DSCore.Math.Pow(t.U - p.U, 2) + DSCore.Math.Pow(t.V - p.V, 2);
                if (d == 0.0)
                {
                    return loc.Value;
                }

                // 1/d^2 creates a suitable fall-off. 
                // The higher the exponent, the more more pixelated the result.

                var w = 1 / Math.Pow(d,2);

                num[0] += loc.Value.Alpha * w;
                num[1] += loc.Value.Red * w;
                num[2] += loc.Value.Green * w;
                num[3] += loc.Value.Blue * w;
                totalArea += w;
            }

            //sw.Stop();
            //Debug.WriteLine("{0} elapsed for interpolation at {1}.", sw.Elapsed, p);

            return Color.ByARGB((int)(num[0] / totalArea),
                (int)(num[1] / totalArea),
                (int)(num[2] / totalArea),
                (int)(num[3] / totalArea));
        }

        /// <summary>
        /// A class for storing a value at a location.
        /// </summary>
        private class ValueLocation<TValue>
        {
            public TValue Value { get; set; }
            public UV Location { get; set; }

            public ValueLocation(TValue value, UV location)
            {
                Value = value;
                Location = location;
            }
        }
    }
}
