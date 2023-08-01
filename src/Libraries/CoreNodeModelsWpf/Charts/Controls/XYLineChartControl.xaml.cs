using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CoreNodeModelsWpf.Charts.Utilities;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for XYLineChartControl.xaml
    /// </summary>
    public partial class XYLineChartControl : UserControl, INotifyPropertyChanged
    {
        private readonly XYLineChartNodeModel model;

        private SolidColorPaint AxisColor { get; set; }
        private SolidColorPaint AxisSeparatorColor { get; set; }

        /// <summary>
        /// Used to get or set the X-axis of the chart
        /// </summary>
        public Axis[] XAxes { get; set; }
        /// <summary>
        /// Used to get or set the Y-axis of the chart
        /// </summary>
        public Axis[] YAxes { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public XYLineChartControl(XYLineChartNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(XYLineChartNodeModel model)
        {
            SetAxes();
            XYLineChart.LegendTextPaint = new SolidColorPaint(ChartStyle.LEGEND_TEXT_COLOR);
            XYLineChart.LegendTextSize = ChartStyle.AXIS_FONT_SIZE;

            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                var seriesRange = DefaultSeries();

                XYLineChart.Series = XYLineChart.Series.Concat(seriesRange);
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected)
            {
                if (model.Labels.Count == model.XValues.Count && model.XValues.Count == model.YValues.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries();

                    XYLineChart.Series = XYLineChart.Series.Concat(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as XYLineChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    XYLineChart.Series = Enumerable.Empty<ISeries>();

                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
                    {
                        var seriesRange = DefaultSeries();
                        XYLineChart.Series = XYLineChart.Series.Concat(seriesRange);
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);
                        XYLineChart.Series = XYLineChart.Series.Concat(seriesRange);

                    }
                });
            }
        }

        private List<LineSeries<ObservablePoint>> DefaultSeries()
        {
            var defaultXValues = new double[][]
                {
                    new double[]{ 0, 1, 2, 3 },
                    new double[]{ 0, 1, 2, 3 },
                    new double[]{ 0, 1, 2, 3 }
                };

            var defaultYValues = new double[][]
            {
                    new double[]{ 0, 1, 2, 3 },
                    new double[]{ 1, 2, 3, 4 },
                    new double[]{ 2, 3, 4, 5 }
            };

            var labels = new List<string> { "Plot 1", "Plot 2", "Plot 3" };
            var seriesRange = new List<LineSeries<ObservablePoint>>();

            for (var i = 0; i < defaultXValues.Length; i++)
            {
                var points = new List<ObservablePoint>();

                for (int j = 0; j < defaultXValues[i].Length; j++)
                {
                    points.Add(new ObservablePoint(defaultXValues[i][j], defaultYValues[i][j]));
                }

                seriesRange.Add(new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Name = labels[i],
                    Fill = null,
                    DataPadding = new LvcPoint(0, 0)
                });
            }           

            return seriesRange;
        }

        private List<LineSeries<ObservablePoint>> UpdateSeries(XYLineChartNodeModel model = null)
        {
            var seriesRange = new List<LineSeries<ObservablePoint>>();
            if(model == null)
            {
                model = this.model;
            }
            if(model.Labels != null && model.Labels.Any()
             && model.XValues != null && model.XValues.Any()
             && model.YValues != null && model.YValues.Any()
             && model.Colors != null && model.Colors.Any())
            {
                for (var i = 0; i < model.Labels.Count; i++)
                {
                    var points = new List<ObservablePoint>();

                    for (int j = 0; j < model.XValues[i].Count; j++)
                    {
                        points.Add(new ObservablePoint(model.XValues[i][j], model.YValues[i][j]));
                    }

                    seriesRange.Add(new LineSeries<ObservablePoint>
                    {
                        Values = points,
                        Name = model.Labels[i],
                        Fill = null,
                        Stroke = new SolidColorPaint(model.Colors[i].ToSKColor()) { StrokeThickness = ChartStyle.XY_LINE_STROKE_THICKNESS },
                        DataPadding = new LvcPoint(0, 0)
                    });
                }
            }

            return seriesRange;
        }

        private void SetAxes()
        {
            AxisColor = new SolidColorPaint(ChartStyle.AXIS_COLOR) { StrokeThickness = ChartStyle.AXIS_STROKE_THICKNESS, SKTypeface = SKTypeface.FromFamilyName(ChartStyle.AXIS_FONT_FAMILY) };
            AxisSeparatorColor = new SolidColorPaint(ChartStyle.AXIS_SEPARATOR_COLOR) { StrokeThickness = ChartStyle.AXIS_STROKE_THICKNESS };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "X-Values",
                    NamePaint = AxisColor,
                    LabelsPaint = AxisColor,
                    Padding = new Padding(ChartStyle.AXIS_NAME_PADDING),
                    TextSize = ChartStyle.AXIS_FONT_SIZE,
                    SeparatorsPaint = AxisSeparatorColor,
                    NameTextSize = ChartStyle.AXIS_FONT_SIZE,
                    MinStep = ChartStyle.LINE_AXIS_MIN_STEP,
                    NamePadding = new Padding(ChartStyle.AXIS_LABEL_PADDING)
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Y-Values",
                    NamePaint = AxisColor,
                    LabelsPaint = AxisColor,
                    Padding = new Padding(ChartStyle.AXIS_NAME_PADDING),
                    TextSize = ChartStyle.AXIS_FONT_SIZE,
                    SeparatorsPaint = AxisSeparatorColor,
                    NameTextSize = ChartStyle.AXIS_FONT_SIZE,
                    NamePadding = new Padding(ChartStyle.AXIS_LABEL_PADDING),
                }
            };
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

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
