using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Dynamo.Core
{
    public class Configurations
    {
        #region Canvas Configurations
        //public static readonly double Minimum

        // Grid Settings
        public static readonly int GridSpacing = 100;
        public static readonly int GridThickness = 2;
        public static readonly Color GridLineColor = Color.FromRgb(232, 232, 232);
        #endregion

        #region Tab Bar Configurations
        // Tabcontrol Settings        
        public static readonly int MinTabsBeforeClipping = 6;
        public static readonly int TabControlMenuWidth = 20;
        public static readonly int TabDefaultWidth = 200;
        #endregion
    }
}
