using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;

namespace Dynamo.Controls
{
    public class GridVisualHost : FrameworkElement
    {
        #region Private Class Data Members

        private const int MinorDivisions = 5;
        private const int MajorGridLineSpacing = 100;
        private const double MinMajorGridSpacing = 50;
        private const double MaxMajorGridSpacing = MinMajorGridSpacing * MinorDivisions;
        private const double ScaleFactor = MaxMajorGridSpacing / MinMajorGridSpacing;

        private WorkspaceViewModel workspaceViewModel;
        private Pen majorGridPen, minorGridPen;
        private DrawingVisual drawingVisual = new DrawingVisual();

        #endregion

        #region Public Class Methods

        public GridVisualHost()
        {
            var majorBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            var minorBrush = new SolidColorBrush(Color.FromArgb(255, 195, 195, 195));
            majorGridPen = new Pen(majorBrush, 1.0);
            minorGridPen = new Pen(minorBrush, 1.0);

            AddVisualChild(drawingVisual);
            SizeChanged += (s, e) => UpdateDrawingVisual();
        }

        internal void HandleViewSettingsChange(double x, double y, double zoom)
        {
            UpdateDrawingVisual(x, y, zoom);
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return drawingVisual;
        }

        #endregion

        #region Private Class Helper and Event Handlers

        private void InitializeWorkspaceModel()
        {
            var workspaceView = WpfUtilities.FindUpVisualTree<WorkspaceView>(this);

            if (workspaceView == null)
            {
                throw new InvalidOperationException(
                    "InfiniteGridView should be a nested element of WorkspaceView");
            }

            workspaceViewModel = workspaceView.ViewModel;
        }

        private void UpdateDrawingVisual()
        {
            if (workspaceViewModel == null)
            {
                // Indicates that this is a first load, so ws should be initialized first
                InitializeWorkspaceModel();
            }
            
            UpdateDrawingVisual(workspaceViewModel.X, workspaceViewModel.Y, workspaceViewModel.Zoom);
        }

        private void UpdateDrawingVisual(double x, double y, double zoom)
        {
            #region Scale Adjustment

            // Bring scale factor to within zoom boundaries.
            var localScale = zoom;
            while (localScale * MajorGridLineSpacing < MinMajorGridSpacing)
                localScale = localScale * ScaleFactor;
            while (localScale * MajorGridLineSpacing > MaxMajorGridSpacing)
                localScale = localScale / ScaleFactor;

            #endregion

            #region Positional Adjustment

            // The scale is know, adjust the top-left corner.
            var scaledMajorGridSpacing = localScale * MajorGridLineSpacing;
            var startX = NormalizeStartPoint(x, scaledMajorGridSpacing);
            var startY = NormalizeStartPoint(y, scaledMajorGridSpacing);

            #endregion

            var unitGrid = (localScale * (MajorGridLineSpacing / MinorDivisions));
            var context = drawingVisual.RenderOpen();

            #region Vertical grid lines

            int counter = 0;
            var pointOne = new Point(startX, 0.0);
            var pointTwo = new Point(startX, ActualHeight);

            while (true)
            {
                var isMajorGridLine = ((counter % MinorDivisions) == 0);

                var offset = unitGrid * counter++;
                if (offset > ActualWidth + scaledMajorGridSpacing)
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
                if (offset > ActualHeight + scaledMajorGridSpacing)
                    break;

                var pen = isMajorGridLine ? majorGridPen : minorGridPen;
                pointOne.Y = startY + offset;
                pointTwo.Y = pointOne.Y;
                context.DrawLine(pen, pointOne, pointTwo);
            }

            #endregion

            context.Close();
        }

        private double NormalizeStartPoint(double value, double scaledMajorGridSpacing)
        {
            if (value > 0)
            {
                // Assuming after applying scale factor, the major grid lines 
                // are 10px apart. If the current WorkspaceModel.X is 24px, then 
                // 
                //      v = floor(24/10) = floor(2.4) = 2
                //      w = 24 - 2 x 10 = 4
                // 
                // We could start drawing the major grid line from 4px, but that 
                // leaves a gap at the left edge. So it would be nice if major 
                // grid line starts from 4 - 10 = -6px, so that it appears that 
                // the left-most grid line is beyond the left edge.
                // 
                var v = ((int)Math.Floor(value / scaledMajorGridSpacing));
                var w = (value - (v * scaledMajorGridSpacing));
                value = w - scaledMajorGridSpacing;
            }
            else if (value < -scaledMajorGridSpacing)
            {
                // Assuming after applying scale factor, the major grid lines 
                // are 10px apart. If the current WorkspaceModel.X is -24px, then 
                // 
                //      value = abs(-24) = 24
                //      v = floor(24/10) = floor(2.4) = 2
                //      w = 24 - 2 x 10 = 4
                //      value = -w = -4
                // 
                value = Math.Abs(value);
                var v = ((int)Math.Floor(value / scaledMajorGridSpacing));
                var w = (value - (v * scaledMajorGridSpacing));
                value = -w;
            }

            return value;
        }

        #endregion
    }

    public partial class InfiniteGridView : UserControl
    {
        public InfiniteGridView()
        {
            InitializeComponent();
        }

        internal void AttachToZoomBorder(ZoomBorder zoomBorder)
        {
            zoomBorder.ViewSettingsChanged += OnViewSettingsChanged;
        }

        internal void DetachFromZoomBorder(ZoomBorder zoomBorder)
        {
            zoomBorder.ViewSettingsChanged -= OnViewSettingsChanged;
        }

        private void OnViewSettingsChanged(ViewSettingsChangedEventArgs e)
        {
            gridVisualHost.HandleViewSettingsChange(e.X, e.Y, e.Zoom);
        }
    }
}
