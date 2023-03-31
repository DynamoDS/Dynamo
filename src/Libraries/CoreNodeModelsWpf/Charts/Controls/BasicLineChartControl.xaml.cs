using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace CoreNodeModelsWpf.Charts
{
    /// <summary>
    /// Interaction logic for BasicLineChartControl.xaml
    /// </summary>
    public partial class BasicLineChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();
        private readonly BasicLineChartNodeModel model;

        private double MIN_WIDTH = 300;
        private double MIN_HEIGHT = 300;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                var seriesRange = DefaultSeries();

                BasicLineChart.Series.AddRange(seriesRange);
            }
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries(model);

                    BasicLineChart.Series.AddRange(seriesRange);
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
                    BasicLineChart.Series.Clear();

                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                    {
                        var seriesRange = DefaultSeries();

                        BasicLineChart.Series.AddRange(seriesRange);
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);

                        BasicLineChart.Series.AddRange(seriesRange);
                    }
                });
            }
        }


        private LineSeries[] DefaultSeries()
        {
            var series = new LineSeries[]
                {
                    new LineSeries { Title = "Series 1", Values = new ChartValues<double> { 4, 6, 5, 2, 4 } },
                    new LineSeries { Title = "Series 2", Values = new ChartValues<double> { 6, 7, 3, 4, 6 } },
                    new LineSeries { Title = "Series 3", Values = new ChartValues<double> { 4, 2, 7, 2, 7 } }
                };

            return series;
        }

        private List<LineSeries> UpdateSeries(BasicLineChartNodeModel model)
        {
            var seriesRange = new List<LineSeries>();

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
                    seriesRange.Add(new LineSeries
                    {
                        Title = model.Labels[i],
                        Values = new ChartValues<double>(model.Values[i]),
                        Stroke = model.Colors[i],
                        StrokeThickness = 2.0,
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

                if (xAdjust >= inputGrid.MinWidth && xAdjust >= MIN_WIDTH)
                {
                    Width = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight && xAdjust >= MIN_HEIGHT)
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
