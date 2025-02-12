using CoreNodeModels;
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
        private CurveMapperControlPoint sineWaveControlPoint1;
        private CurveMapperControlPoint sineWaveControlPoint2;
        private CurveMapperControlPoint cosineWaveControlPoint1;
        private CurveMapperControlPoint cosineWaveControlPoint2;

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

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(curveMapperNodeModel.DynamicCanvasSize))
            {
                double newSize = curveMapperNodeModel.DynamicCanvasSize;

                // Adjust control points to the new canvas size TODO : RATIONALIZE
                if (linearCurveControlPoint1 != null)
                {
                    double newX1 = (curveMapperNodeModel.LinearCurveControlPointData1.X / newSize) * newSize;
                    double newY1 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.LinearCurveControlPointData1.Y) / newSize) * newSize;

                    Canvas.SetLeft(linearCurveControlPoint1, newX1 - offsetValue);
                    Canvas.SetTop(linearCurveControlPoint1, newSize - newY1 - offsetValue);
                }
                if (linearCurveControlPoint2 != null)
                {
                    double newX2 = (curveMapperNodeModel.LinearCurveControlPointData2.X / newSize) * newSize;
                    double newY2 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.LinearCurveControlPointData2.Y) / newSize) * newSize;

                    Canvas.SetLeft(linearCurveControlPoint2, newX2 - offsetValue);
                    Canvas.SetTop(linearCurveControlPoint2, newSize - newY2 - offsetValue);
                }

                if (sineWaveControlPoint1 != null)
                {
                    double newX1 = (curveMapperNodeModel.SineWaveControlPointData1.X / newSize) * newSize;
                    double newY1 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.SineWaveControlPointData1.Y) / newSize) * newSize;

                    Canvas.SetLeft(sineWaveControlPoint1, newX1 - offsetValue);
                    Canvas.SetTop(sineWaveControlPoint1, newSize - newY1 - offsetValue);
                }
                if (sineWaveControlPoint2 != null)
                {
                    double newX2 = (curveMapperNodeModel.SineWaveControlPointData2.X / newSize) * newSize;
                    double newY2 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.SineWaveControlPointData2.Y) / newSize) * newSize;

                    Canvas.SetLeft(sineWaveControlPoint2, newX2 - offsetValue);
                    Canvas.SetTop(sineWaveControlPoint2, newSize - newY2 - offsetValue);
                }

                if (cosineWaveControlPoint1 != null)
                {
                    double newX1 = (curveMapperNodeModel.CosineWaveControlPointData1.X / newSize) * newSize;
                    double newY1 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.CosineWaveControlPointData1.Y) / newSize) * newSize;

                    Canvas.SetLeft(cosineWaveControlPoint1, newX1 - offsetValue);
                    Canvas.SetTop(cosineWaveControlPoint1, newSize - newY1 - offsetValue);
                }
                if (cosineWaveControlPoint2 != null)
                {
                    double newX2 = (curveMapperNodeModel.CosineWaveControlPointData2.X / newSize) * newSize;
                    double newY2 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.CosineWaveControlPointData2.Y) / newSize) * newSize;

                    Canvas.SetLeft(cosineWaveControlPoint2, newX2 - offsetValue);
                    Canvas.SetTop(cosineWaveControlPoint2, newSize - newY2 - offsetValue);
                }
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.SelectedGraphType))
            {
                // Remove existing control points TODO: RATIONALIZE
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

                if (sineWaveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(sineWaveControlPoint1);
                    sineWaveControlPoint1 = null;
                }
                if (sineWaveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(sineWaveControlPoint2);
                    sineWaveControlPoint2 = null;
                }

                if (cosineWaveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(cosineWaveControlPoint1);
                    cosineWaveControlPoint1 = null;
                }
                if (cosineWaveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(cosineWaveControlPoint2);
                    cosineWaveControlPoint2 = null;
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

                curveMapperNodeModel.GenerateOutputValues();
                RenderGraph();
                ToggleControlPointsLock();
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.OutputValuesX) ||
            e.PropertyName == nameof(curveMapperNodeModel.OutputValuesY))
            {
                RenderGraph();
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

            // Ensure the mainGrid doesn't resize below its minimum size
            yAdjust = Math.Max(yAdjust, curveMapperNodeModel.MinGridHeight);
            xAdjust = Math.Max(xAdjust, curveMapperNodeModel.MinGridWidth);

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            curveMapperNodeModel.DynamicCanvasSize = Math.Max(xAdjust - 70, curveMapperNodeModel.MinCanvasSize);
            DrawGrid();

            // Reposition control points based on the new size
            NodeModel_PropertyChanged(this, new PropertyChangedEventArgs(nameof(curveMapperNodeModel.DynamicCanvasSize)));
            curveMapperNodeModel.GenerateOutputValues();
        }       

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (curveMapperNodeModel.IsLocked) return;

            curveMapperNodeModel.ResetCurves();

            // Linear Curve
            // Remove existing control points before recreating them
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
            // Recreate and rebind control points
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


            // Ensure the UI moves the control points to the correct positions
            Dispatcher.Invoke(() =>
            {
                if (curveMapperNodeModel.SelectedGraphType == GraphTypes.LinearCurve)
                {
                    Canvas.SetLeft(linearCurveControlPoint1, curveMapperNodeModel.LinearCurveControlPointData1.X - offsetValue);
                    Canvas.SetTop(linearCurveControlPoint1, curveMapperNodeModel.LinearCurveControlPointData1.Y - offsetValue);
                    Canvas.SetLeft(linearCurveControlPoint2, curveMapperNodeModel.LinearCurveControlPointData2.X - offsetValue);
                    Canvas.SetTop(linearCurveControlPoint2, curveMapperNodeModel.LinearCurveControlPointData2.Y - offsetValue);
                }
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.SineWave)
                {
                    Canvas.SetLeft(sineWaveControlPoint1, curveMapperNodeModel.SineWaveControlPointData1.X - offsetValue);
                    Canvas.SetTop(sineWaveControlPoint1, curveMapperNodeModel.SineWaveControlPointData1.Y - offsetValue);
                    Canvas.SetLeft(sineWaveControlPoint2, curveMapperNodeModel.SineWaveControlPointData2.X - offsetValue);
                    Canvas.SetTop(sineWaveControlPoint2, curveMapperNodeModel.SineWaveControlPointData2.Y - offsetValue);
                }
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.CosineWave)
                {
                    Canvas.SetLeft(cosineWaveControlPoint1, curveMapperNodeModel.CosineWaveControlPointData1.X - offsetValue);
                    Canvas.SetTop(cosineWaveControlPoint1, curveMapperNodeModel.CosineWaveControlPointData1.Y - offsetValue);
                    Canvas.SetLeft(cosineWaveControlPoint2, curveMapperNodeModel.CosineWaveControlPointData2.X - offsetValue);
                    Canvas.SetTop(cosineWaveControlPoint2, curveMapperNodeModel.CosineWaveControlPointData2.Y - offsetValue);
                }

            });

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

        private void UpdateLockButton()
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
