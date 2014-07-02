using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Dynamo.Bloodstone
{
    class Utilities
    {
        /// <summary>
        /// Call this method to convert from HSV color space to RGB color space.
        /// Implementation is derived from this article on Wikipedia:
        /// 
        ///     http://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
        /// 
        /// </summary>
        /// <param name="hue">The hue value that has a range [0, 360)</param>
        /// <param name="saturation">The saturation value from 0.0 to 1.0</param>
        /// <param name="value">The value ranging from 0.0 to 1.0</param>
        /// <returns>Returns the resulting RGB value in Color</returns>
        /// 
        internal static Color HsvToRgb(double hue, double saturation, double value)
        {
            if (value <= 0.0)
                return Color.FromArgb(255, 0, 0, 0);
            if (saturation <= 0.0)
                return FromDoubles(value, value, value);

            while (hue < 0.0) hue += 360.0;
            while (hue >= 360.0) hue -= 360.0;

            double fraction = hue / 60.0;
            int dominant = (int)Math.Floor(fraction);

            double f = fraction - dominant;
            double pv = value * (1.0 - saturation);
            double qv = value * (1.0 - saturation * f);
            double tv = value * (1.0 - saturation * (1.0 - f));

            double red = 0.0, green = 0.0, blue = 0.0;
            switch (dominant)
            {
                case 0: red = value; green = tv; blue = pv; break;
                case 1: red = qv; green = value; blue = pv; break;
                case 2: red = pv; green = value; blue = tv; break;
                case 3: red = pv; green = qv; blue = value; break;
                case 4: red = tv; green = pv; blue = value; break;
                case 5: red = value; green = pv; blue = qv; break;
                case 6: red = value; green = tv; blue = pv; break;
            }

            return FromDoubles(red, green, blue);
        }

        /// <summary>
        /// Call this method to convert from double values to Color value.
        /// </summary>
        /// <param name="rv">Red component ranging from 0.0 to 1.0</param>
        /// <param name="gv">Green component ranging from 0.0 to 1.0</param>
        /// <param name="bv">Blue component ranging from 0.0 to 1.0</param>
        /// <returns>Returns the resulting RGB value in Color</returns>
        /// 
        internal static Color FromDoubles(double rv, double gv, double bv)
        {
            int r = ((int)(rv * 255.0));
            int g = ((int)(gv * 255.0));
            int b = ((int)(bv * 255.0));

            r = ((((r < 0) ? 0 : r) > 255) ? 255 : r);
            g = ((((g < 0) ? 0 : g) > 255) ? 255 : g);
            b = ((((b < 0) ? 0 : b) > 255) ? 255 : b);
            return Color.FromArgb(255, ((byte)r), ((byte)g), ((byte)b));
        }

        /// <summary>
        /// Call this method to create an inversed color of the original.
        /// </summary>
        /// <param name="color">The color to inverse</param>
        /// <returns>The inversed color</returns>
        /// 
        internal static Color Inverse(Color color)
        {
            int r = color.R ^ 0xff;
            int g = color.G ^ 0xff;
            int b = color.B ^ 0xff;
            return Color.FromArgb(255, ((byte)r), ((byte)g), ((byte)b));
        }
    }
}
