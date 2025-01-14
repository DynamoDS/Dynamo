using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControl.xaml
    /// </summary>
    public partial class CurveMapperControl : UserControl, INotifyPropertyChanged
    {
        private readonly CurveMapperNodeModel model;
        private CurveLinear linearCurve;
        public event PropertyChangedEventHandler PropertyChanged;
                
        public double DynamicCanvasSize
        {
            get => dynamicCanvasSize;
            set
            {
                if (dynamicCanvasSize != value)
                {
                    dynamicCanvasSize = Math.Max(value, canvasMinSize);
                    OnPropertyChanged(nameof(DynamicCanvasSize));
                }
            }
        }
        private double dynamicCanvasSize = 240;
        private double previousCanvasSize = 240;

        private readonly double canvasMinSize = 240; // also initial width and height
        private readonly double mainGridMinWidth = 310;
        private readonly double mainGridMinHeigth = 340;
        private int gridSize = 10;

        private void OnPropertyChanged(string propertyName) // RaisePropertyChanged
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CurveMapperControl(CurveMapperNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            this.model = model;
            DataContext = model;

            // ip comment : build this
            //this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            // Redraw canvas when the input changes
            model.PropertyChanged += (s, e) =>
            {
                // Ensure all ports are connected
                // ip : do we need this anymore?
                var inPorts = model.InPorts;
                var allPortsConnected = inPorts[0].IsConnected &&
                    inPorts[1].IsConnected &&
                    inPorts[2].IsConnected &&
                    inPorts[3].IsConnected;

                if (allPortsConnected)
                {
                    if (e.PropertyName == nameof(model.MinLimitX) ||
                    e.PropertyName == nameof(model.MaxLimitX) ||
                    e.PropertyName == nameof(model.MinLimitY) ||
                    e.PropertyName == nameof(model.MaxLimitY))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
                            //UpdateLabels();
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            };

            // Redraw canvas when the node is resized
            GraphCanvas.SizeChanged += (s, e) =>
            {
                double newCanvasSize = DynamicCanvasSize;

                if (model.PointLinearStart != null && model.PointLinearEnd != null)
                {
                    // Update the bounds for movement
                    model.PointLinearStart.LimitWidth = newCanvasSize;
                    model.PointLinearStart.LimitHeight = newCanvasSize;
                    model.PointLinearEnd.LimitWidth = newCanvasSize;
                    model.PointLinearEnd.LimitHeight = newCanvasSize;

                    // Adjust the control points to stay proportional to the canvas size
                    double xRatioStart = model.PointLinearStart.Point.X / previousCanvasSize;
                    double yRatioStart = model.PointLinearStart.Point.Y / previousCanvasSize;
                    double xRatioEnd = model.PointLinearEnd.Point.X / previousCanvasSize;
                    double yRatioEnd = model.PointLinearEnd.Point.Y / previousCanvasSize;

                    model.PointLinearStart.Point = new Point(xRatioStart * newCanvasSize, yRatioStart * newCanvasSize);
                    model.PointLinearEnd.Point = new Point(xRatioEnd * newCanvasSize, yRatioEnd * newCanvasSize);

                    // Update the linear curve's bounds
                    if (model.LinearCurve != null)
                    {
                        model.LinearCurve.MaxWidth = newCanvasSize;
                        model.LinearCurve.MaxHeight = newCanvasSize;
                        model.LinearCurve.Regenerate();
                        Canvas.SetZIndex(model.LinearCurve.PathCurve, 10);
                    }
                }
                previousCanvasSize = newCanvasSize;

                DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
            };

            // Initial draw canvas
            DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as CurveMapperNodeModel;             
            }
        }

        /// <summary>
        /// Redraws the grid on the canvas by removing old grid lines and drawing new ones based.
        /// </summary>
        private void DrawGrid(double xMin, double xMax, double yMin, double yMax)
        {
            // Remove current grid lines
            var gridLines = GraphCanvas.Children
                .OfType<UIElement>()
                .Where(child => (child as FrameworkElement)?.Tag?.ToString() == "GridLine")
                .ToList();

            foreach (var line in gridLines)
            {
                GraphCanvas.Children.Remove(line);
            }

            // Draw grid lines
            double xPixelsPerStep = DynamicCanvasSize / gridSize;
            double yPixelsPerStep = DynamicCanvasSize / gridSize;

            for (int i = 0; i <= gridSize; i++)
            {
                double xPos = i * xPixelsPerStep;
                DrawLine(xPos, 0, xPos, DynamicCanvasSize);
            }

            for (int i = 0; i <= gridSize; i++)
            {
                double yPos = i * yPixelsPerStep;
                DrawLine(0, yPos, DynamicCanvasSize, yPos);
            }
        }

        /// <summary>
        /// Helper function to draw lines on the canvas
        /// </summary>
        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            var line = new System.Windows.Shapes.Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e5e5e")), // TODO : Adjust color
                StrokeThickness = 0.6, // TODO : Slightly thicker for borders
                Tag = "GridLine"
            };
            Canvas.SetZIndex(line, 0);
            GraphCanvas.Children.Add(line);
        }

        private void UpdateLabels()
        {
            // Condition to display "x-min" and "x-max" if both are 0
            if (model.MinLimitX == 0 && model.MaxLimitX == 0)
            {
                minLimitXLabel.Text = "x-min";
                midXLabel.Text = "";
                maxLimitXLabel.Text = "x-max";
            }
            else
            {
                minLimitXLabel.Text = model.MinLimitX.ToString("0.##");
                midXLabel.Text = model.MidValueX.ToString("0.##");
                maxLimitXLabel.Text = model.MaxLimitX.ToString("0.##");
            }

            // Similarly for Y-axis
            if (model.MinLimitY == 0 && model.MaxLimitY == 0)
            {
                minLimitYLabel.Text = "y-min";
                midYLabel.Text = "";
                maxLimitYLabel.Text = "y-max";
            }
            else
            {
                minLimitYLabel.Text = model.MinLimitY.ToString("0.##");
                midYLabel.Text = model.MidValueY.ToString("0.##");
                maxLimitYLabel.Text = model.MaxLimitY.ToString("0.##");
            }
        }


        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var sizeChange = Math.Min(e.VerticalChange, e.HorizontalChange);
            var yAdjust = ActualHeight + sizeChange;
            var xAdjust = ActualWidth + sizeChange;

            // Ensure the node doesn't resize below its minimum size
            if (xAdjust < mainGridMinWidth) xAdjust = mainGridMinWidth;
            if (yAdjust < mainGridMinHeigth) yAdjust = mainGridMinHeigth;

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            DynamicCanvasSize = Math.Max(xAdjust - 70, canvasMinSize);
        }




        /// <summary>
        /// Unsubscribes from ViewModel events
        /// </summary>
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }

        #region Helpers

        // Helper function to find the optimal step size
        private double FindOptimalStepSize(double range, double canvasSize, double[] stepSizes)
        {
            foreach(var step in stepSizes)
            {
                if (range / step <= 10) // maximum 10 columns/rows
                {
                    return step;
                }
            }
            return stepSizes.Last(); // Default to the largest step size
        }

        private double CalculateStepSize(double range, double canvasSize, double minSpacing, double maxSpacing)
        {
            /// Determine the ideal step size in pixels
            double pixelStep = canvasSize / range;

            // Adjust step size to ensure spacing is within minSpacing and maxSpacing
            if (pixelStep < minSpacing)
            {
                // Too fine: merge steps to ensure minimum spacing
                double factor = Math.Ceiling(minSpacing / pixelStep);
                return range / Math.Ceiling(range / factor);
            }
            else if (pixelStep > maxSpacing)
            {
                // Too coarse: split steps to ensure maximum spacing
                double factor = Math.Floor(pixelStep / maxSpacing);
                return range / Math.Floor(range * factor / range);
            }

            // If within bounds, show 1 unit per step
            return 1;
        }

        

        #endregion

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the GraphType is Linear Curve
            if (model.SelectedGraphType == GraphTypes.Linear)
            {
                // Reset the control points to their original positions
                model.PointLinearStart.Point = new Point(0, DynamicCanvasSize);
                model.PointLinearEnd.Point = new Point(DynamicCanvasSize, 0);

                // Update the linear curve's bounds
                if (model.LinearCurve != null)
                {                    
                    model.LinearCurve.Regenerate();
                }
            }
            else if (model.SelectedGraphType == GraphTypes.Bezier)
            {
                model.PointBezierControl1.Point = new Point(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
                model.PointBezierControl2.Point = new Point(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
                model.PointBezierFix1.Point = new Point(0, DynamicCanvasSize);
                model.PointBezierFix2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);

                // Regenerate control lines
                if (model.CurveBezierControlLine1 != null)
                {
                    model.CurveBezierControlLine1.Regenerate(model.PointBezierControl1, model.PointBezierFix1);
                }
                if (model.CurveBezierControlLine2 != null)
                {
                    model.CurveBezierControlLine2.Regenerate(model.PointBezierControl2, model.PointBezierFix2);
                }

                // Regenerate the bezier curve
                if (model.CurveBezier != null)
                {
                    model.CurveBezier.Regenerate(model.PointBezierControl1);
                    model.CurveBezier.Regenerate(model.PointBezierControl2);
                    model.CurveBezier.Regenerate(model.PointBezierFix1);
                    model.CurveBezier.Regenerate(model.PointBezierFix2);
                    
                }
            }
            else if (model.SelectedGraphType == GraphTypes.SineWave)
            {
                model.ControlPointSine1.Point = new Point(0, 0);
                model.ControlPointSine2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                if (model.CurveSine != null)
                {
                    model.CurveSine.Regenerate(model.ControlPointSine1);
                    model.CurveSine.Regenerate(model.ControlPointSine2);
                }
            }
            else if (model.SelectedGraphType == GraphTypes.Parabola)
            {
                model.ControlPointParabolic1.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
                model.ControlPointParabolic2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                if (model.CurveSine != null)
                {
                    model.CurveParabolic.Regenerate(model.ControlPointParabolic1);
                    model.CurveParabolic.Regenerate(model.ControlPointParabolic2);
                }
            }
        }
    }
}
