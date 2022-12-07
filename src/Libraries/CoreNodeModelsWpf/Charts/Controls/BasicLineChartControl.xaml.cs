using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;

namespace CoreNodeModels.Charts.Controls
{
    /// <summary>
    /// Interaction logic for BasicLineChartControl.xaml
    /// </summary>
    public partial class BasicLineChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BasicLineChartControl(BasicLineChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(BasicLineChartNodeModel model)
        {
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                var seriesRange = new LineSeries[]
                {
                    new LineSeries { Title = "Series 1", Values = new ChartValues<double> { 4, 6, 5, 2, 4 } },
                    new LineSeries { Title = "Series 2", Values = new ChartValues<double> { 6, 7, 3, 4, 6 } },
                    new LineSeries { Title = "Series 3", Values = new ChartValues<double> { 4, 2, 7, 2, 7 } }
                };

                BasicLineChart.Series.AddRange(seriesRange);
            }
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    LineSeries[] seriesRange = new LineSeries[model.Labels.Count];

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        seriesRange[i] = new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = new ChartValues<double>(model.Values[i]),
                            Stroke = model.Colors[i],
                            Fill = Brushes.Transparent
                        };
                    }

                    BasicLineChart.Series.AddRange(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as BasicLineChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    LineSeries[] seriesRange = new LineSeries[model.Labels.Count];

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        seriesRange[i] = new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = new ChartValues<double>(model.Values[i]),
                            Stroke = model.Colors[i],
                            Fill = Brushes.Transparent,
                            //PointGeometrySize = 0
                        };
                    }

                    BasicLineChart.Series.Clear();
                    BasicLineChart.Series.AddRange(seriesRange);
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
