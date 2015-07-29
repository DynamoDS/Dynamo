using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.Controls
{
    public class GridVisualHost : FrameworkElement
    {
        private const int MinorDivisions = 5;
        private const int MajorGridLineSpacing = 100;
        private const double MinMajorGridSpacing = 50;
        private const double MaxMajorGridSpacing = MinMajorGridSpacing * MinorDivisions;
        private const double ScaleFactor = MaxMajorGridSpacing / MinMajorGridSpacing;

        // Zoom dependent data members.
        private double startX, startY, scale = 1.0;

        private Pen majorGridPen, minorGridPen;
        private DrawingVisual drawingVisual = new DrawingVisual();

        public GridVisualHost()
        {
            var majorBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            var minorBrush = new SolidColorBrush(Color.FromArgb(255, 195, 195, 195));
            majorGridPen = new Pen(majorBrush, 1.0);
            minorGridPen = new Pen(minorBrush, 1.0);

            AddVisualChild(drawingVisual);
            this.SizeChanged += (s, e) => UpdateDrawingVisual();
            this.MouseWheel += (s, e) =>
            {
                scale += scale * ((e.Delta > 0) ? 0.1 : -0.1);
                UpdateDrawingVisual();
            };
        }

        void VisualHost_MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return drawingVisual;
        }

        private void UpdateDrawingVisual()
        {
            var localScale = scale;
            while (localScale * MajorGridLineSpacing < MinMajorGridSpacing)
                localScale = localScale * ScaleFactor;
            while (localScale * MajorGridLineSpacing > MaxMajorGridSpacing)
                localScale = localScale / ScaleFactor;

            var unitGrid = (localScale * (MajorGridLineSpacing / MinorDivisions));
            var context = drawingVisual.RenderOpen();
            // context.DrawRectangle(Brushes.LimeGreen, null, new Rect(0, 0, ActualWidth, ActualHeight));

            #region Vertical grid lines

            int counter = 0;
            var pointOne = new Point(startX, 0.0);
            var pointTwo = new Point(startX, ActualHeight);

            while (true)
            {
                var isMajorGridLine = ((counter % MinorDivisions) == 0);

                var offset = unitGrid * counter++;
                if (offset > ActualWidth)
                    break;

                var pen = isMajorGridLine ? majorGridPen : minorGridPen;
                pointOne.X = startX + offset;
                pointTwo.X = pointOne.X;
                context.DrawLine(pen, pointOne, pointTwo);
            }

            #endregion

            #region Horizontal grid lines

            counter = 0;
            pointOne = new Point(0.0, startY);
            pointTwo = new Point(ActualWidth, startY);

            while (true)
            {
                var isMajorGridLine = ((counter % MinorDivisions) == 0);

                var offset = unitGrid * counter++;
                if (offset > ActualHeight)
                    break;

                var pen = isMajorGridLine ? majorGridPen : minorGridPen;
                pointOne.Y = startY + offset;
                pointTwo.Y = pointOne.Y;
                context.DrawLine(pen, pointOne, pointTwo);
            }

            #endregion

            context.Close();
        }
    }

    public partial class InfiniteGridView : UserControl
    {
        public InfiniteGridView()
        {
            InitializeComponent();
        }
    }
}
