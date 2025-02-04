using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Properties;
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
        public event PropertyChangedEventHandler PropertyChanged;

        private double previousCanvasSize = 240;

        private readonly double canvasMinSize = 240; // also initial width and height
        private readonly double mainGridMinWidth = 310;
        private readonly double mainGridMinHeight = 340;
        private int gridSize = 10;

        //private void OnPropertyChanged(string propertyName) // RaisePropertyChanged
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        public CurveMapperControl(CurveMapperNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            this.model = model;
            DataContext = model;

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
                double newCanvasSize = model.DynamicCanvasSize;

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
                // Power curve
                if (model.ControlPointPower != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointPower);
                    if (model.PowerCurve != null)
                    {
                        model.PowerCurve.MaxWidth = newCanvasSize;
                        model.PowerCurve.MaxHeight = newCanvasSize;
                        model.PowerCurve.Regenerate();
                        Canvas.SetZIndex(model.PowerCurve.PathCurve, 10);
                    }
                }
                // Square Root curve
                if (model.ControlPointSquareRoot1 != null && model.ControlPointSquareRoot2 != null)
                {
                    UpdateControlPoints(newCanvasSize, model.ControlPointSquareRoot1, model.ControlPointSquareRoot2);
                    if (model.SquareRootCurve != null)
                    {
                        model.SquareRootCurve.MaxWidth = newCanvasSize;
                        model.SquareRootCurve.MaxHeight = newCanvasSize;
                        model.SquareRootCurve.Regenerate();
                        Canvas.SetZIndex(model.SquareRootCurve.PathCurve, 10);
                    }
                }
                // Gaussian curve
                if (model.OrthoControlPointGaussian1 != null && model.OrthoControlPointGaussian2 != null && model.OrthoControlPointGaussian3 != null && model.OrthoControlPointGaussian4 != null)
                {
                    model.GaussianCurve.IsResizing = true;

                    UpdateControlPoints(newCanvasSize, model.OrthoControlPointGaussian1, model.OrthoControlPointGaussian2,
                        model.OrthoControlPointGaussian3, model.OrthoControlPointGaussian4);
                    if (model.GaussianCurve != null)
                    {
                        model.GaussianCurve.MaxWidth = newCanvasSize;
                        model.GaussianCurve.MaxHeight = newCanvasSize;
                        model.GaussianCurve.Regenerate();
                        Canvas.SetZIndex(model.GaussianCurve.PathCurve, 10);
                    }

                    model.GaussianCurve.IsResizing = false;
                }

                previousCanvasSize = newCanvasSize;

                DrawGrid();
            };

            // Initial draw canvas
            DrawGrid();
            UpdateLockButton();
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(model.IsLocked))
            {
                Dispatcher.Invoke(() =>
                {
                    ToggleControlPointsMovability();
                    UpdateLockButton();
                });
            }
        }

        /// <summary>
        /// Updates control points limits and resizes them proportionally based on the new canvas size.
        /// </summary>
        private void UpdateControlPoints(double newCanvasSize, params CurveMapperControlPoint[] points)
        {
            foreach (var point in points.Where(p => p != null))
            {
                if (point.LimitWidth != newCanvasSize || point.LimitHeight != newCanvasSize)
                {
                    point.LimitWidth = newCanvasSize;
                    point.LimitHeight = newCanvasSize;
                    point.Point = new Point((point.Point.X / previousCanvasSize) * newCanvasSize,
                                            (point.Point.Y / previousCanvasSize) * newCanvasSize);
                }
            }
        }

        private void DrawGrid()
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
            double xPixelsPerStep = model.DynamicCanvasSize / gridSize;
            double yPixelsPerStep = model.DynamicCanvasSize / gridSize;

            for (int i = 0; i <= gridSize; i++)
            {
                double xPos = i * xPixelsPerStep;
                DrawLine(xPos, 0, xPos, model.DynamicCanvasSize);
            }

            for (int i = 0; i <= gridSize; i++)
            {
                double yPos = i * yPixelsPerStep;
                DrawLine(0, yPos, model.DynamicCanvasSize, yPos);
            }
        }

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
            if (yAdjust < mainGridMinHeight) yAdjust = mainGridMinHeight;

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            model.DynamicCanvasSize = Math.Max(xAdjust - 70, canvasMinSize);
        }

        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            model.ResetCurves();
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                model.IsLocked = !model.IsLocked;
                UpdateLockButton();
                if (button.ToolTip is ToolTip toolTip)
                {
                    toolTip.Content = model.IsLocked
                        ? CoreNodeModelWpfResources.CurveMapperUnlockButtonToolTip
                        : CoreNodeModelWpfResources.CurveMapperLockButtonToolTip;
                }
            }
        }        

        private void GraphCanvas_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        public void ToggleControlPointsMovability()
        {
            foreach (var child in GraphCanvas.Children)
            {
                if (child is CurveMapperControlPoint controlPoint)
                {
                    controlPoint.IsEnabled = !model.IsLocked;
                }
            }
        }

        private void UpdateLockButton()
        {
            if (LockButton != null)
            {
                LockButton.Tag = model.IsLocked ? "Locked" : "Unlocked";
            }
        }
    }
}
