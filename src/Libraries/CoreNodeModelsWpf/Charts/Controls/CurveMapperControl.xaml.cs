using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CoreNodeModelsWpf.Charts.Utilities;
using GraphLayout;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControl.xaml
    /// </summary>
    public partial class CurveMapperControl : UserControl, INotifyPropertyChanged
    {
        private readonly CurveMapperNodeModel model;
        public event PropertyChangedEventHandler PropertyChanged;

        private double dynamicCanvasWidth = 180;
        private double dynamicCanvasHeight = 180;
        private readonly double minCanvasSize = 250; // also initial width and height

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CurveMapperControl(CurveMapperNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            DataContext = model;


            // ip comment : build this
            //this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;


            // ip comment : build this
            //BuildUI(model);

            // Redraw canvas when the input changes
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(model.MinLimitX) ||
                    e.PropertyName == nameof(model.MaxLimitX) ||
                    e.PropertyName == nameof(model.MinLimitY) ||
                    e.PropertyName == nameof(model.MaxLimitY))
                {
                    DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
                }
            };

            // Redraw canvas when the node is resized. Do we need this?
            GraphCanvas.SizeChanged += (s, e) =>
            {
                DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
            };

            // Initial draw canvas
            DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as CurveMapperNodeModel;

                // ip comment : build this ? do we need SkiaSharpView/libSkiaSharp ?
                //// Invoke on UI thread
                //this.Dispatcher.Invoke(() =>
                //{
                //    PieChart.Series = Enumerable.Empty<ISeries>();

                //    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                //    {
                //        var seriesRange = DefaultSeries();

                //        PieChart.Series = PieChart.Series.Concat(seriesRange);
                //    }
                //    else
                //    {
                //        var seriesRange = UpdateSeries(model);

                //        PieChart.Series = PieChart.Series.Concat(seriesRange);
                //    }
                //});
            }
        }

        private void DrawGrid(double xMin, double xMax, double yMin, double yMax)
        {
            GraphCanvas.Children.Clear();

            double xRange = xMax - xMin;
            double yRange = yMax - yMin;

            // find a way to calculate size by using ActualWidth and ActualHeigh
            //double canvasWidth = GraphCanvas.Width;
            //double canvasHeight = GraphCanvas.Height;
            double canvasWidth = this.dynamicCanvasWidth;
            double canvasHeight = this.dynamicCanvasHeight;
            double canvasWidth1 = GraphCanvas.ActualWidth;
            double canvasHeight1 = GraphCanvas.ActualHeight;


            var c = MainGrid;
            var c1 = GraphCanvas;


            8double xStep = canvasWidth / xRange;
            double yStep = canvasHeight / yRange;

            // Draw verticals
            for (double x = xMin; x <= xMax; x += 1)
            {
                double xPos = (x - xMin) * xStep;
                var line = new System.Windows.Shapes.Line
                {
                    X1 = xPos,
                    Y1 = 0,
                    X2 = xPos,
                    Y2 = canvasHeight,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e5e5e")), // adjust colors
                    StrokeThickness = 0.4 // adjust thickness
                };
                GraphCanvas.Children.Add(line);
            }

            // Draw horizontals
            for (double y = yMin; y <= yMax; y += 1)
            {
                double yPos = canvasHeight - (y - yMin) * yStep; // Flip Y-axis
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    Y1 = yPos,
                    X2 = canvasWidth,
                    Y2 = yPos,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e5e5e")), // adjust colors
                StrokeThickness = 0.4 // adjust thickness
                };
                GraphCanvas.Children.Add(line);
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            // Adjust the size of CurveMapperControl
            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= inputGrid.MinWidth && xAdjust >= ChartStyle.CHART_MIN_WIDTH)
                {
                    Width = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight && xAdjust >= ChartStyle.CHART_MIN_HEIGHT)
                {
                    Height = yAdjust;
                }
            }

            // Adjust the size of the GraphCanvas
            if (MainGrid != null)
            {
                var columnDefinition = MainGrid.ColumnDefinitions[1]; // Grid.Column="2"
                var rowDefinition = MainGrid.RowDefinitions[0]; // Grid.Row="0"

                //GraphCanvas.Width = xAdjust - 60;                
                //GraphCanvas.Height = yAdjust - 120;

                
                

                // Update the column width (GraphCanvas width)
                if (columnDefinition.Width.IsStar)
                {
                    //columnDefinition.Width = new GridLength(xAdjust - 60, GridUnitType.Pixel);
                    GraphCanvas.Width = Math.Max(minCanvasSize, xAdjust - 60);
                    dynamicCanvasWidth = xAdjust - 60;
                }

                // Update the row height (GraphCanvas height)
                if (rowDefinition.Height.IsStar)
                {
                    //rowDefinition.Height = new GridLength(yAdjust - 120, GridUnitType.Pixel);
                    GraphCanvas.Height = Math.Max(minCanvasHeight, yAdjust - 120);
                    dynamicCanvasHeight = yAdjust - 120;
                }
            }
        }




        /// <summary>
        /// Unsubscribes from ViewModel events
        /// </summary>
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }
    }
}
