using CoreNodeModels;
using Lucene.Net.Search;
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

        private const double offsetValue = 6;
        private const int gridSize = 10;
        private const double MinGridWidth = 310;
        private const double MinGridHeight = 340;
        private const double MinCanvasSize = 240;
        private double previousCanvasSize = 240;

        /// <summary> 
        /// Occurs when a property value changes, notifying bound UI elements of updates. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public CurveMapperControl(CurveMapperNodeModel model)
        {
            InitializeComponent();
            this.curveMapperNodeModel = model;
            DataContext = model;

            model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            RenderGraph();
            DrawGrid();
        }

        private void RenderGraph()
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

                // Only render the curve on valid selection
                if (curveMapperNodeModel.SelectedGraphType != GraphTypes.Empty)
                {
                    var paths = CurveRenderer.RenderCurve(
                        curveMapperNodeModel.RenderValuesX,
                        curveMapperNodeModel.RenderValuesY,
                        curveMapperNodeModel.DynamicCanvasSize
                    );

                    if (paths != null)
                    {
                        foreach (var path in paths)
                        {
                            GraphCanvas.Children.Add(path);
                        }                        
                    }
                }

                // Render control line on Bezier curve
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

                    var cl1 = controlLine1[0];
                    var cl2 = controlLine2[0];

                    GraphCanvas.Children.Add(cl1);
                    GraphCanvas.Children.Add(cl2);
                }
            });
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(curveMapperNodeModel.DynamicCanvasSize))
            {
                double newSize = curveMapperNodeModel.DynamicCanvasSize;

                // Dictionary to map UI control points to their corresponding data
                var controlPointMap = new Dictionary<UIElement, ControlPointData>();

                AddToDictionary(controlPointMap, linearCurveControlPoint1, curveMapperNodeModel.LinearCurveControlPointData1);
                AddToDictionary(controlPointMap, linearCurveControlPoint2, curveMapperNodeModel.LinearCurveControlPointData2);
                AddToDictionary(controlPointMap, bezierCurveControlPoint1, curveMapperNodeModel.BezierCurveControlPointData1);
                AddToDictionary(controlPointMap, bezierCurveControlPoint2, curveMapperNodeModel.BezierCurveControlPointData2);
                AddToDictionary(controlPointMap, bezierCurveControlPoint3, curveMapperNodeModel.BezierCurveControlPointData3);
                AddToDictionary(controlPointMap, bezierCurveControlPoint4, curveMapperNodeModel.BezierCurveControlPointData4);
                AddToDictionary(controlPointMap, sineWaveControlPoint1, curveMapperNodeModel.SineWaveControlPointData1);
                AddToDictionary(controlPointMap, sineWaveControlPoint2, curveMapperNodeModel.SineWaveControlPointData2);
                AddToDictionary(controlPointMap, cosineWaveControlPoint1, curveMapperNodeModel.CosineWaveControlPointData1);
                AddToDictionary(controlPointMap, cosineWaveControlPoint2, curveMapperNodeModel.CosineWaveControlPointData2);
                AddToDictionary(controlPointMap, parabolicCurveControlPoint1, curveMapperNodeModel.ParabolicCurveControlPointData1);
                AddToDictionary(controlPointMap, parabolicCurveControlPoint2, curveMapperNodeModel.ParabolicCurveControlPointData2);
                AddToDictionary(controlPointMap, perlinNoiseCurveControlPoint1, curveMapperNodeModel.PerlinNoiseControlPointData1);
                AddToDictionary(controlPointMap, perlinNoiseCurveControlPoint2, curveMapperNodeModel.PerlinNoiseControlPointData2);
                AddToDictionary(controlPointMap, perlinNoiseCurveControlPoint3, curveMapperNodeModel.PerlinNoiseControlPointData3);
                AddToDictionary(controlPointMap, powerCurveControlPoint1, curveMapperNodeModel.PowerCurveControlPointData1);
                AddToDictionary(controlPointMap, squareRootCurveControlPoint1, curveMapperNodeModel.SquareRootCurveControlPointData1);
                AddToDictionary(controlPointMap, squareRootCurveControlPoint2, curveMapperNodeModel.SquareRootCurveControlPointData2);
                AddToDictionary(controlPointMap, gaussianCurveControlPoint1, curveMapperNodeModel.GaussianCurveControlPointData1);
                AddToDictionary(controlPointMap, gaussianCurveControlPoint2, curveMapperNodeModel.GaussianCurveControlPointData2);
                AddToDictionary(controlPointMap, gaussianCurveControlPoint3, curveMapperNodeModel.GaussianCurveControlPointData3);
                AddToDictionary(controlPointMap, gaussianCurveControlPoint4, curveMapperNodeModel.GaussianCurveControlPointData4);

                // Iterate over all control points and update their positions
                foreach (var entry in controlPointMap)
                {
                    UpdateControlPointPosition(entry.Key, entry.Value, newSize);
                }
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.SelectedGraphType))
            {
                // Dictionary to map UI control points to their corresponding data
                var controlPoints = new Dictionary<string, (UIElement Control, ControlPointData Data)>
                {
                    { "LinearCurve1", (linearCurveControlPoint1, curveMapperNodeModel.LinearCurveControlPointData1) },
                    { "LinearCurve2", (linearCurveControlPoint2, curveMapperNodeModel.LinearCurveControlPointData2) },
                    { "BezierCurve1", (bezierCurveControlPoint1, curveMapperNodeModel.BezierCurveControlPointData1) },
                    { "BezierCurve2", (bezierCurveControlPoint2, curveMapperNodeModel.BezierCurveControlPointData2) },
                    { "BezierCurve3", (bezierCurveControlPoint3, curveMapperNodeModel.BezierCurveControlPointData3) },
                    { "BezierCurve4", (bezierCurveControlPoint4, curveMapperNodeModel.BezierCurveControlPointData4) },
                    { "SineWave1", (sineWaveControlPoint1, curveMapperNodeModel.SineWaveControlPointData1) },
                    { "SineWave2", (sineWaveControlPoint2, curveMapperNodeModel.SineWaveControlPointData2) },
                    { "CosineWave1", (cosineWaveControlPoint1, curveMapperNodeModel.CosineWaveControlPointData1) },
                    { "CosineWave2", (cosineWaveControlPoint2, curveMapperNodeModel.CosineWaveControlPointData2) },
                    { "Parabolic1", (parabolicCurveControlPoint1, curveMapperNodeModel.ParabolicCurveControlPointData1) },
                    { "Parabolic2", (parabolicCurveControlPoint2, curveMapperNodeModel.ParabolicCurveControlPointData2) },
                    { "PerlinNoise1", (perlinNoiseCurveControlPoint1, curveMapperNodeModel.PerlinNoiseControlPointData1) },
                    { "PerlinNoise2", (perlinNoiseCurveControlPoint2, curveMapperNodeModel.PerlinNoiseControlPointData2) },
                    { "PerlinNoise3", (perlinNoiseCurveControlPoint3, curveMapperNodeModel.PerlinNoiseControlPointData3) },
                    { "PowerCurve", (powerCurveControlPoint1, curveMapperNodeModel.PowerCurveControlPointData1) },
                    { "SquareRoot1", (squareRootCurveControlPoint1, curveMapperNodeModel.SquareRootCurveControlPointData1) },
                    { "SquareRoot2", (squareRootCurveControlPoint2, curveMapperNodeModel.SquareRootCurveControlPointData2) },
                    { "Gaussian1", (gaussianCurveControlPoint1, curveMapperNodeModel.GaussianCurveControlPointData1) },
                    { "Gaussian2", (gaussianCurveControlPoint2, curveMapperNodeModel.GaussianCurveControlPointData2) },
                    { "Gaussian3", (gaussianCurveControlPoint3, curveMapperNodeModel.GaussianCurveControlPointData3) },
                    { "Gaussian4", (gaussianCurveControlPoint4, curveMapperNodeModel.GaussianCurveControlPointData4) }
                };

                // Remove existing control points
                foreach (var entry in controlPoints)
                {
                    if (entry.Value.Control != null)
                    {
                        GraphCanvas.Children.Remove(entry.Value.Control);
                        controlPoints[entry.Key] = (null, entry.Value.Data);
                    }
                }

                // Re-add control points if "Linear Curve" is selected
                if (curveMapperNodeModel.SelectedGraphType == GraphTypes.LinearCurve)
                {
                    linearCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.LinearCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    linearCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.LinearCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(linearCurveControlPoint1);
                    GraphCanvas.Children.Add(linearCurveControlPoint2);
                }
                // Bezier curve
                if (curveMapperNodeModel.SelectedGraphType == GraphTypes.BezierCurve)
                {
                    bezierCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.BezierCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, true
                    );
                    bezierCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.BezierCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, true
                    );
                    bezierCurveControlPoint3 = new CurveMapperControlPoint(
                        curveMapperNodeModel.BezierCurveControlPointData3,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    bezierCurveControlPoint4 = new CurveMapperControlPoint(
                        curveMapperNodeModel.BezierCurveControlPointData4,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(bezierCurveControlPoint1);
                    GraphCanvas.Children.Add(bezierCurveControlPoint2);
                    GraphCanvas.Children.Add(bezierCurveControlPoint3);
                    GraphCanvas.Children.Add(bezierCurveControlPoint4);
                }
                // Sine wave
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.SineWave)
                {
                    sineWaveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.SineWaveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    sineWaveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.SineWaveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(sineWaveControlPoint1);
                    GraphCanvas.Children.Add(sineWaveControlPoint2);
                }
                // Cosine wave
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.CosineWave)
                {
                    cosineWaveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.CosineWaveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    cosineWaveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.CosineWaveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(cosineWaveControlPoint1);
                    GraphCanvas.Children.Add(cosineWaveControlPoint2);
                }
                // Parabolic curve
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.ParabolicCurve)
                {
                    parabolicCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.ParabolicCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    parabolicCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.ParabolicCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(parabolicCurveControlPoint1);
                    GraphCanvas.Children.Add(parabolicCurveControlPoint2);
                }
                // Perlin noise curve
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.PerlinNoiseCurve)
                {
                    perlinNoiseCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.PerlinNoiseControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, false
                    );
                    perlinNoiseCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.PerlinNoiseControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, true
                    );
                    perlinNoiseCurveControlPoint3 = new CurveMapperControlPoint(
                        curveMapperNodeModel.PerlinNoiseControlPointData3,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(perlinNoiseCurveControlPoint1);
                    GraphCanvas.Children.Add(perlinNoiseCurveControlPoint2);
                    GraphCanvas.Children.Add(perlinNoiseCurveControlPoint3);
                }
                // Power curve
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.PowerCurve)
                {
                    powerCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.PowerCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(powerCurveControlPoint1);
                }
                // Square root curve
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.SquareRootCurve)
                {
                    squareRootCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.SquareRootCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    squareRootCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.SquareRootCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

                    GraphCanvas.Children.Add(squareRootCurveControlPoint1);
                    GraphCanvas.Children.Add(squareRootCurveControlPoint2);
                }
                // Gaussian curve
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.GaussianCurve)
                {
                    gaussianCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, true
                    );
                    gaussianCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, false
                    );
                    gaussianCurveControlPoint3 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData3,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, false
                    );
                    gaussianCurveControlPoint4 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData4,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph,
                        true, false
                    );

                    GraphCanvas.Children.Add(gaussianCurveControlPoint1);
                    GraphCanvas.Children.Add(gaussianCurveControlPoint2);
                    GraphCanvas.Children.Add(gaussianCurveControlPoint3);
                    GraphCanvas.Children.Add(gaussianCurveControlPoint4);
                }

                curveMapperNodeModel.GenerateOutputValues();
                RenderGraph();
                ToggleControlPointsLock();
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.OutputValuesX) ||
            e.PropertyName == nameof(curveMapperNodeModel.OutputValuesY))
            {
                RenderGraph();
            }

            // Handle changes in Gaussian curve control points
            bool point2Updating = false;
            if (e.PropertyName == nameof(curveMapperNodeModel.GaussianCurveControlPointData2))
            {
                point2Updating = true;
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

        // Helper (for resizing node)
        private void AddToDictionary(Dictionary<UIElement, ControlPointData> dictionary, UIElement controlPoint, ControlPointData dataPoint) //
        {
            if (controlPoint != null && dataPoint != null)
            {
                dictionary.Add(controlPoint, dataPoint);
            }
        }

        // Helper (for resizing node)
        private void UpdateControlPointPosition(UIElement controlPoint, ControlPointData dataPoint, double newSize) //
        {
            if (controlPoint != null && dataPoint != null)
            {
                double newX = (dataPoint.X / newSize) * newSize;
                double newY = ((curveMapperNodeModel.DynamicCanvasSize - dataPoint.Y) / newSize) * newSize;

                Canvas.SetLeft(controlPoint, newX - offsetValue);
                Canvas.SetTop(controlPoint, newSize - newY - offsetValue);
            }
        }

        // TODO : Review, might be connected to the Gaussian curve bug
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

        private void Unload(object sender, RoutedEventArgs e)
        {
            this.curveMapperNodeModel.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }

        private void DrawGrid() //
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

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e) //
        {
            var sizeChange = Math.Min(e.VerticalChange, e.HorizontalChange);
            var yAdjust = ActualHeight + sizeChange;
            var xAdjust = ActualWidth + sizeChange;

            // Ensure the mainGrid doesn't resize below its minimum size
            yAdjust = Math.Max(yAdjust, MinGridHeight);
            xAdjust = Math.Max(xAdjust, MinGridWidth);

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            curveMapperNodeModel.DynamicCanvasSize = Math.Max(xAdjust - 70, MinCanvasSize);
            DrawGrid();

            // Reposition control points based on the new size
            NodeModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(curveMapperNodeModel.DynamicCanvasSize)));
            curveMapperNodeModel.GenerateOutputValues();
        }       

        private void ResetButton_Click(object sender, RoutedEventArgs e) //
        {
            if (curveMapperNodeModel.IsLocked) return;

            curveMapperNodeModel.ResetControlPointData();

            // Dictionary to store curve types and their respective control points
            var controlPointsMap = new Dictionary<GraphTypes, (List<string> pointNames, List<ControlPointData> dataPoints)>
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

            // List of control points that need `isOrthogonal = true`
            var orthogonalPoints = new HashSet<string> {
                nameof(bezierCurveControlPoint1), nameof(bezierCurveControlPoint2),
                nameof(perlinNoiseCurveControlPoint1), nameof(perlinNoiseCurveControlPoint2),
                nameof(gaussianCurveControlPoint1), nameof(gaussianCurveControlPoint2), nameof(gaussianCurveControlPoint3), nameof(gaussianCurveControlPoint4)
            };

            // List of control points that need both `isOrthogonal = true` and `isVertical = true`
            var verticalPoints = new HashSet<string>
            {
                nameof(bezierCurveControlPoint1), nameof(bezierCurveControlPoint2),
                nameof(perlinNoiseCurveControlPoint2), nameof(gaussianCurveControlPoint1)
            };


            Type controlType = this.GetType();
            GraphTypes selectedType = curveMapperNodeModel.SelectedGraphType;

            if (controlPointsMap.ContainsKey(selectedType))
            {
                var (pointNames, dataPoints) = controlPointsMap[selectedType];

                for (int i = 0; i < pointNames.Count; i++)
                {
                    // Get the field dynamically
                    var pointField = controlType.GetField(pointNames[i], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var oldPoint = pointField?.GetValue(this) as CurveMapperControlPoint;

                    // Remove old control point from the canvas if applicable
                    if (oldPoint != null) GraphCanvas.Children.Remove(oldPoint);

                    // Determine if this control point should be orthogonal or vertical
                    bool isOrthogonal = orthogonalPoints.Contains(pointNames[i]);
                    bool isVertical = verticalPoints.Contains(pointNames[i]);

                    // Create new instance of the control point with the correct flags
                    var newPoint = new CurveMapperControlPoint(dataPoints[i], curveMapperNodeModel.DynamicCanvasSize, curveMapperNodeModel, RenderGraph, isOrthogonal, isVertical);

                    // Assign new instance to the original field
                    pointField?.SetValue(this, newPoint);
                }

                // Ensure the UI updates control points **only if a UI is present**
                if (Application.Current != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < pointNames.Count; i++)
                        {
                            var pointField = controlType.GetField(pointNames[i], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var point = pointField?.GetValue(this) as CurveMapperControlPoint;

                            if (point != null)
                            {
                                GraphCanvas.Children.Add(point);
                                Canvas.SetLeft(point, dataPoints[i].X - offsetValue);
                                Canvas.SetTop(point, dataPoints[i].Y - offsetValue);
                            }
                        }
                    });
                }
            }

            curveMapperNodeModel.GenerateOutputValues();
            RenderGraph();
        }

        private void LockButton_Click(object sender, RoutedEventArgs e) //
        {
            var button = sender as Button;
            if (button != null)
            {
                curveMapperNodeModel.IsLocked = !curveMapperNodeModel.IsLocked;
                UpdateLockButton();
                ToggleControlPointsLock();
            }
        }

        private void GraphCanvas_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void ToggleControlPointsLock() //
        {
            foreach (var child in GraphCanvas.Children)
            {
                if (child is CurveMapperControlPoint controlPoint)
                {
                    controlPoint.IsEnabled = !curveMapperNodeModel.IsLocked;
                }
            }
        }

        private void UpdateLockButton() //
        {
            if (LockButton != null)
            {
                LockButton.Tag = curveMapperNodeModel.IsLocked ? "Locked" : "Unlocked";
                if (LockButton.ToolTip is ToolTip lockTooltip)
                {
                    lockTooltip.Content = curveMapperNodeModel.IsLocked
                        ? "Curve cannot be modified. Click to unlock."
                        : "Click to lock the curve.";
                }
                ResetButton.Tag = curveMapperNodeModel.IsLocked ? "Locked" : "Unlocked";
                if (ResetButton.ToolTip is ToolTip resetTooltip)
                {
                    resetTooltip.Content = curveMapperNodeModel.IsLocked
                        ? "The curve has been locked and cannot be reset. Please unlock the curve first."
                        : "Reset the curve.";
                }
            }
        }
    }
}
