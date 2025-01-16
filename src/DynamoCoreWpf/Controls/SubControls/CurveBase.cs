using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class CurveBase : UIElement
    {
        // private backing fields
        // TODO: declare in API
        private Path pathCurve;
        private PathGeometry pathGeometry;
        private PathFigure pathFigure;

        public Path PathCurve
        {
            get => pathCurve;
            set => pathCurve = value;
        }

        public PathGeometry PathGeometry
        {
            get => pathGeometry;
            set => pathGeometry = value;
        }

        public PathFigure PathFigure
        {
            get => pathFigure;
            set => pathFigure = value;
        }

        public CurveBase()
        {
            pathGeometry = new PathGeometry();
            pathFigure = new PathFigure();
        }

        /// <summary>
        /// Calculates the Y-axis values for the curve based on input limits and count.
        /// </summary>
        public virtual List<double> GetCurveYValues(double minLimitY, double maxLimitY, int count)
        {
            return new List<double>();
        }

        /// <summary>
        /// Calculates the X-axis values for the curve based on input limits and count.
        /// </summary>
        public virtual List<double> GetCurveXValues(double minLimitX, double maxLimitX, int count)
        {
            return new List<double>();
        }
    }
}
