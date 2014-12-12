using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

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
        /// <returns name="val">Value of the red component.</returns>
        public byte Red
        {
            get { return color.R; }
        }

        /// <summary>
        ///     Find the green component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the green component.</returns>
        public byte Green
        {
            get { return color.G; }
        }

        /// <summary>
        ///     Find the blue component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the blue component.</returns>
        public byte Blue
        {
            get { return color.B; }
        }

        /// <summary>
        ///     Find the alpha component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the alpha component.</returns>
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
        ///     Gets the brightness value for this color.
        /// </summary>
        /// <returns name="val">Brightness value for the color.</returns>
        public static float Brightness(Color c)
        {
            return c.color.GetBrightness();
        }

        /// <summary>
        ///     Gets the saturation value for this color.
        /// </summary>
        /// <returns name="val">Saturation value for the color.</returns>
        public static float Saturation(Color c)
        {
            return c.color.GetSaturation();
        }

        /// <summary>
        ///     Gets the hue value for this color.
        /// </summary>
        /// <returns name="val">Hue value for the color.</returns>
        public static float Hue(Color c)
        {
            return c.color.GetHue();
        }

        /// <summary>
        ///     Lists the components for the color in the order: alpha, red, green, blue.
        /// </summary>
        /// <returns name="val">Saturation value for the color.</returns>
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
        /// Get a color from a color gradient between a start color and an end color.
        /// </summary>
        /// <param name="start">The starting color of the range.</param>
        /// <param name="end">The end color of the range.</param>
        /// <param name="index">The value between 0 and 1 along the range for which you would like to sample the color.</param>
        /// <returns name="color">Color in the given range.</returns>
        /// <search>color,range,gradient</search>
        [IsVisibleInDynamoLibrary(false)]
        public static Color BuildColorFrom1DRange(IList<Color> colors, IList<double> indices, double index)
        {
            // This method is called from AST. We cannot throw an exception.
            // If any of the bounding conditions are not met,
            // just return white.

            if (!colors.Any() || 
                !indices.Any() ||
                colors.Count != indices.Count ||
                index < 0.0 || 
                index > 1.0)
            {
                return Color.ByARGB(255, 255, 255, 255);
            }

            var indexedColors = colors.Zip(indices, (c, i) => new IndexedColor1D(c, i, Math.Sqrt(Math.Pow(index - i, 2)))).ToList();
            var colorsByIndex = indexedColors.OrderBy(ci => ci.Index);

            // If the supplied index matches one of the indexed colors' indices,
            // then just return that color.
            var found = colorsByIndex.FirstOrDefault(ci => ci.Index == index);
            if (found != null)
            {
                return found.Color;
            }

            // If values are not supplied for the 0.0 and 1.0
            // positions, then use the bottom and top colors
            if (!indices.Any(i => i == 0.0))
            {
                indexedColors.Insert(0,new IndexedColor1D(colors.First(), 0.0, index));
            }

            if (!indices.Any(i => i == 1.0))
            {
                indexedColors.Add(new IndexedColor1D(colors.Last(), 1.0, 1.0 - index));
            }

            IndexedColor1D c1, c2;

            c1 = colorsByIndex.First();
            c2 = colorsByIndex.Last();

            // Find the leading and trailing indexed color
            // between which we will linearly interpolate.
            foreach (var ci in colorsByIndex)
            {
                if (ci.Index > c1.Index && ci.Index < index)
                {
                    c1 = ci;
                }

                if (ci.Index > index && ci.Index < c2.Index)
                {
                    c2 = ci;
                }
            }

            return Lerp(c1.Color, c2.Color, (index - c1.Index)/(c2.Index - c1.Index));
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="indices"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Color BuildColorFrom2DRange(IList<Color> colors, IList<UV> indices, UV index)
        {
            if (!colors.Any() ||
                !indices.Any() ||
                colors.Count != indices.Count)
            {
                return Color.ByARGB(255, 255, 255, 255);
            }

            var indexPt = Point.ByCoordinates(index.U, index.V);
            var indexedColors = colors.Zip(indices, (c, i) => new IndexedColor2D(c, i, Area(index,i))).ToList();

            IndexedColor2D ul = null;
            IndexedColor2D ur = null;
            IndexedColor2D lr = null;
            IndexedColor2D ll  = null;

            // If values are not supplied for the range corners,
            // then use the closest colors.
            if (!indices.Any(uv => uv.U == 0.0 && uv.V == 0.0))
            {
                ul = CreateIndexedColor(index, indexedColors, 0, 0);
                indexedColors.Add(ul);
            }
            else
            {
                ul = indexedColors.First(ci => ci.Index.U == 0.0 && ci.Index.V == 0.0);
            }

            if (!indices.Any(uv => uv.U == 1.0 && uv.V == 0.0))
            {
                ur = CreateIndexedColor(index, indexedColors, 1, 0);
                indexedColors.Add(ur);
            }
            else
            {
                ur = indexedColors.First(ci => ci.Index.U == 1.0 && ci.Index.V == 0.0);
            }

            if (!indices.Any(uv => uv.U == 1.0 && uv.V == 1.0))
            {
                lr = CreateIndexedColor(index, indexedColors, 1, 1);
                indexedColors.Add(lr);
            }
            else
            {
                lr = indexedColors.First(ci => ci.Index.U == 1.0 && ci.Index.V == 1.0);
            }

            if (!indices.Any(uv => uv.U == 0.0 && uv.V == 1.0))
            {
                ll = CreateIndexedColor(index, indexedColors, 0, 1);
                indexedColors.Add(ll);
            }
            else
            {
                ll = indexedColors.First(ci => ci.Index.U == 0.0 && ci.Index.V == 1.0);
            }

            // If one of the supplied indices matches 
            // the index exactly, return that color.
            var found =
                indexedColors.FirstOrDefault(ci => Point.ByCoordinates(ci.Index.U, ci.Index.V).IsAlmostEqualTo(indexPt));
            if (found != null)
            {
                return found.Color;
            }

            foreach (var ci in indexedColors)
            {
                var uv = ci.Index;

                if (uv.U < index.U && uv.V < index.V)
                {
                    // ul candidate
                    if (uv.U > ul.Index.U && uv.V > ul.Index.V)
                    {
                        ul = ci;
                    }
                }
                if (uv.U > index.U && uv.V < index.V)
                {
                    // ur candidate
                    if (uv.U < ur.Index.U && uv.V > ur.Index.V)
                    {
                        ur = ci;
                    }
                }
                if (uv.U > index.U && uv.V > index.V)
                {
                    // lr candidate
                    if (uv.U < lr.Index.U && uv.V < lr.Index.V)
                    {
                        lr = ci;
                    }
                }
                if (uv.U < index.U && uv.V > index.V)
                {
                    // ll candidate
                    if (uv.U > ll.Index.U && uv.V < ll.Index.V)
                    {
                        ll = ci;
                    }
                }
            }

            return Blerp(new List<IndexedColor2D>() { ul, ur, lr, ll });

        }

        private static double Area(UV min, UV max)
        {
            var u = System.Math.Sqrt(System.Math.Pow(max.U - min.U, 2));
            var v = System.Math.Sqrt(System.Math.Pow(max.V - min.V, 2));
            return u*v;
        }

        private static IndexedColor2D CreateIndexedColor(UV index, List<IndexedColor2D> indexedColors, double u, double v)
        {
            var uv = UV.ByCoordinates(u, v);

            // Find the indexed color which as the smallest area relative
            // to the UV input. This is the "closest" UV.
            var ord = indexedColors.OrderBy(ci => Area(uv,ci.Index));
            
            return new IndexedColor2D(ord.First().Color, uv, Area(index,uv));
        }

        /// <summary>
        /// Linearly interpolate between two colors.
        /// </summary>
        /// <param name="start">The start color.</param>
        /// <param name="end">The end color.</param>
        /// <param name="t">A parameter between 0.0 and 1.0.</param>
        /// <returns>The interpolated color or white.</returns>
        internal static Color Lerp(Color start, Color end, double t)
        {
            if (start == null || 
                end == null || 
                t < 0.0 || t > 1.0)
            {
                return Color.ByARGB(255, 255, 255, 255);
            }

            // Calculate the weighted average
            var num = new double[4];

            var d1 = 1 - Math.Sqrt(Math.Pow(t - 0.0, 2));
            var d2 = 1 - Math.Sqrt(Math.Pow(t - 1.0, 2));

            num[0] += (start.Alpha * d1)    + (end.Alpha * d2);
            num[1] += (start.Red * d1)      + (end.Red * d2);
            num[2] += (start.Green * d1)    + (end.Green * d2);
            num[3] += (start.Blue * d1)     + (end.Blue * d2);

            return ByARGB(255,
                (int)(num[1] / (d1 + d2)),
                (int)(num[2] / (d1 + d2)),
                (int)(num[3] / (d1 + d2)));
        }

        /// <summary>
        /// Bilinearly interpolate between four colors.
        /// </summary>
        /// <param name="start">The start color.</param>
        /// <param name="end">The end color.</param>
        /// <param name="t">A parameter between 0.0 and 1.0.</param>
        /// <returns>The interpolated color or white.</returns>
        internal static Color Blerp(List<IndexedColor2D> colors)
        {
            // Calculate the weighted average
            var num = new double[4];
            var totalArea = 0.0;
            foreach (var ci in colors)
            {
                var a = 1 - ci.Area;
                num[0] += ci.Color.Alpha * a;
                num[1] += ci.Color.Red * a;
                num[2] += ci.Color.Green * a;
                num[3] += ci.Color.Blue * a;
                totalArea += a;
            }

            return ByARGB(255,
                (int)(num[1] / totalArea),
                (int)(num[2] / totalArea),
                (int)(num[3] / totalArea));
        }

        public override string ToString()
        {
            return string.Format("Color: Red={0}, Green={1}, Blue={2}, Alpha={3}", Red, Green, Blue, Alpha);
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

        public static Color Add(DSCore.Color c1, DSCore.Color c2)
        {
            return Color.ByARGB(
                c1.Alpha + c2.Alpha,
                c1.Red + c2.Red,
                c1.Green + c2.Green,
                c1.Blue + c2.Blue);
        }

        public static Color Multiply(DSCore.Color c1, double div)
        {
            return Color.ByARGB(
                (int)(c1.Alpha * div),
                (int)(c1.Red * div),
                (int)(c1.Green * div),
                (int)(c1.Blue * div));
        }

        public static Color Divide(DSCore.Color c1, double div)
        {
            return Color.ByARGB(
                (int)(c1.Alpha / div),
                (int)(c1.Red / div),
                (int)(c1.Green / div),
                (int)(c1.Blue / div));
        }

        public override int GetHashCode()
        {
            return color.GetHashCode();
        }

        internal abstract class IndexedColorBase
        {
            public Color Color { get; set; }
            
        }

        internal class IndexedColor1D: IndexedColorBase, IComparable
        {
            public double Index { get; set; }
            public double Distance { get; set; }

            internal IndexedColor1D(Color color, double index, double distance)
            {
                Color = color;
                Index = index;
                Distance = distance;
            }

            public int CompareTo(object obj)
            {
                var ic = obj as IndexedColor1D;

                if (ic == null)
                {
                    return -1;
                }

                if (ic.Index > Index)
                {
                    return -1;
                }

                if (ic.Index < Index)
                {
                    return 1;
                }

                if (ic.Index.Equals(Index))
                {
                    return 0;
                }

                return -1;
            }
        }

        internal class IndexedColor2D : IndexedColorBase
        {
            public UV Index { get; set; }
            public double Area { get; set; }

            internal IndexedColor2D(Color color, UV index, double area)
            {
                Color = color;
                Index = index;
                Area = area;
            }
        }
    }

    // Extension methods for the color class to allow the 
    // averaging of colors
    public static class ColorExtensions
    {
        
    }

}