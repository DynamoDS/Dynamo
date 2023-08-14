using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CoreNodeModelsWpf.Charts.Utilities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl, INotifyPropertyChanged
    {
        private readonly BarChartNodeModel model;
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

        public BarChartControl(BarChartNodeModel model)
        {
            InitializeComponent();
            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(BarChartNodeModel model)
        {
            SetAxes();
            BarChartNode.LegendTextPaint = new SolidColorPaint(ChartStyle.LEGEND_TEXT_COLOR);
            BarChartNode.LegendTextSize = ChartStyle.AXIS_FONT_SIZE;

            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                DefaultSeries();
            }
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries(model);

                    BarChartNode.Series = BarChartNode.Series.Concat(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var model = sender as BarChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    BarChartNode.Series = Enumerable.Empty<ISeries>();

                    // Load sample data if any ports are not connected
                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                    {
                        DefaultSeries();
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);
                        BarChartNode.Series = BarChartNode.Series.Concat(seriesRange);
                    }
                });
            }
        }

        private void DefaultSeries()
        {
            BarChartNode.Series = new List<ISeries>()
                {
                    new ColumnSeries<double>
                    {
                        Name = "2019",
                        Values = ChartStyle.GetRandomList(4),
                        Padding = ChartStyle.COLUMN_GROUP_PADDING,
                        MaxBarWidth = ChartStyle.MAX_COLUMN_WIDTH,
                        Rx = ChartStyle.COLUMN_RADIUS,
                        Ry = ChartStyle.COLUMN_RADIUS,
                    },
                    new ColumnSeries<double>
                    {
                        Name = "2020",
                        Values = ChartStyle.GetRandomList(4),
                        Padding = ChartStyle.COLUMN_GROUP_PADDING,
                        MaxBarWidth = ChartStyle.MAX_COLUMN_WIDTH,
                        Rx = ChartStyle.COLUMN_RADIUS,
                        Ry = ChartStyle.COLUMN_RADIUS,
                    },
                    new ColumnSeries<double>
                    {
                        Name = "2021",
                        Values = ChartStyle.GetRandomList(4),
                        Padding = ChartStyle.COLUMN_GROUP_PADDING,
                        MaxBarWidth = ChartStyle.MAX_COLUMN_WIDTH,
                        Rx = ChartStyle.COLUMN_RADIUS,
                        Ry = ChartStyle.COLUMN_RADIUS,
                    }
                };
        }

        private IEnumerable<ISeries> UpdateSeries(BarChartNodeModel model)
        {
            if (model == null)
            {
                model = this.model;
            }

            var seriesRange = new List<ISeries>();

            if (model != null)
            {
                if (model.Labels != null && model.Labels.Any()
                 && model.Values != null && model.Values.Any()
                 && model.Colors != null && model.Colors.Any())
                {
                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        seriesRange.Add(new ColumnSeries<double>
                        {
                            Name = model.Labels[i],
                            Values = model.Values[i].ToArray(),
                            Fill = new SolidColorPaint(model.Colors[i].ToSKColor()),
                            Stroke = new SolidColorPaint(model.Colors[i].ToSKColor()),
                            Padding = ChartStyle.COLUMN_GROUP_PADDING,
                            MaxBarWidth = ChartStyle.MAX_COLUMN_WIDTH,
                            Rx = ChartStyle.COLUMN_RADIUS,
                            Ry = ChartStyle.COLUMN_RADIUS,
                        });
                    }
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
                    Padding = new LiveChartsCore.Drawing.Padding(ChartStyle.AXIS_NAME_PADDING),
                    TextSize = ChartStyle.AXIS_FONT_SIZE,
                    SeparatorsPaint = AxisSeparatorColor,
                    NameTextSize = ChartStyle.AXIS_FONT_SIZE,
                    ShowSeparatorLines = false,
                    MinStep = ChartStyle.COLUMN_AXIS_MIN_STEP,
                    NamePadding = new LiveChartsCore.Drawing.Padding(ChartStyle.AXIS_LABEL_PADDING)
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Values",
                    NamePaint = AxisColor,
                    LabelsPaint = AxisColor,
                    Padding = new LiveChartsCore.Drawing.Padding(ChartStyle.AXIS_NAME_PADDING),
                    TextSize = ChartStyle.AXIS_FONT_SIZE,
                    SeparatorsPaint = AxisSeparatorColor,
                    NameTextSize = ChartStyle.AXIS_FONT_SIZE,
                    MinStep = ChartStyle.COLUMN_AXIS_MIN_STEP,
                    NamePadding = new LiveChartsCore.Drawing.Padding(ChartStyle.AXIS_LABEL_PADDING),
                }
            };
        }

        /// <summary>
        /// Unsubscribes from ViewModel events
        /// </summary>
        /// 
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }
    }
}
