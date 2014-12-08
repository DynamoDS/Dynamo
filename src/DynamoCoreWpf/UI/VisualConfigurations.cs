using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Wpf.UI
{
    public static class VisualConfigurations
    {
        // TODO(Peter): Clean up configurations, these are just of the few moved out of DynamoCore in order
        //              to remove the PresentationCore dependency http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5608
        public static FontWeight LibraryTooltipTextFontWeight = FontWeights.Normal;
        public static FontWeight NodeTooltipTextFontWeight = FontWeights.Light;
        public static FontWeight ErrorTextFontWeight = FontWeights.Normal;

        public static readonly Color GridLineColor = Color.FromRgb(232, 232, 232);
    }
}
