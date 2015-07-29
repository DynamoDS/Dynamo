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
    public class VisualHost : FrameworkElement
    {
        private const int divisions = 5;
        private const int minMajorGap = 100;

        private Pen majorGridPen, minorGridPen;
        private DrawingVisual drawingVisual = new DrawingVisual();

        public VisualHost()
        {
            var majorBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            var minorBrush = new SolidColorBrush(Color.FromArgb(255, 195, 195, 195));
            majorGridPen = new Pen(majorBrush, 1.0);
            minorGridPen = new Pen(minorBrush, 1.0);

            AddVisualChild(drawingVisual);
            this.SizeChanged += (s, e) => UpdateDrawingVisual();
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
            double startX = 0.0, startY = 0.0;
            var unitGrid = minMajorGap / divisions;
            var context = drawingVisual.RenderOpen();

            #region Vertical grid lines

            int counter = 0;
            var pointOne = new Point(startX, 0.0);
            var pointTwo = new Point(startX, ActualHeight);

            while (true)
            {
                var isMajorGridLine = ((counter % divisions) == 0);

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
                var isMajorGridLine = ((counter % divisions) == 0);

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
