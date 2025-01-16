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
        private LinearCurve linearCurve;
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

                // Linear curve
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



                // Bezier curve
                if (model.BezierCurve != null)
                {
                    // Update the bounds for movement for all points
                    var bezierPoints = new[]
                    {
                        model.BezierControlPoint1, model.BezierControlPoint2,
                        model.BezierFixedPoint1, model.BezierFixedPoint2
                    };

                    foreach (var point in bezierPoints)
                    {
                        point.LimitWidth = newCanvasSize;
                        point.LimitHeight = newCanvasSize;

                        // Update point position proportionally
                        double xRatio = point.Point.X / previousCanvasSize;
                        double yRatio = point.Point.Y / previousCanvasSize;
                        point.Point = new Point(xRatio * newCanvasSize, yRatio * newCanvasSize);
                    }

                    // Update Bezier curve bounds and regenerate
                    model.BezierCurve.MaxWidth = newCanvasSize;
                    model.BezierCurve.MaxHeight = newCanvasSize;
                    model.BezierCurve.Regenerate(model.BezierControlPoint1);
                    model.BezierCurve.Regenerate(model.BezierControlPoint2);
                    model.BezierCurve.Regenerate(model.BezierFixedPoint1);
                    model.BezierCurve.Regenerate(model.BezierFixedPoint2);

                    Canvas.SetZIndex(model.BezierCurve.PathCurve, 10);
                }
                // Control curve
                if (model.CurveBezierControlLine1 != null && model.CurveBezierControlLine2 != null)
                {
                    model.CurveBezierControlLine1.Regenerate(model.BezierControlPoint1, model.BezierFixedPoint1);
                    model.CurveBezierControlLine2.Regenerate(model.BezierControlPoint2, model.BezierFixedPoint2);
                }




                // Sine wave
                if (model.ControlPointSine1 != null && model.ControlPointSine2 != null)
                {
                    // Update the bounds for movement - CAN WE UPDATE ALL POINTS AT THE SAME TIME ???
                    model.ControlPointSine1.LimitWidth = newCanvasSize;
                    model.ControlPointSine1.LimitHeight = newCanvasSize;
                    model.ControlPointSine2.LimitWidth = newCanvasSize;
                    model.ControlPointSine2.LimitHeight = newCanvasSize;
                    // Update control points proportionally
                    double xRatioSine1 = model.ControlPointSine1.Point.X / previousCanvasSize;
                    double yRatioSine1 = model.ControlPointSine1.Point.Y / previousCanvasSize;
                    double xRatioSine2 = model.ControlPointSine2.Point.X / previousCanvasSize;
                    double yRatioSine2 = model.ControlPointSine2.Point.Y / previousCanvasSize;

                    model.ControlPointSine1.Point = new Point(xRatioSine1 * newCanvasSize, yRatioSine1 * newCanvasSize);
                    model.ControlPointSine2.Point = new Point(xRatioSine2 * newCanvasSize, yRatioSine2 * newCanvasSize);

                    // Update sine curve bounds and regenerate
                    if (model.SineCurve != null)
                    {
                        model.SineCurve.MaxWidth = newCanvasSize;
                        model.SineCurve.MaxHeight = newCanvasSize;
                        model.SineCurve.Regenerate(model.ControlPointSine1);
                        model.SineCurve.Regenerate(model.ControlPointSine2);
                        Canvas.SetZIndex(model.SineCurve.PathCurve, 10);
                    }
                }
                // Parabolic curve
                if (model.ControlPointParabolic1 != null && model.ControlPointParabolic2 != null)
                {
                    // Update the bounds for movement - CAN WE UPDATE ALL POINTS AT THE SAME TIME ???
                    model.ControlPointParabolic1.LimitWidth = newCanvasSize;
                    model.ControlPointParabolic1.LimitHeight = newCanvasSize;
                    model.ControlPointParabolic2.LimitWidth = newCanvasSize;
                    model.ControlPointParabolic2.LimitHeight = newCanvasSize;
                    // Update control points proportionally
                    double xRatioSine1 = model.ControlPointParabolic1.Point.X / previousCanvasSize;
                    double yRatioSine1 = model.ControlPointParabolic1.Point.Y / previousCanvasSize;
                    double xRatioSine2 = model.ControlPointParabolic2.Point.X / previousCanvasSize;
                    double yRatioSine2 = model.ControlPointParabolic2.Point.Y / previousCanvasSize;

                    model.ControlPointParabolic1.Point = new Point(xRatioSine1 * newCanvasSize, yRatioSine1 * newCanvasSize);
                    model.ControlPointParabolic2.Point = new Point(xRatioSine2 * newCanvasSize, yRatioSine2 * newCanvasSize);

                    // Update sine curve bounds and regenerate
                    if (model.ParabolicCurve != null)
                    {
                        model.ParabolicCurve.MaxWidth = newCanvasSize;
                        model.ParabolicCurve.MaxHeight = newCanvasSize;
                        model.ParabolicCurve.Regenerate(model.ControlPointParabolic1);
                        model.ParabolicCurve.Regenerate(model.ControlPointParabolic2);
                        Canvas.SetZIndex(model.ParabolicCurve.PathCurve, 10);
                    }
                }
                // Pelin noise curve
                if (model.FixedPointPerlin1 != null && model.FixedPointPerlin2 != null && model.ControlPointPerlin != null)
                {
                    // Update the bounds for movement - CAN WE UPDATE ALL POINTS AT THE SAME TIME ???
                    model.FixedPointPerlin1.LimitWidth = newCanvasSize;
                    model.FixedPointPerlin1.LimitHeight = newCanvasSize;
                    model.FixedPointPerlin2.LimitWidth = newCanvasSize;
                    model.FixedPointPerlin2.LimitHeight = newCanvasSize;
                    model.ControlPointPerlin.LimitWidth = newCanvasSize;
                    model.ControlPointPerlin.LimitHeight = newCanvasSize;
                    // Update control points proportionally
                    double xRatioSine1 = model.FixedPointPerlin1.Point.X / previousCanvasSize;
                    double yRatioSine1 = model.FixedPointPerlin1.Point.Y / previousCanvasSize;
                    double xRatioSine2 = model.FixedPointPerlin2.Point.X / previousCanvasSize;
                    double yRatioSine2 = model.FixedPointPerlin2.Point.Y / previousCanvasSize;
                    double xRatioSine3 = model.ControlPointPerlin.Point.X / previousCanvasSize;
                    double yRatioSine3 = model.ControlPointPerlin.Point.Y / previousCanvasSize;

                    model.FixedPointPerlin1.Point = new Point(xRatioSine1 * newCanvasSize, yRatioSine1 * newCanvasSize);
                    model.FixedPointPerlin2.Point = new Point(xRatioSine2 * newCanvasSize, yRatioSine2 * newCanvasSize);
                    model.ControlPointPerlin.Point = new Point(xRatioSine3 * newCanvasSize, yRatioSine3 * newCanvasSize);

                    // Update sine curve bounds and regenerate
                    if (model.ParabolicCurve != null)
                    {
                        model.PerlinCurve.MaxWidth = newCanvasSize;
                        model.PerlinCurve.MaxHeight = newCanvasSize;
                        model.PerlinCurve.Regenerate(model.FixedPointPerlin1);
                        model.PerlinCurve.Regenerate(model.FixedPointPerlin2);
                        model.PerlinCurve.Regenerate(model.ControlPointPerlin);
                        Canvas.SetZIndex(model.PerlinCurve.PathCurve, 10);
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
                model.BezierControlPoint1.Point = new Point(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
                model.BezierControlPoint2.Point = new Point(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
                model.BezierFixedPoint1.Point = new Point(0, DynamicCanvasSize);
                model.BezierFixedPoint2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);

                // Regenerate control lines
                if (model.CurveBezierControlLine1 != null)
                {
                    model.CurveBezierControlLine1.Regenerate(model.BezierControlPoint1, model.BezierFixedPoint1);
                }
                if (model.CurveBezierControlLine2 != null)
                {
                    model.CurveBezierControlLine2.Regenerate(model.BezierControlPoint2, model.BezierFixedPoint2);
                }

                // Regenerate the bezier curve
                if (model.BezierCurve != null)
                {
                    model.BezierCurve.Regenerate(model.BezierControlPoint1);
                    model.BezierCurve.Regenerate(model.BezierControlPoint2);
                    model.BezierCurve.Regenerate(model.BezierFixedPoint1);
                    model.BezierCurve.Regenerate(model.BezierFixedPoint2);
                    
                }
            }
            else if (model.SelectedGraphType == GraphTypes.SineWave)
            {
                model.ControlPointSine1.Point = new Point(0, 0);
                model.ControlPointSine2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                if (model.SineCurve != null)
                {
                    model.SineCurve.Regenerate(model.ControlPointSine1);
                    model.SineCurve.Regenerate(model.ControlPointSine2);
                }
            }
            else if (model.SelectedGraphType == GraphTypes.Parabola)
            {
                model.ControlPointParabolic1.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
                model.ControlPointParabolic2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                if (model.SineCurve != null)
                {
                    model.ParabolicCurve.Regenerate(model.ControlPointParabolic1);
                    model.ParabolicCurve.Regenerate(model.ControlPointParabolic2);
                }
            }
            else if (model.SelectedGraphType == GraphTypes.PerlinNoise)
            {
                model.FixedPointPerlin1.Point = new Point(DynamicCanvasSize * 0.5, 0);
                model.FixedPointPerlin2.Point = new Point(0, DynamicCanvasSize);
                model.ControlPointPerlin.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                if (model.SineCurve != null)
                {
                    model.PerlinCurve.Regenerate(model.FixedPointPerlin1);
                    model.PerlinCurve.Regenerate(model.FixedPointPerlin1);
                    model.PerlinCurve.Regenerate(model.ControlPointPerlin);
                }
            }
        }
    }
}
