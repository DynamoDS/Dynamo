using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Dynamo.UI;
using Dynamo.Utilities;

namespace Dynamo.Wpf.UI
{
    public static class VisualCodeBlockUtils
    {
        /// <summary>
        /// This method handles mapping logical to visual line indices in code block nodes. 
        /// </summary>
        /// <param name="mutableMap">The mutable mapping object to be set by this method</param>
        public static List<int> MapLogicalToVisualLineIndices(string text)
        {
            var logicalToVisualLines = new List<int>();

            var lines = text.Split(new char[] { '\n' }, StringSplitOptions.None);

            // We could have hard-coded "pack" instead of "UriSchemePack" here, 
            // but in NUnit scenario there is no "Application" created. When there 
            // is no Application instance, the Uri format "pack://" will fail Uri 
            // object creation. Adding a reference to "UriSchemePack" resolves 
            // this issue to avoid a "UriFormatException".
            // 
            string pack = System.IO.Packaging.PackUriHelper.UriSchemePack;
            var uri = new Uri(pack + "://application:,,,/DynamoCore;component/");

            var textFontFamily = new System.Windows.Media.FontFamily(uri, ResourceNames.FontResourceUri);

            var typeface = new Typeface(textFontFamily, FontStyles.Normal,
                FontWeights.Normal, FontStretches.Normal);

            int totalVisualLinesSoFar = 0;
            foreach (var line in lines)
            {
                var ft = new FormattedText(
                    line, CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, typeface,
                    Configurations.CBNFontSize, Brushes.Black)
                {
                    MaxTextWidth = Configurations.CBNMaxTextBoxWidth,
                    Trimming = TextTrimming.None
                };

                logicalToVisualLines.Add(totalVisualLinesSoFar);

                // Empty lines (i.e. those with just a "\n" character) will result 
                // in "ft.Extent" to be 0.0, but the line still occupies one line
                // visually. This is why we need to make sure "lineCount" cannot be 
                // zero.
                var lineCount = Math.Floor(ft.Extent / Configurations.CBNFontSize);
                totalVisualLinesSoFar += (lineCount < 1.0 ? 1 : ((int)lineCount));
            }

            return logicalToVisualLines;
        }

    }
}
