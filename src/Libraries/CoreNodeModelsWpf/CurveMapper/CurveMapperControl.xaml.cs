using CoreNodeModels;
using Dynamo.Graph;
using Dynamo.Wpf.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
        private const double offsetValue = 6;


        private double previousCanvasSize = 240;
        private const int gridSize = 10;

        public event PropertyChangedEventHandler PropertyChanged;

        public CurveMapperControl(CurveMapperNodeModel model)
        {
            InitializeComponent();
            this.curveMapperNodeModel = model;
            DataContext = model;

            model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

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


            RenderGraph();
            DrawGrid();
        }

        private void RenderGraph()
        {
            // Remove existing curves (without affecting control points)
            Dispatcher.Invoke(() =>
            {
                // Remove existing curves (without affecting control points)
                for (int i = GraphCanvas.Children.Count - 1; i >= 0; i--)
                {
                    if (GraphCanvas.Children[i] is Path)
                    {
                        GraphCanvas.Children.RemoveAt(i);
                    }
                }

                // Only render the curve if "Linear Curve" is selected
                if (curveMapperNodeModel.SelectedGraphType == GraphTypes.LinearCurve)
                {
                    var path = CurveRenderer.RenderCurve(
                        curveMapperNodeModel.RenderValuesX,
                        curveMapperNodeModel.RenderValuesY,
                        curveMapperNodeModel.DynamicCanvasSize
                    );

                    if (path != null)
                    {
                        GraphCanvas.Children.Add(path);
                    }
                }
            });
        }



        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e) //
        {
            //if (e.PropertyName == nameof(curveMapperNodeModel.IsLocked))
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        ToggleControlPointsLock();
            //        UpdateLockButton();
            //    });
            //}

            if (e.PropertyName == nameof(curveMapperNodeModel.DynamicCanvasSize))
            {
                double newSize = curveMapperNodeModel.DynamicCanvasSize;

                // Adjust control points to the new canvas size
                if (linearCurveControlPoint1 != null)
                {
                    double newX1 = (curveMapperNodeModel.LinearCurveControlPointData1.X / newSize) * newSize;
                    double newY1 = (curveMapperNodeModel.LinearCurveControlPointData1.Y / newSize) * newSize;

                    Canvas.SetLeft(linearCurveControlPoint1, newX1 - offsetValue);
                    Canvas.SetTop(linearCurveControlPoint1, newSize - newY1 - offsetValue); // Review this 
                }

                if (linearCurveControlPoint2 != null)
                {
                    double newX2 = (curveMapperNodeModel.LinearCurveControlPointData2.X / newSize) * newSize;
                    double newY2 = (curveMapperNodeModel.LinearCurveControlPointData2.Y / newSize) * newSize;

                    Canvas.SetLeft(linearCurveControlPoint2, newX2 - offsetValue);
                    Canvas.SetTop(linearCurveControlPoint2, newSize - newY2 - offsetValue);
                }
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.SelectedGraphType))
            {
                // Remove existing control points
                if (linearCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(linearCurveControlPoint1);
                    linearCurveControlPoint1 = null;
                }

                if (linearCurveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(linearCurveControlPoint2);
                    linearCurveControlPoint2 = null;
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

                curveMapperNodeModel.GenerateOutputValues();
                RenderGraph();
                // Dispatcher.Invoke(() => RenderGraph());
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.OutputValuesX) ||
            e.PropertyName == nameof(curveMapperNodeModel.OutputValuesY))
            {
                RenderGraph();
                // Dispatcher.Invoke(() => RenderGraph());
            }
        }
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.curveMapperNodeModel.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
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

        private void DrawLine(double x1, double y1, double x2, double y2) //
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
            yAdjust = Math.Max(yAdjust, curveMapperNodeModel.MinGridHeight);
            xAdjust = Math.Max(xAdjust, curveMapperNodeModel.MinGridWidth);

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            curveMapperNodeModel.DynamicCanvasSize = Math.Max(xAdjust - 70, curveMapperNodeModel.MinCanvasSize);
            DrawGrid();

            // 🔥 Reposition control points based on the new size
            NodeModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(curveMapperNodeModel.DynamicCanvasSize)));

            // 🔥 Ensure the curve redraws after resize
            curveMapperNodeModel.GenerateOutputValues();
        }

        private void UpdateCurvesOnResize(CurveMapperNodeModel model, double newCanvasSize) //
        {
            // Define a list of curves and their control points
            var curves = new List<(Type CurveType, object Curve, CurveMapperControlPoint[] ControlPoints)>
                {
                    //(typeof(LinearCurve), model.LinearCurve, new[] { model.ControlPointLinear1, model.ControlPointLinear2 }),
                    //(typeof(BezierCurve), model.BezierCurve, new[] { model.ControlPointBezier1, model.ControlPointBezier2,
                    //    model.OrthoControlPointBezier1, model.OrthoControlPointBezier2 }),
                    //(typeof(SineCurve), model.SineWave, new[] { model.ControlPointSine1, model.ControlPointSine2 }),
                    //(typeof(SineCurve), model.CosineWave, new[] { model.ControlPointCosine1, model.ControlPointCosine2 }),
                    //(typeof(ParabolicCurve), model.ParabolicCurve, new[] { model.ControlPointParabolic1, model.ControlPointParabolic2 }),
                    //(typeof(PerlinCurve), model.PerlinNoiseCurve, new[] { model.ControlPointPerlin, model.OrthoControlPointPerlin1,
                    //    model.OrthoControlPointPerlin2 }),
                    //(typeof(PowerCurve), model.PowerCurve, new[] { model.ControlPointPower }),
                    //(typeof(SquareRootCurve), model.SquareRootCurve, new[] { model.ControlPointSquareRoot1, model.ControlPointSquareRoot2 }),
                    //(typeof(GaussianCurve), model.GaussianCurve, new[] { model.OrthoControlPointGaussian1, model.OrthoControlPointGaussian2,
                    //    model.OrthoControlPointGaussian3, model.OrthoControlPointGaussian4 })
                };

            // Loop through each curve and update control points
            foreach (var (curveType, curve, controlPoints) in curves)
            {
                if (curve == null || controlPoints.Any(cp => cp == null)) continue;

                dynamic dynCurve = curve;

                //// Disable gaussian curve point updates
                //if (curveType == typeof(GaussianCurve))
                //{
                //    dynCurve.IsResizing = true;
                //}

                //// Update control points
                //UpdateControlPoints(newCanvasSize, controlPoints);


                dynCurve.MaxWidth = newCanvasSize;
                dynCurve.MaxHeight = newCanvasSize;
                dynCurve.Regenerate();
                Canvas.SetZIndex(dynCurve.PathCurve, 10);

                //// Handle special cases based on curve type
                //if (curveType == typeof(BezierCurve))
                //{
                //    model.ControlLineBezier1?.Regenerate(model.ControlPointBezier1, model.OrthoControlPointBezier1);
                //    model.ControlLineBezier2?.Regenerate(model.ControlPointBezier2, model.OrthoControlPointBezier2);
                //}
                //else if (curveType == typeof(ParabolicCurve))
                //{
                //    dynCurve.Regenerate(model.ControlPointParabolic1);
                //    dynCurve.Regenerate(model.ControlPointParabolic2);
                //}
                //else if (curveType == typeof(GaussianCurve))
                //{
                //    dynCurve.IsResizing = false;
                //}
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) //
        {
            //curveMapperNodeModel.ResetCurves();
        }

        private void LockButton_Click(object sender, RoutedEventArgs e) //
        {
            //var button = sender as Button;
            //if (button != null)
            //{
            //    curveMapperNodeModel.IsLocked = !curveMapperNodeModel.IsLocked;
            //    UpdateLockButton();

            //    if (button.ToolTip is ToolTip toolTip)
            //    {
            //        toolTip.Content = curveMapperNodeModel.IsLocked
            //            ? CoreNodeModelWpfResources.CurveMapperUnlockButtonToolTip
            //            : CoreNodeModelWpfResources.CurveMapperLockButtonToolTip;
            //    }
            //}
        }

        private void GraphCanvas_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Enables or disables control points based on the graph's lock state.
        /// </summary>
        public void ToggleControlPointsLock() //
        {
            //foreach (var child in GraphCanvas.Children)
            //{
            //    if (child is CurveMapperControlPoint controlPoint)
            //    {
            //        controlPoint.IsEnabled = !curveMapperNodeModel.IsLocked;
            //    }
            //}
        }

        private void UpdateLockButton() //
        {
            //if (LockButton != null)
            //{
            //    LockButton.Tag = curveMapperNodeModel.IsLocked ? "Locked" : "Unlocked";
            //}
        }
    }
}
