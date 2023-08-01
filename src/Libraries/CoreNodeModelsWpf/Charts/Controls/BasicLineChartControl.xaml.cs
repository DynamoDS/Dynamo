using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CoreNodeModelsWpf.Charts.Utilities;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace CoreNodeModelsWpf.Charts
{
    /// <summary>
    /// Interaction logic for BasicLineChartControl.xaml
    /// </summary>
    public partial class BasicLineChartControl : UserControl, INotifyPropertyChanged
    {
        private readonly BasicLineChartNodeModel model;
        public event PropertyChangedEventHandler PropertyChanged;

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

        public BasicLineChartControl(BasicLineChartNodeModel model)
        {
            InitializeComponent();
            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);
            
            DataContext = this;
        }

        private void BuildUI(BasicLineChartNodeModel model)
        {
            SetAxes();
            BasicLineChart.LegendTextPaint = new SolidColorPaint(ChartStyle.LEGEND_TEXT_COLOR);
            BasicLineChart.LegendTextSize = ChartStyle.AXIS_FONT_SIZE;

            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                DefaultSeries();
            }
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries(model);

                    BasicLineChart.Series = BasicLineChart.Series.Concat(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var model = sender as BasicLineChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    BasicLineChart.Series = Enumerable.Empty<ISeries>();

                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                    {
                        DefaultSeries();
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);
                        BasicLineChart.Series = BasicLineChart.Series.Concat(seriesRange);
                    }
                });
            }
        }


        private void DefaultSeries()
        {
            BasicLineChart.Series = new List<ISeries>()
            {
                    new LineSeries<double> { Name = "Series 1", Values = ChartStyle.GetRandomList(5), DataPadding = new LvcPoint(0, 0), },
                    new LineSeries<double> { Name = "Series 2", Values = ChartStyle.GetRandomList(5), DataPadding = new LvcPoint(0, 0), },
                    new LineSeries<double> { Name = "Series 3", Values = ChartStyle.GetRandomList(5), DataPadding = new LvcPoint(0, 0), }
            };
        }

        private IEnumerable<LineSeries<double>> UpdateSeries(BasicLineChartNodeModel model)
        {
            var seriesRange = new List<LineSeries<double>>();

            if (model == null)
            {
                model = this.model;
            }

            if (model.Labels != null && model.Labels.Any()
             && model.Values != null && model.Values.Any()
             && model.Colors != null && model.Colors.Any())
            {
                for (var i = 0; i < model.Labels.Count; i++)
                {
                    seriesRange.Add(new LineSeries<double>
                    {
                        Name = model.Labels[i],
                        Values = model.Values[i].ToArray(),
                        Fill = new SolidColorPaint(model.Colors[i].ToSKColor().WithAlpha(80)),
                        Stroke = new SolidColorPaint(model.Colors[i].ToSKColor()) { StrokeThickness = ChartStyle.LINE_STROKE_THICKNESS, IsAntialias = true},
                        GeometryStroke = new SolidColorPaint(model.Colors[i].ToSKColor()) { StrokeThickness = ChartStyle.LINE_STROKE_THICKNESS, IsAntialias = true },
                        DataPadding = new LvcPoint(0, 0),
                    });
                }

            }

            return seriesRange;
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

        private void SetAxes()
        {
            AxisColor = new SolidColorPaint(ChartStyle.AXIS_COLOR) { StrokeThickness = ChartStyle.AXIS_STROKE_THICKNESS, SKTypeface = SKTypeface.FromFamilyName(ChartStyle.AXIS_FONT_FAMILY) };
            AxisSeparatorColor = new SolidColorPaint(ChartStyle.AXIS_SEPARATOR_COLOR) { StrokeThickness = ChartStyle.AXIS_STROKE_THICKNESS };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Index",
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
                    Name = "Values",
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
