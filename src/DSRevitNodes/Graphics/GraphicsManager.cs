using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Revit.Graphics
{
    [Browsable(false)]
    public static class GraphicsManager
    {
        /// <summary>
        /// Defines the global level of detail setting for 
        /// object tesselation
        /// </summary>
        private static double tesselationLevelOfDetail = 0.5;
        public static double TesselationLevelOfDetail
        {
            get
            {
                return tesselationLevelOfDetail;
            }
            set
            {
                if (value < 0)
                {
                    tesselationLevelOfDetail = 0;
                    return;
                }

                if (value > 1)
                {
                    tesselationLevelOfDetail = 1;
                    return;
                }

                tesselationLevelOfDetail = value;
            }
        }
    }
}
