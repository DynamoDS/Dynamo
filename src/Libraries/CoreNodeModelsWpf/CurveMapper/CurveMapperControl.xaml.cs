using CoreNodeModels;
using Dynamo.Wpf.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.CurveMapper
{
    /// <summary>
    /// Interaction logic for CurveMapperControl.xaml
    /// </summary>
    public partial class CurveMapperControl : UserControl, INotifyPropertyChanged
    {
        private readonly CurveMapperNodeModel curveMapperNodeModel;
        private CurveMapperControlPoint linearCurveControlPoint1;
        private CurveMapperControlPoint linearCurveControlPoint2;
        private CurveMapperControlPoint bezierCurveControlPoint1;
        private CurveMapperControlPoint bezierCurveControlPoint2;
        private CurveMapperControlPoint bezierCurveControlPoint3;
        private CurveMapperControlPoint bezierCurveControlPoint4;
        private CurveMapperControlPoint sineWaveControlPoint1;
        private CurveMapperControlPoint sineWaveControlPoint2;
        private CurveMapperControlPoint cosineWaveControlPoint1;
        private CurveMapperControlPoint cosineWaveControlPoint2;
        private CurveMapperControlPoint parabolicCurveControlPoint1;
        private CurveMapperControlPoint parabolicCurveControlPoint2;
        private CurveMapperControlPoint perlinNoiseCurveControlPoint1;
        private CurveMapperControlPoint perlinNoiseCurveControlPoint2;
        private CurveMapperControlPoint perlinNoiseCurveControlPoint3;
        private CurveMapperControlPoint powerCurveControlPoint1;
        private CurveMapperControlPoint squareRootCurveControlPoint1;
        private CurveMapperControlPoint squareRootCurveControlPoint2;
        private CurveMapperControlPoint gaussianCurveControlPoint1;
        private CurveMapperControlPoint gaussianCurveControlPoint2;
        private CurveMapperControlPoint gaussianCurveControlPoint3;
        private CurveMapperControlPoint gaussianCurveControlPoint4;

        private readonly Dictionary<string, (bool IsOrthogonal, bool IsVertical)> controlPointProperties =
            new Dictionary<string, (bool, bool)>
            {
                { nameof(bezierCurveControlPoint1), (true, true) },
                { nameof(bezierCurveControlPoint2), (true, true) },
                { nameof(perlinNoiseCurveControlPoint1), (true, false) },
                { nameof(perlinNoiseCurveControlPoint2), (true, true) },
                { nameof(gaussianCurveControlPoint1), (true, true) },
                { nameof(gaussianCurveControlPoint2), (true, false) },
                { nameof(gaussianCurveControlPoint3), (true, false) },
                { nameof(gaussianCurveControlPoint4), (true, false) }
            };

        private const double offsetValue = 6;
        private const int gridSize = 10;
        private const int minCanvasSize = 240;
        private const int controlLabelsWidth = 70;
        private const int controlLabelsHeight = 100;

        /// <summary> 
        /// Occurs when a property value changes, notifying bound UI elements of updates. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public CurveMapperControl(CurveMapperNodeModel model, double canvasSize)
        {
            InitializeComponent();
            this.curveMapperNodeModel = model;
            DataContext = model;

            Width = canvasSize + controlLabelsWidth;
            Height = canvasSize + controlLabelsHeight;


            model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            DrawGrid();

            // Dictionary to map UI control points to their corresponding data
            var controlPointsMap = BuildControlPointsDictionary();
            RecreateControlPoints(controlPointsMap);

            RenderCurve();

            ToggleControlPointsLock();
            UpdateLockButton();
        }

        private void RenderCurve()
        {
            // Remove existing curves (without affecting control points)
            Dispatcher.Invoke(() =>
            {
                for (int i = GraphCanvas.Children.Count - 1; i >= 0; i--)
                {
                    if (GraphCanvas.Children[i] is Path)
                    {
                        GraphCanvas.Children.RemoveAt(i);
                    }
                }

                // Determine rendering behavior based on graph type
                bool isGaussian = curveMapperNodeModel.SelectedGraphType == GraphTypes.GaussianCurve;
                bool isPerlin = curveMapperNodeModel.SelectedGraphType == GraphTypes.PerlinNoiseCurve;

                // Only render the curve on valid selection
                if (curveMapperNodeModel.SelectedGraphType != GraphTypes.Empty)
                {
                    var paths = CurveRenderer.RenderCurve(
                        curveMapperNodeModel.RenderValuesX,
                        curveMapperNodeModel.RenderValuesY,
                        curveMapperNodeModel.DynamicCanvasSize,
                        false, isGaussian, isPerlin
                    );

                    if (paths != null)
                    {
                        paths.ForEach(path => GraphCanvas.Children.Add(path));
                    }
                }

                // Render control lines for Bezier curve
                if (curveMapperNodeModel.SelectedGraphType == GraphTypes.BezierCurve)
                {
                    var controlLine1 = CurveRenderer.RenderCurve(
                        new List<double> {
                            curveMapperNodeModel.BezierCurveControlPointData1.X,
                            curveMapperNodeModel.BezierCurveControlPointData3.X
                        },
                        new List<double> {
                            curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.BezierCurveControlPointData1.Y,
                            curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.BezierCurveControlPointData3.Y
                        },
                        curveMapperNodeModel.DynamicCanvasSize,
                        true
                    );
                    var controlLine2 = CurveRenderer.RenderCurve(
                        new List<double> {
                            curveMapperNodeModel.BezierCurveControlPointData2.X,
                            curveMapperNodeModel.BezierCurveControlPointData4.X
                        },
                        new List<double> {
                            curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.BezierCurveControlPointData2.Y,
                            curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.BezierCurveControlPointData4.Y
                        },
                        curveMapperNodeModel.DynamicCanvasSize,
                        true
                    );

                    GraphCanvas.Children.Add(controlLine1.FirstOrDefault());
                    GraphCanvas.Children.Add(controlLine2.FirstOrDefault());
                }
            });
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (curveMapperNodeModel.IsLocked) return;

            curveMapperNodeModel.ResetControlPointData();

            // Dictionary to map UI control points to their corresponding data
            var controlPointsResetMap = BuildControlPointsDictionary();
            RecreateControlPoints(controlPointsResetMap);

            curveMapperNodeModel.GenerateRenderValues();
            RenderCurve();
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                curveMapperNodeModel.IsLocked = !curveMapperNodeModel.IsLocked;
                UpdateLockButton();
                ToggleControlPointsLock();
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(curveMapperNodeModel.DynamicCanvasSize))
            {
                double newSize = curveMapperNodeModel.DynamicCanvasSize;

                // Dictionary to map UI control points to their corresponding data
                var controlPointsMap = BuildControlPointsDictionary();

                // Dynamically retrieve control points from controlPointsMap
                foreach (var (pointNames, dataPoints) in controlPointsMap.Values)
                {
                    for (int i = 0; i < pointNames.Count; i++)
                    {
                        var pointField = GetType().GetField(pointNames[i], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var controlPoint = pointField?.GetValue(this) as UIElement;

                        if (controlPoint != null && dataPoints[i] != null)
                        {
                            UpdateControlPointPosition(controlPoint, dataPoints[i], newSize);
                        }
                    }
                }

                RenderCurve();
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.SelectedGraphType))
            {
                var controlPointsMap = BuildControlPointsDictionary();
                RecreateControlPoints(controlPointsMap);

                curveMapperNodeModel.GenerateRenderValues();
                RenderCurve();

                ToggleControlPointsLock();
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.RenderValuesX) ||
            e.PropertyName == nameof(curveMapperNodeModel.RenderValuesY))
            {
                RenderCurve();
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.GaussianCurveControlPointData3))
            {
                UpdateGaussianControlPoint(gaussianCurveControlPoint3, curveMapperNodeModel.GaussianCurveControlPointData3);
            }
            if (e.PropertyName == nameof(curveMapperNodeModel.GaussianCurveControlPointData4))
            {
                UpdateGaussianControlPoint(gaussianCurveControlPoint4, curveMapperNodeModel.GaussianCurveControlPointData4);
            }
        }

        private void Unload(object sender, RoutedEventArgs e)
        {
            this.curveMapperNodeModel.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }

        private void RecreateControlPoints(Dictionary<GraphTypes, (List<string> pointNames, List<ControlPointData> dataPoints)> controlPointsMap)
        {
            // Remove existing control points
            var existingControlPoints = GraphCanvas.Children.OfType<CurveMapperControlPoint>().ToList();
            foreach (var cp in existingControlPoints)
            {
                GraphCanvas.Children.Remove(cp);
            }

            // Recreate control points for the selected graph
            var selectedType = curveMapperNodeModel.SelectedGraphType;
            if (controlPointsMap.ContainsKey(selectedType))
            {
                var (pointNames, dataPoints) = controlPointsMap[selectedType];
                Type controlType = this.GetType();

                for (int i = 0; i < pointNames.Count; i++)
                {
                    // Get the field dynamically & remove the old control point
                    var pointField = controlType.GetField(pointNames[i], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var oldPoint = pointField?.GetValue(this) as CurveMapperControlPoint;
                    if (oldPoint != null) GraphCanvas.Children.Remove(oldPoint);

                    // Determine if this control point should be orthogonal or vertical
                    (bool isOrthogonal, bool isVertical) = controlPointProperties.TryGetValue(pointNames[i], out var props)
                        ? props
                        : (false, false);

                    var newPoint = new CurveMapperControlPoint(dataPoints[i], curveMapperNodeModel.DynamicCanvasSize, curveMapperNodeModel, RenderCurve, isOrthogonal, isVertical);
                    pointField?.SetValue(this, newPoint);
                    GraphCanvas.Children.Add(newPoint);

                    // Ensure correct visibility on graph load
                    double newX = dataPoints[i].X;
                    double newY = dataPoints[i].Y;
                    double canvasSize = curveMapperNodeModel.DynamicCanvasSize;
                    newPoint.Visibility = (newX < 0 || newX > canvasSize || newY < 0 || newY > canvasSize)
                        ? Visibility.Hidden
                        : Visibility.Visible;

                    Canvas.SetLeft(newPoint, dataPoints[i].X - offsetValue);
                    Canvas.SetTop(newPoint, dataPoints[i].Y - offsetValue);
                }
            }
        }

        private void UpdateControlPointPosition(UIElement controlPoint, ControlPointData dataPoint, double newSize)
        {
            if (controlPoint != null && dataPoint != null)
            {
                double newX = (dataPoint.X / newSize) * newSize;
                double newY = ((curveMapperNodeModel.DynamicCanvasSize - dataPoint.Y) / newSize) * newSize;

                Canvas.SetLeft(controlPoint, newX - offsetValue);
                Canvas.SetTop(controlPoint, newSize - newY - offsetValue);
            }
        }

        private void UpdateGaussianControlPoint(UIElement controlPoint, ControlPointData dataPoint)
        {
            if (controlPoint != null && dataPoint != null)
            {
                double newX = dataPoint.X;
                double newY = dataPoint.Y;
                double canvasSize = curveMapperNodeModel.DynamicCanvasSize;

                // Update position
                Canvas.SetLeft(controlPoint, newX - offsetValue);
                Canvas.SetTop(controlPoint, newY - offsetValue);

                // Hide if out of bounds
                controlPoint.Visibility = (newX < 0 || newX > canvasSize || newY < 0 || newY > canvasSize)
                    ? Visibility.Hidden
                    : Visibility.Visible;
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
            double xPixelsPerStep = curveMapperNodeModel.DynamicCanvasSize / gridSize;
            double yPixelsPerStep = curveMapperNodeModel.DynamicCanvasSize / gridSize;

            for (int i = 0; i <= gridSize; i++)
            {
                double xPos = i * xPixelsPerStep;
                DrawLine(xPos, 0, xPos, curveMapperNodeModel.DynamicCanvasSize);
            }

            for (int i = 0; i <= gridSize; i++)
            {
                double yPos = i * yPixelsPerStep;
                DrawLine(0, yPos, curveMapperNodeModel.DynamicCanvasSize, yPos);
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

        /// <summary>
        /// Dictionary mapping UI control points to their corresponding data.
        /// Although it seems redundant to recreate this dictionary, reusing a property does not work 
        /// because control point data references are updated dynamically.
        /// </summary>
        private Dictionary<GraphTypes, (List<string> pointNames, List<ControlPointData> dataPoints)> BuildControlPointsDictionary()
        {
            var controlPointsResetMap = new Dictionary<GraphTypes, (List<string> pointNames, List<ControlPointData> dataPoints)>
            {
                { GraphTypes.LinearCurve, (new List<string> { nameof(linearCurveControlPoint1), nameof(linearCurveControlPoint2) },
                new List<ControlPointData> { curveMapperNodeModel.LinearCurveControlPointData1, curveMapperNodeModel.LinearCurveControlPointData2 }) },

                { GraphTypes.BezierCurve, (new List<string> { nameof(bezierCurveControlPoint1), nameof(bezierCurveControlPoint2), nameof(bezierCurveControlPoint3), nameof(bezierCurveControlPoint4) },
                new List<ControlPointData> { curveMapperNodeModel.BezierCurveControlPointData1, curveMapperNodeModel.BezierCurveControlPointData2,
                    curveMapperNodeModel.BezierCurveControlPointData3, curveMapperNodeModel.BezierCurveControlPointData4 }) },

                { GraphTypes.SineWave, (new List<string> { nameof(sineWaveControlPoint1), nameof(sineWaveControlPoint2) },
                new List<ControlPointData> { curveMapperNodeModel.SineWaveControlPointData1, curveMapperNodeModel.SineWaveControlPointData2 }) },

                { GraphTypes.CosineWave, (new List<string> { nameof(cosineWaveControlPoint1), nameof(cosineWaveControlPoint2) },
                new List<ControlPointData> { curveMapperNodeModel.CosineWaveControlPointData1, curveMapperNodeModel.CosineWaveControlPointData2 }) },

                { GraphTypes.ParabolicCurve, (new List<string> { nameof(parabolicCurveControlPoint1), nameof(parabolicCurveControlPoint2) },
                new List<ControlPointData> { curveMapperNodeModel.ParabolicCurveControlPointData1, curveMapperNodeModel.ParabolicCurveControlPointData2 }) },

                { GraphTypes.PerlinNoiseCurve, (new List<string> { nameof(perlinNoiseCurveControlPoint1), nameof(perlinNoiseCurveControlPoint2), nameof(perlinNoiseCurveControlPoint3) },
                new List<ControlPointData> { curveMapperNodeModel.PerlinNoiseControlPointData1, curveMapperNodeModel.PerlinNoiseControlPointData2,
                    curveMapperNodeModel.PerlinNoiseControlPointData3 }) },

                { GraphTypes.PowerCurve, (new List<string> { nameof(powerCurveControlPoint1) },
                new List<ControlPointData> { curveMapperNodeModel.PowerCurveControlPointData1 }) },

                { GraphTypes.SquareRootCurve, (new List<string> { nameof(squareRootCurveControlPoint1), nameof(squareRootCurveControlPoint2) },
                new List<ControlPointData> { curveMapperNodeModel.SquareRootCurveControlPointData1, curveMapperNodeModel.SquareRootCurveControlPointData2 }) },

                { GraphTypes.GaussianCurve, (new List<string> { nameof(gaussianCurveControlPoint1), nameof(gaussianCurveControlPoint2), nameof(gaussianCurveControlPoint3), nameof(gaussianCurveControlPoint4) },
                new List<ControlPointData> { curveMapperNodeModel.GaussianCurveControlPointData1, curveMapperNodeModel.GaussianCurveControlPointData2,
                    curveMapperNodeModel.GaussianCurveControlPointData3, curveMapperNodeModel.GaussianCurveControlPointData4}) }
            };

            return controlPointsResetMap;
        }

        private void ToggleControlPointsLock()
        {
            foreach (var child in GraphCanvas.Children)
            {
                if (child is CurveMapperControlPoint controlPoint)
                {
                    controlPoint.IsMoveEnabled = !curveMapperNodeModel.IsLocked;
                }
            }
        }

        private void UpdateLockButton()
        {
            if (LockButton != null)
            {
                LockButton.Tag = curveMapperNodeModel.IsLocked ? "Locked" : "Unlocked";
                if (LockButton.ToolTip is ToolTip lockTooltip)
                {
                    lockTooltip.Content = curveMapperNodeModel.IsLocked
                        ? CoreNodeModelWpfResources.CurveMapperUnlockToolTip
                        : CoreNodeModelWpfResources.CurveMapperLockToolTip;
                }
                ResetButton.Tag = curveMapperNodeModel.IsLocked ? "Locked" : "Unlocked";
                if (ResetButton.ToolTip is ToolTip resetTooltip)
                {
                    resetTooltip.Content = curveMapperNodeModel.IsLocked
                        ? CoreNodeModelWpfResources.CurveMapperLockedResetToolTip
                        : CoreNodeModelWpfResources.CurveMapperResetToolTip;
                }
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var sizeChange = Math.Min(e.VerticalChange, e.HorizontalChange);
            var yAdjust = ActualHeight + sizeChange;
            var xAdjust = ActualWidth + sizeChange;

            // Ensure the mainGrid doesn't resize below its minimum size
            yAdjust = Math.Max(yAdjust, minCanvasSize + controlLabelsHeight);
            xAdjust = Math.Max(xAdjust, minCanvasSize + controlLabelsWidth);

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            curveMapperNodeModel.DynamicCanvasSize = Math.Max(xAdjust - controlLabelsWidth, minCanvasSize);
            DrawGrid();

            // Reposition control points based on the new size
            NodeModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(curveMapperNodeModel.DynamicCanvasSize)));
            curveMapperNodeModel.GenerateRenderValues();
        }
    }
}
