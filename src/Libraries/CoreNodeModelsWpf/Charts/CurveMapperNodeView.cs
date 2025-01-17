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
        private CurveMapperNodeModel curveMapperNodeModel;
        private CurveMapperControl curveMapperControl;
        private LinearCurve linearCurve;
        private CurveMapperControlPoint startControlPointLinear;
        private CurveMapperControlPoint endControlPointLinear;

        private BezierCurve bezierCurve;
        private ControlLine curveBezierControlLine1;
        private ControlLine curveBezierControlLine2;
        private CurveMapperControlPoint pointBezierControl1;
        private CurveMapperControlPoint pointBezierControl2;
        private CurveMapperControlPoint pointBezierFix1;
        private CurveMapperControlPoint pointBezierFix2;

        private CurveMapperControlPoint controlPointSine1;
        private CurveMapperControlPoint controlPointSine2;
        private SineCurve sineCurve;

        // TODO: check if we should have separate Cosine Curve class
        private CurveMapperControlPoint controlPointCosine1;
        private CurveMapperControlPoint controlPointCosine2;
        private SineCurve cosineCurve;

        private CurveMapperControlPoint controlPointTangent1;
        private CurveMapperControlPoint controlPointTangent2;
        private TangentCurve tangentCurve;

        private CurveMapperControlPoint controlPointParabolic1;
        private CurveMapperControlPoint controlPointParabolic2;
        private ParabolicCurve parabolicCurve;

        private CurveMapperControlPoint fixedPointPerlin1;
        private CurveMapperControlPoint fixedPointPerlin2;
        private CurveMapperControlPoint controlPointPerlin;
        private PerlinCurve perlinCurve;


        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            // Initialize the CurveMapperControl
            curveMapperControl = new CurveMapperControl(model);
            curveMapperControl.DataContext = model;
            curveMapperNodeModel = model;

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
                model.LinearCurve = new LinearCurve(
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
                model.BezierControlPoint1 = pointBezierControl1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.2, curveMapperControl.DynamicCanvasSize * 0.2),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.BezierControlPoint2 = pointBezierControl2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.8, curveMapperControl.DynamicCanvasSize * 0.2),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );

                model.BezierFixedPoint1 = pointBezierFix1 = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, true
                );
                model.BezierFixedPoint2 = pointBezierFix2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, true
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
                    model.BezierControlPoint1.Point,
                    model.BezierFixedPoint1.Point
                );
                model.CurveBezierControlLine2 = curveBezierControlLine2 = new ControlLine(
                    model.BezierControlPoint2.Point,
                    model.BezierFixedPoint2.Point
                );
                curveMapperControl.GraphCanvas.Children.Add(curveBezierControlLine1.PathCurve);
                curveMapperControl.GraphCanvas.Children.Add(curveBezierControlLine2.PathCurve);
                Canvas.SetZIndex(curveBezierControlLine1.PathCurve, 9);
                Canvas.SetZIndex(curveBezierControlLine2.PathCurve, 9);

                // Create the bezier curve and add to the canvas
                model.BezierCurve = new BezierCurve(
                    model.BezierFixedPoint1,
                    model.BezierFixedPoint2,
                    model.BezierControlPoint1,
                    model.BezierControlPoint2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                bezierCurve = model.BezierCurve;
                Canvas.SetZIndex(model.BezierCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.BezierCurve.PathCurve);


                // Assign curves to control points
                model.BezierFixedPoint1.CurveBezier = model.BezierCurve;
                model.BezierFixedPoint2.CurveBezier = model.BezierCurve;
                model.BezierFixedPoint1.ControlLineBezier = model.CurveBezierControlLine1;
                model.BezierFixedPoint2.ControlLineBezier = model.CurveBezierControlLine2;
                model.BezierControlPoint1.CurveBezier = model.BezierCurve;
                model.BezierControlPoint2.CurveBezier = model.BezierCurve;
                model.BezierControlPoint1.ControlLineBezier = model.CurveBezierControlLine1;
                model.BezierControlPoint2.ControlLineBezier = model.CurveBezierControlLine2;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(pointBezierFix1, model, curveMapperControl);
                ApplyBindingsToControlPoints(pointBezierFix2, model, curveMapperControl);

                #endregion

                #region Sine
                // Create control points and add to the canvas
                model.ControlPointSine1 = controlPointSine1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.25, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointSine2 = controlPointSine2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.75, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(controlPointSine1);
                curveMapperControl.GraphCanvas.Children.Add(controlPointSine2);
                Canvas.SetZIndex(controlPointSine1, 20);
                Canvas.SetZIndex(controlPointSine2, 20);

                // Create the sine curve and add to the canvas
                model.SineCurve = new SineCurve(
                    model.ControlPointSine1,
                    model.ControlPointSine2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                sineCurve = model.SineCurve;
                Canvas.SetZIndex(model.SineCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.SineCurve.PathCurve);

                ////// Assign curves to control points
                model.ControlPointSine1.CurveSine = model.SineCurve;
                model.ControlPointSine2.CurveSine = model.SineCurve;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(controlPointSine1, model, curveMapperControl);
                ApplyBindingsToControlPoints(controlPointSine2, model, curveMapperControl);
                #endregion

                #region Cosine
                // TODO: check if we should have separate Cosine Curve class
                // Create control points and add to the canvas
                model.ControlPointCosine1 = controlPointCosine1 = new CurveMapperControlPoint(
                    new Point(0, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointCosine2 = controlPointCosine2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(controlPointCosine1);
                curveMapperControl.GraphCanvas.Children.Add(controlPointCosine2);
                Canvas.SetZIndex(controlPointCosine1, 20);
                Canvas.SetZIndex(controlPointCosine2, 20);

                // Create the sine curve and add to the canvas
                model.CosineCurve = new SineCurve(
                    model.ControlPointCosine1,
                    model.ControlPointCosine2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                cosineCurve = model.CosineCurve;
                Canvas.SetZIndex(model.CosineCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.CosineCurve.PathCurve);

                ////// Assign curves to control points
                model.ControlPointCosine1.CurveCosine = model.CosineCurve;
                model.ControlPointCosine2.CurveCosine = model.CosineCurve;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(controlPointCosine1, model, curveMapperControl);
                ApplyBindingsToControlPoints(controlPointCosine2, model, curveMapperControl);
                #endregion

                #region Tangent
                // Create control points and add to the canvas
                model.ControlPointTangent1 = controlPointTangent1 = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize * 0.5),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointTangent2 = controlPointTangent2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize * 0.5),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(controlPointTangent1);
                curveMapperControl.GraphCanvas.Children.Add(controlPointTangent2);
                Canvas.SetZIndex(controlPointTangent1, 20);
                Canvas.SetZIndex(controlPointTangent2, 20);

                // Create the sine curve and add to the canvas
                model.TangentCurve = new TangentCurve(
                    model.ControlPointTangent1,
                    model.ControlPointTangent2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                tangentCurve = model.TangentCurve;
                Canvas.SetZIndex(model.TangentCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.TangentCurve.PathCurve);

                ////// Assign curves to control points
                model.ControlPointTangent1.CurveTangent = model.TangentCurve;
                model.ControlPointTangent2.CurveTangent = model.TangentCurve;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(controlPointTangent1, model, curveMapperControl);
                ApplyBindingsToControlPoints(controlPointTangent2, model, curveMapperControl);
                #endregion

                #region Parabolic
                // Create control points and add to the canvas
                model.ControlPointParabolic1 = controlPointParabolic1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, curveMapperControl.DynamicCanvasSize * 0.1),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointParabolic2 = controlPointParabolic2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(controlPointParabolic1);
                curveMapperControl.GraphCanvas.Children.Add(controlPointParabolic2);
                Canvas.SetZIndex(controlPointSine1, 20);
                Canvas.SetZIndex(controlPointSine2, 20);

                // Create the sine curve and add to the canvas
                model.ParabolicCurve = new ParabolicCurve(
                    model.ControlPointParabolic1,
                    model.ControlPointParabolic2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                parabolicCurve = model.ParabolicCurve;
                Canvas.SetZIndex(model.ParabolicCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.ParabolicCurve.PathCurve);

                ////// Assign curves to control points
                model.ControlPointParabolic1.CurveParabolic = model.ParabolicCurve;
                model.ControlPointParabolic2.CurveParabolic = model.ParabolicCurve;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(controlPointParabolic1, model, curveMapperControl);
                ApplyBindingsToControlPoints(controlPointParabolic2, model, curveMapperControl);
                #endregion

                #region Perlin
                // Create control points and add to the canvas
                model.FixedPointPerlin1 = fixedPointPerlin1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, false
                );
                model.FixedPointPerlin2 = fixedPointPerlin2 = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, true
                );                
                model.ControlPointPerlin = controlPointPerlin = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, curveMapperControl.DynamicCanvasSize * 0.5),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(fixedPointPerlin1);
                curveMapperControl.GraphCanvas.Children.Add(fixedPointPerlin2);
                curveMapperControl.GraphCanvas.Children.Add(controlPointPerlin);
                Canvas.SetZIndex(fixedPointPerlin1, 20);
                Canvas.SetZIndex(fixedPointPerlin2, 20);
                Canvas.SetZIndex(controlPointPerlin, 20);

                // Create the sine curve and add to the canvas
                model.PerlinCurve = new PerlinCurve(
                    model.FixedPointPerlin1,
                    model.FixedPointPerlin2,
                    model.ControlPointPerlin,
                    1,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                perlinCurve = model.PerlinCurve;
                Canvas.SetZIndex(model.PerlinCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.PerlinCurve.PathCurve);

                ////// Assign curves to control points
                model.FixedPointPerlin1.CurvePerlin = model.PerlinCurve;
                model.FixedPointPerlin2.CurvePerlin = model.PerlinCurve;
                model.ControlPointPerlin.CurvePerlin = model.PerlinCurve;

                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(fixedPointPerlin1, model, curveMapperControl);
                ApplyBindingsToControlPoints(fixedPointPerlin2, model, curveMapperControl);
                ApplyBindingsToControlPoints(controlPointPerlin, model, curveMapperControl);
                #endregion

                // Add visibility binding if needed
                BindVisibility(model);

                // Attach event handlers to detect when control points are released,
                // to trigger curve updates and re-computation.
                model.PointLinearStart.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.PointLinearEnd.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;

                model.BezierControlPoint1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.BezierControlPoint2.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.BezierFixedPoint1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.BezierFixedPoint2.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;

                model.ControlPointSine1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.ControlPointSine1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;

                model.ControlPointCosine1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.ControlPointCosine2.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;

                model.FixedPointPerlin1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.FixedPointPerlin2.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.ControlPointPerlin.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;

                model.ControlPointTangent1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.ControlPointTangent2.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;

                model.ControlPointParabolic1.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
                model.ControlPointParabolic2.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
            };
        }

        private void CanvasPreviewMouseLeftUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Find the associated CurveMapperNodeModel
            var nodeModel = curveMapperNodeModel;
            if (nodeModel == null)
                return;

            // Update outputs and notify Dynamo that the node has been modified
            nodeModel.GenerateOutputValues();
            nodeModel.OnNodeModified();
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
            if (model.BezierCurve != null)
                model.BezierCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);

            // Visibility binding for Sine GraphType
            var sineVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.SineWave, // Only show for Sine GraphType
                Mode = BindingMode.OneWay
            };
            if (controlPointSine1 != null)
                controlPointSine1.SetBinding(UIElement.VisibilityProperty, sineVisibilityBinding);
            if (controlPointSine2 != null)
                controlPointSine2.SetBinding(UIElement.VisibilityProperty, sineVisibilityBinding);
            if (model.SineCurve != null)
                model.SineCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, sineVisibilityBinding);

            // Visibility binding for Cosine GraphType
            var cosineVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.CosineWave, // Only show for Cosine GraphType
                Mode = BindingMode.OneWay
            };
            if (controlPointCosine1 != null)
                controlPointCosine1.SetBinding(UIElement.VisibilityProperty, cosineVisibilityBinding);
            if (controlPointCosine2 != null)
                controlPointCosine2.SetBinding(UIElement.VisibilityProperty, cosineVisibilityBinding);
            if (model.CosineCurve != null)
                model.CosineCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, cosineVisibilityBinding);

            // Visibility binding for Tangent GraphType
            var tangentVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.TangentWave, // Only show for Cosine GraphType
                Mode = BindingMode.OneWay
            };
            if (controlPointTangent1 != null)
                controlPointTangent1.SetBinding(UIElement.VisibilityProperty, tangentVisibilityBinding);
            if (controlPointTangent2 != null)
                controlPointTangent2.SetBinding(UIElement.VisibilityProperty, tangentVisibilityBinding);
            if (model.TangentCurve != null)
                model.TangentCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, tangentVisibilityBinding);

            // Visibility binding for Parabolic GraphType
            var parabolicVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.Parabola, // Only show for Parabolic GraphType
                Mode = BindingMode.OneWay
            };
            if (controlPointParabolic1 != null)
                controlPointParabolic1.SetBinding(UIElement.VisibilityProperty, parabolicVisibilityBinding);
            if (controlPointParabolic2 != null)
                controlPointParabolic2.SetBinding(UIElement.VisibilityProperty, parabolicVisibilityBinding);
            if (model.ParabolicCurve != null)
                model.ParabolicCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, parabolicVisibilityBinding);

            // Visibility binding for Perlin GraphType
            var perlinVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.PerlinNoise, // Only show for Parabolic GraphType
                Mode = BindingMode.OneWay
            };
            if (fixedPointPerlin1 != null)
                fixedPointPerlin1.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
            if (fixedPointPerlin2 != null)
                fixedPointPerlin2.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
            if (controlPointPerlin != null)
                controlPointPerlin.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
            if (model.PerlinCurve != null)
                model.PerlinCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
        }

        /// <summary>
        /// Helper method to bind a dependency property of a control point to a model or control.
        /// </summary>
        private void BindControlPoint(CurveMapperControlPoint controlPoint, DependencyProperty property, object source, string path)
        {
            controlPoint.SetBinding(property, new Binding(path)
            {
                Source = source,
                Mode = BindingMode.OneWay
            });
        }

        /// <summary>
        /// Applies bindings for both control points (startControlPoint and endControlPoint).
        /// </summary>
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
