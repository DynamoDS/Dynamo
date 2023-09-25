using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Dynamo.UI;

namespace CoreNodeModelsWpf.Charts.Utilities
{
    /// <summary>
    /// Helper class providing colors based on the Dynamo color palette
    /// The color palette is defined inside the LiveChartDictionary resource
    /// </summary>
    public static class Colors
    {
        private static int current = 0;

        private static IEnumerable<Color> _colors => SharedDictionaryManager.LiveChartDictionary["ColorsCollection"] as IEnumerable<Color>;

        /// <summary>
        /// Cycles through all the colors inside the palette returning the next color
        /// Loops through all colors then starts back at 0
        /// </summary>
        /// <returns></returns>
        public static Color GetColor()
        {
            int index = current++;
            if (current == _colors.Count()) current = 0;
            return _colors.ElementAt(index);
        }

        /// <summary>
        /// Resets the color sequence for consistency
        /// </summary>
        public static void ResetColors()
        {
            current = 0;
        }
    }
}
