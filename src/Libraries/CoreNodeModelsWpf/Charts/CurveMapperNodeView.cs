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
            model.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;

            // Bind MainGrid Width and Height to model properties
            curveMapperControl.SetBinding(CurveMapperControl.WidthProperty,
                new Binding(nameof(model.MainGridWidth)) { Source = model, Mode = BindingMode.TwoWay });
            curveMapperControl.SetBinding(CurveMapperControl.HeightProperty,
                new Binding(nameof(model.MainGridHeight)) { Source = model, Mode = BindingMode.TwoWay });

            // Add the control to the NodeView's inputGrid
            nodeView.inputGrid.Children.Add(curveMapperControl);

            // Defer adding elements until the canvas is loaded
            curveMapperControl.GraphCanvas.Loaded += (s, e) =>
            {
                #region Add curves / points to canvas

                // Linear curve
                ApplyBindingsToControlPoints(model.ControlPointLinear1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointLinear2, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointLinear1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointLinear2);
                curveMapperControl.GraphCanvas.Children.Add(model.LinearCurve.PathCurve);

                // Bezier curve
                ApplyBindingsToControlPoints(model.OrthoControlPointBezier1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointBezier2, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointBezier1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointBezier2);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointBezier1);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointBezier2);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlLineBezier1.PathCurve);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlLineBezier2.PathCurve);
                curveMapperControl.GraphCanvas.Children.Add(model.BezierCurve.PathCurve);

                // Sine wave
                ApplyBindingsToControlPoints(model.ControlPointSine1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointSine2, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointSine1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointSine2);
                curveMapperControl.GraphCanvas.Children.Add(model.SineWave.PathCurve);

                // Cosine wave
                ApplyBindingsToControlPoints(model.ControlPointCosine1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointCosine2, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointCosine1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointCosine2);
                curveMapperControl.GraphCanvas.Children.Add(model.CosineWave.PathCurve);

                // Parabolic curve
                ApplyBindingsToControlPoints(model.ControlPointParabolic1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointParabolic2, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointParabolic1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointParabolic2);
                curveMapperControl.GraphCanvas.Children.Add(model.ParabolicCurve.PathCurve);

                // Perlin noise
                ApplyBindingsToControlPoints(model.OrthoControlPointPerlin1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointPerlin2, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointPerlin, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointPerlin1);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointPerlin2);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointPerlin);
                curveMapperControl.GraphCanvas.Children.Add(model.PerlinNoiseCurve.PathCurve);

                // Power curve
                ApplyBindingsToControlPoints(model.ControlPointPower, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointPower);
                curveMapperControl.GraphCanvas.Children.Add(model.PowerCurve.PathCurve);

                // Square Root curve
                ApplyBindingsToControlPoints(model.ControlPointSquareRoot1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.ControlPointSquareRoot2, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointSquareRoot1);
                curveMapperControl.GraphCanvas.Children.Add(model.ControlPointSquareRoot2);
                curveMapperControl.GraphCanvas.Children.Add(model.SquareRootCurve.PathCurve);

                // Gaussian curve
                ApplyBindingsToControlPoints(model.OrthoControlPointGaussian1, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointGaussian2, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointGaussian3, model, curveMapperControl);
                ApplyBindingsToControlPoints(model.OrthoControlPointGaussian4, model, curveMapperControl);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointGaussian1);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointGaussian2);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointGaussian3);
                curveMapperControl.GraphCanvas.Children.Add(model.OrthoControlPointGaussian4);
                curveMapperControl.GraphCanvas.Children.Add(model.GaussianCurve.PathCurve);

                // Set a Z-index for control points and paths.
                foreach (var child in curveMapperControl.GraphCanvas.Children)
                {
                    if (child is CurveMapperControlPoint controlPoint)
                    {
                        Canvas.SetZIndex(controlPoint, 20);
                    }
                    else if (child is System.Windows.Shapes.Path path)
                    {
                        Canvas.SetZIndex(path, 10);
                    }
                }

                // Ensure Bezier control lines have the correct Z-index
                Canvas.SetZIndex(model.ControlLineBezier1.PathCurve, 9);
                Canvas.SetZIndex(model.ControlLineBezier2.PathCurve, 9);

                #endregion

                curveMapperControl.ToggleControlPointsMovability();

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
                AttachMouseUpEvent(model.ControlPointParabolic1, model.ControlPointParabolic2);
                AttachMouseUpEvent(model.ControlPointPower);
                AttachMouseUpEvent(model.ControlPointSquareRoot1, model.ControlPointSquareRoot2);
                AttachMouseUpEvent(model.OrthoControlPointGaussian1, model.OrthoControlPointGaussian2,
                    model.OrthoControlPointGaussian3, model.OrthoControlPointGaussian4);
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
            var linearVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.LinearCurve,
                Mode = BindingMode.OneWay
            };
            model.ControlPointLinear1?.SetBinding(UIElement.VisibilityProperty, linearVisBinding);
            model.ControlPointLinear2?.SetBinding(UIElement.VisibilityProperty, linearVisBinding);
            model.LinearCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, linearVisBinding);

            // Bezier curve
            var bezierVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.BezierCurve,
                Mode = BindingMode.OneWay
            };
            model.ControlPointBezier1?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);
            model.ControlPointBezier2?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);
            model.OrthoControlPointBezier1?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);
            model.OrthoControlPointBezier2?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);
            model.ControlLineBezier1?.PathCurve?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);
            model.ControlLineBezier2?.PathCurve?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);
            model.BezierCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, bezierVisBinding);

            // Sine wave
            var sineVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.SineWave,
                Mode = BindingMode.OneWay
            };
            model.ControlPointSine1?.SetBinding(UIElement.VisibilityProperty, sineVisBinding);
            model.ControlPointSine2?.SetBinding(UIElement.VisibilityProperty, sineVisBinding);
            model.SineWave?.PathCurve?.SetBinding(UIElement.VisibilityProperty, sineVisBinding);

            // Cosine wave
            var cosineVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.CosineWave,
                Mode = BindingMode.OneWay
            };
            model.ControlPointCosine1?.SetBinding(UIElement.VisibilityProperty, cosineVisBinding);
            model.ControlPointCosine2?.SetBinding(UIElement.VisibilityProperty, cosineVisBinding);
            model.CosineWave?.PathCurve?.SetBinding(UIElement.VisibilityProperty, cosineVisBinding);

            // Parabolic curve
            var parabolicVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.ParabolicCurve,
                Mode = BindingMode.OneWay
            };
            model.ControlPointParabolic1?.SetBinding(UIElement.VisibilityProperty, parabolicVisBinding);
            model.ControlPointParabolic2?.SetBinding(UIElement.VisibilityProperty, parabolicVisBinding);
            model.ParabolicCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, parabolicVisBinding);

            // Perlin noise
            var perlinVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.PerlinNoiseCurve,
                Mode = BindingMode.OneWay
            };
            model.OrthoControlPointPerlin1?.SetBinding(UIElement.VisibilityProperty, perlinVisBinding);
            model.OrthoControlPointPerlin2?.SetBinding(UIElement.VisibilityProperty, perlinVisBinding);
            model.ControlPointPerlin?.SetBinding(UIElement.VisibilityProperty, perlinVisBinding);
            model.PerlinNoiseCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, perlinVisBinding);

            // Power curve
            var powerVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.PowerCurve,
                Mode = BindingMode.OneWay
            };
            model.ControlPointPower?.SetBinding(UIElement.VisibilityProperty, powerVisBinding);
            model.PowerCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, powerVisBinding);

            // Square Root curve
            var squareRootVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.SquareRootCurve,
                Mode = BindingMode.OneWay
            };
            model.ControlPointSquareRoot1?.SetBinding(UIElement.VisibilityProperty, squareRootVisBinding);
            model.ControlPointSquareRoot2?.SetBinding(UIElement.VisibilityProperty, squareRootVisBinding);
            model.SquareRootCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, squareRootVisBinding);

            // Gaussian curve
            var gaussianVisBinding = new Binding("SelectedGraphType")
            {
                Source = model,
                Converter = new GraphTypeToVisibilityConverter(),
                ConverterParameter = GraphTypes.GaussianCurve,
                Mode = BindingMode.OneWay
            };
            model.OrthoControlPointGaussian1?.SetBinding(UIElement.VisibilityProperty, gaussianVisBinding);
            model.OrthoControlPointGaussian2?.SetBinding(UIElement.VisibilityProperty, gaussianVisBinding);
            model.GaussianCurve?.PathCurve?.SetBinding(UIElement.VisibilityProperty, gaussianVisBinding);

            var gaussianMultiBinding = new MultiBinding()
            {
                Converter = new GraphTypeAndBoundsToVisibilityConverter(),
                Mode = BindingMode.OneWay
            };
            gaussianMultiBinding.Bindings.Add(new Binding("SelectedGraphType") { Source = model });
            gaussianMultiBinding.Bindings.Add(new Binding("IsWithinBounds") { Source = model.OrthoControlPointGaussian3 });
            model.OrthoControlPointGaussian3?.SetBinding(UIElement.VisibilityProperty, gaussianMultiBinding);

            gaussianMultiBinding = new MultiBinding()
            {
                Converter = new GraphTypeAndBoundsToVisibilityConverter(),
                Mode = BindingMode.OneWay
            };
            gaussianMultiBinding.Bindings.Add(new Binding("SelectedGraphType") { Source = model });
            gaussianMultiBinding.Bindings.Add(new Binding("IsWithinBounds") { Source = model.OrthoControlPointGaussian4 });
            model.OrthoControlPointGaussian4?.SetBinding(UIElement.VisibilityProperty, gaussianMultiBinding);


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
            BindControlPoint(controlPoint, CurveMapperControlPoint.DynamicCanvasSizeProperty, curveMapperControl, nameof(model.DynamicCanvasSize));
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
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointParabolic1, curveMapperNodeModel.ControlPointParabolic2);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointPower);
            DetachMouseUpEvent(curveMapperNodeModel.ControlPointSquareRoot1, curveMapperNodeModel.ControlPointSquareRoot2);
            DetachMouseUpEvent(curveMapperNodeModel.OrthoControlPointGaussian1, curveMapperNodeModel.OrthoControlPointGaussian2,
                curveMapperNodeModel.OrthoControlPointGaussian3, curveMapperNodeModel.OrthoControlPointGaussian4);
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
                return graphType == expectedType ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GraphTypeAndBoundsToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Collapsed;

            bool isSelectedGraphType = values[0] is GraphTypes type && type == GraphTypes.GaussianCurve;
            bool isWithinBounds = values[1] is bool withinBounds && withinBounds;

            return (isSelectedGraphType && isWithinBounds) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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
