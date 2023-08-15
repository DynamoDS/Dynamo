using System;
using System.Collections.Generic;
using System.Windows.Media;
using Dynamo.UI;
using SkiaSharp;

namespace CoreNodeModelsWpf.Charts.Utilities
{
    /// <summary>
    /// Helper class providing chart style parameters
    /// </summary>
    public static class ChartStyle
    {
        private static Random rnd = new Random();
        private static readonly SKColor defaultColor = SKColors.LightGray;

        //Constants for Chart Styling
        internal static double COLUMN_GROUP_PADDING = 4.0;
        internal static readonly double COLUMN_RADIUS = 0;
        internal static double MAX_COLUMN_WIDTH = 20.0;
        internal static readonly double COLUMN_AXIS_MIN_STEP = 1;

        internal static readonly double LINE_AXIS_MIN_STEP = 3;
        internal static readonly float LINE_STROKE_THICKNESS = 1.5f;
        internal static readonly float XY_LINE_STROKE_THICKNESS = 2.0f;

        internal static readonly float PIE_LABEL_TEXT_SIZE = 12;

        internal static readonly double CHART_MIN_WIDTH = 300;
        internal static readonly double CHART_MIN_HEIGHT = 300;
        internal static readonly double AXIS_FONT_SIZE = 12;
        internal static readonly string AXIS_FONT_FAMILY = "Artifakt Element";
        internal static readonly double AXIS_NAME_PADDING = 0.5;
        internal static readonly double AXIS_LABEL_PADDING = 0.5;
        internal static readonly float AXIS_STROKE_THICKNESS = 0.5f;
        internal static readonly double AXIS_MIN_STEP = 1;

        internal static SKColor AXIS_COLOR = HexToSKColor("#989898");
        internal static SKColor AXIS_SEPARATOR_COLOR = HexToSKColor("#666666");
        internal static SKColor LEGEND_TEXT_COLOR = HexToSKColor("#DCDCDC");
        internal static SKColor HEATMAP_DEFAULT_START_COLOR = HexToSKColor("#FFFFFF");

        /// <summary>
        /// Method to convert HEX color to Skia color, if no color is provided
        /// it will return the default color
        /// </summary>
        /// <param name="hex">Hex code of the color to be converted</param>
        /// <returns></returns>
        private static SKColor HexToSKColor(string hex)
        {
            SKColor skc;
            if (!string.IsNullOrWhiteSpace(hex) && SKColor.TryParse(hex, out skc))
            {
                return skc;
            }
            
            return defaultColor;
        }

        /// <summary>
        /// Returns a list of random doubles of the given size, with a max limit
        /// </summary>
        /// <param name="size">Size of the random double's list</param>
        /// <param name="max">Max limit of random numbers</param>
        /// <returns></returns>
        internal static IEnumerable<double> GetRandomList(int size, int max = 30)
        {
            List<double> rndList = new List<double>();
            for (int i = 0; i < size; i++)
            {
                rndList.Add(Math.Round(rnd.NextDouble() * max, 1));
            }
            return rndList;
        }

        /// <summary>
        /// Returns a random integer within the given min and max limit
        /// </summary>
        /// <param name="min">Min limit of random number</param>
        /// <param name="max">Max limit of random number</param>
        /// <returns></returns>
        internal static int GetRandomInt(int min = 0, int max = 30)
        {
            return rnd.Next(min, max);
        }

        /// <summary>
        /// Returns a random double within the given max limit
        /// </summary>
        /// <param name="max">Max limit of random number</param>
        /// <returns></returns>
        internal static double GetRandomDouble(int max = 30)
        {
            return Math.Round(rnd.NextDouble() * max, 1);
        }
    }
}
