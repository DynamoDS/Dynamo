using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

using Newtonsoft.Json;

namespace DSCore
{
    public class Color
    {
        private readonly System.Drawing.Color color;

        // Exposed only for unit test purposes.
        internal System.Drawing.Color InternalColor { get { return color; } }

        /// <summary>
        ///     Find the red component of a color, 0 to 255.
        /// </summary>
        /// <returns name="red">int between 0 and 255 inclusive.</returns>
        [JsonProperty(PropertyName = "R")]
        public byte Red
        {
            get { return color.R; }
        }

        /// <summary>
        ///     Find the green component of a color, 0 to 255.
        /// </summary>
        /// <returns name="green">int between 0 and 255 inclusive.</returns>
        [JsonProperty(PropertyName = "G")]
        public byte Green
        {
            get { return color.G; }
        }

        /// <summary>
        ///     Find the blue component of a color, 0 to 255.
        /// </summary>
        /// <returns name="blue">int between 0 and 255 inclusive.</returns>
        [JsonProperty(PropertyName = "B")]
        public byte Blue
        {
            get { return color.B; }
        }

        /// <summary>
        ///     Find the alpha component of a color, 0 to 255.
        /// </summary>
        /// <returns name="alpha">int between 0 and 255 inclusive.</returns>
        [JsonProperty(PropertyName = "A")]
        public byte Alpha
        {
            get { return color.A; }
        }

        private Color(System.Drawing.Color color)
        {
            this.color = color;
        }

