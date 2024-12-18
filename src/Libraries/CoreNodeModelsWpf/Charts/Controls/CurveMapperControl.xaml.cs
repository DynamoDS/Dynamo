using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CoreNodeModelsWpf.Charts.Utilities;
using Dynamo.Wpf.UI.GuidedTour;
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
                
        public double DynamicCanvasSize
        {
            get => dynamicCanvasSize;
            set
            {
                if (dynamicCanvasSize != value)
                {
                    dynamicCanvasSize = Math.Max(value, canvasMinSize);
                    OnPropertyChanged(nameof(DynamicCanvasSize));
                }
            }
        }
        private double dynamicCanvasSize = 240;

        private readonly double canvasMinSize = 240; // also initial width and height
        private readonly double mainGridMinWidth = 310;
        private readonly double mainGridMinHeigth = 340;
        private int gridSize = 10;

        private void OnPropertyChanged(string propertyName) // RaisePropertyChanged
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

            // Redraw canvas when the input changes
            model.PropertyChanged += (s, e) =>
            {
                // Ensure all ports are connected
                // ip : do we need this anymore?
                var inPorts = model.InPorts;
                var allPortsConnected = inPorts[0].IsConnected &&
                    inPorts[1].IsConnected &&
                    inPorts[2].IsConnected &&
                    inPorts[3].IsConnected;

                if (allPortsConnected)
                {
                    if (e.PropertyName == nameof(model.MinLimitX) ||
                    e.PropertyName == nameof(model.MaxLimitX) ||
                    e.PropertyName == nameof(model.MinLimitY) ||
                    e.PropertyName == nameof(model.MaxLimitY))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
                            UpdateLabels();
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }                
            };

            // Redraw canvas when the node is resized. Do we need this?
            GraphCanvas.SizeChanged += (s, e) =>
            {
                DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
            };

            // Initial draw canvas
            DrawGrid(model.MinLimitX, model.MaxLimitX, model.MinLimitY, model.MaxLimitY);
            UpdateLabels();
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as CurveMapperNodeModel;             
            }
        }

        private void DrawGrid(double xMin, double xMax, double yMin, double yMax)
        {
            GraphCanvas.Children.Clear();

            double canvasSize = DynamicCanvasSize; // Square Canvas
            double[] stepSizes = { 1, 2, 5, 10, 20, 50, 100, 500, 1000, 10000 };

            double xRange = xMax - xMin;
            double yRange = yMax - yMin;

            double xStepSize = FindOptimalStepSize(xRange, canvasSize, stepSizes);
            double yStepSize = FindOptimalStepSize(yRange, canvasSize, stepSizes);

            double xPixelsPerUnit = canvasSize / xRange;
            double yPixelsPerUnit = canvasSize / yRange;

            double xStart = Math.Floor(xMin / xStepSize) * xStepSize;
            double yStart = Math.Floor(yMin / yStepSize) * yStepSize;

            // Draw vertical grid lines (X-axis)
            for (double x = xStart; x <= xMax; x += xStepSize)
            {
                if (x < xMin || x > xMax) continue;

                double xPos = (x - xMin) * xPixelsPerUnit;
                if (xPos >= 0 && xPos <= canvasSize)
                {
                    DrawLine(xPos, 0, xPos, canvasSize);
                }                    
            }

            // Draw horizontal grid lines (Y-axis)
            for (double y = yStart; y <= yMax; y += yStepSize)
            {
                if (y < yMin || y > yMax) continue;

                double yPos = canvasSize - (y - yMin) * yPixelsPerUnit; // Flip Y-axis
                if (yPos >= 0 && yPos <= canvasSize)
                {
                    DrawLine(0, yPos, canvasSize, yPos);
                }                
            }

            // Draw border
            DrawBorder(canvasSize, canvasSize);
        }

        private void DrawBorder(double width, double height)
        {
            var border = new System.Windows.Shapes.Rectangle
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e5e5e")),
                StrokeThickness = 0.6
            };

            Panel.SetZIndex(border, -1); // Send it behind other elements
            GraphCanvas.Children.Add(border);
        }

        private void UpdateLabels()
        {
            // Condition to display "x-min" and "x-max" if both are 0
            if (model.MinLimitX == 0 && model.MaxLimitX == 0)
            {
                minLimitXLabel.Text = "x-min";
                midXLabel.Text = "";
                maxLimitXLabel.Text = "x-max";
            }
            else
            {
                minLimitXLabel.Text = model.MinLimitX.ToString("0.##");
                midXLabel.Text = model.MidValueX.ToString("0.##");
                maxLimitXLabel.Text = model.MaxLimitX.ToString("0.##");
            }

            // Similarly for Y-axis
            if (model.MinLimitY == 0 && model.MaxLimitY == 0)
            {
                minLimitYLabel.Text = "y-min";
                midYLabel.Text = "";
                maxLimitYLabel.Text = "y-max";
            }
            else
            {
                minLimitYLabel.Text = model.MinLimitY.ToString("0.##");
                midYLabel.Text = model.MidValueY.ToString("0.##");
                maxLimitYLabel.Text = model.MaxLimitY.ToString("0.##");
            }
        }


        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var sizeChange = Math.Min(e.VerticalChange, e.HorizontalChange);
            var yAdjust = ActualHeight + sizeChange;
            var xAdjust = ActualWidth + sizeChange;

            // Ensure the node doesn't resize below its minimum size
            if (xAdjust < mainGridMinWidth) xAdjust = mainGridMinWidth;
            if (yAdjust < mainGridMinHeigth) yAdjust = mainGridMinHeigth;

            Width = xAdjust;
            Height = yAdjust;

            // Adjust the size of the GraphCanvas dynamically
            DynamicCanvasSize = Math.Max(xAdjust - 70, canvasMinSize);
        }




        /// <summary>
        /// Unsubscribes from ViewModel events
        /// </summary>
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }

        #region Helpers

        // Helper function to find the optimal step size
        private double FindOptimalStepSize(double range, double canvasSize, double[] stepSizes)
        {
            foreach(var step in stepSizes)
            {
                if (range / step <= 10) // maximum 10 columns/rows
                {
                    return step;
                }
            }
            return stepSizes.Last(); // Default to the largest step size
        }

        private double CalculateStepSize(double range, double canvasSize, double minSpacing, double maxSpacing)
        {
            /// Determine the ideal step size in pixels
            double pixelStep = canvasSize / range;

            // Adjust step size to ensure spacing is within minSpacing and maxSpacing
            if (pixelStep < minSpacing)
            {
                // Too fine: merge steps to ensure minimum spacing
                double factor = Math.Ceiling(minSpacing / pixelStep);
                return range / Math.Ceiling(range / factor);
            }
            else if (pixelStep > maxSpacing)
            {
                // Too coarse: split steps to ensure maximum spacing
                double factor = Math.Floor(pixelStep / maxSpacing);
                return range / Math.Floor(range * factor / range);
            }

            // If within bounds, show 1 unit per step
            return 1;
        }

        // Helper function to draw lines on the canvas
        private void DrawLine(double x1, double y1, double x2, double y2)
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
            GraphCanvas.Children.Add(line);
        }

        #endregion
    }
}
