using LiveCharts;
using LiveCharts.Wpf;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();
        private readonly BarChartNodeModel model;

        public event PropertyChangedEventHandler PropertyChanged;
        private static double PADDING = 4.0;
        private static double MAX_COLUMN_WIDTH = 20.0;

        private double MIN_WIDTH = 300;
        private double MIN_HEIGHT = 300;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                DefaultSeries();
            }
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries(model);

                    BarChart.Series.AddRange(seriesRange);
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
                    BarChart.Series.Clear();

                    // Load sample data if any ports are not connected
                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                    {
                        DefaultSeries();
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);

                        BarChart.Series.AddRange(seriesRange.ToArray());
                    }
                });
            }
        }

        private void DefaultSeries()
        {
            BarChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2019",
                    Values = new ChartValues<double> { 5, 6, 7, 8 },
                    ColumnPadding = PADDING,
                    MaxColumnWidth = MAX_COLUMN_WIDTH,
                },
                new ColumnSeries
                {
                    Title = "2020",
                    Values = new ChartValues<double> { 10, 12, 14, 16 },
                    ColumnPadding = PADDING,
                    MaxColumnWidth = MAX_COLUMN_WIDTH,
                },
                new ColumnSeries
                {
                    Title = "2021",
                    Values = new ChartValues<double> { 15, 18, 21, 24 },
                    ColumnPadding = PADDING,
                    MaxColumnWidth = MAX_COLUMN_WIDTH,
                }
            };
        }

        private List<ColumnSeries> UpdateSeries(BarChartNodeModel model)
        {
            var seriesRange = new List<ColumnSeries>();

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
                    seriesRange.Add(new ColumnSeries
                    {
                        Title = model.Labels[i],
                        Values = new ChartValues<double>(model.Values[i]),
                        Fill = model.Colors[i],
                        Stroke = model.Colors[i],
                        ColumnPadding = PADDING,
                        MaxColumnWidth = MAX_COLUMN_WIDTH,
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
        /// 
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }
    }
}
