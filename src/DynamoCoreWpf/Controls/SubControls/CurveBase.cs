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

        public virtual List<double> GetValuesFromAssignedParameters(double lowLimit, double highLimit, int count)
        {
            return new List<double>();
        }
    }
}
