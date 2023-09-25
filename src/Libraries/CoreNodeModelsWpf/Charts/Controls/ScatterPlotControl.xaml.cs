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
    /// Interaction logic for ScatterPlotControl.xaml
    /// </summary>
    public partial class ScatterPlotControl : UserControl, INotifyPropertyChanged
    {
        private readonly ScatterPlotNodeModel model;

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

        public ScatterPlotControl(ScatterPlotNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(ScatterPlotNodeModel model)
        {
            SetAxes();
            ScatterPlot.LegendTextPaint = new SolidColorPaint(ChartStyle.LEGEND_TEXT_COLOR);
            ScatterPlot.LegendTextSize = ChartStyle.AXIS_FONT_SIZE;

            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                var plots = DefaultSeries();
                ScatterPlot.Series = ScatterPlot.Series.Concat(plots);
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected)
            {
                if (model.Labels.Count == model.XValues.Count && model.XValues.Count == model.YValues.Count && model.Labels.Count > 0)
                {
                    var plots = UpdateSeries();
                    ScatterPlot.Series = ScatterPlot.Series.Concat(plots);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as ScatterPlotNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    ScatterPlot.Series = Enumerable.Empty<ISeries>();

                    // Load sample data if any ports are not connected
                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
                    {
                        var plots = DefaultSeries();
                        ScatterPlot.Series = ScatterPlot.Series.Concat(plots);
                    }
                    else
                    {
                        var plots = UpdateSeries(model);
                        ScatterPlot.Series = ScatterPlot.Series.Concat(plots);
                    }
                });
            }
        }

        private IEnumerable<ScatterSeries<ObservablePoint>> DefaultSeries()
        {
            var ValuesA = new ObservableCollection<ObservablePoint>();
            var ValuesB = new ObservableCollection<ObservablePoint>();
            var ValuesC = new ObservableCollection<ObservablePoint>();

            for (var i = 0; i < 20; i++)
            {
                ValuesA.Add(new ObservablePoint(ChartStyle.GetRandomDouble(10), ChartStyle.GetRandomDouble(10)));
                ValuesB.Add(new ObservablePoint(ChartStyle.GetRandomDouble(10), ChartStyle.GetRandomDouble(10)));
                ValuesC.Add(new ObservablePoint(ChartStyle.GetRandomDouble(10), ChartStyle.GetRandomDouble(10)));
            }

            var plot1 = new ScatterSeries<ObservablePoint> { Name = "Plot 1", Values = ValuesA };
            var plot2 = new ScatterSeries<ObservablePoint> { Name = "Plot 2", Values = ValuesB };
            var plot3 = new ScatterSeries<ObservablePoint> { Name = "Plot 3", Values = ValuesC };

            var plots = new List<ScatterSeries<ObservablePoint>>() { plot1, plot2, plot3 };

            return plots;
        }

        private List<ScatterSeries<ObservablePoint>> UpdateSeries(ScatterPlotNodeModel model = null)
        {
            var plots = new List<ScatterSeries<ObservablePoint>>();

            if(model == null)
            {
                model = this.model;
            }

            if (model.Labels != null && model.Labels.Any()
             && model.XValues != null && model.XValues.Any()
             && model.YValues != null && model.YValues.Any()
             && model.Colors != null && model.Colors.Any())
            {
                // For each set of points
                for (var i = 0; i < model.Labels.Count; i++)
                {
                    var points = new ObservableCollection<ObservablePoint>();

                    // For each x-value list
                    for (int j = 0; j < model.XValues[i].Count; j++)
                    {
                        points.Add(new ObservablePoint(model.XValues[i][j], model.YValues[i][j]));
                    }

                    plots.Add(new ScatterSeries<ObservablePoint>
                    {
                        Name = model.Labels[i],
                        Values = points,
                        Fill = new SolidColorPaint(model.Colors[i].ToSKColor()),
                    });
                }
            }

            return plots;
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= ChartStyle.CHART_MIN_WIDTH/*inputGrid.MinWidth*/ )
                {
                    Width = xAdjust;
                }

                if (yAdjust >= ChartStyle.CHART_MIN_HEIGHT/*inputGrid.MinHeight*/)
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
