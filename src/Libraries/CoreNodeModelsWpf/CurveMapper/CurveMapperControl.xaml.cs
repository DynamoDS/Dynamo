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
            else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.BezierCurve)
            {
                bezierCurveControlPoint1 = new CurveMapperControlPoint(
                    curveMapperNodeModel.BezierCurveControlPointData1,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
                );
                bezierCurveControlPoint2 = new CurveMapperControlPoint(
                    curveMapperNodeModel.BezierCurveControlPointData2,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
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
            else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.PerlinNoiseCurve)
            {
                perlinNoiseCurveControlPoint1 = new CurveMapperControlPoint(
                    curveMapperNodeModel.PerlinNoiseControlPointData1,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
                );
                perlinNoiseCurveControlPoint2 = new CurveMapperControlPoint(
                    curveMapperNodeModel.PerlinNoiseControlPointData2,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
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
            else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.GaussianCurve)
            {
                gaussianCurveControlPoint1 = new CurveMapperControlPoint(
                    curveMapperNodeModel.GaussianCurveControlPointData1,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
                );
                gaussianCurveControlPoint2 = new CurveMapperControlPoint(
                    curveMapperNodeModel.GaussianCurveControlPointData2,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
                );
                gaussianCurveControlPoint3 = new CurveMapperControlPoint(
                    curveMapperNodeModel.GaussianCurveControlPointData3,
                    curveMapperNodeModel.DynamicCanvasSize,
                    curveMapperNodeModel,
                    RenderGraph
                );

                GraphCanvas.Children.Add(gaussianCurveControlPoint1);
                GraphCanvas.Children.Add(gaussianCurveControlPoint2);
                GraphCanvas.Children.Add(gaussianCurveControlPoint3);
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

                if (parabolicCurveControlPoint1 != null)
                {
                    double newX1 = (curveMapperNodeModel.ParabolicCurveControlPointData1.X / newSize) * newSize;
                    double newY1 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.ParabolicCurveControlPointData1.Y) / newSize) * newSize;

                    Canvas.SetLeft(parabolicCurveControlPoint1, newX1 - offsetValue);
                    Canvas.SetTop(parabolicCurveControlPoint1, newSize - newY1 - offsetValue);
                }
                if (parabolicCurveControlPoint2 != null)
                {
                    double newX2 = (curveMapperNodeModel.ParabolicCurveControlPointData2.X / newSize) * newSize;
                    double newY2 = ((curveMapperNodeModel.DynamicCanvasSize - curveMapperNodeModel.ParabolicCurveControlPointData2.Y) / newSize) * newSize;

                    Canvas.SetLeft(parabolicCurveControlPoint2, newX2 - offsetValue);
                    Canvas.SetTop(parabolicCurveControlPoint2, newSize - newY2 - offsetValue);
                }
            }

            if (e.PropertyName == nameof(curveMapperNodeModel.SelectedGraphType))
            {
                // Remove existing control points TODO: RATIONALIZE
                // Linear curve
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
                // Bezier curve
                if (bezierCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(bezierCurveControlPoint1);
                    bezierCurveControlPoint1 = null;
                }
                if (bezierCurveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(bezierCurveControlPoint2);
                    bezierCurveControlPoint2 = null;
                }
                if (bezierCurveControlPoint3 != null)
                {
                    GraphCanvas.Children.Remove(bezierCurveControlPoint3);
                    bezierCurveControlPoint3 = null;
                }
                if (bezierCurveControlPoint4 != null)
                {
                    GraphCanvas.Children.Remove(bezierCurveControlPoint4);
                    bezierCurveControlPoint4 = null;
                }
                // Sine wave
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
                // Cosine wave
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
                // Parabolic curve
                if (parabolicCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(parabolicCurveControlPoint1);
                    parabolicCurveControlPoint1 = null;
                }
                if (parabolicCurveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(parabolicCurveControlPoint2);
                    parabolicCurveControlPoint2 = null;
                }
                // Perlin noise curve
                if (perlinNoiseCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(perlinNoiseCurveControlPoint1);
                    perlinNoiseCurveControlPoint1 = null;
                }
                if (perlinNoiseCurveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(perlinNoiseCurveControlPoint2);
                    perlinNoiseCurveControlPoint2 = null;
                }
                if (perlinNoiseCurveControlPoint3 != null)
                {
                    GraphCanvas.Children.Remove(perlinNoiseCurveControlPoint3);
                    perlinNoiseCurveControlPoint3 = null;
                }
                // Power curve
                if (powerCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(powerCurveControlPoint1);
                    powerCurveControlPoint1 = null;
                }                
                // Square root curve
                if (squareRootCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(squareRootCurveControlPoint1);
                    squareRootCurveControlPoint1 = null;
                }
                if (squareRootCurveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(squareRootCurveControlPoint2);
                    squareRootCurveControlPoint2 = null;
                }
                // Gaussian Curve
                if (gaussianCurveControlPoint1 != null)
                {
                    GraphCanvas.Children.Remove(gaussianCurveControlPoint1);
                    gaussianCurveControlPoint1 = null;
                }
                if (gaussianCurveControlPoint2 != null)
                {
                    GraphCanvas.Children.Remove(gaussianCurveControlPoint2);
                    gaussianCurveControlPoint2 = null;
                }
                if (gaussianCurveControlPoint3 != null)
                {
                    GraphCanvas.Children.Remove(gaussianCurveControlPoint3);
                    gaussianCurveControlPoint3 = null;
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
                        RenderGraph
                    );
                    bezierCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.BezierCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
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
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.PerlinNoiseCurve)
                {
                    perlinNoiseCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.PerlinNoiseControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    perlinNoiseCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.PerlinNoiseControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
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
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.GaussianCurve)
                {
                    gaussianCurveControlPoint1 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData1,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    gaussianCurveControlPoint2 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData2,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );
                    gaussianCurveControlPoint3 = new CurveMapperControlPoint(
                        curveMapperNodeModel.GaussianCurveControlPointData3,
                        curveMapperNodeModel.DynamicCanvasSize,
                        curveMapperNodeModel,
                        RenderGraph
                    );

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
                else if (curveMapperNodeModel.SelectedGraphType == GraphTypes.ParabolicCurve)
                {
                    Canvas.SetLeft(parabolicCurveControlPoint1, curveMapperNodeModel.ParabolicCurveControlPointData1.X - offsetValue);
                    Canvas.SetTop(parabolicCurveControlPoint1, curveMapperNodeModel.ParabolicCurveControlPointData1.Y - offsetValue);
                    Canvas.SetLeft(parabolicCurveControlPoint2, curveMapperNodeModel.ParabolicCurveControlPointData2.X - offsetValue);
                    Canvas.SetTop(parabolicCurveControlPoint2, curveMapperNodeModel.ParabolicCurveControlPointData2.Y - offsetValue);
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
