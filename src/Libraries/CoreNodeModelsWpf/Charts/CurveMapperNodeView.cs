using CoreNodeModelsWpf.Charts;
using CoreNodeModelsWpf.Charts.Controls;
using Dynamo.Controls;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Controls.SubControls;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dynamo.Wpf.Charts
{
    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        #region Properties

        private CurveMapperNodeModel curveMapperNodeModel;
        private CurveMapperControl curveMapperControl;

        #endregion


        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            // Initialize the CurveMapperControl
            curveMapperControl = new CurveMapperControl(model);
            curveMapperControl.DataContext = model;
            curveMapperNodeModel = model;
            model.CurveMapperControl = curveMapperControl;

            // Add the control to the NodeView's inputGrid
            nodeView.inputGrid.Children.Add(curveMapperControl);

            // Defer adding elements until the canvas is loaded
            curveMapperControl.GraphCanvas.Loaded += (s, e) =>
            {
                #region Create curves and points and add to canvas

                // Linear curve
                // Create the control points based on DynamicCanvasSize and add to canvas
                model.ControlPointLinear1  = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointLinear2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                // Bind properties for startControlPoint and endControlPoint
                ApplyBindingsToControlPoints(model.ControlPointLinear1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointLinear2, model, curveMapperControl);
                // Add the control points to the canvas
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointLinear1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointLinear2);
                Canvas.SetZIndex(model.ControlPointLinear1, 20);
                Canvas.SetZIndex(model.ControlPointLinear2, 20);
                // Add the linear curve
                model.LinearCurve = new LinearCurve(
                    model.ControlPointLinear1,
                    model.ControlPointLinear2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize
                );
                Canvas.SetZIndex(model.LinearCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.LinearCurve.PathCurve);
                // Assign curves to control points
                model.ControlPointLinear1.CurveLinear = model.LinearCurve;
                model.ControlPointLinear2.CurveLinear = model.LinearCurve;

                // Bezier curve
                model.ControlPointBezier1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.2, curveMapperControl.DynamicCanvasSize * 0.2),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointBezier2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.8, curveMapperControl.DynamicCanvasSize * 0.2),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.OrthoControlPointBezier1 = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, true
                );
                model.OrthoControlPointBezier2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, true
                );
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointBezier1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointBezier2);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointBezier1);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointBezier2);
                Canvas.SetZIndex(model.ControlPointBezier1, 10);
                Canvas.SetZIndex(model.ControlPointBezier2, 10);
                Canvas.SetZIndex(model.OrthoControlPointBezier1, 20);
                Canvas.SetZIndex(model.OrthoControlPointBezier2, 20);

                model.ControlLineBezier1 = new ControlLine(
                    model.ControlPointBezier1.Point,
                    model.OrthoControlPointBezier1.Point
                );
                model.ControlLineBezier2 = new ControlLine(
                    model.ControlPointBezier2.Point,
                    model.OrthoControlPointBezier2.Point
                );
                curveMapperControl.GraphCanvas.Children.Add(model.ControlLineBezier1.PathCurve);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlLineBezier2.PathCurve);
                Canvas.SetZIndex(model.ControlLineBezier1.PathCurve, 9);
                Canvas.SetZIndex(model.ControlLineBezier2.PathCurve, 9);

                model.BezierCurve = new BezierCurve(
                    model.OrthoControlPointBezier1,
                    model.OrthoControlPointBezier2,
                    model.ControlPointBezier1,
                    model.ControlPointBezier2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                Canvas.SetZIndex(model.BezierCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.BezierCurve.PathCurve);

                model.OrthoControlPointBezier1.CurveBezier = model.BezierCurve;
                model.OrthoControlPointBezier2.CurveBezier = model.BezierCurve;
                model.OrthoControlPointBezier1.ControlLineBezier = model.ControlLineBezier1;
                model.OrthoControlPointBezier2.ControlLineBezier = model.ControlLineBezier2;
                model.ControlPointBezier1.CurveBezier = model.BezierCurve;
                model.ControlPointBezier2.CurveBezier = model.BezierCurve;
                model.ControlPointBezier1.ControlLineBezier = model.ControlLineBezier1;
                model.ControlPointBezier2.ControlLineBezier = model.ControlLineBezier2;

                ApplyBindingsToControlPoints(model.OrthoControlPointBezier1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointBezier2, model, curveMapperControl);

                // Sine wave
                model.ControlPointSine1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.25, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointSine2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.75, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointSine1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointSine2);
                Canvas.SetZIndex(model.ControlPointSine1, 20);
                Canvas.SetZIndex(model.ControlPointSine2, 20);

                model.SineWave = new SineCurve(
                    model.ControlPointSine1,
                    model.ControlPointSine2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                Canvas.SetZIndex(model.SineWave, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.SineWave.PathCurve);

                model.ControlPointSine1.CurveSine = model.SineWave;
                model.ControlPointSine2.CurveSine = model.SineWave;

                ApplyBindingsToControlPoints(model.ControlPointSine1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointSine2, model, curveMapperControl);

                // Cosine wave
                // TODO: check if we should have separate Cosine Curve class
                model.ControlPointCosine1 = new CurveMapperControlPoint(
                    new Point(0, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointCosine2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointCosine1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointCosine2);
                Canvas.SetZIndex(model.ControlPointCosine1, 20);
                Canvas.SetZIndex(model.ControlPointCosine2, 20);

                model.CosineWave = new SineCurve(
                    model.ControlPointCosine1,
                    model.ControlPointCosine2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                Canvas.SetZIndex(model.CosineWave, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.CosineWave.PathCurve);

                model.ControlPointCosine1.CurveCosine = model.CosineWave;
                model.ControlPointCosine2.CurveCosine = model.CosineWave;

                ApplyBindingsToControlPoints(model.ControlPointCosine1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointCosine2, model, curveMapperControl);

                // Tangent
                model.ControlPointTangent1 = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize * 0.5),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointTangent2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize * 0.5),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointTangent1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointTangent2);
                Canvas.SetZIndex(model.ControlPointTangent1, 20);
                Canvas.SetZIndex(model.ControlPointTangent2, 20);

                model.TangentCurve = new TangentCurve(
                    model.ControlPointTangent1,
                    model.ControlPointTangent2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                Canvas.SetZIndex(model.TangentCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.TangentCurve.PathCurve);

                model.ControlPointTangent1.CurveTangent = model.TangentCurve;
                model.ControlPointTangent2.CurveTangent = model.TangentCurve;

                ApplyBindingsToControlPoints(model.ControlPointTangent1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointTangent2, model, curveMapperControl);

                // Parabolic curve
                model.ControlPointParabolic1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, curveMapperControl.DynamicCanvasSize * 0.1),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                model.ControlPointParabolic2 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointParabolic1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointParabolic2);
                Canvas.SetZIndex(model.ControlPointParabolic1, 20);
                Canvas.SetZIndex(model.ControlPointParabolic2, 20);

                model.ParabolicCurve = new ParabolicCurve(
                    model.ControlPointParabolic1,
                    model.ControlPointParabolic2,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                Canvas.SetZIndex(model.ParabolicCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.ParabolicCurve.PathCurve);

                model.ControlPointParabolic1.CurveParabolic = model.ParabolicCurve;
                model.ControlPointParabolic2.CurveParabolic = model.ParabolicCurve;

                ApplyBindingsToControlPoints(model.ControlPointParabolic1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointParabolic2, model, curveMapperControl);

                // Perlin noise
                model.OrthoControlPointPerlin1 = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, 0),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, false
                );
                model.OrthoControlPointPerlin2 = new CurveMapperControlPoint(
                    new Point(0, curveMapperControl.DynamicCanvasSize),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize,
                    true, true
                );                
                model.ControlPointPerlin = new CurveMapperControlPoint(
                    new Point(curveMapperControl.DynamicCanvasSize * 0.5, curveMapperControl.DynamicCanvasSize * 0.5),
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize,
                    model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY, curveMapperControl.DynamicCanvasSize
                );
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointPerlin1);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointPerlin2);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointPerlin);
                Canvas.SetZIndex(model.OrthoControlPointPerlin1, 20);
                Canvas.SetZIndex(model.OrthoControlPointPerlin2, 20);
                Canvas.SetZIndex(model.ControlPointPerlin, 20);

                model.PerlinNoiseCurve = new PerlinCurve(
                    model.OrthoControlPointPerlin1,
                    model.OrthoControlPointPerlin2,
                    model.ControlPointPerlin, 1,
                    curveMapperControl.DynamicCanvasSize,
                    curveMapperControl.DynamicCanvasSize);
                Canvas.SetZIndex(model.PerlinNoiseCurve, 10);
                curveMapperControl.GraphCanvas.Children.Add(model.PerlinNoiseCurve.PathCurve);

                model.OrthoControlPointPerlin1.CurvePerlin = model.PerlinNoiseCurve;
                model.OrthoControlPointPerlin2.CurvePerlin = model.PerlinNoiseCurve;
                model.ControlPointPerlin.CurvePerlin = model.PerlinNoiseCurve;

                ApplyBindingsToControlPoints(model.OrthoControlPointPerlin1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointPerlin2, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointPerlin, model, curveMapperControl);

                #endregion

                BindVisibility(model);

                // Attach event handlers to detect when control points are released,
                // to trigger curve updates and re-computation.
                AttachMouseUpEvent(model.ControlPointLinear1, model.ControlPointLinear2);
                AttachMouseUpEvent(model.ControlPointBezier1, model.ControlPointBezier2,
                    model.OrthoControlPointBezier1, model.OrthoControlPointBezier2);
                AttachMouseUpEvent(model.ControlPointSine1, model.ControlPointSine2);
                AttachMouseUpEvent(model.ControlPointCosine1, model.ControlPointCosine2);
                AttachMouseUpEvent(model.OrthoControlPointPerlin1, model.OrthoControlPointPerlin2,
                    model.ControlPointPerlin);
                AttachMouseUpEvent(model.ControlPointTangent1, model.ControlPointTangent2);
                AttachMouseUpEvent(model.ControlPointParabolic1, model.ControlPointParabolic2);
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
            // Linear curve
            var linearVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.LinearCurve,
                Mode = BindingMode.OneWay
            };
            if (model.ControlPointLinear1 != null)
                model.ControlPointLinear1.SetBinding(UIElement.VisibilityProperty, linearVisibilityBinding);
            if (model.ControlPointLinear2 != null)
                model.ControlPointLinear2.SetBinding(UIElement.VisibilityProperty, linearVisibilityBinding);
            if (model.LinearCurve != null)
                model.LinearCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, linearVisibilityBinding);

            // Bezier curve
            var bezierVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.BezierCurve,
                Mode = BindingMode.OneWay
            };
            if (model.ControlPointBezier1 != null)
                model.ControlPointBezier1.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.ControlPointBezier2 != null)
                model.ControlPointBezier2.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.OrthoControlPointBezier1 != null)
                model.OrthoControlPointBezier1.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.OrthoControlPointBezier2 != null)
                model.OrthoControlPointBezier2.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.ControlLineBezier1 != null)
                model.ControlLineBezier1.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.ControlLineBezier2 != null)
                model.ControlLineBezier2.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);
            if (model.BezierCurve != null)
                model.BezierCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, bezierVisibilityBinding);

            // Sine wave
            var sineVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.SineWave,
                Mode = BindingMode.OneWay
            };
            if (model.ControlPointSine1 != null)
                model.ControlPointSine1.SetBinding(UIElement.VisibilityProperty, sineVisibilityBinding);
            if (model.ControlPointSine2 != null)
                model.ControlPointSine2.SetBinding(UIElement.VisibilityProperty, sineVisibilityBinding);
            if (model.SineWave != null)
                model.SineWave.PathCurve.SetBinding(UIElement.VisibilityProperty, sineVisibilityBinding);

            // Cosine wave
            var cosineVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.CosineWave,
                Mode = BindingMode.OneWay
            };
            if (model.ControlPointCosine1 != null)
                model.ControlPointCosine1.SetBinding(UIElement.VisibilityProperty, cosineVisibilityBinding);
            if (model.ControlPointCosine2 != null)
                model.ControlPointCosine2.SetBinding(UIElement.VisibilityProperty, cosineVisibilityBinding);
            if (model.CosineWave != null)
                model.CosineWave.PathCurve.SetBinding(UIElement.VisibilityProperty, cosineVisibilityBinding);

            // Tangent wave
            var tangentVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.TangentWave,
                Mode = BindingMode.OneWay
            };
            if (model.ControlPointTangent1 != null)
                model.ControlPointTangent1.SetBinding(UIElement.VisibilityProperty, tangentVisibilityBinding);
            if (model.ControlPointTangent2 != null)
                model.ControlPointTangent2.SetBinding(UIElement.VisibilityProperty, tangentVisibilityBinding);
            if (model.TangentCurve != null)
                model.TangentCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, tangentVisibilityBinding);

            // Parabolic curve
            var parabolicVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.ParabolicCurve,
                Mode = BindingMode.OneWay
            };
            if (model.ControlPointParabolic1 != null)
                model.ControlPointParabolic1.SetBinding(UIElement.VisibilityProperty, parabolicVisibilityBinding);
            if (model.ControlPointParabolic2 != null)
                model.ControlPointParabolic2.SetBinding(UIElement.VisibilityProperty, parabolicVisibilityBinding);
            if (model.ParabolicCurve != null)
                model.ParabolicCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, parabolicVisibilityBinding);

            // Perlin noise
            var perlinVisibilityBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.PerlinNoiseCurve,
                Mode = BindingMode.OneWay
            };
            if (model.OrthoControlPointPerlin1 != null)
                model.OrthoControlPointPerlin1.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
            if (model.OrthoControlPointPerlin2 != null)
                model.OrthoControlPointPerlin2.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
            if (model.ControlPointPerlin != null)
                model.ControlPointPerlin.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
            if (model.PerlinNoiseCurve != null)
                model.PerlinNoiseCurve.PathCurve.SetBinding(UIElement.VisibilityProperty, perlinVisibilityBinding);
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
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointLinear1, curveMapperNodeModel.ControlPointLinear2);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointBezier1, curveMapperNodeModel.ControlPointBezier2,
                curveMapperNodeModel.OrthoControlPointBezier1, curveMapperNodeModel.OrthoControlPointBezier2);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointSine1, curveMapperNodeModel.ControlPointSine2);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointCosine1, curveMapperNodeModel.ControlPointCosine2);
            DetachMouseUpEvent(curveMapperNodeModel.OrthoControlPointPerlin1, curveMapperNodeModel.OrthoControlPointPerlin2,
                curveMapperNodeModel.ControlPointPerlin);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointTangent1, curveMapperNodeModel.ControlPointTangent2);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointParabolic1, curveMapperNodeModel.ControlPointParabolic2);
        }

        private void AttachMouseUpEvent(params CurveMapperControlPoint[] controlPoints)
        {
            foreach (var point in controlPoints)
            {
                if (point != null)
                    point.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
            }
        }

        private void DetachMouseUpEvent(params CurveMapperControlPoint[] controlPoints)
        {
            foreach (var point in controlPoints)
            {
                if (point != null)
                    point.PreviewMouseLeftButtonUp -= CanvasPreviewMouseLeftUp;
            }
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
