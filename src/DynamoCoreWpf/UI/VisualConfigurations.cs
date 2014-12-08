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
        public static FontWeight LibraryTooltipTextFontWeight = FontWeights.Normal;
        public static FontWeight NodeTooltipTextFontWeight = FontWeights.Light;
        public static FontWeight ErrorTextFontWeight = FontWeights.Normal;

        public static readonly Color GridLineColor = Color.FromRgb(232, 232, 232);
    }
}
