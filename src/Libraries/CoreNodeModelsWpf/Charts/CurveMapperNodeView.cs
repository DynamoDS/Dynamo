using CoreNodeModelsWpf.Charts;
using CoreNodeModelsWpf.Charts.Controls;
using Dynamo.Controls;
using Dynamo.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dynamo.Wpf.Charts
{
    public class CurveMapperNodeView : INodeViewCustomization<CurveMapperNodeModel>
    {
        private CurveMapperNodeModel curveMapperNodeModel;
        private CurveMapperControl curveMapperControl;

        public void CustomizeView(CurveMapperNodeModel model, NodeView nodeView)
        {
            // Initialize the CurveMapperControl
            curveMapperControl = new CurveMapperControl(model);
            curveMapperControl.DataContext = model;
            curveMapperNodeModel = model;
            model.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController; // TODO: Remove if not required

            // Add the control to inputGrid and bind its grid size to the model properties
            nodeView.inputGrid.Children.Add(curveMapperControl);
            BindGridSize(curveMapperNodeModel);

            // Defer adding elements until the canvas is loaded
            curveMapperControl.GraphCanvas.Loaded += (s, e) =>
            {
                // Add control points and curves to the canvas and bind visibility properties
                AddPointsAndCurvesToCanvas(curveMapperNodeModel);
                BindVisibility(model);

                // Lock or unlock control points based on graph state
                curveMapperControl.ToggleControlPointsLock();

                // Attach event handlers to update curves when control points are moved
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

        private void AddPointsAndCurvesToCanvas(CurveMapperNodeModel model) //
        {
            var graphElements = new Dictionary<GraphTypes, List<UIElement>>
            {
                { GraphTypes.LinearCurve, new List<UIElement> { model.ControlPointLinear1, model.ControlPointLinear2, model.LinearCurve?.PathCurve } },
                { GraphTypes.BezierCurve, new List<UIElement> { model.ControlPointBezier1, model.ControlPointBezier2, model.OrthoControlPointBezier1,
                    model.OrthoControlPointBezier2, model.ControlLineBezier1?.PathCurve, model.ControlLineBezier2?.PathCurve, model.BezierCurve?.PathCurve } },
                { GraphTypes.SineWave, new List<UIElement> { model.ControlPointSine1, model.ControlPointSine2, model.SineWave?.PathCurve } },
                { GraphTypes.CosineWave, new List<UIElement> { model.ControlPointCosine1, model.ControlPointCosine2, model.CosineWave?.PathCurve } },
                { GraphTypes.ParabolicCurve, new List<UIElement> { model.ControlPointParabolic1, model.ControlPointParabolic2, model.ParabolicCurve?.PathCurve } },
                { GraphTypes.PerlinNoiseCurve, new List<UIElement> { model.OrthoControlPointPerlin1, model.OrthoControlPointPerlin2, model.ControlPointPerlin, model.PerlinNoiseCurve?.PathCurve } },
                { GraphTypes.PowerCurve, new List<UIElement> { model.ControlPointPower, model.PowerCurve?.PathCurve } },
                { GraphTypes.SquareRootCurve, new List<UIElement> { model.ControlPointSquareRoot1, model.ControlPointSquareRoot2, model.SquareRootCurve?.PathCurve } },
                { GraphTypes.GaussianCurve, new List<UIElement> { model.OrthoControlPointGaussian1, model.OrthoControlPointGaussian2, model.OrthoControlPointGaussian3,
                    model.OrthoControlPointGaussian4, model.GaussianCurve?.PathCurve } }
            };

            // Apply bindings and add to canvas
            foreach (var elements in graphElements.Values)
            {
                foreach (var element in elements)
                {
                    if (element is CurveMapperControlPoint controlPoint)
                    {
                        ApplyBindingsToControlPoints(controlPoint, model, curveMapperControl);
                    }
                    curveMapperControl.GraphCanvas.Children.Add(element);
                }
            }

            // Set Z-index for control points and paths
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
        }

        private void CanvasPreviewMouseLeftUp(object sender, System.Windows.Input.MouseButtonEventArgs e) //
        {
            var nodeModel = curveMapperNodeModel;
            if (nodeModel == null) return;

            nodeModel.GenerateOutputValues();
            nodeModel.OnNodeModified();
        }

        private void BindVisibility(CurveMapperNodeModel model) //
        {
            var visibilityBindings = new Dictionary<GraphTypes, UIElement[]>
            {
                { GraphTypes.LinearCurve, new UIElement[] { model.ControlPointLinear1, model.ControlPointLinear2, model.LinearCurve?.PathCurve } },
                { GraphTypes.BezierCurve, new UIElement[] { model.ControlPointBezier1, model.ControlPointBezier2, model.OrthoControlPointBezier1,
                    model.OrthoControlPointBezier2, model.ControlLineBezier1?.PathCurve, model.ControlLineBezier2?.PathCurve, model.BezierCurve?.PathCurve } },
                { GraphTypes.SineWave, new UIElement[] { model.ControlPointSine1, model.ControlPointSine2, model.SineWave?.PathCurve } },
                { GraphTypes.CosineWave, new UIElement[] { model.ControlPointCosine1, model.ControlPointCosine2, model.CosineWave?.PathCurve } },
                { GraphTypes.ParabolicCurve, new UIElement[] { model.ControlPointParabolic1, model.ControlPointParabolic2, model.ParabolicCurve?.PathCurve } },
                { GraphTypes.PerlinNoiseCurve, new UIElement[] { model.OrthoControlPointPerlin1, model.OrthoControlPointPerlin2, model.ControlPointPerlin,
                    model.PerlinNoiseCurve?.PathCurve } },
                { GraphTypes.PowerCurve, new UIElement[] { model.ControlPointPower, model.PowerCurve?.PathCurve } },
                { GraphTypes.SquareRootCurve, new UIElement[] { model.ControlPointSquareRoot1, model.ControlPointSquareRoot2, model.SquareRootCurve?.PathCurve } },
                { GraphTypes.GaussianCurve, new UIElement[] { model.OrthoControlPointGaussian1, model.OrthoControlPointGaussian2, model.GaussianCurve?.PathCurve } }
            };

            // Iterate through the dictionary and set bindings dynamically
            foreach (var entry in visibilityBindings)
            {
                var binding = new Binding("SelectedGraphType")
                {
                    Source = model,
                    Converter = new GraphTypeToVisibilityConverter(),
                    ConverterParameter = entry.Key,
                    Mode = BindingMode.OneWay
                };

                foreach (var element in entry.Value)
                {
                    (element as FrameworkElement)?.SetBinding(UIElement.VisibilityProperty, binding);
                }
            }

            // Gaussian MultiBindings
            foreach (var controlPoint in new UIElement[] { model.OrthoControlPointGaussian3, model.OrthoControlPointGaussian4 })
            {
                var gaussianMultiBinding = new MultiBinding()
                {
                    Converter = new GraphTypeAndBoundsToVisibilityConverter(),
                    Mode = BindingMode.OneWay
                };
                gaussianMultiBinding.Bindings.Add(new Binding("SelectedGraphType") { Source = model });
                gaussianMultiBinding.Bindings.Add(new Binding("IsWithinBounds") { Source = controlPoint });

                (controlPoint as FrameworkElement)?.SetBinding(UIElement.VisibilityProperty, gaussianMultiBinding);
            }
        }

        /// <summary>
        /// Helper method to bind a dependency property of a control point to a model or control.
        /// </summary>
        private void BindControlPoint(CurveMapperControlPoint controlPoint, DependencyProperty property, object source, string path) //
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
        private void ApplyBindingsToControlPoints(CurveMapperControlPoint controlPoint, CurveMapperNodeModel model, CurveMapperControl curveMapperControl) //
        {
            BindControlPoint(controlPoint, CurveMapperControlPoint.MinLimitXProperty, model, nameof(model.MinLimitX));
            BindControlPoint(controlPoint, CurveMapperControlPoint.MaxLimitXProperty, model, nameof(model.MaxLimitX));
            BindControlPoint(controlPoint, CurveMapperControlPoint.MinLimitYProperty, model, nameof(model.MinLimitY));
            BindControlPoint(controlPoint, CurveMapperControlPoint.MaxLimitYProperty, model, nameof(model.MaxLimitY));
            BindControlPoint(controlPoint, CurveMapperControlPoint.DynamicCanvasSizeProperty, curveMapperControl, nameof(model.DynamicCanvasSize));
        }

        private void BindGridSize(CurveMapperNodeModel model) //
        {
            curveMapperControl.SetBinding(CurveMapperControl.WidthProperty,
                new Binding(nameof(model.MainGridWidth))
                { Source = model, Mode = BindingMode.TwoWay });

            curveMapperControl.SetBinding(CurveMapperControl.HeightProperty,
                new Binding(nameof(model.MainGridHeight))
                { Source = model, Mode = BindingMode.TwoWay });
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

        private void AttachMouseUpEvent(params CurveMapperControlPoint[] controlPoints) //
        {
            foreach (var point in controlPoints)
            {
                if (point != null)
                    point.PreviewMouseLeftButtonUp += CanvasPreviewMouseLeftUp;
            }
        }

        private void DetachMouseUpEvent(params CurveMapperControlPoint[] controlPoints) //
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

    /// <summary>
    /// Converts a selected graph type and a boolean bound check into a visibility state.
    /// </summary>
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
}
