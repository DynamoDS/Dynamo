using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class HeatSeriesControl : UserControl, INotifyPropertyChanged
    {
        private readonly HeatSeriesNodeModel model;

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

        public HeatSeriesControl(HeatSeriesNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(HeatSeriesNodeModel model)
        {
            SetAxes();
            HeatSeriesUI.Legend = null;
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                HeatSeriesUI.Series = DefaultValues();
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.XLabels.Count == model.Values.Count && model.XLabels.Count > 0)
                {
                    UpdateValues(model);
                }
            }
        }
        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var model = sender as HeatSeriesNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
                    {
                        HeatSeriesUI.Series = DefaultValues();
                    }
                    else
                    {
                        UpdateValues(model);
                    }
                });
            }
        }

        private IEnumerable<HeatSeries<WeightedPoint>> DefaultValues()
        {
            // X - Products
            var XLabels = new[]
            {
                    "Item-1",
                    "Item-2",
                    "Item-3",
                    "Item-4",
                    "Item-5"
                };

            // Y - Day of the week
            var YLabels = new[]
            {
                    "Monday",
                    "Tuesday",
                    "Wednesday",
                    "Thursday",
                    "Friday",
                    "Saturday",
                    "Sunday"
                };

            // Value for each product on every day of the week
            var chartValues = new ObservableCollection<WeightedPoint>();

            for (var i = 0; i < XLabels.Length; i++)
            {
                for (var j = 0; j < YLabels.Length; j++)
                {
                    chartValues.Add(new WeightedPoint(i, j, ChartStyle.GetRandomInt(0, 10)));
                }
            }

            XAxes.FirstOrDefault().Labels = XLabels;
            YAxes.FirstOrDefault().Labels = YLabels;

            var seriesRange = new List<HeatSeries<WeightedPoint>>()
            {
                new HeatSeries<WeightedPoint>
                {
                    Values = chartValues,
                    PointPadding = new Padding(1),
                }
            };

            return seriesRange;
        }

        private void UpdateValues(HeatSeriesNodeModel model)
        {
            var chartValues = new ObservableCollection<WeightedPoint>();
            HeatSeriesUI.Series = Enumerable.Empty<ISeries>();

            if (model == null)
            {
                model = this.model;
            }

            if (model.XLabels != null && model.XLabels.Any()
             && model.YLabels != null && model.YLabels.Any()
             && model.Values != null && model.Values.Any()
             && model.Colors != null && model.Colors.Any())
            {
                var colors = BuildColors(model);

                for (var i = 0; i < model.XLabels.Count; i++)
                {
                    for (var j = 0; j < model.YLabels.Count; j++)
                    {
                        chartValues.Add(new WeightedPoint(i, j, model.Values[i][j]));
                    }
                }

                XAxes.FirstOrDefault().Labels = model.XLabels;
                YAxes.FirstOrDefault().Labels = model.YLabels;


                HeatSeriesUI.Series = HeatSeriesUI.Series.Concat(new List<HeatSeries<WeightedPoint>>() {
                    new HeatSeries<WeightedPoint>
                    {
                        Values = chartValues,
                        HeatMap = colors.ToArray(),
                        PointPadding = new Padding(1),
                    }
                });
            }
        }

        private IEnumerable<LvcColor> BuildColors(HeatSeriesNodeModel model)
        {
            var colors = new List<LvcColor>();

            // If provided with a single color create range from transparent white to color
            if (model.Colors.Count == 1)
            {
                colors.Add(new SKColor(255, 255, 255).AsLvcColor());
                colors.Add((model.Colors[0].ToSKColor()).AsLvcColor());
            }
            // If provided with several colors create a range for provided colors
            else if (model.Colors.Count > 1)
            {
                for (var i = 0; i < model.Colors.Count; i++)
                {
                    colors.Add((model.Colors[i].ToSKColor()).AsLvcColor());
                }
            }

            return colors;
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

                if (yAdjust >= inputGrid.MinHeight && yAdjust >= ChartStyle.CHART_MIN_HEIGHT)
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
                    TextSize = ChartStyle.AXIS_FONT_SIZE,
                    SeparatorsPaint = AxisSeparatorColor,
                    NameTextSize = ChartStyle.AXIS_FONT_SIZE,
                    MinStep = ChartStyle.AXIS_MIN_STEP,
                    LabelsRotation = -15,
                    NamePadding = new Padding(ChartStyle.AXIS_LABEL_PADDING),
                    Padding = new Padding(ChartStyle.AXIS_LABEL_PADDING),
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Values",
                    NamePaint = AxisColor,
                    LabelsPaint = AxisColor,
                    TextSize = ChartStyle.AXIS_FONT_SIZE,
                    SeparatorsPaint = AxisSeparatorColor,
                    NameTextSize = ChartStyle.AXIS_FONT_SIZE,
                    NamePadding = new Padding(ChartStyle.AXIS_LABEL_PADDING),
                    Padding = new Padding(ChartStyle.AXIS_LABEL_PADDING),
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
