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
    /// Interaction logic for PieChartControl.xaml
    /// </summary>
    public partial class PieChartControl : UserControl, INotifyPropertyChanged
    {
        private readonly PieChartNodeModel model;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PieChartControl(PieChartNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(PieChartNodeModel model)
        {
            PieChart.LegendTextPaint = new SolidColorPaint(ChartStyle.LEGEND_TEXT_COLOR);
            PieChart.LegendTextSize = ChartStyle.AXIS_FONT_SIZE;
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                var seriesRange = DefaultSeries();

                PieChart.Series = PieChart.Series.Concat(seriesRange);
            }

            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries(model);

                    PieChart.Series = PieChart.Series.Concat(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as PieChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    PieChart.Series = Enumerable.Empty<ISeries>();

                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                    {
                        var seriesRange = DefaultSeries();

                        PieChart.Series = PieChart.Series.Concat(seriesRange);
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);

                        PieChart.Series = PieChart.Series.Concat(seriesRange);
                    }
                });
            }
        }

        private IEnumerable<ISeries> DefaultSeries()
        {
            var series = new List<ISeries>()
            {
                new PieSeries<double> { Name = "Item1", Values = new List<double> { 100.0 }, DataLabelsPaint = new SolidColorPaint(SKColors.White), DataLabelsSize = ChartStyle.PIE_LABEL_TEXT_SIZE },
                new PieSeries<double> { Name = "Item2", Values = new List<double> { 100.0 }, DataLabelsPaint = new SolidColorPaint(SKColors.White), DataLabelsSize = ChartStyle.PIE_LABEL_TEXT_SIZE },
                new PieSeries<double> { Name = "Item3", Values = new List<double> { 100.0 }, DataLabelsPaint = new SolidColorPaint(SKColors.White), DataLabelsSize = ChartStyle.PIE_LABEL_TEXT_SIZE }
            };

            return series;
        }

        private IEnumerable<ISeries> UpdateSeries(PieChartNodeModel model)
        {
            var seriesRange = new List<ISeries>();

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
                    seriesRange.Add(new PieSeries<double>
                    {
                        Name = model.Labels[i],
                        Values = new List<double> { model.Values[i] },
                        Fill = new SolidColorPaint(model.Colors[i].ToSKColor()),
                        DataLabelsPaint = new SolidColorPaint(SKColors.White),
                        DataLabelsSize = ChartStyle.PIE_LABEL_TEXT_SIZE
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
                    Height = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight && xAdjust >= ChartStyle.CHART_MIN_HEIGHT)
                {
                    Width = yAdjust;
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
