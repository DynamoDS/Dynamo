using CoreNodeModelsWpf.Charts;
using CoreNodeModelsWpf.Charts.Controls;
using Dynamo.Controls;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

            // Defer adding elements until the canvas is loaded
            curveMapperControl.GraphCanvas.Loaded += (s, e) =>
            {
                // Create the control points based on DynamicCanvasSize and add to canvas
                model.PointLinearStart = startControlPoint = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.PointLinearEnd = endControlPoint = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(startControlPoint, model, curveMapperControl);
                ApplyBindingsToControlPoints(endControlPoint, model, curveMapperControl);
                // Add the control points to the canvas
                curveMapperControl.GraphCanvas.Children.Add(startControlPoint);
                curveMapperControl.GraphCanvas.Children.Add(endControlPoint);
                Canvas.SetZIndex(startControlPoint, 20);
                Canvas.SetZIndex(endControlPoint, 20);

                // Add the linear curve
                model.LinearCurve = new CurveLinear(
                    startControlPoint,
                    endControlPoint,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize
                );
                linearCurve = model.LinearCurve;
                Canvas.SetZIndex(model.LinearCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.LinearCurve.PathCurve);
                // Assign curves to control points
                model.PointLinearStart.CurveLinear = model.LinearCurve;
                model.PointLinearEnd.CurveLinear = model.LinearCurve;

                // Add visibility binding if needed
                BindVisibility(model);
            };
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

        /// <summary> Helper method to bind a dependency property of a control point to a model or control.</summary>
        private void BindControlPoint(CurveMapperControlPoint controlPoint, DependencyProperty property, object source, string path)
        {
            controlPoint.SetBinding(property, new Binding(path)
            {
                Source = source,
                Mode = BindingMode.OneWay
            });
        }

        /// <summary> Applies bindings for both control points (startControlPoint and endControlPoint) </summary>
        private void ApplyBindingsToControlPoints(CurveMapperControlPoint controlPoint, CurveMapperNodeModel model, CurveMapperControl curveMapperControl)
        {
            BindControlPoint(controlPoint, CurveMapperControlPoint.MinLimitXProperty, model, nameof(model.MinLimitX));
            BindControlPoint(controlPoint, CurveMapperControlPoint.MaxLimitXProperty, model, nameof(model.MaxLimitX));
            BindControlPoint(controlPoint, CurveMapperControlPoint.MinLimitYProperty, model, nameof(model.MinLimitY));
            BindControlPoint(controlPoint, CurveMapperControlPoint.MaxLimitYProperty, model, nameof(model.MaxLimitY));
            BindControlPoint(controlPoint, CurveMapperControlPoint.DynamicCanvasSizeProperty, curveMapperControl, nameof(curveMapperControl.DynamicCanvasSize));
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a GraphType value to a Visibility state based on a specified expected type.
    /// </summary>
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
