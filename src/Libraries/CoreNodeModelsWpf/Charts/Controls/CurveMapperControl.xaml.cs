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
                if (model.ControlPointLinear1 != null && model.ControlPointLinear2 != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointLinear1, model.ControlPointLinear2);
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
                    UpdateControlPoints(newCanvasSize, model.ControlPointBezier1, model.ControlPointBezier2,
                        model.OrthoControlPointBezier1, model.OrthoControlPointBezier2);

                    model.ControlLineBezier1?.Regenerate(model.ControlPointBezier1, model.OrthoControlPointBezier1);
                    model.ControlLineBezier2?.Regenerate(model.ControlPointBezier2, model.OrthoControlPointBezier2);

                    model.BezierCurve.MaxWidth = newCanvasSize;
                    model.BezierCurve.MaxHeight = newCanvasSize;
                    model.BezierCurve.Regenerate();
                    Canvas.SetZIndex(model.BezierCurve.PathCurve, 10);
                }
                // Sine wave
                if (model.ControlPointSine1 != null && model.ControlPointSine2 != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointSine1, model.ControlPointSine2);
                    if (model.SineWave != null)
                    {
                        model.SineWave.MaxWidth = newCanvasSize;
                        model.SineWave.MaxHeight = newCanvasSize;
                        model.SineWave.Regenerate();
                        Canvas.SetZIndex(model.SineWave.PathCurve, 10);
                    }
                }
                // Cosine wave
                if (model.ControlPointSine1 != null && model.ControlPointSine2 != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointCosine1, model.ControlPointCosine2);
                    if (model.CosineWave != null)
                    {
                        model.CosineWave.MaxWidth = newCanvasSize;
                        model.CosineWave.MaxHeight = newCanvasSize;
                        model.CosineWave.Regenerate();
                        Canvas.SetZIndex(model.CosineWave.PathCurve, 10);
                    }
                }
                // Parabolic curve
                if (model.ControlPointParabolic1 != null && model.ControlPointParabolic2 != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointParabolic1, model.ControlPointParabolic2);
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
                if (model.OrthoControlPointPerlin1 != null && model.OrthoControlPointPerlin2 != null && model.ControlPointPerlin != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointPerlin,
                        model.OrthoControlPointPerlin1, model.OrthoControlPointPerlin2);
                    if (model.ParabolicCurve != null)
                    {
                        model.PerlinNoiseCurve.MaxWidth = newCanvasSize;
                        model.PerlinNoiseCurve.MaxHeight = newCanvasSize;
                        model.PerlinNoiseCurve.Regenerate();
                        Canvas.SetZIndex(model.PerlinNoiseCurve.PathCurve, 10);
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
        /// Updates control points limits and resizes them proportionally based on the new canvas size.
        /// </summary>
        private void UpdateControlPoints(double newCanvasSize, params CurveMapperControlPoint[] points)
        {
            foreach (var point in points.Where(p => p != null))
            {
                point.LimitWidth = newCanvasSize;
                point.LimitHeight = newCanvasSize;
                point.Point = new Point((point.Point.X / previousCanvasSize) * newCanvasSize,
                                        (point.Point.Y / previousCanvasSize) * newCanvasSize);
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
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e5e5e")),
                StrokeThickness = 0.6,
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
            // Reset the control points to their original positions and regenerate curves
            switch (model.SelectedGraphType)
            {
                case GraphTypes.LinearCurve:
                    model.ControlPointLinear1.Point = new Point(0, DynamicCanvasSize);
                    model.ControlPointLinear2.Point = new Point(DynamicCanvasSize, 0);
                    model.LinearCurve?.Regenerate();
                    break;
                case GraphTypes.BezierCurve:
                    model.ControlPointBezier1.Point = new Point(DynamicCanvasSize * 0.2, DynamicCanvasSize * 0.2);
                    model.ControlPointBezier2.Point = new Point(DynamicCanvasSize * 0.8, DynamicCanvasSize * 0.2);
                    model.OrthoControlPointBezier1.Point = new Point(0, DynamicCanvasSize);
                    model.OrthoControlPointBezier2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                    model.ControlLineBezier1?.Regenerate(model.ControlPointBezier1, model.OrthoControlPointBezier1);
                    model.ControlLineBezier2?.Regenerate(model.ControlPointBezier2, model.OrthoControlPointBezier2);
                    model.BezierCurve?.Regenerate();
                    break;
                case GraphTypes.SineWave:
                    model.ControlPointSine1.Point = new Point(DynamicCanvasSize * 0.25, 0);
                    model.ControlPointSine2.Point = new Point(DynamicCanvasSize * 0.75, DynamicCanvasSize);
                    model.SineWave?.Regenerate();
                    break;
                case GraphTypes.CosineWave:
                    model.ControlPointCosine1.Point = new Point(0, 0);
                    model.ControlPointCosine2.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize);
                    model.CosineWave?.Regenerate();
                    break;
                case GraphTypes.ParabolicCurve:
                    model.ControlPointParabolic1.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.1);
                    model.ControlPointParabolic2.Point = new Point(DynamicCanvasSize, DynamicCanvasSize);
                    model.ParabolicCurve?.Regenerate();
                    break;
                case GraphTypes.PerlinNoiseCurve:
                    model.OrthoControlPointPerlin1.Point = new Point(DynamicCanvasSize * 0.5, 0);
                    model.OrthoControlPointPerlin2.Point = new Point(0, DynamicCanvasSize);
                    model.ControlPointPerlin.Point = new Point(DynamicCanvasSize * 0.5, DynamicCanvasSize * 0.5);
                    model.PerlinNoiseCurve?.Regenerate();
                    break;
                // Add mode curves here
            }

            // Force Output Recalculation
            model.GenerateOutputValues();
            model.OnNodeModified();
        }

        private void GraphCanvas_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }
    }
}