        private Color(int a, int r, int g, int b)
        {
            color = System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        ///     Construct a color by alpha, red, green, and blue components.
        /// </summary>
        /// <param name="a">The alpha value.</param>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        /// <returns name="color">Color.</returns>
        /// <search>color</search>
        public static Color ByARGB(int a = 255, int r = 0, int g = 0, int b = 0)
        {
            return new Color(a, r, g, b);
        }

        internal static Color ByColor(System.Drawing.Color color)
        {
            return new Color(color);
        }

        // This fails "GraphUtilities.PreloadAssembly", fix later.
        // After fixing, restore "TestConstructorBySystemColor" test case.
        // 
#if false
        public static Color BySystemColor(System.Drawing.Color c)
        {
            return new Color(c.A, c.R, c.G, c.B);
        }
#endif

        /// <summary>
        /// Returns the brightness value for this color.
        /// </summary>
        /// <returns name="brightness">double between 0 and 1 inclusive.</returns>
        public static float Brightness(Color c)
        {
            return c.color.GetBrightness();
        }

        /// <summary>
        /// Returns the saturation value for this color.
        /// </summary>
        /// <returns name="saturation">double between 0 and 1 inclusive.</returns>
        public static float Saturation(Color c)
        {
            return c.color.GetSaturation();
        }

        /// <summary>
        /// Returns the hue value for this color.
        /// </summary>
        /// <returns name="hue">double between 0 and 1 inclusive.</returns>
        /// <search>hues</search>
        public static float Hue(Color c)
        {
            return c.color.GetHue();
        }

        /// <summary>
        ///     Lists the components for the color in the order: alpha, red, green, blue.
        /// </summary>
        /// <returns name="a">alpha value</returns>
        /// <returns name="r">red value</returns>
        /// <returns name="g">green value</returns>
        /// <returns name="b">blue value</returns>
        /// <search>alpha,red,green,blue</search>
        [MultiReturn("a", "r", "g", "b")]
        public static Dictionary<string, byte> Components(Color c)
        {
            return new Dictionary<string, byte>
            {
                {"a", c.color.A}, 
                {"r", c.color.R},
                {"g", c.color.G},
                {"b", c.color.B}, 
            };
        }

        /// <summary>
        /// Returns a color from a color gradient between a start color and an end color.
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="parameters">The values between 0 and 1 along the range for which you would like to sample the color.</param>
        /// <param name="parameter"></param>
        /// <returns name="colors">Colors in the given range.</returns>
        /// <search>color,range,gradient</search>
        [IsVisibleInDynamoLibrary(false)]
        public static Color BuildColorFrom1DRange(List<Color> colors, List<double> parameters, double parameter)
        {
            var colorRange = ColorRange1D.ByColorsAndParameters(colors, parameters);
            return ColorRange1D.GetColorAtParameter(colorRange,parameter);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Color BuildColorFrom2DRange(IList<Color> colors, IList<UV> parameters, UV parameter)
        {
            var colorRange = ColorRange.ByColorsAndParameters(colors, parameters);
            return colorRange.GetColorAtParameter(parameter);
        }

        /// <summary>
        /// Linearly interpolate between two colors.
        /// </summary>
        /// <param name="start">The start color.</param>
        /// <param name="end">The end color.</param>
        /// <param name="t">A parameter between 0.0 and 1.0.</param>
        /// <returns>The interpolated color or white.</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Color Lerp(Color start, Color end, double t)
        {
            if (start == null || 
                end == null || 
                t < 0.0 || t > 1.0)
            {
                return ByARGB(255, 255, 255, 255);
            }

            // Calculate the weighted average
            var num = new int[4];

            var d1 = 1 - t;
            var d2 = t;

            num[0] = (int)Math.Round((start.Alpha * d1)    + (end.Alpha * d2));
            num[1] = (int)Math.Round((start.Red * d1)      + (end.Red * d2));
            num[2] = (int)Math.Round((start.Green * d1)    + (end.Green * d2));
            num[3] = (int)Math.Round((start.Blue * d1)     + (end.Blue * d2));

            return ByARGB(num[0], num[1], num[2], num[3]);
        }

        /// <summary>
        /// Bilinearly interpolate between a set of colors.
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="parameter"></param>
        /// <returns>The interpolated color or white.</returns>
        /// This algorithm is not the same as the solution found from wikipedia 
        /// (reference: https://en.wikipedia.org/wiki/Bilinear_interpolation)
        [IsVisibleInDynamoLibrary(false)]
        public static Color Blerp(IList<IndexedColor2D> colors, UV parameter)
        {
            var num = new double[4];
            var totalArea = 0.0;
            foreach (var ci in colors)
            {
                var t = ci.Parameter;
                var d = Math.Sqrt(Math.Pow(t.U - parameter.U, 2) + Math.Pow(t.V - parameter.V, 2));
                if (d == 0.0)
                {
                    return ci.Color;
                }
                var w = 1 / d;

                num[0] += ci.Color.Alpha * w;
                num[1] += ci.Color.Red * w;
                num[2] += ci.Color.Green * w;
                num[3] += ci.Color.Blue * w;
                totalArea += w;
            }

            return ByARGB((int)(num[0] / totalArea),
                (int)(num[1] / totalArea),
                (int)(num[2] / totalArea),
                (int)(num[3] / totalArea));
        }



        private static double Area(UV min, UV max)
        {
            var u = System.Math.Sqrt(System.Math.Pow(max.U - min.U, 2));
            var v = System.Math.Sqrt(System.Math.Pow(max.V - min.V, 2));
            return u * v;
        }

        public override string ToString()
        {
            return string.Format("Color(Red = {0}, Green = {1}, Blue = {2}, Alpha = {3})", Red, Green, Blue, Alpha);
        }

        [IsVisibleInDynamoLibrary(false)]
        protected bool Equals(Color other)
        {
            return color.Equals(other.color);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Color)obj);
        }
        /// <summary>
        /// Construct a Color by combining two input Colors.
        /// </summary>
        /// <param name="Color 1"></param>
        /// <param name="Color 2"></param>
        /// <returns></returns>
        public static Color Add(Color c1, Color c2)
        {
            return ByARGB(
                c1.Alpha + c2.Alpha,
                c1.Red + c2.Red,
                c1.Green + c2.Green,
                c1.Blue + c2.Blue);
        }
        /// <summary>
        /// Multiply an input color with a number multiplier to produce a darker color. Input color must have an alpha less than 255.
        /// </summary>
        /// <param name="Color 1"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static Color Multiply(Color c1, double div)
        {
            return ByARGB(
                (int)(c1.Alpha * div),
                (int)(c1.Red * div),
                (int)(c1.Green * div),
                (int)(c1.Blue * div));
        }
        /// <summary>
        /// Divide an input color with a number divider to produce a brighter color and remove color tint
        /// </summary>
        /// <param name="Color 1"></param>
        /// <param name="divider"></param>
        /// <returns></returns>
        public static Color Divide(Color c1, double div)
        {
            return ByARGB(
                (int)(c1.Alpha / div),
                (int)(c1.Red / div),
                (int)(c1.Green / div),
                (int)(c1.Blue / div));
        }

