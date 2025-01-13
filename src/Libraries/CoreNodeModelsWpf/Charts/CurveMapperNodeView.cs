using CoreNodeModelsWpf.Charts;
using CoreNodeModelsWpf.Charts.Controls;
using CoreNodeModelsWpf.Converters;
using Dynamo.Controls;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Charts
{
    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        private CurveMapperControl curveMapperControl;
        private CurveLinear linearCurve;
        private CurveMapperControlPoint startControlPointLinear;
        private CurveMapperControlPoint endControlPointLinear;

        private CurveBezier bezierCurve;
        private ControlLine curveBezierControlLine1;
        private ControlLine curveBezierControlLine2;
        private CurveMapperControlPoint pointBezierControl1;
        private CurveMapperControlPoint pointBezierControl2;
        private CurveMapperControlPointOrtho pointBezierFix1;
        private CurveMapperControlPointOrtho pointBezierFix2;
        


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
                #region Linear
                // Create the control points based on DynamicCanvasSize and add to canvas
                model.PointLinearStart = startControlPointLinear = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.PointLinearEnd = endControlPointLinear = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(startControlPointLinear, model, curveMapperControl);
                ApplyBindingsToControlPoints(endControlPointLinear, model, curveMapperControl);
                // Add the control points to the canvas
                curveMapperControl.GraphCanvas.Children.Add(startControlPointLinear);
                curveMapperControl.GraphCanvas.Children.Add(endControlPointLinear);
                Canvas.SetZIndex(startControlPointLinear, 20);
                Canvas.SetZIndex(endControlPointLinear, 20);

                // Add the linear curve
                model.LinearCurve = new CurveLinear(
                    startControlPointLinear,
                    endControlPointLinear,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize
                );
                linearCurve = model.LinearCurve;
                Canvas.SetZIndex(model.LinearCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.LinearCurve.PathCurve);

                // Assign curves to control points
                model.PointLinearStart.CurveLinear = model.LinearCurve;
                model.PointLinearEnd.CurveLinear = model.LinearCurve;
                #endregion

                #region Bezier

                // Create control points and add to the canvas
                model.PointBezierControl1 = pointBezierControl1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.2, curveMapperControl.DynamicCanvasSize * 0.2),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.PointBezierControl2 = pointBezierControl2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.8, curveMapperControl.DynamicCanvasSize * 0.2),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );

                model.PointBezierFix1 = pointBezierFix1 = new CurveMapperControlPointOrtho(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    true,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.PointBezierFix2 = pointBezierFix2 = new CurveMapperControlPointOrtho(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize),
                    true,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(pointBezierControl1);
                curveMapperControl.GraphCanvas.Children.Add(pointBezierControl2);
                curveMapperControl.GraphCanvas.Children.Add(pointBezierFix1);
                curveMapperControl.GraphCanvas.Children.Add(pointBezierFix2);
                Canvas.SetZIndex(pointBezierControl1, 10);
                Canvas.SetZIndex(pointBezierControl2, 10);
                Canvas.SetZIndex(pointBezierFix1, 20);
                Canvas.SetZIndex(pointBezierFix2, 20);

                // Create the control lines add to the canvas
                model.CurveBezierControlLine1 = curveBezierControlLine1 = new ControlLine(
                    model.PointBezierControl1.Point,
                    model.PointBezierFix1.Point
                );
                model.CurveBezierControlLine2 = curveBezierControlLine2 = new ControlLine(
                    model.PointBezierControl2.Point,
                    model.PointBezierFix2.Point
                );
                curveMapperControl.GraphCanvas.Children.Add(curveBezierControlLine1.PathCurve);
                curveMapperControl.GraphCanvas.Children.Add(curveBezierControlLine2.PathCurve);
                Canvas.SetZIndex(curveBezierControlLine1, 9);
                Canvas.SetZIndex(curveBezierControlLine2, 9);

                // Create the bezier curve and add to the canvas
                model.CurveBezier = new CurveBezier(
                    model.PointBezierFix1,
                    model.PointBezierFix2,
                    model.PointBezierControl1,
                    model.PointBezierControl2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                bezierCurve = model.CurveBezier;
                Canvas.SetZIndex(model.CurveBezier, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.CurveBezier.PathCurve);


                // Assign curves to control points
                model.PointBezierFix1.CurveBezier = model.CurveBezier;
                model.PointBezierFix2.CurveBezier = model.CurveBezier;
                model.PointBezierFix1.ControlLineBezier = model.CurveBezierControlLine1;
                model.PointBezierFix2.ControlLineBezier = model.CurveBezierControlLine2;
                model.PointBezierControl1.CurveBezier = model.CurveBezier;
                model.PointBezierControl2.CurveBezier = model.CurveBezier;
                model.PointBezierControl1.ControlLineBezier = model.CurveBezierControlLine1;
                model.PointBezierControl2.ControlLineBezier = model.CurveBezierControlLine2;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPointsOrtho(pointBezierFix1, model, curveMapperControl);
                ApplyBindingsToControlPointsOrtho(pointBezierFix2, model, curveMapperControl);

                #endregion



                // Add visibility binding if needed
                BindVisibility(model);
            };               
           
        }

        private void BindVisibility(CurveMapperNodeModel model)
        {
            // Bind the visibility of the curve and control points to the GraphType
            var linearVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.Linear, // Only show for Linear GraphType
                Mode = BindingMode.OneWay
            };
            // Bind visibility for the controls
            if (startControlPointLinear != null)
                startControlPointLinear.SetBinding(UIElement.VisibilityProperty, linearVisibilityBinding);
            if (endControlPointLinear != null)
                endControlPointLinear.SetBinding(UIElement.VisibilityProperty, linearVisibilityBinding);
            if (linearCurve != null)
                linearCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, linearVisibilityBinding);



            // Visibility binding for Bezier GraphType
            var bezierVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.Bezier, // Only show for Bezier GraphType
                Mode = BindingMode.OneWay
            };
            // Bind visibility for bezier controls
            if (pointBezierControl1 != null)
                pointBezierControl1.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (pointBezierControl2 != null)
                pointBezierControl2.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (pointBezierFix1 != null)
                pointBezierFix1.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (pointBezierFix2 != null)
                pointBezierFix2.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (curveBezierControlLine1 != null)
                curveBezierControlLine1.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (curveBezierControlLine2 != null)
                curveBezierControlLine2.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.CurveBezier != null)
                model.CurveBezier.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
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
        private void BindControlPointOrtho(CurveMapperControlPointOrtho controlPoint, DependencyProperty property, object source, string path)
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
        private void ApplyBindingsToControlPointsOrtho(CurveMapperControlPointOrtho controlPoint, CurveMapperNodeModel model, CurveMapperControl curveMapperControl)
        {
            BindControlPointOrtho(controlPoint, CurveMapperControlPointOrtho.MinLimitXPropertyOrtho, model, nameof(model.MinLimitX));
            BindControlPointOrtho(controlPoint, CurveMapperControlPointOrtho.MaxLimitXPropertyOrtho, model, nameof(model.MaxLimitX));
            BindControlPointOrtho(controlPoint, CurveMapperControlPointOrtho.MinLimitYPropertyOrtho, model, nameof(model.MinLimitY));
            BindControlPointOrtho(controlPoint, CurveMapperControlPointOrtho.MaxLimitYPropertyOrtho, model, nameof(model.MaxLimitY));
            BindControlPointOrtho(controlPoint, CurveMapperControlPointOrtho.DynamicCanvasSizeProperty, curveMapperControl, nameof(curveMapperControl.DynamicCanvasSize));
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

    public class EnumGraphTypesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GraphTypes ct = (GraphTypes)value;
            GraphTypes pt = (GraphTypes)parameter;

            if (ct.Equals(pt))
                return Visibility.Visible;

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GraphTypes)parameter;
        }
    }
}
