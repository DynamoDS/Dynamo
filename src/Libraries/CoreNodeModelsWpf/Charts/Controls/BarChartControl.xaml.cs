using LiveCharts;
using LiveCharts.Wpf;
using SharpDX.Direct2D1;
using System;
using System.ComponentModel;
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
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = new ColumnSeries[model.Labels.Count];

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        seriesRange[i] = new ColumnSeries
                        {
                            Title = model.Labels[i],
                            Values = new ChartValues<double>(model.Values[i]),
                            Fill = model.Colors[i],
                            Stroke = model.Colors[i],
                            ColumnPadding = PADDING,
                            MaxColumnWidth = MAX_COLUMN_WIDTH,
                        };
                    }

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
                    var seriesRange = new ColumnSeries[model.Labels.Count];

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        seriesRange[i] = new ColumnSeries
                        {
                            Title = model.Labels[i],
                            Values = new ChartValues<double>(model.Values[i]),
                            Fill = model.Colors[i],
                            Stroke = model.Colors[i],
                            ColumnPadding = PADDING,
                            MaxColumnWidth = MAX_COLUMN_WIDTH,
                        };
                    }

                    BarChart.Series.Clear();
                    BarChart.Series.AddRange(seriesRange);
                });
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= inputGrid.MinWidth)
                {
                    Width = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight)
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