        public override int GetHashCode()
        {
            return color.GetHashCode();
        }

        [IsVisibleInDynamoLibrary(false)]
        public class IndexedColor1D: IComparable
        {
            public Color Color { get; set; }
            public double Parameter { get; set; }

            public IndexedColor1D(Color color, double parameter)
            {
                Color = color;
                Parameter = parameter;
            }

            public int CompareTo(object obj)
            {
                var ic = obj as IndexedColor1D;

                if (ic == null)
                {
                    return -1;
                }

                if (ic.Parameter > Parameter)
                {
                    return -1;
                }

                if (ic.Parameter < Parameter)
                {
                    return 1;
                }

                if (ic.Parameter.Equals(Parameter))
                {
                    return 0;
                }

                return -1;
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public class IndexedColor2D
        {
            public Color Color { get; set; }
            public UV Parameter { get; set; }

            public IndexedColor2D(Color color, UV parameter)
            {
                Color = color;
                Parameter = parameter;
            }
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class ColorRange1D
    {
        private readonly IList<Color.IndexedColor1D> indexedColors;
        public IEnumerable<Color.IndexedColor1D> IndexedColors { get { return indexedColors; } }
 
        private ColorRange1D(IEnumerable<Color> colors, IEnumerable<double> parameters)
        {
            var colorMap = colors.Zip(parameters, (c, i) => new Color.IndexedColor1D(c, i)).ToList();

            // Sort the colors by index.
            indexedColors = colorMap.OrderBy(ci => ci.Parameter).ToList();
        }

        /// <summary>
        /// Create a ColorRange1D by supplying lists of colors and parameters.
        /// </summary>
        /// <param name="colors">A list of colors.</param>
        /// <param name="parameters">A list of parameters between 0.0 and 1.0.</param>
        /// <returns>A ColorRange1D object.</returns>
        public static ColorRange1D ByColorsAndParameters(
            List<Color> colors, List<double> parameters)
        {
            if (colors == null)
                colors = new List<Color>();

            colors.RemoveAll(c => c == null);

            if(parameters == null)
                parameters = new List<double>();

            // If there's no colors supplied, then supply
            // a default gradient
            if (!colors.Any())
            {
                colors = new List<Color>();
                colors.AddRange(DefaultColorRanges.Analysis);
            }

            // If there's no parameters supplied, then set the parameters
            // in an even range across the colors. 
            if (!parameters.Any())
            {
                if (colors.Any())
                {
                    var step = 1.0/(colors.Count()-1);
                    for (var i = 0; i < colors.Count(); i ++)
                    {
                        parameters.Add(i*step);
                    }
                }
            }

            // Remap the parameters into the 0->1 range
            var max = parameters.Max();
            var min = parameters.Min();
            var domain = max - min;

            if (domain == 0.0)
            {
                parameters.Clear();
                parameters.Add(0.0);
            }
            else
            {
                for (var i = 0; i < parameters.Count(); i++)
                {
                    parameters[i] = (parameters[i] - min) / domain;
                }
            }
            

            // If the number of colors is greater than the 
            // number of parameters
            if (colors.Count() > parameters.Count())
            {
                var diff = colors.Count() - parameters.Count();
                for (var i = 0; i < diff; i++)
                {
                    // Put the color in the middle
                    parameters.Add(0.5);
                }
            }
            // If the number of parameters is greater than the
            // number of colors
            else if (parameters.Count() > colors.Count())
            {
                var diff = parameters.Count() - colors.Count();
                for (var i = 0; i < diff; i++)
                {
                    colors.Add(DefaultColorRanges.Analysis.Last());
                }
            }

            return new ColorRange1D(colors, parameters);
        }

        /// <summary>
        /// Returns the color in this color range at the specified parameter.
        /// </summary>
        /// <param name="colorRange"></param>
        /// <param name="parameter">A value between 0.0 and 1.0.</param>
        /// <returns>A Color.</returns>
        public static Color GetColorAtParameter(ColorRange1D colorRange, double parameter = 0.0)
        {
            // If the supplied index matches one of the indexed colors' indices,
            // then just return that color.
            var found = colorRange.indexedColors.FirstOrDefault(ci => ci.Parameter == parameter);
            if (found != null)
            {
                return found.Color;
            }

            if (colorRange.indexedColors.Count == 1)
            {
                return colorRange.indexedColors.First().Color;
            }

            Color.IndexedColor1D c1, c2;

            c1 = colorRange.indexedColors.First();
            c2 = colorRange.indexedColors.Last();

            // Find the leading and trailing indexed color
            // between which we will linearly interpolate.
            foreach (var ci in colorRange.indexedColors)
            {
                if (ci.Parameter > c1.Parameter && ci.Parameter < parameter)
                {
                    c1 = ci;
                }

                if (ci.Parameter > parameter && ci.Parameter < c2.Parameter)
                {
                    c2 = ci;
                }
            }

            return Color.Lerp(c1.Color, c2.Color, (parameter - c1.Parameter) / (c2.Parameter - c1.Parameter));
        }

        /// <summary>
        /// Create a ColorRange1D with the default color scheme.
        /// </summary>
        /// <returns></returns>
        public static ColorRange1D Default()
        {
            return new ColorRange1D(DefaultColorRanges.Analysis, new [] {0.0,0.5,1.0});
        }

        public override string ToString()
        {
            var result = base.ToString();
            using (var sw = new StringWriter())
            {
                sw.WriteLine("Color Range:");
                foreach (var ci in indexedColors)
                {
                    sw.WriteLine(string.Format("\t{0}:{1}", ci.Color, ci.Parameter));
                }
                result = sw.ToString();
            }
            return result;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public static class DefaultColorRanges
    {
        public static readonly IEnumerable<Color> Analysis = new List<Color>
        {
            Color.ByARGB(255,255,100,100), // orange
            Color.ByARGB(255,255,255,0), // yellow
            Color.ByARGB(255,0,255,255) // cyan
        };
    }

    [IsVisibleInDynamoLibrary(true)]
    public class ColorRange
    {
        private Quadtree quadtree;
        
        private IList<Color.IndexedColor2D> indexedColors;

        private ColorRange(IEnumerable<Color> colors, IEnumerable<UV> parameters)
        {
            var parameterList = parameters as UV[] ?? parameters.ToArray();
            var colorList = colors as Color[] ?? colors.ToArray();
            indexedColors = colorList.Zip(parameterList, (c, t) => new Color.IndexedColor2D(c, t)).ToList();
        }

        /// <summary>
        /// Create a ColorRange by supplying lists of colors and UVs.
        /// </summary>
        /// <param name="colors">A list of colors.</param>
        /// <param name="parameters">A list of parameters between (0.0,0.0) and (1.0,1.0).</param>
        /// <returns>A ColorRange object.</returns>
        public static ColorRange ByColorsAndParameters(
            IList<Color> colors, IList<UV> parameters)
        {
            return new ColorRange(colors, parameters);
        }

        /// <summary>
        /// Returns the color in this color range at the specified parameter.
        /// </summary>
        /// <param name="parameter">A UV between (0.0,0.0) and (1.0,1.0).</param>
        /// <returns>A Color.</returns>
        public Color GetColorAtParameter(UV parameter)
        {
            var color = Color.ByARGB(255, 255, 255, 255);

            var weightedColors = indexedColors.ToList()
                .OrderBy(ic => ic.Parameter.Area(parameter)).Take(4).ToList();

            color = Color.Blerp(weightedColors, parameter);

            return color;
        }
    }
}