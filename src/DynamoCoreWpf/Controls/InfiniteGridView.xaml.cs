using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #region Private Class Data Members

        private const int MinorDivisions = 5;
        private const int MajorGridLineSpacing = 100;
        private const double MinMajorGridSpacing = 50;
        private const double MaxMajorGridSpacing = MinMajorGridSpacing * MinorDivisions;
        private const double ScaleFactor = MaxMajorGridSpacing / MinMajorGridSpacing;

        // Zoom dependent data members.
        private double startX, startY;

        private WorkspaceModel workspaceModel;
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
            this.SizeChanged += (s, e) => UpdateDrawingVisual();
            this.Loaded += (s, e) => InitializeOnce();
            this.Unloaded += (s, e) => UninitializeOnce();
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

        private void InitializeOnce()
        {
            var workspaceView = WpfUtilities.FindUpVisualTree<WorkspaceView>(this);

            if (workspaceView == null)
            {
                throw new InvalidOperationException(
                    "InfiniteGridView should be a nested element of WorkspaceView");
            }

            workspaceModel = workspaceView.ViewModel.Model;
            workspaceModel.PropertyChanged += OnWorkspacePropertyChanged;
        }

        void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                case "Y":
                case "Zoom":
                    UpdateDrawingVisual();
                    break;
            }
        }

        private void UninitializeOnce()
        {
            if (workspaceModel != null)
            {
                workspaceModel.PropertyChanged -= OnWorkspacePropertyChanged;
            }
        }

        private void UpdateDrawingVisual()
        {
            if (workspaceModel == null)
                return;

            var localScale = workspaceModel.Zoom;
            while (localScale * MajorGridLineSpacing < MinMajorGridSpacing)
                localScale = localScale * ScaleFactor;
            while (localScale * MajorGridLineSpacing > MaxMajorGridSpacing)
                localScale = localScale / ScaleFactor;

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

        #endregion
    }

    public partial class InfiniteGridView : UserControl
    {
        public InfiniteGridView()
        {
            InitializeComponent();
        }
    }
}
