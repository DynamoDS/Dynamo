using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CoreNodeModels.Charts.Controls
{
    /// <summary>
    /// Interaction logic for BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BarChartControl(BarChartNodeModel model)
        {

            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

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
                        Values = new ChartValues<double> { 5, 6, 7, 8 }
                    },
                    new ColumnSeries
                    {
                        Title = "2020",
                        Values = new ChartValues<double> { 10, 12, 14, 16 }
                    },
                    new ColumnSeries
                    {
                        Title = "2021",
                        Values = new ChartValues<double> { 15, 18, 21, 24 }
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
                            Fill = model.Colors[i]
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
                            Fill = model.Colors[i]
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
    }
}
