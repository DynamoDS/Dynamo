using CoreNodeModelsWpf.Charts;
using CoreNodeModelsWpf.Charts.Controls;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Dynamo.Wpf.Charts
{
    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        private CurveMapperControl curveMapperControl;
        private CurveLinear linearCurve;
        private CurveMapperControlPoint startControlPoint;
        private CurveMapperControlPoint endControlPoint;

        private bool isGraphCanvasInitialized = false;


        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            // Initialize the CurveMapperControl
            curveMapperControl = new CurveMapperControl(model);
            curveMapperControl.DataContext = model;

            // Add the control to the NodeView's inputGrid
            nodeView.inputGrid.Children.Add(curveMapperControl);

            //model.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController; // check where this is used and if it is needed

            // Defer adding elements until the canvas is loaded
            curveMapperControl.GraphCanvas.Loaded += (s, e) =>
            {
                // Create the control points based on DynamicCanvasSize and add to canvas
                model.PointLinearStart = startControlPoint = new CurveMapperControlPoint(
                    new Point(0, 240),
                    240,
                    240);
                model.PointLinearEnd = endControlPoint = new CurveMapperControlPoint(
                    new Point(240, 0),
                    240,
                    240 );
                curveMapperControl.GraphCanvas.Children.Add(startControlPoint);
                curveMapperControl.GraphCanvas.Children.Add(endControlPoint);
                Canvas.SetZIndex(startControlPoint, 74);
                Canvas.SetZIndex(endControlPoint, 74);

                // Add the linear curve
                model.LinearCurve = new CurveLinear(
                    startControlPoint,
                    endControlPoint,
                    240,
                    240
                );
                linearCurve = model.LinearCurve;
                Canvas.SetZIndex(model.LinearCurve, 73);
                curveMapperControl.GraphCanvas.Children.Add(model.LinearCurve.PathCurve);

                // Assign curves to control points
                model.PointLinearStart.CurveLinear = model.LinearCurve;
                model.PointLinearEnd.CurveLinear = model.LinearCurve;

                // Optional: Attach event handlers for control points to regenerate the curve
                //startControlPoint.PointMoved += (s, e) => linearCurve?.Regenerate(startControlPoint, endControlPoint);
                //endControlPoint.PointMoved += (s, e) => linearCurve?.Regenerate(startControlPoint, endControlPoint);

                // Add visibility binding if needed
                BindVisibility(model);
            };


            
        }

        private void UpdateControlPoints(double dynamicCanvasSize)
        {
            if (startControlPoint != null)
            {
                startControlPoint.Point = new System.Windows.Point(0, dynamicCanvasSize);
                startControlPoint.LimitWidth = dynamicCanvasSize;
                startControlPoint.LimitHeight = dynamicCanvasSize;
            }

            if (endControlPoint != null)
            {
                endControlPoint.Point = new System.Windows.Point(dynamicCanvasSize, 0);
                endControlPoint.LimitWidth = dynamicCanvasSize;
                endControlPoint.LimitHeight = dynamicCanvasSize;
            }
        }

        private void BindVisibility(CurveMapperNodeModel model)
        {
            // Bind the visibility of the curve and control points to the GraphType
            var visibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.Linear, // Only show for Linear GraphType
                Mode = BindingMode.OneWay
            };

            // Bind visibility for the controls
            if (startControlPoint != null)
                startControlPoint.SetBinding(UIElement.VisibilityProperty, visibilityBinding);
            if (endControlPoint != null)
                endControlPoint.SetBinding(UIElement.VisibilityProperty, visibilityBinding);
            if (linearCurve != null)
                linearCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, visibilityBinding);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
















        // ip code: methods from ViewControll
        private void DrawGrid(Canvas graphCanvas, double xMin, double xMax, double yMin, double yMax)
        {
            graphCanvas.Children.Clear();

            double canvasSize = graphCanvas.ActualHeight; // Square Canvas
            var c1 = canvasSize;

            // Ensure grid is always 10x10
            int gridCount = 10;
            double xStepSize = (xMax - xMin) / gridCount;
            double yStepSize = (yMax - yMin) / gridCount;

            double xPixelsPerStep = canvasSize / gridCount;
            double yPixelsPerStep = canvasSize / gridCount;

            // Draw vertical grid lines (X-axis)
            for (int i = 0; i <= gridCount; i++)
            {
                double xPos = i * xPixelsPerStep;
                DrawLine(graphCanvas, xPos, 0, xPos, canvasSize);
            }

            // Draw horizontal grid lines (Y-axis)
            for (int i = 0; i <= gridCount; i++)
            {
                double yPos = i * yPixelsPerStep;
                DrawLine(graphCanvas, 0, yPos, canvasSize, yPos);
            }
        }

        // Helper function to draw lines on the canvas
        private void DrawLine(Canvas graphCanvas, double x1, double y1, double x2, double y2)
        {
            var line = new System.Windows.Shapes.Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e5e5e")), // Adjust color
                StrokeThickness = 0.6 // Slightly thicker for borders
            };
            Canvas.SetZIndex(line, 0);
            graphCanvas.Children.Add(line);
        }
    }

    public class GraphTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GraphTypes graphType && parameter is GraphTypes expectedType)
            {
                var c1 = graphType == expectedType ? Visibility.Visible : Visibility.Collapsed;
                return graphType == expectedType ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
