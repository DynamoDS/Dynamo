using System.Collections.Generic;
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
        ///     Get a color from a color gradient between a start color and an end color.
        /// </summary>
        /// <param name="start">The starting color of the range.</param>
        /// <param name="end">The end color of the range.</param>
        /// <param name="value">The value between 0 and 1 along the range for which you would like to sample the color.</param>
        /// <returns name="color">Color in the given range.</returns>
        /// <search>color,range,gradient</search>
        [IsVisibleInDynamoLibrary(false)]
        public static Color BuildColorFromRange(Color start, Color end, double value)
        {
            var selRed = (int)(start.Red + (end.Red - start.Red) * value);
            var selGreen = (int)(start.Green + (end.Green - start.Green) * value);
            var selBlue = (int)(start.Blue + (end.Blue - start.Blue) * value);

            return ByARGB(255, selRed, selGreen, selBlue);
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

        public override int GetHashCode()
        {
            return color.GetHashCode();
        }
    }

}